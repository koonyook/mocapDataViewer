using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.IO;

namespace SyncAll
{
    public class Camera
    {
        public Vector3 Position = Vector3.Zero;
        //(float)Math.PI
        public Vector3 Orientation = new Vector3(0f, 0f, 0f);   //only 2 angles are used here 1.azimuth 2.attitude

		public float fovy = 1.3f;	//field of view angle for Y axis (in rad)
		public float zNear = 0.01f;
		public float zFar = 15f;

		public float aspectRatio = 1280f/720f;	//must get updated (width/height)

		public float MoveSpeed = 0.05f;
        public float MouseSensitivity = 0.01f;

        public Matrix4 GetViewMatrix()
        {
            Vector3 direction = new Vector3();
            // there is no orientation.Z
            direction.X = (float)(Math.Sin((float)Orientation.X) * Math.Cos((float)Orientation.Y));
            direction.Y = (float)Math.Sin((float)Orientation.Y);
            direction.Z = (float)(Math.Cos((float)Orientation.X) * Math.Cos((float)Orientation.Y));

            return Matrix4.LookAt(Position, Position + direction, Vector3.UnitY);
        }

		public Matrix4 getViewProjectionMatrix()
		{
			return GetViewMatrix()*Matrix4.CreatePerspectiveFieldOfView(fovy, aspectRatio, zNear, zFar);
		}

		public void updateAspectRatio(int width,int height)
		{
			aspectRatio=(float)(width)/(float)(height);
		}

		public void reset()
		{
			Position = Vector3.Zero;
			Orientation = Vector3.Zero;
		}

        public void Move(float x, float y, float z)
        {
            //i don't understand the way he calculate offset
            Vector3 offset = new Vector3();

            Vector3 forward = new Vector3((float)Math.Sin((float)Orientation.X), 0, (float)Math.Cos((float)Orientation.X));
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, MoveSpeed);

            Position += offset;
        }

        public void AddRotation(float x, float y)
        {
            x = x * MouseSensitivity;
            y = y * MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
        }
    }
}
