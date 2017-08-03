using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.IO;

using Microsoft.Kinect;
using System.Diagnostics;
namespace SyncAll
{
	public abstract class Thing
	{
		public Space parentSpace;

		public bool visible = true;

		public Vector3 position = Vector3.Zero;
		public Vector3 rotation = Vector3.Zero;
		public Vector3 scale = Vector3.One;

		public int vertexCount;
		public int indexCount;     //may equal to zero
		public int colorCount;  //should be equal to vertexCount

		public Vector3[] vertexArray;	//this is Space.vertexArray (shared across all things in that space)
		public Vector3[] colorArray;
		public int[] indexArray;

		public int firstVertexPtr;
		public int firstColorPtr;
		public int firstIndexPtr;

		public Matrix4 modelMatrix = Matrix4.Identity;
		public Matrix4 modelViewProjectionMatrix = Matrix4.Identity; //last projection matrix

		public abstract void initializeArray();

		public abstract void updateVertex();
		public abstract void updateColor();
		public abstract void updateIndex();

		public abstract void updateAnimationAndModelMatrix(float time);

		public abstract void draw(ref int currentIndex, ref int currentVertex);
	}

	public class Octahedron : Thing
	{
		private Vector3[] allVertex;
		private Vector3[] allColor;
		private int[] allIndex;

		public Octahedron(Space parentSpace, float radius, Vector3 color)
		{
			this.parentSpace = parentSpace;

			vertexCount = 6;
			indexCount = 24;
			colorCount = 6;

			allVertex = new Vector3[] {
				new Vector3(0, 0,  radius),
				new Vector3(radius,0,  0),
				new Vector3( 0, radius,  0),
				new Vector3(-radius, 0,  0),
				new Vector3(0, -radius,  0),
				new Vector3(0, 0,  -radius)
			};

			allColor = new Vector3[] {
				color,
				color,
				color,
				color,
				color,
				color
			};

			allIndex = new int[] {
                //top
                0, 1, 2,
				0, 2, 3,
				0, 3, 4,
				0, 4, 1,
                //bottom
                5, 1, 2,
				5, 2, 3,
				5, 3, 4,
				5, 4, 1
			};
		}

		public override void initializeArray()
		{
			for (int i = 0; i < vertexCount; i++)
				vertexArray[firstVertexPtr + i] = allVertex[i];

			for (int i = 0; i < colorCount; i++)
				colorArray[firstColorPtr + i] = allColor[i];

			for (int i = 0; i < indexCount; i++)
				indexArray[firstIndexPtr + i] = allIndex[i] + firstVertexPtr;
		}

		public void changeColor(Vector3 newColor)
		{
			for (int i = 0; i < colorCount; i++)
				colorArray[firstColorPtr + i] = newColor;
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
			modelMatrix = Matrix4.CreateTranslation(position) * parentSpace.modelMatrixFromCameraSpace;
		}

		public override void draw(ref int currentIndex, ref int currentVertex)
		{
			if (visible)
				GL.DrawElements(BeginMode.Triangles, indexCount, DrawElementsType.UnsignedInt, currentIndex * sizeof(uint));

			currentIndex += indexCount;
			currentVertex += vertexCount;
		}
	}
	public class Cube : Thing
	{
		private int style;  //can be 0, 1 or 2

		private Vector3[] allVertex;
		private Vector3[] allColor;
		private int[] allIndex;

		public Cube(Space parentSpace, int selectedStyle)
		{
			this.parentSpace = parentSpace;

			style = selectedStyle;
			vertexCount = 8;
			indexCount = 36;
			colorCount = 8;

			allVertex = new Vector3[] {
				new Vector3(-0.5f, -0.5f,  -0.5f),
				new Vector3(0.5f, -0.5f,  -0.5f),
				new Vector3(0.5f, 0.5f,  -0.5f),
				new Vector3(-0.5f, 0.5f,  -0.5f),
				new Vector3(-0.5f, -0.5f,  0.5f),
				new Vector3(0.5f, -0.5f,  0.5f),
				new Vector3(0.5f, 0.5f,  0.5f),
				new Vector3(-0.5f, 0.5f,  0.5f),
			};

			allColor = new Vector3[] {
				new Vector3( 1f, 0f, 0f),
				new Vector3( 0f, 0f, 1f),
				new Vector3( 0f, 1f, 0f),
				new Vector3( 1f, 0f, 0f),
				new Vector3( 0f, 0f, 1f),
				new Vector3( 0f, 1f, 0f),
				new Vector3( 1f, 0f, 0f),
				new Vector3( 0f, 0f, 1f)
			};

			allIndex = new int[] {
                //left
                0, 2, 1,
				0, 3, 2,
                //back
                1, 2, 6,
				6, 5, 1,
                //right
                4, 5, 6,
				6, 7, 4,
                //top
                2, 3, 6,
				6, 3, 7,
                //front
                0, 7, 3,
				0, 4, 7,
                //bottom
                0, 1, 5,
				0, 5, 4
			};
		}

		public override void initializeArray()
		{
			for (int i = 0; i < vertexCount; i++)
				vertexArray[firstVertexPtr + i] = allVertex[i];

			for (int i = 0; i < colorCount; i++)
				colorArray[firstColorPtr + i] = allColor[i];

			for (int i = 0; i < indexCount; i++)
				indexArray[firstIndexPtr + i] = allIndex[i] + firstVertexPtr;
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
			if (style == 0)
			{
				;
			}
			else if (style == 1)
			{
				position = new Vector3(0.3f, -0.5f + (float)Math.Sin(time), -3.0f);
				rotation = new Vector3(0.55f * time, 0.25f * time, 0);
				scale = new Vector3(0.1f, 0.1f, 0.1f);
			}
			else if (style == 2)
			{
				position = new Vector3(-1f, 0.5f + (float)Math.Cos(time), -2.0f);
				rotation = new Vector3(-0.25f * time, -0.35f * time, 0);
				scale = new Vector3(0.25f, 0.25f, 0.25f);
			}

			modelMatrix = Matrix4.CreateScale(scale) * Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationZ(rotation.Z) * Matrix4.CreateTranslation(position);
		}

		public override void draw(ref int currentIndex, ref int currentVertex)
		{
			GL.DrawElements(BeginMode.Triangles, indexCount, DrawElementsType.UnsignedInt, currentIndex * sizeof(uint));

			currentIndex += indexCount;
			currentVertex += vertexCount;
		}
	}

