using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using Microsoft.Kinect;

namespace SyncAll
{

	public static class access
	{
		static List<Tuple<Thing, string>> all;

		public static void addObject(Thing e, string name)
		{
			all.Add(new Tuple<Thing, string>(e, name));
		}

		public static void addObject(Thing e)
		{
			all.Add(new Tuple<Thing, string>(e, ""));
		}

		public static KinectPointCloud kinectPointCloud;

		public static Line upwardLine;

		public static ThreeAxis offsetAxis;
		public static ThreeAxis fixedAxis;

		public static ThreeAxis imuRodAxis;
		public static Line xLine, yLine, zLine;
		public static Line magneticLine;

		public static ThreeAxis visionRodAxis;
		//public static CalibrationRod calibrationRod;
		static public ThreeAxis upperTorso;
		static public Octahedron rightShoulder, leftShoulder;

		static public Bone rightUpperArm;

		public static Bone rightForearm;
		public static Bone leftForearm;

		public static Bone rightForearmOriginal;    //use sensor quaternion
		public static Bone leftForearmOriginal;		//use sensor quaternion (and rotate 180 deg around Z axis to make IMU Y axis point in forearm pointing direction 

		//public static Bone leftForearm;
		
		static public Octahedron strapSurface;
		static public Octahedron rightIMU;
		static public Octahedron debugMarker;
		static public Octahedron sensorSphereCenter;

		static public LinePath kinectArm;

		static public Line v0;

		static public Octahedron head;
		static public Octahedron neck;
		static public Octahedron spineMid, spineBase;
		static public Octahedron wristRight,elbowRight,shoulderRight;
		static public Octahedron kneeLeft,kneeRight;

		//enhanced set (should be in orange)
		static public Octahedron enSpineShoulder;
		static public Octahedron enShoulderLeft;
		static public Octahedron enShoulderRight;
		static public Octahedron enElbowRight;
		static public Octahedron enWristRight;

		//for preprocessing test
		const int normalN = 250;


		static public KinectSkeleton kinectSkeleton;
		static public ViconSkeleton viconSkeleton;



		static public Octahedron x0,x1,x2;

		static public Octahedron[] bgPoints = new Octahedron[1000];

		

		public static List<Tuple<Thing, string>> getAllThings(Space parent)
		{
			all = new List<Tuple<Thing, string>>();

			kinectPointCloud = new KinectPointCloud(parent);
			addObject(kinectPointCloud);

			//addObject(new Grid(4.5f, 0.25f));

			upwardLine = new Line(parent, new Vector3(1f, 0f, 0f));
			upwardLine.begin = new Vector3(0, 0, 0);
			upwardLine.end = parent.deviceUpwardDirection;
			addObject(upwardLine);

			//addObject(new KinectOpticalAxis(parent, new Vector3(0, 1, 1)));

			rightForearm = new Bone(parent, new Vector3(1f, 1f, 0f), JointType.ElbowRight, JointType.WristRight);
			addObject(rightForearm);

			leftForearm = new Bone(parent, new Vector3(1f, 1f, 0f), JointType.ElbowLeft, JointType.WristLeft);
			addObject(leftForearm);

			
			rightForearmOriginal = new Bone(parent, new Vector3(0f, 1f, 0f), JointType.ElbowRight, JointType.WristRight);
			addObject(rightForearmOriginal);

			leftForearmOriginal = new Bone(parent, new Vector3(0f, 1f, 0f), JointType.ElbowLeft, JointType.WristLeft);
			addObject(leftForearmOriginal);

			rightUpperArm = new Bone(parent, new Vector3(0f, 1f, 1f), JointType.ShoulderRight, JointType.ElbowRight);
			addObject(rightUpperArm);

			upperTorso = new ThreeAxis(parent, new Vector3(1f, 1f, 1f));
			addObject(upperTorso);

			rightShoulder = new Octahedron(parent, 0.01f, new Vector3(0.1f, 0.9f, 0.1f));
			addObject(rightShoulder);
			leftShoulder = new Octahedron(parent, 0.01f, new Vector3(0.1f, 0.9f, 0.1f));
			addObject(leftShoulder);

			head = new Octahedron(parent, 0.01f, new Vector3(0.1f, 0.9f, 0.1f));
			addObject(head);
			neck = new Octahedron(parent, 0.01f, new Vector3(0.1f, 0.9f, 0.1f));
			addObject(neck);
			spineMid = new Octahedron(parent, 0.01f, new Vector3(0.1f, 0.9f, 0.1f));
			addObject(spineMid);
			spineBase = new Octahedron(parent, 0.01f, new Vector3(0.1f, 0.9f, 0.1f));
			addObject(spineBase);
			wristRight = new Octahedron(parent, 0.01f, new Vector3(0.1f, 0.9f, 0.1f));
			addObject(wristRight);
			elbowRight = new Octahedron(parent, 0.01f, new Vector3(0.1f, 0.9f, 0.1f));
			addObject(elbowRight);
			shoulderRight = new Octahedron(parent, 0.01f, new Vector3(0.1f, 0.9f, 0.1f));
			addObject(shoulderRight);
			
			kneeLeft = new Octahedron(parent, 0.01f, new Vector3(0.1f,0.9f,0.1f));
			addObject(kneeLeft);
			kneeRight = new Octahedron(parent, 0.01f, new Vector3(0.1f,0.9f,0.1f));
			addObject(kneeRight);

			enSpineShoulder = new Octahedron(parent, 0.02f, new Vector3(1.0f, 0.27f, 0f));
			addObject(enSpineShoulder);
			enShoulderLeft = new Octahedron(parent, 0.02f, new Vector3(1.0f, 0.27f, 0f));
			addObject(enShoulderLeft);
			enShoulderRight = new Octahedron(parent, 0.02f, new Vector3(1.0f, 0.27f, 0f));
			addObject(enShoulderRight);
			enElbowRight = new Octahedron(parent, 0.02f, new Vector3(1.0f, 0.27f, 0f));
			addObject(enElbowRight);
			enWristRight = new Octahedron(parent, 0.02f, new Vector3(1.0f, 0.27f, 0f));
			addObject(enWristRight);

			enSpineShoulder.visible = false;
			enShoulderLeft.visible = false;
			enShoulderRight.visible = false;
			enElbowRight.visible = false;
			enWristRight.visible = false;

			
			kinectSkeleton = new KinectSkeleton(parent, new Vector3(1f,0.4f,0.4f));
			kinectSkeleton.displayMode = KinectSkeleton.DisplayMode.All;
			addObject(kinectSkeleton);

			viconSkeleton = new ViconSkeleton(parent, new Vector3(0.6f,1f,0.8f));
			addObject(viconSkeleton);


			x0 = new Octahedron(parent, 0.01f, new Vector3(1.0f, 0.27f, 0f));
			addObject(x0);
			x1 = new Octahedron(parent, 0.01f, new Vector3(0f, 1f, 0.27f));
			addObject(x1);
			x2 = new Octahedron(parent, 0.01f, new Vector3(0.27f, 0f, 1f));
			addObject(x2);
			x0.visible=x1.visible=x2.visible=false;

			for(int i=0;i<1000;i++)
			{ 
				bgPoints[i]=new Octahedron(parent,0.01f,new Vector3(0.1f,0.9f,0.2f));
				addObject(bgPoints[i]);
			}


			return all;
		}
	}
}
