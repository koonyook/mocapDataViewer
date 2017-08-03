using Google.Protobuf.MocapRecord;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncAll
{
	//[Serializable]	//don't want to serialize this anymore
	public class RawSample
	{
		public byte blockNumber;   //0-99 always in the first byte
		public byte deviceID;

		public UInt32 timestamp;
		public Quaternion orientation;
		public float forearmTilt;
		public Int32 pressure;
		public UInt16 capTouchUp;
		public UInt16 capTouchDown;
		public raw_short acc;
		public raw_short gyro;
		public raw_short mag;
		public byte button;
		public byte battery;
		public byte worn;

		//important augmented data
		public double btime;    //timestamp in baseIMU clock
		public double ktime;	//timestamp in Kinect clock (ms)

		public Vector3 gyroBias;

		public RawSample()
		{ }

		public RawSample(RawSampleProto input)
		{
			this.blockNumber=(byte)input.BlockNumber;
			this.deviceID=(byte)input.DeviceID;
			this.timestamp=input.Timestamp;
			this.orientation.W=input.Orientation.W;
			this.orientation.X=input.Orientation.X;
			this.orientation.Y=input.Orientation.Y;
			this.orientation.Z=input.Orientation.Z;
			this.forearmTilt=input.ForearmTilt;
			this.pressure=input.Pressure;
			this.capTouchUp=(UInt16)input.CapTouchUp;
			this.capTouchDown=(UInt16)input.CapTouchDown;
			this.acc.x=(Int16)input.Acc.X;
			this.acc.y=(Int16)input.Acc.Y;
			this.acc.z=(Int16)input.Acc.Z;
			this.gyro.x=(Int16)input.Gyro.X;
			this.gyro.y=(Int16)input.Gyro.Y;
			this.gyro.z=(Int16)input.Gyro.Z;
			this.mag.x=(Int16)input.Mag.X;
			this.mag.y=(Int16)input.Mag.Y;
			this.mag.z=(Int16)input.Mag.Z;
			this.button=(byte)input.Button;
			this.battery=(byte)input.Battery;
			this.worn=(byte)input.Worn;
			this.btime=input.Btime;
			this.ktime=input.Ktime;

			this.gyroBias.X=input.GyroBias.X;
			this.gyroBias.Y=input.GyroBias.Y;
			this.gyroBias.Z=input.GyroBias.Z;
		}

		public RawSampleProto getRawSampleProto()
		{
			return new RawSampleProto
				{
					BlockNumber=this.blockNumber,
					DeviceID=this.deviceID,
					Timestamp=this.timestamp,
					Orientation=new QuaternionProto {W=this.orientation.W, X=this.orientation.X, Y=this.orientation.Y, Z=this.orientation.Z },
					ForearmTilt=this.forearmTilt,
					Pressure=this.pressure,
					CapTouchUp=this.capTouchUp,
					CapTouchDown=this.capTouchDown,
					Acc=new RawShortProto {X=this.acc.x, Y=this.acc.y, Z=this.acc.z },
					Gyro=new RawShortProto {X=this.gyro.x, Y=this.gyro.y, Z=this.gyro.z },
					Mag=new RawShortProto {X=this.mag.x, Y=this.mag.y, Z=this.mag.z },
					Button=this.button,
					Battery=this.battery,
					Worn=this.worn,
					Btime=this.btime,
					Ktime=this.ktime,
					GyroBias=new Vector3Proto {X=this.gyroBias.X, Y=this.gyroBias.Y, Z=this.gyroBias.Z }
				};
		}
	}

	//[Serializable]
	public struct raw_short
	{
		public short x, y, z;

		public Vector3 quickCast()
		{
			return new Vector3(x, y, z);
		}
	}
}