	public class Grid : Thing
	{
		private Vector3[] allVertex, allColor;

		public Grid(float radius, float squareSize)
		{
			vertexCount = (int)(4 * (1 + 2 * radius / squareSize));
			indexCount = 0;
			colorCount = vertexCount;

			allVertex = new Vector3[vertexCount];
			allColor = new Vector3[colorCount];
			int i = 0;
			for (float point = -radius; point <= radius; point += squareSize)
			{
				allVertex[i] = new Vector3(point, 0, -radius);
				allVertex[i + 1] = new Vector3(point, 0, +radius);
				allVertex[i + 2] = new Vector3(-radius, 0, point);
				allVertex[i + 3] = new Vector3(+radius, 0, point);

				if (point == 0f)
				{
					//red for z positive
					allColor[i] = new Vector3(1f, 1f, 0);
					allColor[i + 1] = new Vector3(1f, 0, 0);
					//green for x positive
					allColor[i + 2] = new Vector3(1f, 1f, 0);
					allColor[i + 3] = new Vector3(0, 1f, 0);
				}
				else
				{
					allColor[i] = new Vector3(0, 0, 1);
					allColor[i + 1] = new Vector3(0, 0, 1);
					allColor[i + 2] = new Vector3(0, 0, 1);
					allColor[i + 3] = new Vector3(0, 0, 1);
				}

				i += 4;
			}
			/*
            allColor = new Vector3[colorCount];
            for(i=0;i<colorCount;i++)
            {
                allColor[i] = new Vector3(1f, 1f, 0f);
            }
             */
		}

		public override void initializeArray()
		{
			for (int i = 0; i < vertexCount; i++)
				vertexArray[firstVertexPtr + i] = allVertex[i];

			for (int i = 0; i < colorCount; i++)
				colorArray[firstColorPtr + i] = allColor[i];
		}

		public override void updateVertex()
		{
			return;
		}

		public override void updateIndex()
		{
			return;
		}

		public override void updateColor()
		{
			return;
		}

		public override void updateAnimationAndModelMatrix(float time)
		{
			//no change in scale, rotation, position

			//no change in modelMatrix
			return;
		}

		public override void draw(ref int currentIndex, ref int currentVertex)
		{
			GL.DrawArrays(PrimitiveType.Lines, currentVertex, vertexCount);

			currentIndex += indexCount;
			currentVertex += vertexCount;
		}
	}

	public class Protractor : Thing
	{
		private Vector3[] allVertex;
		private float radius = 2f;
		public Protractor()
		{
			vertexCount = colorCount = 8;
			indexCount = 0;
			allVertex = new Vector3[vertexCount];

			allVertex[0] = new Vector3(-radius, -radius, 0);
			allVertex[1] = new Vector3(radius, radius, 0);

			allVertex[2] = new Vector3(-radius, radius, 0);
			allVertex[3] = new Vector3(radius, -radius, 0);

			allVertex[4] = new Vector3(-radius, 0, 0);
			allVertex[5] = new Vector3(radius, 0, 0);

			allVertex[6] = new Vector3(0, radius, 0);
			allVertex[7] = new Vector3(0, -radius, 0);
		}

		public override void initializeArray()
		{
			for (int i = 0; i < vertexCount; i++)
				vertexArray[firstVertexPtr + i] = allVertex[i];

			for (int i = 0; i < colorCount; i++)
				colorArray[firstColorPtr + i] = new Vector3(1f, 1f, 1f);
		}

		public override void updateVertex()
		{
			return;
		}

		public override void updateIndex()
		{
			return;
		}

		public override void updateColor()
		{
			return;
		}

		public override void updateAnimationAndModelMatrix(float time)
		{
			//no change in scale, rotation, position

			//no change in modelMatrix
			return;
		}

		public override void draw(ref int currentIndex, ref int currentVertex)
		{
			GL.DrawArrays(PrimitiveType.Lines, currentVertex, vertexCount);

			currentIndex += indexCount;
			currentVertex += vertexCount;
		}
	}

	public class Line : Thing
	{
		public Vector3 begin, end;
		public Vector3 colorBegin, colorEnd;

		public Line(Space parentSpace, Vector3 lineColor)
		{
			this.parentSpace = parentSpace;

			vertexCount = 2;
			indexCount = 0;
			colorCount = 2;

			begin = new Vector3(0);
			end = new Vector3(0);

			colorBegin = colorEnd = lineColor;
		}

		public Line(Space parentSpace, Vector3 lineColorBegin, Vector3 lineColorEnd)
		{
			this.parentSpace = parentSpace;

			vertexCount = 2;
			indexCount = 0;
			colorCount = 2;

			begin = new Vector3(0);
			end = new Vector3(0);

			colorBegin = lineColorBegin;
			colorEnd = lineColorEnd;
		}

		public override void initializeArray()
		{
			vertexArray[firstVertexPtr + 0] = begin;
			vertexArray[firstVertexPtr + 1] = end;

			colorArray[firstColorPtr + 0] = colorBegin;
			colorArray[firstColorPtr + 1] = colorEnd;
		}

		public override void updateVertex()
		{
			vertexArray[firstVertexPtr] = begin;
			vertexArray[firstVertexPtr + 1] = end;

			//vertexArray[firstVertexPtr + 1] = parentSpace.deviceUpwardDirection;
		}

		public override void updateIndex()
		{
			return;
		}

		public override void updateColor()
		{
			return;
		}

		public override void updateAnimationAndModelMatrix(float time)
		{
			//no change in scale, rotation, position

			//no change in modelMatrix
			modelMatrix = parentSpace.modelMatrixFromCameraSpace;
			return;
		}

		public override void draw(ref int currentIndex, ref int currentVertex)
		{
			if (visible)
			{
				GL.LineWidth(5f);
				GL.DrawArrays(PrimitiveType.Lines, currentVertex, vertexCount);
				GL.LineWidth(1f);
			}
			currentIndex += indexCount;
			currentVertex += vertexCount;
		}
	}

	public class NormalCloud : Thing    //for normal vector
	{
		public int maxSize = 20000; //maximum number of line to display
		public int displaySize = 0;
		public Vector3 beginColor, endColor;

		public NormalCloud(Space parentSpace, Vector3 beginColor, Vector3 endColor)
		{
			this.parentSpace = parentSpace;

			vertexCount = maxSize * 2;
			indexCount = 0;
			colorCount = maxSize * 2;

			this.beginColor = beginColor;
			this.endColor = endColor;
		}

