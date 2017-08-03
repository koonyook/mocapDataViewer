using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenTK;

namespace SyncAll
{
    /// <summary>
    /// quaternion-based Kalman filter
    /// 
    /// input:  
    ///     9-axis
    ///     acc must be normalized
    ///     mag must be calibrated and normalized
    ///     gyro offset must be removed
    /// 
    /// output: orientation quaternion
    /// </summary>
    public class Kalman
    {
		private int IMU_SAMPLING_RATE;
		float dt;     //sampling period in second (12.5 ms)

		public float[] x = new float[4];    //4-state vector in quaternion form
        public float[,] P;  //estimation covariance matrix

        
        float [,] Q = new float[4,4]{
                {0.00005f,0,0,0 },
                {0,0.00005f,0,0 },
                {0,0,0.00005f,0 },
                {0,0,0,0.00005f }
            };
        float[,] R = new float[4, 4]{
                {0.6f,0,0,0 },
                {0,0.6f,0,0 },
                {0,0,0.6f,0 },
                {0,0,0,0.6f }
            };
        /// <summary>
        /// be careful about the input
        /// </summary>
        /// <param name="MagneticSensorQuaternion">Normalized Quaternion of sensor frame relative to magnetic frame (Sensor->Magnetic)</param>
        public void initialize(int IMU_SAMPLING_RATE) //(Quaternion SEq)
        {
			this.IMU_SAMPLING_RATE = IMU_SAMPLING_RATE;
			dt = 1f / IMU_SAMPLING_RATE;

			/*
            //must be normalized
            x[0] = MagneticSensorQuaternion.W;
            x[1] = MagneticSensorQuaternion.X;
            x[2] = MagneticSensorQuaternion.Y;
            x[3] = MagneticSensorQuaternion.Z;
            */

			//follow the paper. it will converge in a few iteration
			x[0] = 1;
            x[1] = 0;
            x[2] = 0;
            x[3] = 0;

            P = new float[4, 4] {
                {1,0,0,0 },
                {0,1,0,0 },
                {0,0,1,0 },
                {0,0,0,1 }
            };
        }

		public void initialize(int IMU_SAMPLING_RATE, Quaternion magneticSensorQuaternion) //in the case that can estimate the orientation
		{
			this.IMU_SAMPLING_RATE = IMU_SAMPLING_RATE;
			dt = 1f / IMU_SAMPLING_RATE;

			magneticSensorQuaternion.Normalize();

            x[0] = magneticSensorQuaternion.W;
            x[1] = magneticSensorQuaternion.X;
            x[2] = magneticSensorQuaternion.Y;
            x[3] = magneticSensorQuaternion.Z;
            
			P = new float[4, 4] {
				{1,0,0,0 },
				{0,1,0,0 },
				{0,0,1,0 },
				{0,0,0,1 }
			};
		}

		public void filterUpdate(Vector3 gyro, Vector3 acc, Vector3 mag)
		{
			filterUpdate(gyro.X, gyro.Y, gyro.Z, acc.X, acc.Y, acc.Z, mag.X, mag.Y, mag.Z);
		}

