using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncAll
{
    public class GyroBiasTracker
    {
		//this 2 value doesn't have to change according to the sampling rate, it is not serious
		private const int GYRO_BASELINE_WINDOW = 120 * 80;   //120 seconds
		private const int REQUIRED_GYRO_BASELINE_WINDOW = 5 * 80; //5 seconds

		public float rgbx, rgby, rgbz;
		public float gbx, gby, gbz;   //gyro baseline
		int last_gx = 0, last_gy = 0, last_gz = 0;
		public bool gyroReady;
		int gbCounter;  //counter to collect gyro baseline

		//can start with previously calibrated data
		public GyroBiasTracker(float gbx=0,float gby=0,float gbz=0)
		{
			if(gbx==0 && gby==0 && gbz==0)
			{
				this.gbx = 0;
				this.gby = 0;
				this.gbz = 0;

				gyroReady = false;
				gbCounter = 1;
			}
			else
			{
				this.gbx = gbx;
				this.gby = gby;
				this.gbz = gbz;

				gyroReady = true;
				gbCounter = REQUIRED_GYRO_BASELINE_WINDOW;
			}

			rgbx = gbx;
			rgby = gby;
			rgbz = gbz;
		}

		public GyroBiasTracker(byte[] flattenVector)
		{
			gbx = BitConverter.ToSingle(flattenVector, 0);
			gby = BitConverter.ToSingle(flattenVector, 4);
			gbz = BitConverter.ToSingle(flattenVector, 8);

			if (gbx == 0 && gby == 0 && gbz == 0)
			{
				gyroReady = false;
				gbCounter = 1;
			}
			else
			{
				gyroReady = true;
				gbCounter = REQUIRED_GYRO_BASELINE_WINDOW;
			}

			rgbx = gbx;
			rgby = gby;
			rgbz = gbz;
		}

		public bool reset()
		{
			//go back to the point it was constructed
			gbx = rgbx;
			gby = rgby;
			gbz = rgbz;

			if (gbx == 0 && gby == 0 && gbz == 0)
			{
				gyroReady = false;
				gbCounter = 1;
			}
			else
			{
				gyroReady = true;
				gbCounter = REQUIRED_GYRO_BASELINE_WINDOW;
			}

			return gyroReady;
		}

		public void saveBiasStarterToFile(string filename)
		{
			SettingManager.saveVector3(filename, new Vector3(rgbx, rgby, rgbz));
		}

		public void loadBiasStarterFromFile(string filename)
		{
			Vector3 v=SettingManager.loadVector3(filename);
			rgbx = v.X;
			rgby = v.Y;
			rgbz = v.Z;

			gbCounter = 1;
		}

		public bool updateBias(raw_short g)
		{
			if (Math.Abs(g.x - last_gx) < 50
				&& Math.Abs(g.y - last_gy) < 50
				&& Math.Abs(g.z - last_gz) < 50
				)
			{
				//still enough, can be added to average
				gbx = (gbx * gbCounter + g.x) / (gbCounter + 1);
				gby = (gby * gbCounter + g.y) / (gbCounter + 1);
				gbz = (gbz * gbCounter + g.z) / (gbCounter + 1);

				gbCounter = Math.Min(++gbCounter, GYRO_BASELINE_WINDOW);

				//Debug.WriteLine(gbCounter);

				if (gyroReady == false && gbCounter == REQUIRED_GYRO_BASELINE_WINDOW)
				{
					gyroReady = true;   //allow to use in 10 seconds
				}
			}

			last_gx = g.x;
			last_gy = g.y;
			last_gz = g.z;

			return gyroReady;
		}

		public Vector3 convertToRadPerSec(raw_short g, double GYRO_SENSITIVITY)
		{
			float gx = (float)((g.x - gbx) * GYRO_SENSITIVITY);
			float gy = (float)((g.y - gby) * GYRO_SENSITIVITY);
			float gz = (float)((g.z - gbz) * GYRO_SENSITIVITY);

			return new Vector3(gx, gy, gz);
		}

		internal Vector3 getCurrentBias()
		{
			return new Vector3(gbx, gby, gbz);
		}

		internal void overwriteBias(Vector3 gyroBias)
		{
			gbx=gyroBias.X;
			gby=gyroBias.Y;
			gbz=gyroBias.Z;
		}
	}
}
