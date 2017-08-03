using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace SyncAll
{
	public class ObjOfBool
	{
		public bool state=false;

		public void set()
		{
			state=true;
		}

		public void reset()
		{
			state=false;
		}
	}

    class PixelCluster
    {
        public int leftMost;
        public int rightMost;
        public int upMost;
        public int downMost;

        public int size;

        public int color;

        public PixelCluster(int starti, int startj, int color)
        {
            leftMost = rightMost = startj;
            upMost = downMost = starti;
            size = 1;

            this.color = color;
        }

        public int getRectangularArea()
        {
            return (rightMost - leftMost + 1) * (downMost - upMost + 1);
        }
    }

    public struct PixelPosition
    {
        public int i;
        public int j;

		public PixelPosition(int i, int j)
		{
			this.i = i;
			this.j = j;
		}
    }

	public struct SpaceTime
	{
		public double timestamp;
		public Vector3 position;

		public SpaceTime(double timestamp, Vector3 position)
		{
			this.timestamp = timestamp;
			this.position = position;
		}
	}

	public class LinearPath
	{
		Vector3d a, b;

		public LinearPath(Vector3d a, Vector3d b)
		{
			this.a = a;
			this.b = b;
		}

		public Vector3 getPosition(double timestamp)
		{
			return (Vector3)(a * timestamp  + b);
		}

		public Vector3 getVelocity(double timestamp)
		{
			return (Vector3)(a);
		}

	}

	public class PalabolicPath
	{
		Vector3d a, b, c;

		public PalabolicPath(Vector3d a, Vector3d b, Vector3d c)
		{
			this.a = a;
			this.b = b;
			this.c = c;
		}

		public Vector3 getPosition(double timestamp)
		{
			return (Vector3)(a * timestamp * timestamp + b * timestamp + c);
		}

		public Vector3 getVelocity(double timestamp)
		{
			return (Vector3)(2 * a * timestamp + b);
		}

	}

	public class ErrorToExport
	{
		public double timestamp = double.NaN;
		public Vector3 eaglePosition = new Vector3(Single.NaN, Single.NaN, Single.NaN);
		public Vector3 kinectPosition = new Vector3(Single.NaN, Single.NaN, Single.NaN);
		public Vector3 fusionPosition = new Vector3(Single.NaN, Single.NaN, Single.NaN);

		public Vector3 kinectThreeDegreeError = new Vector3(Single.NaN, Single.NaN, Single.NaN);
		public Vector3 fusionThreeDegreeError = new Vector3(Single.NaN, Single.NaN, Single.NaN);

		public float kinectOneDegreeError = Single.NaN;
		public float fusionOneDegreeError = Single.NaN;

		public bool isOccluded = false;
	}
}
