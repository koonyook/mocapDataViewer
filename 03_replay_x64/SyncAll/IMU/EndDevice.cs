using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncAll
{
	public class EndDevice
	{
		public byte deviceID=0xFF;
		public byte deviceType=0xFF;

		public int listIndex = 0;

		public byte mode = 0xFF;
		public byte samplingRate = 0;

		//public UInt32 syncTimestamp = 0;
		public double slopeToBase=1, interceptToBase=0; //(for mobile only) used to convert timestamp from mobile to base system
		public double slopeToKinect = 0, interceptToKinect = 0; //(for base only) used to convert timestamp from base to kinect system

		private float[] magCenter=new float[3] { 0, 0, 0 };
		private float[,] magComp = new float[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

		public BaseOrientationTracker baseOrientationTracker = new BaseOrientationTracker();
		public GyroBiasTracker gyroBiasTracker = new GyroBiasTracker();
		public Kalman kalmanFilter = new Kalman();

		private double ACC_SENSITIVITY, GYRO_SENSITIVITY, MAG_SENSITIVITY;

		public EndDevice(byte id,byte type, double ACC_SENSITIVITY=0.00018,double GYRO_SENSITIVITY= 70.0 / 1000 * Math.PI / 180, double MAG_SENSITIVITY=0.080)
		{
			deviceID = id;
			deviceType = type;

			this.ACC_SENSITIVITY = ACC_SENSITIVITY;
			this.GYRO_SENSITIVITY = GYRO_SENSITIVITY;
			this.MAG_SENSITIVITY = MAG_SENSITIVITY;
		}
		

		private void initMagCalParams(byte[] flattenParameters)	//change magnetometer calibration parameter
		{
			//length of flattenParameters = 48 bytes (12 float)
			Buffer.BlockCopy(flattenParameters, 0, magCenter, 0, 12);
			Buffer.BlockCopy(flattenParameters, 12, magComp, 0, 36);
		}

		public void saveMagCalParamsToFile(string filename)		//save for offline case
		{
			SettingManager.saveMagneticCalibrationToFile(filename, magCenter, magComp);
		}

		public void loadMagCalParamsFromFile(string filename)	//offline case
		{
			SettingManager.loadMagneticCalibrationFromFile(filename, ref magCenter, ref magComp);
		}

		public Quaternion quickMagneticSensorQuaternion(RawSample raw)
		{
			//calculate from acc and mag, enough for a static measurement such as panel calibration
			return MyMath.calculateMagneticSensorQuaternionFromMagneticGravity(transformRawAcc(raw.acc), transformRawMag(raw.mag));
		}

		public Vector3 transformRawAcc(raw_short a)
		{
			return new Vector3(a.x,a.y,a.z)*(float)ACC_SENSITIVITY;
		}

		public Vector3 transformRawGyro(raw_short g)
		{
			return gyroBiasTracker.convertToRadPerSec(g, GYRO_SENSITIVITY);
		}

		public Vector3 transformRawGyro(raw_short g, Vector3 bias)
		{
			float gx = (float)((g.x - bias.X) * GYRO_SENSITIVITY);
			float gy = (float)((g.y - bias.Y) * GYRO_SENSITIVITY);
			float gz = (float)((g.z - bias.Z) * GYRO_SENSITIVITY);

			return new Vector3(gx, gy, gz);
		}

		public Vector3 transformRawMag(raw_short m)
		{
			//offset
			float fx = m.x - magCenter[0];
			float fy = m.y - magCenter[1];
			float fz = m.z - magCenter[2];

			//transform
			float rx = (float)(fx * magComp[0, 0] + fy * magComp[0, 1] + fz * magComp[0, 2]);
			float ry = (float)(fx * magComp[1, 0] + fy * magComp[1, 1] + fz * magComp[1, 2]);
			float rz = (float)(fx * magComp[2, 0] + fy * magComp[2, 1] + fz * magComp[2, 2]);

			return new Vector3(rx, ry, rz )*(float)MAG_SENSITIVITY;
		}

		public void calculateTimeParameters(double[] mobileTimestamp,double[] baseTimestamp, int n)	//n must be at least 2
		{
			MyMath.linearRegression(mobileTimestamp, baseTimestamp, n, out this.slopeToBase, out this.interceptToBase);
		}

		public double convertTimestampToBaseSystem(double mTime)
		{
			return mTime * slopeToBase + interceptToBase;	//this will last about 6 hours (prescale:64)
		}

		public void updateKalmanFilter(RawSample raw)
		{
			//Vector3 g = transformRawGyro(raw.gyro);	//original, good result
			Vector3 g = transformRawGyro(raw.gyro,raw.gyroBias); //this will make calculation depend on current bias, not future bias
			Vector3 a = transformRawAcc(raw.acc);
			Vector3 m = transformRawMag(raw.mag);
			kalmanFilter.filterUpdate(g, a, m);
		}
	}
}
