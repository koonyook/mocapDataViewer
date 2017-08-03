using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SyncAll
{
	public class KinectSkeleton:Thing
	{ 
		public enum DisplayMode { Skeleton, Joint, All };
		public DisplayMode displayMode = DisplayMode.Joint;

		Vector3 color;

		int edgeIndexCount=24*2;
		//int pointIndexCount=0;

		public KinectSkeleton(Space parentSpace, Vector3 color)
		{
			this.parentSpace = parentSpace;
			//JointType 0 - 24
			vertexCount = 25;
			indexCount = edgeIndexCount;	//skeleton
			colorCount = vertexCount;

			this.color = color;
        }

		public override void initializeArray()
		{
			int[] allIndex = new int[] {
                //spineBase to head
				0, 1,
				1, 20,
				20, 2,
				2, 3,
				//spineShoulder to handTipLeft and thumb
				20, 4,
				4, 5,
				5, 6,
				6, 7,
				7, 21,
				7, 22,
				//spineShoulder to handTipRight and thumb
				20, 8,
				8, 9,
				9, 10,
				10, 11,
				11, 23,
				11, 24,
				//spineBase to footLeft
				0, 12,
				12, 13,
				13, 14,
				14, 15,
				//spineBase to footRight
				0, 16,
				16, 17,
				17, 18,
				18, 19
				//joint node do not need index, just render those point directly
				//0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24
			};

			for (int i = 0; i < colorCount; i++)
				colorArray[firstColorPtr + i] = color;

			for (int i = 0; i < indexCount; i++)
				indexArray[firstIndexPtr + i] = allIndex[i] + firstVertexPtr;
		}

		public void updateVertexFromCore(Body body)
		{
			for(int i=0;i<25;i++)
			{
				vertexArray[firstVertexPtr + i] = KinectUtil.convertCameraSpacePointToVector3(body.Joints[(JointType)i].Position);
			}
		}

		public void updateVertexFromCore(MyBody body)
		{
			for(int i=0;i<25;i++)
			{
				vertexArray[firstVertexPtr + i] = body.Joints[(JointType)i].Position.getVector3();
			}
		}

		public override void updateVertex()
		{
			return;
		}

		public override void updateColor()
		{
			return;
		}

		public override void updateIndex()
		{
			return;
		}

		public override void updateAnimationAndModelMatrix(float time)
		{
			modelMatrix = parentSpace.modelMatrixFromCameraSpace;
			return;
		}

		public override void draw(ref int currentIndex, ref int currentVertex)
		{
			if(visible)
			{ 
				if(displayMode== DisplayMode.Skeleton || displayMode==DisplayMode.All)
				{ 
					GL.LineWidth(5f);
					GL.DrawElements(BeginMode.Lines, edgeIndexCount, DrawElementsType.UnsignedInt, currentIndex * sizeof(uint));
					GL.LineWidth(1f);
				}

				if(displayMode==DisplayMode.Joint || displayMode==DisplayMode.All)
				{
					GL.PointSize(10f);
					GL.DrawArrays(PrimitiveType.Points, currentVertex, vertexCount);
					GL.PointSize(1f);
				}
			}

			currentIndex += indexCount;
			currentVertex += vertexCount;
		}
	}
}
