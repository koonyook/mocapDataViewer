using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Kinect;
using OpenTK;

namespace SyncAll
{
    public class KinectRecord
    {
        //it is very important to dispose all the frame after use these frames
        public double timestamp;
        public byte[] depthBuffer = new byte[424 * 512 * 2]; //change to camera space later
        public byte[] bodyIndexBuffer = new byte[424 * 512 * 1];

		public byte[] colorBuffer;  // 1920×1080*2 (color generate around 125 MB per second) 
		public byte[] irBuffer;     // 424x512*2 (2bytes per pixel)

		//public Body[] bodies = new Body[6]{null,null,null,null,null,null};
		//public Microsoft.Kinect.Vector4 floorClipPlane;

		//public OfflineBodyFrame offlineBodyFrame;  //floorClipPlane is inside
		public MyBodyFrame myBodyFrame;

		public KinectRecord(BodyFrame bodyFrame,DepthFrame depthFrame,BodyIndexFrame bodyIndexFrame,ColorFrame colorFrame, InfraredFrame irFrame,double timestamp, bool USE_RGB, bool USE_IR)
        {
            //cannot keep Frame, choose only important component of it
            this.timestamp = timestamp; //ms since the beginning (since the Kinect got energy)
			//Debug.WriteLine("Kinect:" + timestamp);

            using (Microsoft.Kinect.KinectBuffer depthTempBuffer = depthFrame.LockImageBuffer())
            {
                // verify data , map to camera space , put in render buffer
                if (((424 * 512) == (depthTempBuffer.Size / 2)))    //2 bytes per pixel
                {
                    Marshal.Copy(depthTempBuffer.UnderlyingBuffer,depthBuffer,0, (int)depthTempBuffer.Size);   
                }
            }

            using (Microsoft.Kinect.KinectBuffer bodyIndexTempBuffer = bodyIndexFrame.LockImageBuffer())
            {
                if (((424 * 512) == (bodyIndexTempBuffer.Size / 1)))       //it's only 1 byte per pixel
                {
                    Marshal.Copy(bodyIndexTempBuffer.UnderlyingBuffer,bodyIndexBuffer,0 ,(int)bodyIndexTempBuffer.Size);
                }
            }

			//offlineBodyFrame = new OfflineBodyFrame(bodyFrame, timestamp); 
			myBodyFrame = new MyBodyFrame(bodyFrame, timestamp);

			//get floorClipPlane
            //floorClipPlane = bodyFrame.FloorClipPlane;  //copy (structure)

            //get body data
            //bodyFrame.GetAndRefreshBodyData(bodies);

			if(USE_RGB)
			{
				colorBuffer = new byte[1920*1080 * 2];
				//get colorFrame
				using (Microsoft.Kinect.KinectBuffer colorTempBuffer = colorFrame.LockRawImageBuffer())
				{
					//colorTempBuffer.Size == 1920*1080*2
					//colorFrame.RawColorImageFormat=Yuy2 https://support.microsoft.com/en-us/kb/294880
					//4bytes store data of 2 pixels. Y1,U,Y2,V
					//Y1 = luminance of pixel 1
					//Y2 = luminance of pixel 2
					//U and V = same chrominance for both pixels
					colorFrame.CopyRawFrameDataToArray(colorBuffer);  

					//colorFrame.CopyConvertedFrameDataToArray(colorBuffer, ColorImageFormat.Rgba);
					//Marshal.Copy(colorTempBuffer.UnderlyingBuffer, colorBuffer, 0, (int)colorTempBuffer.Size);
					
				}
			}

			if(USE_IR)
			{
				irBuffer = new byte[424 * 512 * 2];
				//get irFrame
				using (Microsoft.Kinect.KinectBuffer irTempBuffer = irFrame.LockImageBuffer())
				{
					// verify data , map to camera space , put in render buffer
					if (((424 * 512) == (irTempBuffer.Size / 2)))    //2 bytes per pixel
					{
						Marshal.Copy(irTempBuffer.UnderlyingBuffer, irBuffer, 0, (int)irTempBuffer.Size);
					}
				}
			}
        }

        public unsafe void mapToCameraSpace(CoordinateMapper mapper, IntPtr dest)
        {
            fixed(byte* tmp = depthBuffer)
            {
                IntPtr depthFramePtr = (IntPtr)tmp;
                mapper.MapDepthFrameToCameraSpaceUsingIntPtr(depthFramePtr, (uint)(424 * 512 * 2), dest, (uint)(424 * 512 * sizeof(CameraSpacePoint)));
            }
        }

        public void clean()
        {
            //nothing to clean now
        }

		public ushort getDepth(int x,int y)
		{
			int index = y * 512 + x;
			return (ushort)((((ushort)this.depthBuffer[index*2+1])<<8) | (ushort)this.depthBuffer[index*2]);
		}

		public ushort getInfrared(int x, int y)
		{
			int index = y * 512 + x;
			return (ushort)((((ushort)this.irBuffer[index * 2 + 1]) << 8) | (ushort)this.irBuffer[index * 2]);
		}

		public byte getColorIllumination(Vector2 p)
		{
			//look at 4 pixel around that position
			int x0 = (int)Math.Floor(p.X);
			int x1 = (int)Math.Ceiling(p.X);

			int y0 = (int)Math.Floor(p.Y);
			int y1 = (int)Math.Ceiling(p.Y);

			byte pixel1 = colorBuffer[(y0 * 1920 + x0) * 2];
			byte pixel2 = colorBuffer[(y0 * 1920 + x1) * 2];
			byte pixel3 = colorBuffer[(y1 * 1920 + x0) * 2];
			byte pixel4 = colorBuffer[(y1 * 1920 + x1) * 2];

			//get the brightest pixel
			return Math.Max(Math.Max(pixel1,pixel2), Math.Max(pixel3,pixel4));
		}

		
    }
}
