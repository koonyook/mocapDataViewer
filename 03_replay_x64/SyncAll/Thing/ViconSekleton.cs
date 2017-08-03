using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace SyncAll
{
	public class ViconSkeleton:Thing	//only upper body
	{
		//public enum DisplayMode { Skeleton, Joint, All };
		//public DisplayMode displayMode = DisplayMode.Joint;
		public bool displayRealMarker=true;
		public bool displayVirtualMarker=true;
		public bool displayVirtualLink=true;

		Vector3 color;

		//int edgeIndexCount=24*2;
		//int pointIndexCount=0;

		public ViconSkeleton(Space parentSpace, Vector3 color)
		{
			this.parentSpace = parentSpace;

			vertexCount = markerOrder.Length;
			indexCount = allIndex.Length;	//skeleton
			colorCount = vertexCount;

			this.color = color;
        }

		string[] markerOrder = new string[] {
			"LFHD","RFHD","LBHD","RBHD",		//0-3
			"C7","T10","CLAV","STRN","RBAK",	//4-8
			"LSHO","LUPA","LELB","LFRA","LWRA","LWRB","LFIN",	//9-15
			"RSHO","RUPA","RELB","RFRA","RWRA","RWRB","RFIN",	//16-22
			"LASI","RASI","LPSI","RPSI",						//23-26
			//virtual markers
			"LSJC","LEJC","LWJC",								//27-29
			"RSJC","REJC","RWJC",								//30-32
			"THOO", //thorax origin (a bit behind the clavical)	//33
		};

		int[] allIndex = new int[] {
                //left branch
				33, 27,
				27, 28,
				28, 29,
				//right branch
				33, 30,
				30, 31,
				31, 32,
				//joint node do not need index, just render those point directly
				//0-26 for real markers
				//27-33 for virtual markers
		};

		public override void initializeArray()
		{
			for (int i = 0; i < colorCount; i++)
				colorArray[firstColorPtr + i] = color;

			for (int i = 0; i < indexCount; i++)
				indexArray[firstIndexPtr + i] = allIndex[i] + firstVertexPtr;
		}

		//convert to Kinect ref frame before feeding in
		public void updateVertexFromCore(Dictionary<string, Vector3> markerPosition)	//include virtual marker
		{
			for(int i=0;i<markerOrder.Length;i++)
			{
				if(markerPosition.ContainsKey(markerOrder[i])) 
					vertexArray[firstVertexPtr + i] = markerPosition[markerOrder[i]];
				else
				{
					vertexArray[firstVertexPtr + i] = Vector3.Zero;
					// Debug.WriteLine("missing:"+markerOrder[i]);
				}
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
				 
				if(displayVirtualLink)
				{ 
					GL.LineWidth(5f);
					GL.DrawElements(BeginMode.Lines, indexCount, DrawElementsType.UnsignedInt, currentIndex * sizeof(uint));
					GL.LineWidth(1f);
				}

				if(displayRealMarker && displayVirtualMarker)
				{
					GL.PointSize(10f);
					GL.DrawArrays(PrimitiveType.Points, currentVertex, vertexCount);	//all of them
					GL.PointSize(1f);
				}
				else if(displayRealMarker)
				{
					GL.PointSize(10f);
					GL.DrawArrays(PrimitiveType.Points, currentVertex, 27);
					GL.PointSize(1f);
				}
				else if(displayVirtualMarker)
				{
					GL.PointSize(10f);
					GL.DrawArrays(PrimitiveType.Points, currentVertex+27, 7);
					GL.PointSize(1f);
				}
			}

			currentIndex += indexCount;
			currentVertex += vertexCount;
		}
	}
}
