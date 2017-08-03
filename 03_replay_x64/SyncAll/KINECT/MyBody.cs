using Microsoft.Kinect;
using OpenTK;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SyncAll
{
	public class BodyFrameCollector
	{
		List<MyBodyFrame> list;

		public BodyFrameCollector()
		{
			//RuntimeTypeModel.Default.Add(typeof(MyBodyFrame), true)[3].SupportNull = true;
		}

		public void reset()
		{
			list = new List<MyBodyFrame>();
		}

		public void add(MyBodyFrame frame)
		{
			list.Add(frame);
		}

		public void save(string filename)
		{
			using (var file = File.Create(filename))
			{
				Serializer.Serialize(file, list);
			}
		}

		public List<MyBodyFrame> load(string filename)
		{
			using (var file = File.OpenRead(filename))
			{
				list = Serializer.Deserialize<List<MyBodyFrame>>(file);
			}
			return list;
		}
	}

	[ProtoContract]
	public class MyBodyFrame
	{
		[ProtoMember(1)]
		public double kTime;

		[ProtoMember(2)]
		public MyVector4 FloorClipPlane;

		[ProtoMember(3)]
		public MyBody[] bodies; 

		public MyBodyFrame() { }

		public MyBodyFrame(BodyFrame bodyFrame, double kTime = 0)
		{
			this.kTime = kTime;

			FloorClipPlane = new MyVector4(bodyFrame.FloorClipPlane);

			Body[] tmpBodies = new Body[6] { null, null, null, null, null, null };
			bodyFrame.GetAndRefreshBodyData(tmpBodies);

			bodies = new MyBody[6] { null, null, null, null, null, null };
			for (int i = 0; i < 6; i++)
			{
				if (tmpBodies[i] != null)
				{
					bodies[i] = new MyBody(tmpBodies[i]);
				}
			}
		}

		public MyBody getFirstAvailableBody()
		{
			for (int i = 0; i < 6; i++)
			{
				if (bodies[i] != null && bodies[i].IsTracked)
				{
					return bodies[i];
				}
			}
			return null;
		}

		public int getFirstAvailableBodyIndex()
		{
			for (int i = 0; i < 6; i++)
			{
				if (bodies[i] != null && bodies[i].IsTracked)
				{
					return i;
				}
			}
			return -1;
		}

		public MyBody getNearestBodyToKinect()
		{
			int nearestIndex = -1;
			float nearestDistance = 999999;
			for (int i = 0; i < 6; i++)
			{
				if (bodies[i] != null && bodies[i].IsTracked && bodies[i].Joints[JointType.SpineShoulder].TrackingState!=TrackingState.NotTracked)
				{
					if(bodies[i].Joints[JointType.SpineShoulder].Position.Z < nearestDistance)
					{
						nearestDistance = bodies[i].Joints[JointType.SpineShoulder].Position.Z;
						nearestIndex = i;
					}
				}
			}

			if (nearestIndex == -1)
				return null;
			else
				return bodies[nearestIndex];
		}

		public MyBody getBodyByTrackingID(ulong targetTrackingID)
		{
			for (int i = 0; i < 6; i++)
			{
				if (bodies[i] != null && bodies[i].IsTracked && bodies[i].TrackingId==targetTrackingID)
				{
					return bodies[i];
				}
			}
			return null;
		}

		public int getBodyIndexByTrackingID(ulong targetTrackingID)
		{
			for (int i = 0; i < 6; i++)
			{
				if (bodies[i] != null && bodies[i].IsTracked && bodies[i].TrackingId==targetTrackingID)
				{
					return i;
				}
			}
			return -1;
		}
	}

	[ProtoContract]
	public class MyBody    //Body is not serializable, this class is
	{
		[ProtoMember(1)]
		public ulong TrackingId;
		[ProtoMember(2)]
		public bool IsTracked;
		[ProtoMember(3)]
		public Dictionary<JointType, MyVector4> JointOrientations=new Dictionary<JointType, MyVector4>();
		[ProtoMember(4)]
		public Dictionary<JointType, MyJoint> Joints = new Dictionary<JointType, MyJoint>();
		[ProtoMember(5)]
		public TrackingConfidence HandLeftConfidence;
		[ProtoMember(6)]
		public HandState HandLeftState;
		[ProtoMember(7)]
		public TrackingConfidence HandRightConfidence;
		[ProtoMember(8)]
		public HandState HandRightState;

		public MyBody() { }

		public MyBody(Body body)
		{
			TrackingId = body.TrackingId;
			IsTracked = body.IsTracked;

			foreach (JointType jt in Enum.GetValues(typeof(JointType)))
			{
				JointOrientations[jt] = new MyVector4(body.JointOrientations[jt].Orientation);
				Joints[jt] = new MyJoint(body.Joints[jt]);
			}

			HandLeftConfidence = body.HandLeftConfidence;
			HandLeftState = body.HandLeftState;
			HandRightConfidence = body.HandRightConfidence;
			HandRightState = body.HandRightState;
		}
	}

	[ProtoContract]
	public struct MyJoint
	{
		[ProtoMember(1)]
		public MyVector3 Position;
		[ProtoMember(2)]
		public TrackingState TrackingState;

		public MyJoint(Joint joint)
		{
			Position = new MyVector3(joint.Position);
			TrackingState = joint.TrackingState;
		}
	}

	[ProtoContract]
	public struct MyVector4
	{
		[ProtoMember(1)]
		public float W;
		[ProtoMember(2)]
		public float X;
		[ProtoMember(3)]
		public float Y;
		[ProtoMember(4)]
		public float Z;

		public MyVector4(Microsoft.Kinect.Vector4 v4)
		{
			W = v4.W;
			X = v4.X;
			Y = v4.Y;
			Z = v4.Z;
		}

		public Quaternion getQuaternion()
		{
			return new Quaternion(X, Y, Z, W);
		}

		public Vector3 getXyz()
		{
			return new Vector3(X,Y,Z);
		}

		public Vector3 getEulerAngle()
		{
			return MyMath.getEulerAngleFromQuaternion(new Quaternion(X, Y, Z, W));
		}
	}

	[ProtoContract]
	public struct MyVector3
	{
		[ProtoMember(1)]
		public float X;
		[ProtoMember(2)]
		public float Y;
		[ProtoMember(3)]
		public float Z;

		public MyVector3(CameraSpacePoint v3)
		{ 
			X = v3.X;
			Y = v3.Y;
			Z = v3.Z;
		}

		public CameraSpacePoint getCameraSpacePoint()
		{
			CameraSpacePoint a = new CameraSpacePoint();
			a.X = X;
			a.Y = Y;
			a.Z = Z;
			return a;
		}

		public Vector3 getVector3()
		{
			return new Vector3(X, Y, Z);
		}
	}
}
