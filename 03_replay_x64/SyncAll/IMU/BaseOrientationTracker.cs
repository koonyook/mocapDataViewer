using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncAll
{
    public class BaseOrientationTracker
    {
		private const float BASE_AVERAGE_RATIO = 0.95f;

		Vector3 baseAccAvg = new Vector3(0),
				baseMagAvg = new Vector3(0);

		Quaternion magneticBaseQuaternion = new Quaternion(0, 0, 0, 1);
		double g_m_angle = 90;  //will be changed by base data

		public Quaternion update(Vector3 acc, Vector3 mag)
		{
			if (baseAccAvg.Length == 0)
			{
				baseAccAvg = acc;
				baseMagAvg = mag;
			}
			else
			{
				baseAccAvg = baseAccAvg * BASE_AVERAGE_RATIO + acc * (1 - BASE_AVERAGE_RATIO);
				baseMagAvg = baseMagAvg * BASE_AVERAGE_RATIO + mag * (1 - BASE_AVERAGE_RATIO);
			}

			//static position, this method is good enough
			magneticBaseQuaternion = MyMath.calculateMagneticSensorQuaternionFromMagneticGravity(baseAccAvg, baseMagAvg);
			g_m_angle = Math.Acos(Vector3.Dot(baseAccAvg.Normalized(), baseMagAvg.Normalized()));

			return magneticBaseQuaternion;
		}
    }
}
