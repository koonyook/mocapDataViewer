using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.IO;

namespace SyncAll
{
	public struct EnhanceBody
	{
		//not the final version but enhanced from the visual data
		public ComplexSample spineShoulder;
		public PointSample shoulderRight, shoulderLeft;
		public PointSample elbowRight;
		public PointSample wristRight;

		public void clear()
		{
			spineShoulder.isVoid = true;
			spineShoulder.isInterpolated = false;
			shoulderLeft.isVoid = true;
			shoulderLeft.isInterpolated = false;
			shoulderRight.isVoid = true;
			shoulderRight.isInterpolated = false;
			elbowRight.isVoid = true;
			elbowRight.isInterpolated = false;
			wristRight.isVoid = true;
			wristRight.isInterpolated = false;
		}

		public bool haveAllData()
		{
			return spineShoulder.haveData() && shoulderRight.haveData() && shoulderLeft.haveData() && elbowRight.haveData() && wristRight.haveData();
		}
	}

	public struct ComplexSample
	{
		public bool isVoid;		//no data
		public bool isInterpolated;
		public Vector3 position;
		public Quaternion orientation;

		public bool haveData()
		{
			return !isVoid || isInterpolated;
		}

		public Vector3 getColor()
		{
			if (isInterpolated)
				return new Vector3(1, 1, 1);	//white
			else
				return new Vector3(1.0f, 0.27f, 0f);	//orange
		}
	}

	public struct PointSample
	{
		public bool isVoid;		//no data
		public bool isInterpolated;
		public Vector3 position;

		public bool haveData()
		{
			return !isVoid || isInterpolated;
		}

		public Vector3 getColor()
		{
			if (isInterpolated)
				return new Vector3(1, 1, 1);    //white
			else
				return new Vector3(1.0f, 0.27f, 0f);    //orange
		}
	}

	/// <summary>
	/// what ever in this class must be easy to understand
	/// </summary>
    public struct ExtractedSample
    {
		public bool valid;  //if valid==false, don't use this sample in training

		//variable that are not used in learning but important as intermediate variable
		public int visualIndex;
		public double ktime;    //use a lot in interpolation

		public Quaternion kTorsoQuaternion;
		public Vector3 kTorsoPosition;
		public Vector3 kRightShoulderPosition;
		public Vector3 kRightElbowPosition;
		
		public Quaternion kRightForearmQuaternion;
		public Vector3 kRightForearmPointingDirection;
		public Vector3 kGravityDirection;
		public Quaternion kRightUpperArmQuaternion_tmp;	//for debugging
		public Quaternion kRightUpperArmQuaternion;
		public Quaternion torsoRightUpperArmQuaternion_tmp;	//for debugging
		public Vector3 kRightUpperArmPointingDirection;
		public Vector3 torso_rightElbowPosition;

		//additional parameter from extractAllMetricsFromIMUandFittingResult
		public Vector3[] jointPosition;
		public Matrix3	frame9Orientation;
		public Quaternion kRightForearmQuaternionFromIMU;

		//right arm
		public Vector3 rightWristAcc;	//in g unit (average from 3 samples) [invalid: (0,0,0)]
		public Vector3 rightWristGyro;  //in deg/s (average from 3 samples)	 [invalid: (0,0,0)]

		public Vector3 torso_rightWristPosition;    //in meter [invalid: (0,0,0)]
		public float rightWristSpeed;   // m/s positive only [invalid: -1]

		public float rightElbowAngle;    //in deg (0-180) [invalid: -1]
		public float rightElbowAngleVelocity;    //in deg/sec [invalid: 999999]	
		public Vector3 torso_rightForearmPointingDirection;  //a unit vector [invalid: (0,0,0)]
		public float rightForearmPronation;  //in deg [invalid: 999]
		public Vector3 torso_rightUpperArmPointingDirection;    //a unit vector [invalid: (0,0,0)]
		public float rightShoulderInternalRotation; //in deg	[invalid: 999999]
		public Vector3 rightUpperArmVirtualGyro;    //in deg/s  [invalid: (999,999,999)]