		public override void initializeArray()
		{
			for (int i = 0; i < maxSize; i++)
			{
				vertexArray[firstVertexPtr + i * 2 + 0] = new Vector3(0);
				vertexArray[firstVertexPtr + i * 2 + 1] = new Vector3(0);

				colorArray[firstColorPtr + i * 2 + 0] = beginColor;
				colorArray[firstColorPtr + i * 2 + 1] = endColor;
			}
		}

		public void updateFromCore(List<Vector3> beginList, List<Vector3> endList)
		{
			displaySize = Math.Min(beginList.Count, maxSize);
			for (int i = 0; i < displaySize; i++)
			{
				vertexArray[firstVertexPtr + i * 2 + 0] = beginList[i];
				vertexArray[firstVertexPtr + i * 2 + 1] = endList[i];
			}
		}

		public override void updateVertex()
		{
			//vertexArray[firstVertexPtr] = begin;
			//vertexArray[firstVertexPtr + 1] = end;

			//vertexArray[firstVertexPtr + 1] = parentSpace.deviceUpwardDirection;
		}

		public override void updateIndex()
		{
			return;
		}

		public override void updateColor()
		{
			return;
		}

		public override void updateAnimationAndModelMatrix(float time)
		{
			//no change in scale, rotation, position

			//no change in modelMatrix
			modelMatrix = parentSpace.modelMatrixFromCameraSpace;
			return;
		}

		public override void draw(ref int currentIndex, ref int currentVertex)
		{
			//GL.LineWidth(5f);
			GL.DrawArrays(PrimitiveType.Lines, currentVertex, displaySize * 2);
			//GL.LineWidth(1f);

			currentIndex += indexCount;
			currentVertex += vertexCount;
		}
	}

	public class RigidPointCloud : Thing
	{
		public Quaternion orientation = new Quaternion();

		public int maxSize = 20000; //maximum number of line to display
		public int displaySize = 0;
		public Vector3 color;

		public RigidPointCloud(Space parentSpace, Vector3 color)
		{
			this.parentSpace = parentSpace;

			vertexCount = maxSize;
			indexCount = 0;
			colorCount = maxSize;

			this.color = color;
		}

		public override void initializeArray()
		{
			for (int i = 0; i < maxSize; i++)
			{
				vertexArray[firstVertexPtr + i] = new Vector3(0);
				colorArray[firstColorPtr + i] = color;
			}
		}

		public void addVertexFromCore(List<Vector3> pointList)
		{
			for (int i = 0; i < pointList.Count && displaySize < maxSize; i++, displaySize++)
			{
				vertexArray[firstVertexPtr + displaySize] = pointList[i];
			}
		}

		public void updateAllVertexFromCore(List<Vector3> pointList)
		{
			for (int i = 0; i < pointList.Count && i < maxSize; i++)
			{
				vertexArray[firstVertexPtr + i] = pointList[i];
			}
			displaySize = pointList.Count;
		}

		public override void updateVertex()
		{
			return;
		}

		public override void updateIndex()
		{
			return;
		}

		public override void updateColor()
		{
			return;
		}

		public override void updateAnimationAndModelMatrix(float time)
		{

			//Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationZ(rotation.Z) * Matrix4.CreateTranslation(position);

			//change in modelMatrix
			modelMatrix = Matrix4.CreateFromQuaternion(orientation) * Matrix4.CreateTranslation(position) * parentSpace.modelMatrixFromCameraSpace;
			return;
		}

		public override void draw(ref int currentIndex, ref int currentVertex)
		{
			if (visible && displaySize > 0)
			{
				GL.DrawArrays(PrimitiveType.Points, currentVertex, displaySize);
			}
			currentIndex += indexCount;
			currentVertex += vertexCount;
		}
	}

	public class PointCloud : Thing
	{
		//private Vector3[] allVertex, allColor;
		private System.Random r = new System.Random();

		float randomInRange(float a, float b)
		{
			return (float)(a + r.NextDouble() * (b - a));
		}

		public PointCloud()
		{
			vertexCount = 512 * 424;
			indexCount = 0;
			colorCount = vertexCount;

			//allVertex = new Vector3[vertexCount];
			//allColor = new Vector3[colorCount];
		}

		public override void initializeArray()
		{
			int i = 0;
			for (i = 0; i < vertexCount; i++)
			{
				vertexArray[firstVertexPtr + i] = new Vector3(randomInRange(-1f, 1f), randomInRange(0, 1), randomInRange(-1f, 1f));
			}


			for (i = 0; i < colorCount; i++)
			{
				colorArray[firstColorPtr + i] = new Vector3(vertexArray[firstVertexPtr + i].Y, 0f, 0f);
			}
		}

		public override void updateVertex()
		{
			for (int i = 0; i < vertexCount; i++)
			{
				vertexArray[firstVertexPtr + i] += new Vector3(randomInRange(-0.01f, 0.01f), randomInRange(-0.01f, 0.01f), randomInRange(-0.01f, 0.01f));
			}

		}

		public override void updateIndex()
		{
			return;
		}

		public override void updateColor()
		{
			return;
		}

		public override void updateAnimationAndModelMatrix(float time)
		{
			//no change in scale, rotation, position

			//no change in modelMatrix
			return;
		}

		public override void draw(ref int currentIndex, ref int currentVertex)
		{
			GL.DrawArrays(PrimitiveType.Points, currentVertex, vertexCount);

			currentIndex += indexCount;
			currentVertex += vertexCount;
		}
	}



	public class KinectPointCloud : Thing
	{
		public IntPtr cloudVertexPtr;
		Vector3 backgroundColor = new Vector3(0.25f,0.25f,0.25f);

		public KinectPointCloud(Space parentSpace)
		{
			this.parentSpace = parentSpace;

			vertexCount = 512 * 424;
			indexCount = 0;
			colorCount = vertexCount;

			//allVertex = new Vector3[vertexCount];
			//allColor = new Vector3[colorCount];
		}

		public unsafe override void initializeArray()
		{

			fixed (Vector3* tmp = this.parentSpace.vertexArray)
			{
				cloudVertexPtr = (IntPtr)tmp;
				//cloudVertexPtr = IntPtr.Add((IntPtr)tmp,firstVertexPtr*sizeof(Vector3));
			}

			int i = 0;
			for (i = 0; i < vertexCount; i++)
			{
				vertexArray[firstVertexPtr + i] = new Vector3(0f, 0f, 0f);
			}


			for (i = 0; i < colorCount; i++)
			{
				colorArray[firstColorPtr + i] = backgroundColor;
			}
		}