		public void filterUpdate(float wx, float wy, float wz, float ax, float ay, float az, float mx, float my, float mz)
        {
            //integration(prediction) step (use only gyro)
            float[,] F = new float[4,4] {
                {      1 , -wx*dt/2, -wy*dt/2, -wz*dt/2 },
                { wx*dt/2 ,        1,  wz*dt/2, -wy*dt/2 },
                { wy*dt/2 , -wz*dt/2,        1,  wx*dt/2 },
                { wz*dt/2 ,  wy*dt/2, -wx*dt/2,        1 }
            };

            float[] x_predict = MxV(F, x);
            float[,] P_predict = MplusM( MxM(MxM(F,P),transpose(F)) , Q);

            //vector observation step (create quaternion from acc and mag)
            float[] z = calculateSEQuaternionFromMagneticGravity(new Vector3(ax,ay,az),new Vector3(mx,my,mz));

            //there are 2 candidates (z and -z)
            //choose the one that is close to the predicted quaternion 
            float[] dZPlus = VminusV(z, x_predict);
            float[] dZMinus = VplusV(z, x_predict);

            if(dZPlus[0]*dZPlus[0]+dZPlus[1]*dZPlus[1]+dZPlus[2]*dZPlus[2]+dZPlus[3]*dZPlus[3] > 
                dZMinus[0] * dZMinus[0] + dZMinus[1] * dZMinus[1] + dZMinus[2] * dZMinus[2] + dZMinus[3] * dZMinus[3])
            {
                z[0] = -z[0];
                z[1] = -z[1];
                z[2] = -z[2];
                z[3] = -z[3];
            }

            //Kalman gain
            float[,] K = MxM(P_predict, inverse(MplusM(P_predict, R)));

            //state update
            x = VplusV(x_predict, MxV(K, VminusV(z, x_predict)));

            //P update
            P = MminusM(P_predict, MxM(K, P_predict));

            //normalize (not written in the paper, but I think it is important
            float norm = (float)Math.Sqrt(x[0] * x[0] + x[1] * x[1] + x[2] * x[2] + x[3] * x[3]);
            x[0] /= norm;
            x[1] /= norm;
            x[2] /= norm;
            x[3] /= norm;
        }

        public Quaternion getMagneticSensorQuaternion()
        {
            return new Quaternion(x[1], x[2], x[3], x[0]);  //OpenTK Quaternion
        }

        /// <summary>
        /// matrix x vector multiplication
        /// </summary>
        /// <param name="m">4x4 matrix</param>
        /// <param name="v">4x1 vector</param>
        /// <returns></returns>
        private float[] MxV(float[,] m, float[] v)
        {
            return new float[4]
            {
                m[0,0]*v[0]+m[0,1]*v[1]+m[0,2]*v[2]+m[0,3]*v[3],
                m[1,0]*v[0]+m[1,1]*v[1]+m[1,2]*v[2]+m[1,3]*v[3],
                m[2,0]*v[0]+m[2,1]*v[1]+m[2,2]*v[2]+m[2,3]*v[3],
                m[3,0]*v[0]+m[3,1]*v[1]+m[3,2]*v[2]+m[3,3]*v[3]
            };
        }

        private float[,] MxM(float[,] a, float[,] b)
        {
            float[,] c = new float[4, 4];
            for(int i=0;i<4;i++)
            {
                for(int j=0;j<4;j++)
                {
                    c[i, j] = a[i, 0] * b[0, j] + a[i, 1] * b[1, j] + a[i, 2] * b[2, j] + a[i, 3] * b[3, j];
                }
            }

            return c;
        }

        private float[,] MplusM(float[,] a, float[,] b)
        {
            float[,] c = new float[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    c[i, j] = a[i, j] + b[i, j];
                }
            }

            return c;
        }

        private float[,] MminusM(float[,] a, float[,] b)
        {
            float[,] c = new float[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    c[i, j] = a[i, j] - b[i, j];
                }
            }

