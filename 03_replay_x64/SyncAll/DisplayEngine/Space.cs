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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SyncAll
{
    /// <summary>
    /// This class contain list of objects
    /// and bind them to buffer
    /// </summary>
    public class Space
    {
        public GraphicWindow gwin;
        public List<Thing> things = new List<Thing>();

		//control
		public bool isGoingToTakeScreenShot=false;

        //central parameter
        private Vector3 DeviceUpwardDirection = new Vector3(0,1f,0);
		public Matrix4 modelMatrixFromCameraSpace = Matrix4.Identity; //preset to make it displayable without kinect 

		Dictionary<string, Thing> o = new Dictionary<string, Thing>();	//o = object, this will be called a lot
		private void installThings(List<Tuple<Thing,string>> allThings )	//(Thing, name)
		{
			foreach(Tuple<Thing,string> e in allThings)
			{
				o[e.Item2] = e.Item1;
				things.Add(e.Item1);
			}
			initializeDataArray();
		}

		//public float rotationX=0f;  //for anything that are from cameraSpace 
		public Vector3 deviceUpwardDirection
        {
            get
            {
                return this.DeviceUpwardDirection;
            }
            set
            {
                Vector3 targetDirection = new Vector3(0,1f,0);
                
                float diffAngle = Vector3.CalculateAngle(value,targetDirection);
                Vector3 rotateAxis = Vector3.Cross(value,targetDirection).Normalized();
                modelMatrixFromCameraSpace = Matrix4.CreateFromAxisAngle(rotateAxis, diffAngle);
                /*
                if (value.Z > 0)
                    rotationX = -diffAngle;
                else
                    rotationX = diffAngle;
                */
                this.DeviceUpwardDirection = value;
            }
        }

        public Vector3[] vertexArray;
        public Vector3[] colorArray;
        public int[] indexArray;

        int pgmID;
        int vsID;
        int fsID;

        int attribute_vcol;
        int attribute_vpos;
        int uniform_mview;

        int ibo_elements;
        int vbo_position;
        int vbo_color;
        int vbo_mview;

        private string glslVersion = "330"; //"120" or "330"

		public Space(GraphicWindow parentGraphicWindow)      //for OnLoad
		{
			this.gwin = parentGraphicWindow;
		}

		//must be called as soon as possible
		public void init(List<Tuple<Thing, string>> allThings)
		{
			//this.prepareThings();   //initialize all the object
			this.installThings(allThings);

            pgmID = GL.CreateProgram();
            loadShader("shader/vs"+glslVersion+".glsl", ShaderType.VertexShader, pgmID, out vsID);
            loadShader("shader/fs"+glslVersion+".glsl", ShaderType.FragmentShader, pgmID, out fsID);

            GL.LinkProgram(pgmID);
            Console.WriteLine(GL.GetProgramInfoLog(pgmID));

            attribute_vpos = GL.GetAttribLocation(pgmID, "vPosition");
            attribute_vcol = GL.GetAttribLocation(pgmID, "vColor");
            uniform_mview = GL.GetUniformLocation(pgmID, "modelview");

            if (attribute_vpos == -1 || attribute_vcol == -1 || uniform_mview == -1)
            {
                Console.WriteLine("Error binding attributes");
            }

            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out vbo_color);
            GL.GenBuffers(1, out vbo_mview);    //it's okay not to have this line??
            GL.GenBuffers(1, out ibo_elements);
        }

        private void loadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }
		
        public void initializeDataArray()
        {
            //use this after the number of thing has changed
            int totalVertex=0;
            int totalColor=0;
            int totalIndex=0;
            foreach (Thing v in things)
            {
                totalVertex += v.vertexCount;
                totalColor += v.colorCount;
                totalIndex += v.indexCount;
            }

            vertexArray = new Vector3[totalVertex];
            colorArray = new Vector3[totalColor];
            indexArray = new int[totalIndex];

            //assign starting pointer to each thing
            int currentVertexPtr = 0;
            int currentColorPtr = 0;
            int currentIndexPtr = 0;
            foreach (Thing v in things)
            {
                v.vertexArray = vertexArray;
                v.colorArray = colorArray;
                v.indexArray = indexArray;
                
                v.firstVertexPtr = currentVertexPtr;
                v.firstColorPtr = currentColorPtr;
                v.firstIndexPtr = currentIndexPtr;

                currentVertexPtr += v.vertexCount;
                currentColorPtr += v.colorCount;
                currentIndexPtr += v.indexCount;

                v.initializeArray();
            }
        }

        //public int[] colorBoard = new int[512 * 424];
        
        public Vector3[] finalEndPointVector = new Vector3[4];
        public bool calibrationRodIsDetected = false;
        public bool pcaIsWorking = false;
        public Quaternion rodRotationQuaternion;
        public Vector3 rodPosition;
        
        //-------------------------

        public bool isGoingToSaveOffset = false;

        public List<TimeSpan> timestamp = new List<TimeSpan>();
        public List<Quaternion> quat = new List<Quaternion>();

        public void arrangeDataArray()     
        {
            foreach(Thing v in things)
            {
                v.updateVertex();
                v.updateColor();
                v.updateIndex();
            }
            //for OnUpdateFrame
            /*
            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();

            int vertcount = 0;

            foreach (Thing v in things)
            {
                verts.AddRange(v.getVertex().ToList());
                inds.AddRange(v.getIndex(vertcount).ToList());
                colors.AddRange(v.getColor().ToList());
                vertcount += v.vertexCount;
            }

            vertexArray = verts.ToArray();
            indexArray = inds.ToArray();
            colorArray = colors.ToArray();
            */
        }

        public void bindBuffer()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexArray.Length * sizeof(int)), indexArray, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertexArray.Length * Vector3.SizeInBytes), vertexArray, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(colorArray.Length * Vector3.SizeInBytes), colorArray, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, true, 0, 0);

            
            GL.UseProgram(pgmID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void updateAnimation(float time)
        {
            //for OnUpdateFrame
            //update everything depended on time (e.g. position, rotation, scale)
            foreach (Thing v in things)
            {
                v.updateAnimationAndModelMatrix(time);
            }

        }

        public void updateModelViewProjectionMatrix(Camera cam) //Matrix4 cameraViewMatrix,Size ClientSize)
        {
            //for OnUpdateFrame
            //update last matrix
            //Matrix4 viewProjectionMatrix = cameraViewMatrix * Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 0.01f, 40.0f);
			Matrix4 viewProjectionMatrix = cam.getViewProjectionMatrix();

			foreach (Thing v in things)
            {
                v.modelViewProjectionMatrix = v.modelMatrix * viewProjectionMatrix;
            }
        }

        public void render(GLControl game, int renderWidth, int renderHeight)
        {
			//if(game!=null)
			//	GL.Viewport(0, 0, game.Width, game.Height);
			//else
			//	GL.Viewport(0, 0, gwin.outputVideoSize.Width, gwin.outputVideoSize.Height);

			GL.Viewport(0, 0, renderWidth, renderHeight);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            GL.EnableVertexAttribArray(attribute_vpos);
            GL.EnableVertexAttribArray(attribute_vcol);

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            //GL.DrawElements(BeginMode.Triangles, indicedata.Length, DrawElementsType.UnsignedInt, 0);

            int currentIndex = 0;
            int currentVertex = 0;
            foreach (Thing v in things)
            {
				//if (v.visible)	//cannot skip it that way because currentIndex and currentVertex will not shift forward
				//{
					GL.UniformMatrix4(uniform_mview, false, ref v.modelViewProjectionMatrix);
					v.draw(ref currentIndex, ref currentVertex);
				//}
            }     

            GL.DisableVertexAttribArray(attribute_vpos);
            GL.DisableVertexAttribArray(attribute_vcol);

            GL.Flush();

			//I should try to get image here
			if(isGoingToTakeScreenShot)
			{ 
				isGoingToTakeScreenShot=false;
				IntPtr imagePtr = Marshal.AllocHGlobal(4*game.Width*game.Height);
				GL.ReadPixels(0,0,game.Width,game.Height,PixelFormat.Bgra,PixelType.UnsignedByte,imagePtr);	//use rgba to make stride easy to calculate stride
				Bitmap bitmap=new Bitmap(game.Width,game.Height,4*game.Width,System.Drawing.Imaging.PixelFormat.Format32bppRgb,imagePtr);
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);	//because pixel 0,0 is at the lower-left corner 
				bitmap.Save("screenShot.bmp");
				Marshal.FreeHGlobal(imagePtr);

				Debug.WriteLine("screen shot saved.");
			}

			//if(game!=null)
			if(gwin.renderMode==RenderMode.SCREEN)
				game.SwapBuffers();

        }
    }

}