		public override void updateVertex()
		{
			/*
            for (int i = 0; i < vertexCount; i++)
            {
                vertexArray[firstVertexPtr + i] += new Vector3(randomInRange(-0.01f, 0.01f), randomInRange(-0.01f, 0.01f), randomInRange(-0.01f, 0.01f));
            }
             */
			return;
		}

		public override void updateIndex()
		{
			return;
		}

		public void resetColor()
		{
			for (int i = 0; i < colorCount; i++)
			{
				colorArray[firstColorPtr + i] = backgroundColor;
			}
		}

		static Vector3[] bodyIndexColor = new Vector3[] {
			new Vector3(1.0f, 0.8f, 0.4f),	//pale orange
			new Vector3(1.0f, 0.8f, 0.4f),	//pale orange
			new Vector3(1.0f, 0.8f, 0.4f),	//pale orange
			new Vector3(1.0f, 0.8f, 0.4f),	//pale orange
			new Vector3(1.0f, 0.8f, 0.4f),	//pale orange
			new Vector3(1.0f, 0.8f, 0.4f),	//pale orange

			//new Vector3(0.6f, 1.0f, 0.2f),	//leaf green
			//new Vector3(0.4f, 1.0f, 1.0f),	//sky blue
			//new Vector3(1.0f, 0.8f, 0.1f),	//bright pink
			//new Vector3(0.1f, 0.9f, 0.9f),
			//new Vector3(0.9f, 0.1f, 0.9f),
		};

		public void changeColorByBodyIndex(byte[] bodyIndexData)
		{
			for (int i = 0; i < colorCount; i++)
			{
				if(bodyIndexData[i]<6)
					colorArray[firstColorPtr + i] = bodyIndexColor[bodyIndexData[i]];
				else
					colorArray[firstColorPtr + i] = backgroundColor;
			}
		}

		public void changeBodyColor(bool[] isBody, Vector3 color)
		{
			for (int i = 0; i < colorCount; i++)
			{
				if(isBody[i])
					colorArray[firstColorPtr + i] = color;
				else
					colorArray[firstColorPtr + i] = backgroundColor;
			}
		}

		public override void updateColor()
		{

			//display
			/*
            for (int i = 0; i < colorCount; i++)
            {
                if (parentSpace.colorBoard[i]>0)
                    colorArray[firstColorPtr + i] = new Vector3(1f, 0f, 0f);
                else if (parentSpace.colorBoard[i]==-1000)
                    colorArray[firstColorPtr + i] = new Vector3(0f,1f, 0f);
                else if (parentSpace.colorBoard[i] == -2000)
                    colorArray[firstColorPtr + i] = new Vector3(0f, 0f, 1f);
                else
                    colorArray[firstColorPtr + i] = new Vector3(0.75f, 0.75f, 0.75f);
            }
            */
			/*
            for (int i = 0; i < colorCount; i++)
            {
                if (parentSpace.bodyIndex[i] < 6)
                    colorArray[firstColorPtr + i] = new Vector3(1f, 0f, 0f);
                else
                    colorArray[firstColorPtr + i] = new Vector3(0.75f, 0.75f, 0.75f);
            }
            */
		}

		public override void updateAnimationAndModelMatrix(float time)
		{
			//change in rotation
			//Vector3 targetDirection = new Vector3(0,1f,0);
			//float diffAngle = Vector3.CalculateAngle(targetDirection, parentSpace.deviceUpwardDirection);
			//if (parentSpace.deviceUpwardDirection.Z > 0)
			//    diffAngle = -diffAngle;
			//rotation = new Vector3(0, diffAngle, 0);

			//no change in scale, position

			//change in modelMatrix
			modelMatrix = parentSpace.modelMatrixFromCameraSpace;
			//modelMatrix = Matrix4.CreateRotationX(parentSpace.rotationX);
		}

		public override void draw(ref int currentIndex, ref int currentVertex)
		{
			GL.DrawArrays(PrimitiveType.Points, currentVertex, vertexCount);

			currentIndex += indexCount;
			currentVertex += vertexCount;
		}

		
	}

	// obsolete
	public class CalibrationRod : Thing
	{

		public CalibrationRod(Space parentSpace)
		{
			this.parentSpace = parentSpace;

			vertexCount = 4;
			indexCount = 0;
			colorCount = 4;
		}

		public override void initializeArray()
		{
			vertexArray[firstVertexPtr + 0] = new Vector3(0);
			vertexArray[firstVertexPtr + 1] = new Vector3(0);

			vertexArray[firstVertexPtr + 2] = new Vector3(0);
			vertexArray[firstVertexPtr + 3] = new Vector3(0);

			colorArray[firstColorPtr + 0] = new Vector3(0, 1f, 0);
			colorArray[firstColorPtr + 1] = new Vector3(0, 0, 1f);

			colorArray[firstColorPtr + 2] = new Vector3(1, 1, 1);
			colorArray[firstColorPtr + 3] = new Vector3(0, 0, 0);


		}

		public void upateFromCore(Vector3[] finalEndPointVector)
		{
			if (finalEndPointVector != null)
			{
				vertexArray[firstVertexPtr + 0] = finalEndPointVector[0];
				vertexArray[firstVertexPtr + 1] = finalEndPointVector[1];
				vertexArray[firstVertexPtr + 2] = finalEndPointVector[2];
				vertexArray[firstVertexPtr + 3] = finalEndPointVector[3];
			}
			else
			{
				vertexArray[firstVertexPtr + 0] = new Vector3(0);
				vertexArray[firstVertexPtr + 1] = new Vector3(0);
				vertexArray[firstVertexPtr + 2] = new Vector3(0);
				vertexArray[firstVertexPtr + 3] = new Vector3(0);
			}
		}

		public override void updateVertex()
		{
			/*
            if (parentSpace.calibrationRodIsDetected && parentSpace.pcaIsWorking)
            {
                    
                    vertexArray[firstVertexPtr + 0] = parentSpace.finalEndPointVector[0];
                    vertexArray[firstVertexPtr + 1] = parentSpace.finalEndPointVector[1];

                    vertexArray[firstVertexPtr + 2] = parentSpace.finalEndPointVector[2];
                    
                   
                    vertexArray[firstVertexPtr + 3] = parentSpace.finalEndPointVector[3];

            }
            else
            {
                vertexArray[firstVertexPtr + 0] = new Vector3(0);
                vertexArray[firstVertexPtr + 1] = new Vector3(0);
                vertexArray[firstVertexPtr + 2] = new Vector3(0);
                vertexArray[firstVertexPtr + 3] = new Vector3(0);
            }
            */
		}

