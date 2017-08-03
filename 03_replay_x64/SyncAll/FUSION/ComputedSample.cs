using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncAll
{
	public class ComputedSample  //data that are computed later (not in the saved file)
	{
		public double ktime;    //in ms in kinect time system

		//processed data
		public Vector3 acc;    //g
		public Vector3 gyro;    //rad/s
		public Vector3 mag;     //mgauss

		public Quaternion MagneticSensorQuaternion;  //S -> E quaternion

		public ComputedSample()
		{

		}

		//interpolate from 2 inertiaRecord
		public ComputedSample(ComputedSample before, ComputedSample after, double targetTimestamp)
		{
			double beforeRatio = (targetTimestamp - before.ktime) / (after.ktime - before.ktime);
			double afterRatio = 1 - beforeRatio;

			//timestamp = (int)(before.timestamp * beforeRatio + after.timestamp * afterRatio);
			ktime = targetTimestamp;

			acc = before.acc * (float)beforeRatio + after.acc * (float)afterRatio;
			gyro = before.gyro * (float)beforeRatio + after.gyro * (float)afterRatio;
			mag = before.mag * (float)beforeRatio + after.mag * (float)afterRatio;

			MagneticSensorQuaternion = MyMath.quaternionInterpolate_SLERP(before.MagneticSensorQuaternion, after.MagneticSensorQuaternion, (float)beforeRatio);

		}

	}
}