		//trunk
		public float trunkFrontalTilt; //deg (0=face down, 90=normal, 180=face up)
		public float trunkLateralTilt; //deg (0=left shoulder point upward ,90=normal,180=left shoulder point downward) 

	}

	public static class ToPython
	{
		public static void saveExtractedSample(ExtractedSample[] data, int firstValidIndex, int lastValidIndex, string filepath)
		{
			
			using (BinaryWriter binWriter = new BinaryWriter(File.Open(filepath, FileMode.Create)))
			{
				
				//http://stackoverflow.com/questions/21641735/python-struct-calcsize-length
				//Any particular type(byte, integer, etc.) can only begin at an offset that is a multiple of its standard size.
				//so it is much safer to put large variable at front, follow be smaller type
				string format = "difffffffffffffffffffffffff"; //1double, 1int , 25 float
				byte d = (byte)format.Length;    //dimension of the data (index + timestamp + 25)
				//int w = 4 + 8 + 4 * 25;
				List<string> headerlist = new List<string>(new string[] {
					"time",
					"index",
					"accX",
					"accY",
					"accZ",
					"gyroX",
					"gyroY",
					"gyroZ",
					"wristX",
					"wristY",
					"wristZ",
					"wristSpd",
					"elbowAng",
					"elbowAngVel",
					"forePointX",
					"forePointY",
					"forePointZ",
					"forePronate",
					"upPointX",
					"upPointY",
					"upPointZ",
					"shoIntRot",
					"upGyroX",
					"upGyroY",
					"upGyroZ",
					"trunkFTilt",
					"trunkLTilt"
				});

				//generate header: tell about the following file infastructure
				binWriter.Write(d);
				binWriter.Write(System.Text.Encoding.ASCII.GetBytes(format));	//if I put string directly, it will append the string size at the first byte (which I don't need)
				foreach (string header in headerlist)
				{
					binWriter.Write((byte)header.Length);
					binWriter.Write(System.Text.Encoding.ASCII.GetBytes(header));
				}

				//put data in
				for (int i = firstValidIndex; i <= lastValidIndex; i++)
				{
					binWriter.Write(data[i].ktime);
					binWriter.Write(data[i].visualIndex);
					binWriter.Write(data[i].rightWristAcc.X);
					binWriter.Write(data[i].rightWristAcc.Y);
					binWriter.Write(data[i].rightWristAcc.Z);
					binWriter.Write(data[i].rightWristGyro.X);
					binWriter.Write(data[i].rightWristGyro.Y);
					binWriter.Write(data[i].rightWristGyro.Z);
					binWriter.Write(data[i].torso_rightWristPosition.X);
					binWriter.Write(data[i].torso_rightWristPosition.Y);
					binWriter.Write(data[i].torso_rightWristPosition.Z);
					binWriter.Write(data[i].rightWristSpeed);
					binWriter.Write(data[i].rightElbowAngle);
					binWriter.Write(data[i].rightElbowAngleVelocity);
					binWriter.Write(data[i].torso_rightForearmPointingDirection.X);
					binWriter.Write(data[i].torso_rightForearmPointingDirection.Y);
					binWriter.Write(data[i].torso_rightForearmPointingDirection.Z);
					binWriter.Write(data[i].rightForearmPronation);
					binWriter.Write(data[i].torso_rightUpperArmPointingDirection.X);
					binWriter.Write(data[i].torso_rightUpperArmPointingDirection.Y);
					binWriter.Write(data[i].torso_rightUpperArmPointingDirection.Z);
					binWriter.Write(data[i].rightShoulderInternalRotation);
					binWriter.Write(data[i].rightUpperArmVirtualGyro.X);
					binWriter.Write(data[i].rightUpperArmVirtualGyro.Y);
					binWriter.Write(data[i].rightUpperArmVirtualGyro.Z);
					binWriter.Write(data[i].trunkFrontalTilt);
					binWriter.Write(data[i].trunkLateralTilt);
				}
			}
		}
	}
}