		public override void updateIndex()
		{
			return;
		}

		public override void updateColor()
		{
			return;
		}

		public override void updateAnimationAndModelMatrix(float time)
		{
			//no change in scale, rotation, position

			// change in modelMatrix
			modelMatrix = parentSpace.modelMatrixFromCameraSpace;
			//modelMatrix = Matrix4.CreateRotationX(parentSpace.rotationX);
		}

		public override void draw(ref int currentIndex, ref int currentVertex)
		{
			GL.LineWidth(5f);
			GL.DrawArrays(PrimitiveType.Lines, currentVertex, vertexCount);
			GL.LineWidth(1f);

			currentIndex += indexCount;
			currentVertex += vertexCount;
		}
	}

	public class LinePath : Thing
	{
		public Vector3 color;
		public float lineWidth;

		public LinePath(Space parentSpace, Vector3 lineColor, int pointCount, float lineWidth=1f)
		{
			this.parentSpace = parentSpace;

			vertexCount = pointCount;
			indexCount = (pointCount-1)*2;
			colorCount = pointCount;

			color = lineColor;
			this.lineWidth = lineWidth;
		}

		public override void initializeArray()
		{
			for (int i = 0; i < vertexCount; i++)
			{
				vertexArray[firstVertexPtr + i] = new Vector3(0, 0, 0);
				colorArray[firstColorPtr + i] = color;
			}

			for (int i=0;i<vertexCount-1;i++)
			{
				indexArray[firstIndexPtr + 2 * i] = firstVertexPtr + i;
				indexArray[firstIndexPtr + 2 * i + 1] = firstVertexPtr + i + 1;
			}
		}

		public override void updateVertex()
		{
		}

		public void updateFromCore(Vector3[] trajectory)
		{
			for (int i = 0; i < vertexCount; i++)
			{
				vertexArray[firstVertexPtr + i] = trajectory[i];
			}
		}
		public override void updateIndex()
		{
			return;
		}

		public override void updateColor()
		{
			return;
		}

		public override void updateAnimationAndModelMatrix(float time)
		{
			//no change in scale, rotation, position

			// change in modelMatrix
			modelMatrix = parentSpace.modelMatrixFromCameraSpace;
			//modelMatrix = Matrix4.CreateRotationX(parentSpace.rotationX);
		}

		public override void draw(ref int currentIndex, ref int currentVertex)
		{
			if (visible)
			{
				GL.LineWidth(lineWidth);
				//GL.DrawArrays(PrimitiveType.Lines, currentVertex, vertexCount);
				GL.DrawElements(BeginMode.Lines, indexCount, DrawElementsType.UnsignedInt, currentIndex * sizeof(uint));
				GL.LineWidth(1f);

				
			}

			currentIndex += indexCount;
			currentVertex += vertexCount;
		}
	}

    public class Bone : Thing
    {
		public const int ARM_LENGTH_WINDOW = 5 * 30;    //5 seconds

		public Vector3 color;

        JointType startJoint;
        JointType endJoint;

        public Bone(Space parentSpace, Vector3 lineColor, JointType startJoint, JointType endJoint)
        {
            this.parentSpace = parentSpace;

            this.startJoint = startJoint;
            this.endJoint = endJoint;

            vertexCount = 2+6;
            indexCount = 0;
            colorCount = 2+6;

            color = lineColor;
        }

        public override void initializeArray()
        {
            //Debug.WriteLine("init forearm");
            vertexArray[firstVertexPtr + 0] = new Vector3(0);
            vertexArray[firstVertexPtr + 1] = new Vector3(0);

            colorArray[firstColorPtr + 0] = color;
            colorArray[firstColorPtr + 1] = color;

            ///////// joint orientation (at the end)
            vertexArray[firstVertexPtr + 2] = new Vector3(0, 0, 0);
            vertexArray[firstVertexPtr + 3] = new Vector3(0.1f, 0, 0);
            vertexArray[firstVertexPtr + 4] = new Vector3(0, 0, 0);
            vertexArray[firstVertexPtr + 5] = new Vector3(0, 0.1f, 0);
            vertexArray[firstVertexPtr + 6] = new Vector3(0, 0, 0);
            vertexArray[firstVertexPtr + 7] = new Vector3(0, 0, 0.1f);

            colorArray[firstColorPtr + 2] = new Vector3(1f, 0, 0);
            colorArray[firstColorPtr + 3] = new Vector3(1f, 0, 0);
            colorArray[firstColorPtr + 4] = new Vector3( 0,1f, 0);
            colorArray[firstColorPtr + 5] = new Vector3( 0,1f, 0);
            colorArray[firstColorPtr + 6] = new Vector3( 0, 0,1f);
            colorArray[firstColorPtr + 7] = new Vector3( 0, 0,1f);
        }
        public override void updateVertex()
        {
        }

        float avgLength = 0.3f;
        int avgCounter = 1;

