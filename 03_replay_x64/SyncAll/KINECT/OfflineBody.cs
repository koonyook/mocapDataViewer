using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncAll
{
	/*
	[Serializable]		
    public class OfflineBody	//Body is not serializable, this class is
    {
		public ulong TrackingId;

		public bool IsTracked;
		public IReadOnlyDictionary<JointType, JointOrientation> JointOrientations;
		public IReadOnlyDictionary<JointType, Joint> Joints;

		public TrackingConfidence HandLeftConfidence;
		public HandState HandLeftState;
		public TrackingConfidence HandRightConfidence;
		public HandState HandRightState;

		public OfflineBody(Body body)
		{
			TrackingId = body.TrackingId;

			IsTracked = body.IsTracked;
			JointOrientations = body.JointOrientations;
			Joints = body.Joints;

			HandLeftConfidence = body.HandLeftConfidence;
			HandLeftState = body.HandLeftState;
			HandRightConfidence = body.HandRightConfidence;
			HandRightState = body.HandRightState;
		}
	}

	[Serializable]
	public class OfflineBodyFrame
	{
		double kTime;
		public Vector4 FloorClipPlane;
		public OfflineBody[] bodies = new OfflineBody[6] { null, null, null, null, null, null };

		public OfflineBodyFrame(BodyFrame bodyFrame, double kTime=0)
		{
			this.kTime = kTime;

			FloorClipPlane = bodyFrame.FloorClipPlane;
			Body[] tmpBodies = new Body[6] { null, null, null, null, null, null };
			bodyFrame.GetAndRefreshBodyData(tmpBodies);

			for(int i=0;i<6;i++)
			{
				if(tmpBodies[i]!=null)
				{
					bodies[i] = new OfflineBody(tmpBodies[i]);
				}
			}
		}
	}
	*/
	

}
