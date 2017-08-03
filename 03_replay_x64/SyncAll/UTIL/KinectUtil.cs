using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;
using OpenTK;

namespace SyncAll
{
	public static class KinectUtil
	{
		public static int getFirstTrackedBodyIndex(Body[] bodies)
		{
			int targetBodyIndex = -1;
			int i;
			for (i = 0; i < 6; i++)
			{
				Body body = bodies[i];

				if (body != null && body.IsTracked)
				{
					targetBodyIndex = i;
					break;
				}
			}
			if (i == 6)
				return -1;   //there is no one in this frame
			else
				return targetBodyIndex;
		}

		public static Quaternion convertVector4ToQuaternion(Microsoft.Kinect.Vector4 v)
		{
			return new Quaternion(v.X, v.Y, v.Z, v.W);
		}

		public static Vector3 convertCameraSpacePointToVector3(Microsoft.Kinect.CameraSpacePoint p)
		{
			return new Vector3(p.X, p.Y, p.Z);
		}

		public static Vector2 convertDepthSpacePointToVector2(DepthSpacePoint p)
		{
			return new Vector2(p.X, p.Y);
		}

		public static Vector2 convertColorSpacePointToVector2(ColorSpacePoint p)
		{
			return new Vector2(p.X, p.Y);
		}

		public static bool isWithinColorWindow(Vector2 p)
		{
			if (p.X < 0 || p.X >= 1920 || p.Y < 0 || p.Y >= 1080)
				return false;
			else
				return true;
		}
	}
}
