using System;
using System.Collections.Generic;

using System.Windows.Forms;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms.Integration;

namespace SyncAll
{
	public enum RenderMode
	{
		SCREEN=0,
		VDO=1
	}

	public class GraphicWindow
    {
		bool AUTO_UPDATE_FRAME;
		public ObjOfBool hasNewUpdate = new ObjOfBool();

		public GLControl glControl1=null;
		//public GLControl glControlDummy=null;

		//public WindowsFormsHost wHostDummy=null;
        //public MainWindow mainWindow;

		public System.Windows.Controls.Image depthImage;
		public BitmapDisplay bitmapDisplay=null;
		public float[] depthTableToDisplay=null;

        public bool loaded = false;

        bool lockCursor = false;

        public Camera cam = new Camera();

        DateTimeOffset startTime;
        float time = 0.0f;
        public Space space;

        public DispatcherTimer dispatcherTimer;

        public int frameCounter = 0;

		public List<Action> actionList = new List<Action>();

		//noGUI rendering setting
		public Size outputVideoSize = new Size(1,1);

		public RenderMode renderMode;

		//AUTO_UPDATE_FRAME=keep rendering without update flag
        public GraphicWindow(RenderMode mode,WindowsFormsHost wHost, System.Windows.Controls.Image depthImage=null, bool AUTO_UPDATE_FRAME=false, int vdoW=1,int vdoH=1)
        {
			renderMode = mode;

			this.AUTO_UPDATE_FRAME = AUTO_UPDATE_FRAME;

            //this.mainWindow = mainWindow;
			this.depthImage = depthImage;
			if(depthImage!=null)
			{
				bitmapDisplay = new BitmapDisplay(depthImage);
			}

			glControl1 = new GLControl();
            glControl1.Load += glControl1_Load;
			wHost.Child = glControl1;

			if(mode==RenderMode.SCREEN)
			{ 
				glControl1.KeyPress += glControl1_KeyPress;
				glControl1.Click += glControl1_Click;
				glControl1.Resize += GlControl1_Resize;

				cam.aspectRatio = glControl1.AspectRatio;
			}
			else if(mode==RenderMode.VDO)
			{
				outputVideoSize = new Size(vdoW,vdoH);
				cam.updateAspectRatio(outputVideoSize.Width,outputVideoSize.Height);
			}

			/*
			else
			{
				//console rendering (no GUI, no WindowsFormsHost, no glControl)
				space = new Space(this);
				space.init(access.getAllThings(space));
				GL.ClearColor(Color.Black);
				GL.PointSize(1f);

				//set camera position
				cam.reset();
				cam.updateAspectRatio(outputVideoSize.Width,outputVideoSize.Height);
			}*/
           
        }
		/*
		public GraphicWindow(WindowsFormsHost wHost, int width, int height)
		{
			glControlDummy = new GLControl();
			glControlDummy.Load += GlControlDummy_Load;

			//wHostDummy = new WindowsFormsHost();
			wHost.Child=glControlDummy;

			while(!loaded) { }	//wait until glControlDummy is loaded

			space = new Space(this);
			space.init(access.getAllThings(space));
			GL.ClearColor(Color.Black);
			GL.PointSize(1f);

			//set camera position
			cam.reset();
			outputVideoSize = new Size(width,height);
			cam.updateAspectRatio(outputVideoSize.Width,outputVideoSize.Height);
		}

		private void GlControlDummy_Load(object sender, EventArgs e)
		{
			Console.WriteLine("GlControlDummy loaded");
			loaded=true;
		}
		*/
		private void GlControl1_Resize(object sender, EventArgs e)
		{
			cam.aspectRatio = glControl1.AspectRatio;
			hasNewUpdate.set();
		}

		private void glControl1_Load(object sender, EventArgs e)
        {
            //replace OnLoad

            space = new Space(this);
			space.init(access.getAllThings(space));

			GL.ClearColor(Color.Black);
            GL.PointSize(1f);

			cam.aspectRatio=glControl1.AspectRatio;

			//if (AUTO_UPDATE_FRAME)
			if (renderMode == RenderMode.SCREEN)
				DispatcherTimerSetup();

            //updateFrame();
            loaded = true;

        }

        public void DispatcherTimerSetup()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(25);
            startTime = DateTimeOffset.Now;

            //dispatcherTimer.Start();
        }

        public void startToEnableCameraControl()
        {
            dispatcherTimer.Start();    
        }