            return c;
        }

        private float[] VplusV(float[] a, float[] b)
        {
            return new float[4] { a[0] + b[0], a[1] + b[1], a[2] + b[2], a[3] + b[3] };
        }

        private float[] VminusV(float[] a, float[] b)
        {
            return new float[4] { a[0] - b[0], a[1] - b[1], a[2] - b[2], a[3] - b[3] };
        }

        private float[,] transpose(float[,] m)
        {
            return new float[4, 4] {
                { m[0,0], m[1,0], m[2,0], m[3,0] },
                { m[0,1], m[1,1], m[2,1], m[3,1] },
                { m[0,2], m[1,2], m[2,2], m[3,2] },
                { m[0,3], m[1,3], m[2,3], m[3,3] }
            };
        }

        private float[,] inverse(float[,] a)
        {

            Matrix4 m = new Matrix4(
                a[0, 0], a[0, 1], a[0, 2], a[0, 3],
                a[1, 0], a[1, 1], a[1, 2], a[1, 3],
                a[2, 0], a[2, 1], a[2, 2], a[2, 3],
                a[3, 0], a[3, 1], a[3, 2], a[3, 3]
                );

            m.Invert(); // http://www.cg.info.hiroshima-cu.ac.jp/~miyazaki/knowledge/teche23.html

            return new float[4, 4] {
                { m.M11, m.M12, m.M13, m.M14 },
                { m.M21, m.M22, m.M23, m.M24 },
                { m.M31, m.M32, m.M33, m.M34 },
                { m.M41, m.M42, m.M43, m.M44 },
            };

        }

        private float[] calculateSEQuaternionFromMagneticGravity(Vector3 gravity, Vector3 mag)
        {
            Vector3 Zaxis = gravity.Normalized();
            Vector3 Yaxis = Vector3.Cross(Zaxis, mag.Normalized()).Normalized();    //very important to normalize after cross product
            Vector3 Xaxis = Vector3.Cross(Yaxis, Zaxis).Normalized();               //very important to normalize after cross product

            Quaternion ESq = getQuaternionFromMatrix(new Matrix3(
                Xaxis.X, Yaxis.X, Zaxis.X,
                Xaxis.Y, Yaxis.Y, Zaxis.Y,
                Xaxis.Z, Yaxis.Z, Zaxis.Z
                )).Normalized();

            ESq.Conjugate();    //change to SEq
            return new float[] { ESq.W, ESq.X, ESq.Y, ESq.Z };
        }

        private Quaternion getQuaternionFromMatrix(Matrix3 matrix)
        {
            //copy from openTK
            Quaternion result = new Quaternion();

            float trace = matrix.Trace;

            if (trace > 0)
            {
                double s = Math.Sqrt(trace + 1) * 2;
                //if (Global.isGoingToPrintDebug)
                //    Debug.WriteLine(s);
                //float invS = 1f / s;

                result.W = (float)(s * 0.25f);
                result.X = (float)((matrix.Row2.Y - matrix.Row1.Z) / s);
                result.Y = (float)((matrix.Row0.Z - matrix.Row2.X) / s);
                result.Z = (float)((matrix.Row1.X - matrix.Row0.Y) / s);
            }
            else
            {
                float m00 = matrix.Row0.X, m11 = matrix.Row1.Y, m22 = matrix.Row2.Z;

                if (m00 > m11 && m00 > m22)
                {
                    double s = Math.Sqrt(1 + m00 - m11 - m22) * 2;
                    //float invS = 1f / s;
                    //if (Global.isGoingToPrintDebug)
                    //    Debug.WriteLine(s);
                    result.W = (float)((matrix.Row2.Y - matrix.Row1.Z) / s);
                    result.X = (float)(s * 0.25f);
                    result.Y = (float)((matrix.Row0.Y + matrix.Row1.X) / s);
                    result.Z = (float)((matrix.Row0.Z + matrix.Row2.X) / s);
                }
                else if (m11 > m22)
                {
                    double s = Math.Sqrt(1 + m11 - m00 - m22) * 2;
                    //float invS = 1f / s;
                    //if (Global.isGoingToPrintDebug)
                    //    Debug.WriteLine(s);
                    result.W = (float)((matrix.Row0.Z - matrix.Row2.X) / s);
                    result.X = (float)((matrix.Row0.Y + matrix.Row1.X) / s);
                    result.Y = (float)(s * 0.25f);
                    result.Z = (float)((matrix.Row1.Z + matrix.Row2.Y) / s);
                }
                else
                {
                    double s = Math.Sqrt(1 + m22 - m00 - m11) * 2;
                    //float invS = 1f / s;
                    //if (Global.isGoingToPrintDebug)
                    //    Debug.WriteLine(s);
                    result.W = (float)((matrix.Row1.X - matrix.Row0.Y) / s);
                    result.X = (float)((matrix.Row0.Z + matrix.Row2.X) / s);
                    result.Y = (float)((matrix.Row1.Z + matrix.Row2.Y) / s);
                    result.Z = (float)(s * 0.25f);
                }
            }

            return result;
        }


    }

    
}