        //update with imuOrientation
        public void updateFromCore(MyBody body,Quaternion kinect_segment_q)
        {
            /*
            MyBody body = null;
            for(int i=0;i<6;i++)
            {
                if (bodies[i] != null && bodies[i].IsTracked)
                {
                    body = bodies[i];
                    break;
                }
            }
			*/
            if (body != null)
            {
                //Debug.WriteLine("e:"+body.Joints[JointType.ElbowRight].TrackingState);
                //Debug.WriteLine("w:" + body.Joints[JointType.WristRight].TrackingState);

                if (body.Joints[startJoint].TrackingState == TrackingState.Tracked
                    && body.Joints[endJoint].TrackingState == TrackingState.Tracked
                    )
                {
                    
                    var elbow=body.Joints[startJoint].Position;
                    var wrist=body.Joints[endJoint].Position;

                    Vector3 kinectElbow = new Vector3(elbow.X,elbow.Y,elbow.Z);
                    Vector3 kinectWrist = new Vector3(wrist.X,wrist.Y,wrist.Z);

                    //calculate new armlength
                    avgLength = (avgLength * avgCounter + (kinectWrist - kinectElbow).Length) / (avgCounter + 1);
                    avgCounter = Math.Min(avgCounter + 1, ARM_LENGTH_WINDOW);

                    vertexArray[firstVertexPtr + 0] = kinectElbow;
                    //vertexArray[firstVertexPtr + 1] = kinectWrist;
                    vertexArray[firstVertexPtr + 1] = kinectElbow + MyMath.rotateVector(kinect_segment_q,new Vector3(0,avgLength,0));

                    //Debug.WriteLine("wrist:"+vertexArray[firstVertexPtr + 1]);

                    //rotate orientation using quaternion
                    //Microsoft.Kinect.Vector4 wristOrientation = body.JointOrientations[JointType.WristRight].Orientation;
                    //Quaternion q = new Quaternion(wristOrientation.X, wristOrientation.Y, wristOrientation.Z, wristOrientation.W);

                    Quaternion q = kinect_segment_q;

                    //move to the wrist
                    
                    vertexArray[firstVertexPtr + 2] = vertexArray[firstVertexPtr + 1];
                    vertexArray[firstVertexPtr + 3] = MyMath.rotateVector(q, new Vector3(0.1f, 0, 0)) + vertexArray[firstVertexPtr + 1];
                    //vertexArray[firstVertexPtr + 3] = Quaternion.Multiply( Quaternion.Multiply(q,new Quaternion(0.1f,0,0,0)) , qc).Xyz + vertexArray[firstVertexPtr + 1];
                    
                    vertexArray[firstVertexPtr + 4] = vertexArray[firstVertexPtr + 1];
                    vertexArray[firstVertexPtr + 5] = MyMath.rotateVector(q, new Vector3(0,0.1f,  0)) + vertexArray[firstVertexPtr + 1];
                    //vertexArray[firstVertexPtr + 5] = Quaternion.Multiply(Quaternion.Multiply(q, new Quaternion(0, 0.1f, 0, 0)), qc).Xyz + vertexArray[firstVertexPtr + 1];
                    
                    vertexArray[firstVertexPtr + 6] = vertexArray[firstVertexPtr + 1];
                    vertexArray[firstVertexPtr + 7] = MyMath.rotateVector(q, new Vector3(0,0,0.1f)) + vertexArray[firstVertexPtr + 1];
                    //vertexArray[firstVertexPtr + 7] = Quaternion.Multiply(Quaternion.Multiply(q, new Quaternion(0, 0, 0.1f, 0)), qc).Xyz + vertexArray[firstVertexPtr + 1];
                    
                    
                    //body.JointOrientations[JointType.ElbowRight].Orientation
                    //Microsoft.Kinect.Vector4 zeroOrientation = body.JointOrientations[JointType.SpineBase].Orientation;
                    //this.parentSpace.parentGraphicWindow.mainWindow.textA.Content = zeroOrientation.X.ToString() + " , " + zeroOrientation.Y.ToString() + " , " + zeroOrientation.Z.ToString() +" , "+ zeroOrientation.W.ToString();

                    //zeroOrientation = body.JointOrientations[JointType.WristRight].Orientation;
                    //this.parentSpace.parentGraphicWindow.mainWindow.textB.Content = zeroOrientation.X.ToString() + " , " + zeroOrientation.Y.ToString() + " , " + zeroOrientation.Z.ToString() + " , " + zeroOrientation.W.ToString();

                }
                else
                {
                    vertexArray[firstVertexPtr + 0] = new Vector3(0);

                    vertexArray[firstVertexPtr + 1] = new Vector3(0);
                }
            }
        }

        //update with kinect orientation
        public void updateFromCore(Body body)
        {
            if (body != null)
            {
                if (body.Joints[startJoint].TrackingState == TrackingState.Tracked
                    && body.Joints[endJoint].TrackingState == TrackingState.Tracked
                    )
                {

                    CameraSpacePoint elbow = body.Joints[startJoint].Position;
                    CameraSpacePoint wrist = body.Joints[endJoint].Position;

                    Vector3 kinectElbow = new Vector3(elbow.X, elbow.Y, elbow.Z);
                    Vector3 kinectWrist = new Vector3(wrist.X, wrist.Y, wrist.Z);



                    //calculate new armlength
                    avgLength = (avgLength * avgCounter + (kinectWrist - kinectElbow).Length) / (avgCounter + 1);
                    avgCounter = Math.Min(avgCounter + 1, ARM_LENGTH_WINDOW);

                    vertexArray[firstVertexPtr + 0] = kinectElbow;
                    vertexArray[firstVertexPtr + 1] = kinectWrist;
                    //vertexArray[firstVertexPtr + 1] = kinectElbow + MyMath.rotateVector(imuOrientation, new Vector3(0, avgLength, 0));

                    //Debug.WriteLine("wrist:"+vertexArray[firstVertexPtr + 1]);

                    //rotate orientation using quaternion
                    Microsoft.Kinect.Vector4 wristOrientation = body.JointOrientations[JointType.WristRight].Orientation;
                    Quaternion q = new Quaternion(wristOrientation.X, wristOrientation.Y, wristOrientation.Z, wristOrientation.W);

                    //Quaternion q = imuOrientation;

                    //move to the wrist

                    vertexArray[firstVertexPtr + 2] = vertexArray[firstVertexPtr + 1];
                    vertexArray[firstVertexPtr + 3] = MyMath.rotateVector(q, new Vector3(0.1f, 0, 0)) + vertexArray[firstVertexPtr + 1];
                    //vertexArray[firstVertexPtr + 3] = Quaternion.Multiply( Quaternion.Multiply(q,new Quaternion(0.1f,0,0,0)) , qc).Xyz + vertexArray[firstVertexPtr + 1];

                    vertexArray[firstVertexPtr + 4] = vertexArray[firstVertexPtr + 1];
                    vertexArray[firstVertexPtr + 5] = MyMath.rotateVector(q, new Vector3(0, 0.1f, 0)) + vertexArray[firstVertexPtr + 1];
                    //vertexArray[firstVertexPtr + 5] = Quaternion.Multiply(Quaternion.Multiply(q, new Quaternion(0, 0.1f, 0, 0)), qc).Xyz + vertexArray[firstVertexPtr + 1];

                    vertexArray[firstVertexPtr + 6] = vertexArray[firstVertexPtr + 1];
                    vertexArray[firstVertexPtr + 7] = MyMath.rotateVector(q, new Vector3(0, 0, 0.1f)) + vertexArray[firstVertexPtr + 1];
                    //vertexArray[firstVertexPtr + 7] = Quaternion.Multiply(Quaternion.Multiply(q, new Quaternion(0, 0, 0.1f, 0)), qc).Xyz + vertexArray[firstVertexPtr + 1];


                    //body.JointOrientations[JointType.ElbowRight].Orientation
                    //Microsoft.Kinect.Vector4 zeroOrientation = body.JointOrientations[JointType.SpineBase].Orientation;
                    //this.parentSpace.parentGraphicWindow.mainWindow.textA.Content = zeroOrientation.X.ToString() + " , " + zeroOrientation.Y.ToString() + " , " + zeroOrientation.Z.ToString() +" , "+ zeroOrientation.W.ToString();

                    //zeroOrientation = body.JointOrientations[JointType.WristRight].Orientation;
                    //this.parentSpace.parentGraphicWindow.mainWindow.textB.Content = zeroOrientation.X.ToString() + " , " + zeroOrientation.Y.ToString() + " , " + zeroOrientation.Z.ToString() + " , " + zeroOrientation.W.ToString();

                }
                else
                {
                    vertexArray[firstVertexPtr + 0] = new Vector3(0);

                    vertexArray[firstVertexPtr + 1] = new Vector3(0);
                }
            }
        }