        void dispatcherTimer_Tick(object sender, object e)
        {
            DateTimeOffset currentTime = DateTimeOffset.Now;
            time = (float)((currentTime - startTime).TotalSeconds);

            if (lockCursor)
            {
                Vector2 delta = lastMousePos - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
				if (delta.X != 0 || delta.Y != 0)
				{
					cam.AddRotation(delta.X, delta.Y);
					resetCursor();
					hasNewUpdate.state = true;
				}
            }

            frameCounter++;

			//mainWindow.detailText.Text = mainWindow.textToDisplay;
			//mainWindow.counterLabel.Content = frameCounter.ToString();

			foreach(Action action in actionList)
			{
				action();
			}

			if (AUTO_UPDATE_FRAME && hasNewUpdate.state==true)
			{
				hasNewUpdate.state = false;
				updateFrame();    //if AUTO_UPDATE_FRAME==false, Core will invoke this function by itself
			}
        }

		public DateTime latestFrameUpdateTime = DateTime.Now;
		public bool hasPendingUpdateTask = false;

        public void updateFrame()
        {
            //space.centralCalculation();

            space.arrangeDataArray();
            space.bindBuffer();
            space.updateAnimation(time);    //now i don't care about time

			space.updateModelViewProjectionMatrix(cam);

			//if (glControl1!=null)
			//	space.updateModelViewProjectionMatrix(cam.GetViewMatrix(), glControl1.ClientSize);
			//else
			//	space.updateModelViewProjectionMatrix(cam.GetViewMatrix(), outputVideoSize);

			//render suddenly after update (not good)
			if(renderMode==RenderMode.SCREEN)
				space.render(glControl1,glControl1.Width,glControl1.Height);
			else if(renderMode==RenderMode.VDO)
				space.render(glControl1,outputVideoSize.Width,outputVideoSize.Height);

			if(bitmapDisplay!=null)
			{
				bitmapDisplay.updateDepthMap(depthTableToDisplay);
			}

			latestFrameUpdateTime = DateTime.Now;
			hasPendingUpdateTask = false;

            //paintCounter++;
            //mainWindow.textC.Content = paintCounter.ToString();
        }

		public Bitmap getRenderResult()	//for video only
		{
			IntPtr imagePtr = Marshal.AllocHGlobal(4*outputVideoSize.Width*outputVideoSize.Height);
			GL.ReadPixels(0,0,outputVideoSize.Width,outputVideoSize.Height,PixelFormat.Bgra,PixelType.UnsignedByte,imagePtr);	//use rgba to make stride easy to calculate stride
			Bitmap bitmap=new Bitmap(outputVideoSize.Width,outputVideoSize.Height,4*outputVideoSize.Width,System.Drawing.Imaging.PixelFormat.Format32bppRgb,imagePtr);
			bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);  //because pixel 0,0 is at the lower-left corner 
			Marshal.FreeHGlobal(imagePtr);
			return bitmap;
		}

		/*
        void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (lockCursor)
            {

                Vector2 delta = lastMousePos - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);

                cam.AddRotation(delta.X, delta.Y);
                resetCursor();
            }
        }
		*/
        void glControl1_Click(object sender, EventArgs e)
        {
            lockCursor = true;
            resetCursor();
        }

        Vector2 lastMousePos = new Vector2();
        void resetCursor()
        {
            Point screenPoint = glControl1.PointToScreen(new Point(0, 0));
            OpenTK.Input.Mouse.SetPosition(screenPoint.X + glControl1.Bounds.Left + glControl1.Bounds.Width / 2, screenPoint.Y + glControl1.Bounds.Top + glControl1.Bounds.Height / 2);
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }

        void glControl1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            //if (e.KeyChar == '1')
			if (e.KeyChar == (Char)Keys.Escape || e.KeyChar == '1')
            {
                lockCursor = false;
            }

            switch (e.KeyChar)
            {
                case 'w':
                    cam.Move(0f, 0.1f, 0f);
                    break;
                case 'a':
                    cam.Move(-0.1f, 0f, 0f);
                    break;
                case 's':
                    cam.Move(0f, -0.1f, 0f);
                    break;
                case 'd':
                    cam.Move(0.1f, 0f, 0f);
                    break;
                case 'q':
                    cam.Move(0f, 0f, 0.1f);
                    break;
                case 'e':
                    cam.Move(0f, 0f, -0.1f);
                    break;
				case '0':
					cam.reset();
					break;
            }
			hasNewUpdate.state = true;
			//updateFrame();
			//updateFrame();
			//space.render(glControl1);
		}

        
        /* this is a bug // do not use
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            //event for the first time only
            if (!loaded)
                return;

            space.render(glControl1);
        }
        */
    }
}
