using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Kinect;
using OpenTK;
namespace SyncAll
{
	static public class OfflineCoordinateMapper
	{
		static public CameraIntrinsics intrinsics;
		static public PointF[] depthToCameraSpaceTable;

		static public void initForOfflineUsage(string kinectDirectory)
		{
			intrinsics = loadCameraIntrinsics(kinectDirectory+"cameraIntrinsics.bin");
			depthToCameraSpaceTable = loadDepthFrameToCameraSpaceTable(kinectDirectory+"depthToCameraSpaceTable.bin");
		}

		static public void saveForOfflineUsage(string kinectDirectory, CoordinateMapper coordinateMapper)
		{
			saveCameraIntrinsicsToFile(kinectDirectory + "cameraIntrinsics.bin", coordinateMapper.GetDepthCameraIntrinsics());
			saveDepthFrameToCameraSpaceTable(kinectDirectory + "depthToCameraSpaceTable.bin", coordinateMapper.GetDepthFrameToCameraSpaceTable());
		}

		//tested
		static public Vector2 project3DPointToDepth(Vector3 pointInKinectRef)
		{
			float x = pointInKinectRef.X / pointInKinectRef.Z;
			float y = -pointInKinectRef.Y / pointInKinectRef.Z;	//need negative sign because it is left-handed
			float r_2 = x * x + y * y;
			float r_4 = r_2 * r_2;
			float r_6 = r_4 * r_2;
			float rd = 1 + intrinsics.RadialDistortionSecondOrder * r_2 + intrinsics.RadialDistortionFourthOrder * r_4 + intrinsics.RadialDistortionSixthOrder * r_6;

			x = intrinsics.FocalLengthX * (x * rd) + intrinsics.PrincipalPointX;
			y = intrinsics.FocalLengthY * (y * rd) + intrinsics.PrincipalPointY;

			Vector2 pf;
			pf.X = x;
			pf.Y = y;
			return pf;
		} 

		//not tested yet
		static public float pointCloudSurfaceDepth(ushort[] depth, Vector3 pointInKinectRef)
		{
			//if drawing a ray from to the point
			//this function try to answer that at what depth the point cloud surface will cut that ray
			//this function is useful when I want to check if a joint might be occluded by something

			Vector2 depthPixel = project3DPointToDepth(pointInKinectRef);

			int x = (int)Math.Round(depthPixel.X);
			int y = (int)Math.Round(depthPixel.Y);

			if(x<=0 || y<=0 || x>=511 || y>=423)
			{
				return 0;	//not in field of view
			}
			else
			{
				float nearestToTheCamera = 5f;
				//take 9 points around that area
				for(int i=-1;i<=1;i++)
				{
					for(int j=-1;j<=1;j++)
					{
						float tmp = depth[(y + i) * 512 + (x + j)] / 1000f;
						if (tmp != 0 && tmp < nearestToTheCamera)
							nearestToTheCamera = tmp;
					}
				}

				if(nearestToTheCamera==5f)
				{
					return 0;
				}
				else
				{
					return nearestToTheCamera;
				}
			}
		}

		static public Vector3 projectDepthTo3D(int depthPixelRow, int depthPixelColumn, float depth)   //depth in meters
		{
			PointF lutValue = depthToCameraSpaceTable[depthPixelRow * 512 + depthPixelColumn]; //not sure about the order
			return new Vector3(lutValue.X * depth, lutValue.Y * depth, depth);
		}

		static public void MapDepthFrameToCameraSpaceUsingIntPtr(ushort[] depth, IntPtr dstVec3Ptr)
		{
			unsafe
			{
				Vector3* dst = (Vector3*)dstVec3Ptr.ToPointer();
				int len = 424 * 512;
				for (int i=0;i<len;i++)
				{
					float d = depth[i] / 1000f;
					dst[i] = new Vector3(depthToCameraSpaceTable[i].X * d, depthToCameraSpaceTable[i].Y * d, d);
				}
			}
		}

		static public void saveCameraIntrinsicsToFile(string filename,CameraIntrinsics intr)
		{
			using (BinaryWriter binWriter = new BinaryWriter(File.Open(filename, FileMode.Create)))
			{
				binWriter.Write(intr.FocalLengthX);
				binWriter.Write(intr.FocalLengthY);
				binWriter.Write(intr.PrincipalPointX);
				binWriter.Write(intr.PrincipalPointY);
				binWriter.Write(intr.RadialDistortionSecondOrder);
				binWriter.Write(intr.RadialDistortionFourthOrder);
				binWriter.Write(intr.RadialDistortionSixthOrder);
			}
		}

		static public CameraIntrinsics loadCameraIntrinsics(string filename)
		{
			CameraIntrinsics intr = new CameraIntrinsics();

			using (BinaryReader binReader = new BinaryReader(File.Open(filename, FileMode.Open)))
			{
				intr.FocalLengthX = binReader.ReadSingle();
				intr.FocalLengthY = binReader.ReadSingle();

				intr.PrincipalPointX = binReader.ReadSingle();
				intr.PrincipalPointY = binReader.ReadSingle();

				intr.RadialDistortionSecondOrder = binReader.ReadSingle();
				intr.RadialDistortionFourthOrder = binReader.ReadSingle();
				intr.RadialDistortionSixthOrder = binReader.ReadSingle();
			}
			return intr;
		}

		static public void saveDepthFrameToCameraSpaceTable(string filename, PointF[] table)
		{
			using (BinaryWriter binWriter = new BinaryWriter(File.Open(filename, FileMode.Create)))
			{
				for (int i = 0; i < 424 * 512; i++)
				{
					binWriter.Write(table[i].X);
					binWriter.Write(table[i].Y);
                }
			}
		}

		static public PointF[] loadDepthFrameToCameraSpaceTable(string filename)
		{
			PointF[] table = new PointF[424 * 512];

			using (BinaryReader binReader = new BinaryReader(File.Open(filename, FileMode.Open)))
			{
				for (int i = 0; i < 424 * 512; i++)
				{
					table[i].X = binReader.ReadSingle();
					table[i].Y = binReader.ReadSingle();
				}
			}
			return table;
		}


	}
}