		public void updateFromCore(Vector3 proximalJointPosition, Vector3 distalJointPosition, Quaternion kinect_segment_q)
		{
			vertexArray[firstVertexPtr + 0] = proximalJointPosition;
			vertexArray[firstVertexPtr + 1] = distalJointPosition;

			//move to the wrist

			vertexArray[firstVertexPtr + 2] = vertexArray[firstVertexPtr + 1];
			vertexArray[firstVertexPtr + 3] = MyMath.rotateVector(kinect_segment_q, new Vector3(0.1f, 0, 0)) + vertexArray[firstVertexPtr + 1];

			vertexArray[firstVertexPtr + 4] = vertexArray[firstVertexPtr + 1];
			vertexArray[firstVertexPtr + 5] = MyMath.rotateVector(kinect_segment_q, new Vector3(0, 0.1f, 0)) + vertexArray[firstVertexPtr + 1];

			vertexArray[firstVertexPtr + 6] = vertexArray[firstVertexPtr + 1];
			vertexArray[firstVertexPtr + 7] = MyMath.rotateVector(kinect_segment_q, new Vector3(0, 0, 0.1f)) + vertexArray[firstVertexPtr + 1];
		}

		public void updateFromCore(Vector3 proximalJointPosition, Quaternion kinect_segment_q, Vector3 segment_extendUnitDirection, float segmentLength)
		{
			Vector3 distalJointPosition = proximalJointPosition + segmentLength*MyMath.rotateVector(kinect_segment_q,segment_extendUnitDirection);
			updateFromCore(proximalJointPosition,distalJointPosition,kinect_segment_q);
		}

		public override void updateIndex()
        {
            return;
        }

        public override void updateColor()
        {
            return;
        }

        public override void updateAnimationAndModelMatrix(float time)
        {
            //no change in scale, rotation, position

            // change in modelMatrix
            modelMatrix = parentSpace.modelMatrixFromCameraSpace;
            //modelMatrix = Matrix4.CreateRotationX(parentSpace.rotationX);
        }

        public override void draw(ref int currentIndex, ref int currentVertex)
        {
			if (visible)
			{
				GL.LineWidth(5f);
				GL.DrawArrays(PrimitiveType.Lines, currentVertex, vertexCount);
				GL.LineWidth(1f);
			}

            currentIndex += indexCount;
            currentVertex += vertexCount;
        }
    }
    

   

    public class ThreeAxis : Thing
    {

        private float length = 0.15f;

        //public bool isVisible = true;
        public Quaternion orientation = new Quaternion();		// KinectFrame_thisFrame_Quaternion
        public Vector3 coordinatePosition = new Vector3(0);

        public Vector3 coreColor;

        public ThreeAxis(Space parentSpace, Vector3 coreColor, float length = 0.15f)
        {
            this.parentSpace = parentSpace;

            this.coreColor = coreColor;
			this.length = length;

            vertexCount = 6;
            indexCount = 0;
            colorCount = 6;

        }

        public override void initializeArray()
        {

            vertexArray[firstVertexPtr + 0] = new Vector3(0);
            vertexArray[firstVertexPtr + 1] = new Vector3(0);
            vertexArray[firstVertexPtr + 2] = new Vector3(0);
            vertexArray[firstVertexPtr + 3] = new Vector3(0);
            vertexArray[firstVertexPtr + 4] = new Vector3(0);
            vertexArray[firstVertexPtr + 5] = new Vector3(0);

            colorArray[firstColorPtr + 0] = coreColor; // new Vector3(1, 0.5f, 0.5f);
            colorArray[firstColorPtr + 1] = new Vector3(1, 0, 0);
            colorArray[firstColorPtr + 2] = coreColor; //new Vector3(0.5f, 1, 0.5f);
            colorArray[firstColorPtr + 3] = new Vector3(0, 1, 0);
            colorArray[firstColorPtr + 4] = coreColor; //new Vector3(0.5f, 0.5f, 1);
            colorArray[firstColorPtr + 5] = new Vector3(0, 0, 1);
        }

        public override void updateVertex()
        {
            //rotate orientation using quaternion
            Quaternion q = orientation;
            Quaternion qc = Quaternion.Conjugate(q);

            //move to the wrist

            vertexArray[firstVertexPtr + 0] = coordinatePosition;
            vertexArray[firstVertexPtr + 1] = Quaternion.Multiply(Quaternion.Multiply(q, new Quaternion(length, 0, 0, 0)), qc).Xyz + coordinatePosition;

            vertexArray[firstVertexPtr + 2] = coordinatePosition;
            vertexArray[firstVertexPtr + 3] = Quaternion.Multiply(Quaternion.Multiply(q, new Quaternion(0, length, 0, 0)), qc).Xyz + coordinatePosition;

            vertexArray[firstVertexPtr + 4] = coordinatePosition;
            vertexArray[firstVertexPtr + 5] = Quaternion.Multiply(Quaternion.Multiply(q, new Quaternion(0, 0, length, 0)), qc).Xyz + coordinatePosition;

        }

        public override void updateIndex()
        {
            return;
        }

        public override void updateColor()
        {
            return;
        }

        public override void updateAnimationAndModelMatrix(float time)
        {
            //no change in scale, rotation, position

            // change in modelMatrix
            modelMatrix = parentSpace.modelMatrixFromCameraSpace;
            //modelMatrix = Matrix4.CreateRotationX(parentSpace.rotationX);
        }

        public override void draw(ref int currentIndex, ref int currentVertex)
        {
			if (visible)
			{
				GL.LineWidth(5f);
				GL.DrawArrays(PrimitiveType.Lines, currentVertex, vertexCount);
				GL.LineWidth(1f);
			}
            currentIndex += indexCount;
            currentVertex += vertexCount;
        }
    }
    /*
    public class FocusedPoint : Thing
    {
        Vector3 color;

        int selectedX=212, selectedY=256;

        public FocusedPoint(Space parentSpace, KinectPointCloud kpc,Vector3 color)
        {
            this.parentSpace = parentSpace;

            vertexCount = 1;
            indexCount = 0;
            colorCount = 1;

            this.color = color;
           
        }

        public override void initializeArray()
        {
            vertexArray[firstVertexPtr] = new Vector3(0,0,0);
            colorArray[firstColorPtr] = this.color;
        }

        public void selectPoint(int i,int j)
        {
            selectedX = i;
            selectedY = j;
        }

        int targetFrameCount;
        Vector3[] sampling;
        int currentSampling;
        public bool isRecording = false;
        public bool isDone = false;
        public void initializeRecording(int targetFrameCount)
        {
            isDone = false;
            this.targetFrameCount = targetFrameCount;
            sampling = new Vector3[targetFrameCount];
            currentSampling = 0;
            isRecording = true;
        }

        

        public override void updateVertex()
        {
            vertexArray[firstVertexPtr] = vertexArray[selectedX * 512 + selectedY];
            parentSpace.gwin.mainWindow.depthLabel.Text = vertexArray[firstVertexPtr].Z.ToString();
            if (isRecording)
            {
                sampling[currentSampling] = vertexArray[firstVertexPtr];
                currentSampling++;
                if (currentSampling>=targetFrameCount)
                {
                    finalizeRecording();
                }
            }
        }
        
        private void finalizeRecording()
        {
            isRecording = false;
            isDone = true;
            parentSpace.gwin.mainWindow.textB.Text = "done";
        }
        double[] result;

        public void concludeResult()
        {
            if(isDone)
            {
                float sumX=0;
                float sumY=0;
                float sumZ=0;
                int i;
                for(i=0;i<targetFrameCount;i++)
                {
                    sumX += sampling[i].X;
                    sumY += sampling[i].Y;
                    sumZ += sampling[i].Z;
                }
                Vector3 avg = new Vector3(sumX / targetFrameCount, sumY / targetFrameCount, sumZ / targetFrameCount);

                result =new double[targetFrameCount];
                Vector3 error;
                double sumSquareError=0;
                double squareError;
                for (i = 0; i < targetFrameCount; i++)
                {
                    error = sampling[i] - avg;
                    squareError = error.X * error.X + error.Y * error.Y + error.Z * error.Z;
                    sumSquareError += squareError;
                    result[i] = Math.Sqrt(squareError);
                    if (error.Z<0)
                    {
                        result[i] *= -1;
                    }
                }
                Array.Sort(result);
                parentSpace.gwin.mainWindow.textA.Text = result[0].ToString() + " , " + result[targetFrameCount - 1].ToString();
                parentSpace.gwin.mainWindow.textB.Text = "S.D. = " + Math.Sqrt(sumSquareError / (targetFrameCount - 1));

                //distinct count
                Dictionary<double,int> hist=new Dictionary<double,int>();
                double currentValue = result[0];
                int currentCount = 0;
                for(i=0;i<targetFrameCount;i++)
                {
                    if(result[i]==currentValue)
                    {
                        currentCount++;
                    }
                    else
                    {
                        hist.Add(currentValue, currentCount);
                        currentValue = result[i];
                        currentCount = 1;
                    }
                }
                hist.Add(currentValue, currentCount);

                //write to csv
                
                var csv = new StringBuilder();

                foreach(double aKey in hist.Keys)
                {
                    var newLine = string.Format("{0},{1}{2}", aKey,hist[aKey], Environment.NewLine);
                    csv.Append(newLine);
                }
                File.WriteAllText("histogram.csv",csv.ToString());
                
            }
            
        }

        public override void updateIndex()
        {
            return;
        }

        public override void updateColor()
        {
            return;
        }

        public override void updateAnimationAndModelMatrix(float time)
        {
            //no change in scale, rotation, position

            // change in modelMatrix
            modelMatrix = parentSpace.modelMatrixFromCameraSpace;
            //modelMatrix = Matrix4.CreateRotationX(parentSpace.rotationX);
        }

        public override void draw(ref int currentIndex, ref int currentVertex)
        {
            GL.PointSize(12f);
            GL.DrawArrays(PrimitiveType.Points, currentVertex, vertexCount);
            GL.PointSize(1f);

            currentIndex += indexCount;
            currentVertex += vertexCount;
        }
    }
    */
    public class KinectOpticalAxis : Thing
    {
        public Vector3 color;


        public KinectOpticalAxis(Space parentSpace, Vector3 lineColor)
        {
            this.parentSpace = parentSpace;

            vertexCount = 2;
            indexCount = 0;
            colorCount = 2;

            color = lineColor;
        }

        public override void initializeArray()
        {
            vertexArray[firstVertexPtr + 0] = new Vector3(0);
            vertexArray[firstVertexPtr + 1] = new Vector3(0,0,8);

            colorArray[firstColorPtr + 0] = color;
            colorArray[firstColorPtr + 1] = color;

        }

        public override void updateVertex()
        {
            return;
        }

        public override void updateIndex()
        {
            return;
        }

        public override void updateColor()
        {
            return;
        }

        public override void updateAnimationAndModelMatrix(float time)
        {
            //no change in scale, rotation, position

            // change in modelMatrix
            modelMatrix = parentSpace.modelMatrixFromCameraSpace;
            //modelMatrix = Matrix4.CreateRotationX(parentSpace.rotationX);
        }

        public override void draw(ref int currentIndex, ref int currentVertex)
        {
            GL.LineWidth(3f);
            GL.DrawArrays(PrimitiveType.Lines, currentVertex, vertexCount);
            GL.LineWidth(1f);

            currentIndex += indexCount;
            currentVertex += vertexCount;
        }
    }
}
