using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace SyncAll
{
	//this will obsolete soon because of the new PCA class
    public class PCAResult
    {
        public Vector3 average = new Vector3();
        public Vector3 unitDirection = new Vector3();

        public double significant = 0;
    }

    public static class MyMath
    {
        public static Matrix3 AddMatrix3(Matrix3 a, Matrix3 b)
        {
            return new Matrix3(a.M11 + b.M11, a.M12 + b.M12, a.M13 + b.M13, a.M21 + b.M21, a.M22 + b.M22, a.M23 + b.M23, a.M31 + b.M31, a.M32 + b.M32, a.M33 + b.M33);
        }

        public static Matrix3 FixRotationMatrix3(Matrix3 a, float b)
        {
            Vector3 x = a.Column0;
            Vector3 y = a.Column1;
            Vector3 z = a.Column2;
            //choose the maximum length to fix (it will be almost b), which is the most precise
            if (x.Length > y.Length && x.Length > z.Length)
            {
                //fix x axis
                x.Normalize();
                if (y.Length > z.Length)
                {
                    y.Normalize();
                    z = Vector3.Cross(x, y);
                    y = Vector3.Cross(z, x);
                }
                else
                {
                    z.Normalize();
                    y = Vector3.Cross(z, x);
                    z = Vector3.Cross(x, y);
                }
            }
            else if (y.Length > x.Length && y.Length > z.Length)
            {
                //fix y axis
                y.Normalize();
                if (x.Length > z.Length)
                {
                    x.Normalize();
                    z = Vector3.Cross(x, y);
                    x = Vector3.Cross(y, z);
                }
                else
                {
                    z.Normalize();
                    x = Vector3.Cross(y, z);
                    z = Vector3.Cross(x, y);
                }
            }
            else
            {
                //fix z axis
                z.Normalize();
                if (x.Length > y.Length)
                {
                    x.Normalize();
                    y = Vector3.Cross(z, x);
                    x = Vector3.Cross(y, z);
                }
                else
                {
                    y.Normalize();
                    x = Vector3.Cross(y, z);
                    y = Vector3.Cross(z, x);
                }
            }

            Matrix3 result = new Matrix3(x, y, z);
            result.Transpose();
            return result;
        }

		/*
        public unsafe static PCAResult calculatePCA(List<Vector3> pointList)
        {
            if (pointList.Count < 2)
                return null;

            int i, j, k;

            PCAResult result = new PCAResult();
            Vector3 total = new Vector3();
            foreach (Vector3 aPoint in pointList)
            {
                total = total + aPoint;
            }
            result.average = total / pointList.Count;

            //subtract the average put to the new array (calculate matrix is faster on array)
            Vector3[] dataMatrix = new Vector3[pointList.Count];

            for (i = 0; i < pointList.Count; i++)
            {
                dataMatrix[i] = pointList[i] - result.average;
            }

            //get covariance matrix (3x3)
            double[,] cov = new double[3, 3];
            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    cov[i, j] = 0;
                }
            }

            for (i = 0; i < 3; i++)
            {
                for (j = i; j < 3; j++)   //calculate only top half
                {
                    for (k = 0; k < pointList.Count; k++)
                    {
                        fixed (void* d = (&dataMatrix[k]))
                        {
                            float* tmp = ((float*)d);
                            cov[i, j] += tmp[i] * tmp[j];
                        }
                    }
                }
            }
            //then copy 3 elements
            cov[1, 0] = cov[0, 1];
            cov[2, 0] = cov[0, 2];
            cov[2, 1] = cov[1, 2];
            //covariance matrix is done
            double[] eigenValues = new double[3];
            double[,] eigenVectors = new double[3, 3];
            double[,] bMatrix = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

            bool isSolved = alglib.spdgevd.smatrixgevd(cov, 3, true, bMatrix, true, 1, 1, ref eigenValues, ref eigenVectors);
            if (isSolved)
            {


                double maxEigenValue = Math.Max(eigenValues[0], Math.Max(eigenValues[1], eigenValues[2]));
                int selectedEigen;
                if (maxEigenValue == eigenValues[0])
                    selectedEigen = 0;
                else if (maxEigenValue == eigenValues[1])
                    selectedEigen = 1;
                else
                    selectedEigen = 2;

                //this is only unit vector
                //result.unitDirection.X = (float)eigenVectors[selectedEigen, 0];
                //result.unitDirection.Y = (float)eigenVectors[selectedEigen, 1];
                //result.unitDirection.Z = (float)eigenVectors[selectedEigen, 2];

                result.unitDirection.X = (float)eigenVectors[0, selectedEigen];
                result.unitDirection.Y = (float)eigenVectors[1, selectedEigen];
                result.unitDirection.Z = (float)eigenVectors[2, selectedEigen];

                result.significant = maxEigenValue / (eigenValues[0] + eigenValues[1] + eigenValues[2]);

                return result;
            }
            else
            {
                return null;
            }
        }
		*/

		public static Matrix4 getTransformationMatrixFromRotationAndTranslation(Vector3 w, Vector3 translation)
		{
			var angleSquare=w.LengthSquared;
			var angleSquareFixed=Math.Max(angleSquare,0.000000000001f);
			var angle=(float)Math.Sqrt(angleSquareFixed);
			var wUnit=w/angle;
			var sin = (float)Math.Sin(angle);
			var cos = (float)Math.Cos(angle);
			var oneNcos = 1-cos;
			var wx=wUnit.X;
			var wy=wUnit.Y;
			var wz=wUnit.Z;

			return new Matrix4(
				cos+wx*wx*oneNcos, wx*wy*oneNcos-wz*sin, wy*sin+wx*wz*oneNcos, translation[0],
				wz*sin+wx*wy*oneNcos, cos+wy*wy*oneNcos, wy*wz*oneNcos-wx*sin, translation[1],
				wx*wz*oneNcos-wy*sin, wx*sin+wy*wz*oneNcos, cos+wz*wz*oneNcos, translation[2],
				0,0,0,1
				);
		}

		/*
		public static Vector3 calculateSurfaceNormalFromCovMatrix(Matrix3 m)
        {
            double[,] cov = new double[3, 3] { { m.M11, m.M12, m.M13 }, { m.M21, m.M22, m.M23 }, { m.M31, m.M32, m.M33 } };

            double[] eigenValues = new double[3];
            double[,] eigenVectors = new double[3, 3];
            double[,] bMatrix = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

            bool isSolved = alglib.spdgevd.smatrixgevd(cov, 3, true, bMatrix, true, 1, 1, ref eigenValues, ref eigenVectors);
            if (isSolved)
            {


                double minEigenValue = Math.Min(eigenValues[0], Math.Max(eigenValues[1], eigenValues[2]));

                int selectedEigenA, selectedEigenB;

                if (minEigenValue == eigenValues[0])
                {
                    selectedEigenA = 1;
                    selectedEigenB = 2;
                }
                else if (minEigenValue == eigenValues[1])
                {
                    selectedEigenA = 0;
                    selectedEigenB = 2;
                }
                else
                {
                    selectedEigenA = 0;
                    selectedEigenB = 1;
                }

                //cross 2 selected columns to get surface normal

                Vector3 result = Vector3.Cross(
                    new Vector3((float)eigenVectors[0, selectedEigenA], (float)eigenVectors[1, selectedEigenA], (float)eigenVectors[2, selectedEigenA]),
                    new Vector3((float)eigenVectors[0, selectedEigenB], (float)eigenVectors[1, selectedEigenB], (float)eigenVectors[2, selectedEigenB])
                );

                if (result.Z < 0)
                    return result;
                else
                    return -result;
            }
            else
            {
                return new Vector3(0);
            }

        }
		*/

        public static Quaternion calculateKinectSensorQuaternion(Quaternion MagneticSensorQuaternion, Quaternion MagneticBaseQuaternion, Quaternion KinectBaseQuaternion) //the last parameter is offset
        {
            return Quaternion.Multiply(Quaternion.Multiply(KinectBaseQuaternion, MagneticBaseQuaternion.Inverted()), MagneticSensorQuaternion);

        }

        //vector must have w=0
        //convert from reference frame A to reference frame B
        //vector v are in reference frame A
        public static Vector3 rotateVector(Quaternion BrefArefQuaternion, Vector3 vInRefA)
        {
            Quaternion vectorInQuaternionForm = new Quaternion(vInRefA, 0);
            BrefArefQuaternion.Normalize();
            Quaternion qc = BrefArefQuaternion;
            qc.Conjugate();
            
            return Quaternion.Multiply(Quaternion.Multiply(BrefArefQuaternion, vectorInQuaternionForm), qc).Xyz;
        }
        /// <summary>
        /// Be careful, this function return quaternion Sensor -> Magnetic
        /// </summary>
        /// <param name="gravity"></param>
        /// <param name="mag"></param>
        /// <returns></returns>
        public static Quaternion calculateMagneticSensorQuaternionFromMagneticGravity(Vector3 gravity,Vector3 mag)
        {
            Vector3 Zaxis = gravity.Normalized();
            Vector3 Yaxis = Vector3.Cross(Zaxis, mag.Normalized()).Normalized();    //very important to normalize after cross product
            Vector3 Xaxis = Vector3.Cross(Yaxis, Zaxis).Normalized();               //very important to normalize after cross product

            //Quaternion ESq = Quaternion.FromMatrix(new Matrix3(
            Quaternion MagneticSensorQuaternion = MyMath.getQuaternionFromMatrix(new Matrix3(
                Xaxis.X, Yaxis.X, Zaxis.X,
                Xaxis.Y, Yaxis.Y, Zaxis.Y,
                Xaxis.Z, Yaxis.Z, Zaxis.Z
                )).Normalized();    //this is SensorMagneticQuaternion

            MagneticSensorQuaternion.Conjugate();    //change to MagneticSensorQuaternion
            return MagneticSensorQuaternion;
        }

		public static Quaternion get_B_A_QuaternionFromThreeAxis(Vector3 B_xAxisA, Vector3 B_yAxisA, Vector3 B_zAxisA)
		{
			//confirm
			//if those axis is frameA in Kinect reference frame
			//the result from this function will be kinect_A_q

			Matrix3 m = new Matrix3(B_xAxisA, B_yAxisA, B_zAxisA);  
			m.Transpose();
			return getQuaternionFromMatrix(m);
		}

        public static Quaternion getQuaternionFromMatrix(Matrix3 matrix)
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
                    result.W = (float)((matrix.Row2.Y - matrix.Row1.Z) /s);
                    result.X = (float)(s * 0.25f);
                    result.Y = (float)((matrix.Row0.Y + matrix.Row1.X) /s);
                    result.Z = (float)((matrix.Row0.Z + matrix.Row2.X) /s);
                }
                else if (m11 > m22)
                {
                    double s = Math.Sqrt(1 + m11 - m00 - m22) * 2;
                    //float invS = 1f / s;
                    //if (Global.isGoingToPrintDebug)
                    //    Debug.WriteLine(s);
                    result.W = (float)((matrix.Row0.Z - matrix.Row2.X) /s);
                    result.X = (float)((matrix.Row0.Y + matrix.Row1.X) /s);
                    result.Y = (float)(s * 0.25f);
                    result.Z = (float)((matrix.Row1.Z + matrix.Row2.Y) /s);
                }
                else
                {
                    double s = Math.Sqrt(1 + m22 - m00 - m11) * 2;
                    //float invS = 1f / s;
                    //if (Global.isGoingToPrintDebug)
                    //    Debug.WriteLine(s);
                    result.W = (float)((matrix.Row1.X - matrix.Row0.Y) /s);
                    result.X = (float)((matrix.Row0.Z + matrix.Row2.X) /s);
                    result.Y = (float)((matrix.Row1.Z + matrix.Row2.Y) /s);
                    result.Z = (float)(s * 0.25f);
                }
            }

            return result;
        }

        public static void printMatrix(Matrix3 m)
        {
            printVector(m.Row0);
            printVector(m.Row1);
            printVector(m.Row2);
            Debug.WriteLine("------------");
        }

        public static void printVector(Vector3 v)
        {
            Debug.WriteLine("{0},{1},{2}",v.X,v.Y,v.Z);
        }
        
        public static Quaternion fixRodDegree(Quaternion imuRodSEq) //never worked
        {
            //Quaternion q2 = Quaternion.FromMatrix(Matrix3.CreateRotationZ((float)(45 * Math.PI / 180)));    //it does not work this way
            
            Vector3 imuZinEarthFrame = MyMath.rotateVector(imuRodSEq,new Vector3(0,0,1));

            Quaternion q2 = Quaternion.FromAxisAngle(imuZinEarthFrame.Normalized(),(float)(-6 * Math.PI / 180));

            //Matrix3 tmp = Matrix3.Mult(Matrix3.CreateFromQuaternion(imuRodSEq),Matrix3.CreateRotationZ((float)(6 * Math.PI / 180)));
            //Matrix3 tmp = Matrix3.CreateFromQuaternion(imuRodSEq);
            //imuRodSEq = Quaternion.FromMatrix(tmp);

            return Quaternion.Multiply(imuRodSEq,q2.Normalized());
        }

		public static Vector3 calculateOrientationDifferenceIn3Axis(Quaternion a, Quaternion b)
		{
			a.Normalized();
			b.Normalized();
			Vector3 ax = MyMath.rotateVector(a, new Vector3(1, 0, 0));
			Vector3 bx = MyMath.rotateVector(b, new Vector3(1, 0, 0));
			Vector3 ay = MyMath.rotateVector(a, new Vector3(0, 1, 0));
			Vector3 by = MyMath.rotateVector(b, new Vector3(0, 1, 0));
			Vector3 az = MyMath.rotateVector(a, new Vector3(0, 0, 1));
			Vector3 bz = MyMath.rotateVector(b, new Vector3(0, 0, 1));
			return (new Vector3(Vector3.CalculateAngle(ax, bx), Vector3.CalculateAngle(ay, by), Vector3.CalculateAngle(az, bz))) * 180 / (float)Math.PI;
		}

		public static float calculateMaxDegree(Quaternion a,Quaternion b)
        {
			//try not to use this, use calculateOrientationDifferenceInOneRotationAngle for more meaningful result
			Vector3 tmp = calculateOrientationDifferenceIn3Axis(a, b);
			return Math.Max(tmp.X, Math.Max(tmp.Y,tmp.Z));
        }

		public static float calculateOrientationDifferenceInOneRotationAngle(Quaternion a, Quaternion b)
		{
			a.Normalize();
			b.Normalize();
			Quaternion a_b_quaternion = quaternionMultiplySequence(a.Inverted(), b);
			float angle = (float)(2 * Math.Acos(a_b_quaternion.W) * 180 / Math.PI);

			if (angle < 180)
				return angle;
			else
				return 360 - angle; 
		}

		public static Vector3 getEulerAngleFromQuaternion(Quaternion q)
		{
			float angle = (float)(2 * Math.Acos(q.W));
			float x = (float)(q.X / Math.Sqrt(1 - q.W * q.W));
			float y = (float)(q.Y / Math.Sqrt(1 - q.W * q.W));
			float z = (float)(q.Z / Math.Sqrt(1 - q.W * q.W));
			return new Vector3(x, y, z).Normalized() * angle;
		}

		public static Vector3 getEulerSequenceZXYfromQuaternion(Quaternion q)
		{
			//input of this function can be
			// spineBase_upperArmLeft_quaternion
			// spineBase_upperArmRight_quaternion
			// spineBase_head_quaternion
			// kinect_spineBase_quaternion

			Vector3 yAxis = rotateVector(q, new Vector3(0, 1, 0));
			float theta1 = (float)Math.Atan2(-yAxis.X, yAxis.Y);    //full range [-pi, pi]
			float theta2 = (float)Math.Asin(yAxis.Z);               //limited to [-pi/2, pi/2] (because it is enough)

			Quaternion q_y = Quaternion.Multiply(Quaternion.Multiply(Quaternion.FromAxisAngle(new Vector3(1, 0, 0), theta2).Inverted(), Quaternion.FromAxisAngle(new Vector3(0, 0, 1), theta1).Inverted()), q);

			float theta3 = 2 * (float)Math.Atan2(q_y.Y, q_y.W);

			return new Vector3(theta1, theta2, theta3);
		}


		//tested: correct
		public static Vector3 getEulerSequenceYZYfromQuaternion(Quaternion q, bool isLeftArm)
		{
			//input must be EndOpt_forearm_quaternion
			//(Y -> Z -> Y)

			//for right side 
			//+theta1 = shoulder external rotation
			//+theta2 = elbow flexion
			//+theta3 = forearm supination

			//for left side
			//+theta1 = shoulder internal rotation
			//+theta2 = elbow extension
			//+theta3 = forearm pronation

			double theta1 = (Math.Atan2(q.Y, q.W) + Math.Atan2(q.X, q.Z));

			double theta3 = (Math.Atan2(q.Y, q.W) - Math.Atan2(q.X, q.Z));

			double theta2 = 2 * Math.Atan2(q.X / Math.Sin((theta1 - theta3) / 2), q.W / Math.Cos((theta1 + theta3) / 2));

			//alternative option (for left arm)
			//this will help left elbow to follow kinect convention
			if (isLeftArm)
			{
				theta1 += Math.PI;
				theta2 = -theta2;
				theta3 -= Math.PI;
			}

			return new Vector3((float)theta1, (float)theta2, (float)theta3);
		}

		//TODO
		public static Vector3 getEulerSequenceZXZfromQuaternion(Quaternion q, float firstZ)
		{
			//input must be spineShoulder_clavicle_quaternion
			//(Z -> X -> Z)

			//the first Z is known (constant)
			//so, solve just the second and the third parameter

			//for right side 
			//+theta1 = constant
			//+theta2 = 
			//+theta3 = 

			//for left side
			//+theta1 = constant
			//+theta2 = 
			//+theta3 = 

			Quaternion q23 = Quaternion.Multiply(Quaternion.FromAxisAngle(unitZ, firstZ).Inverted(), q);

			double theta1 = firstZ;

			double theta2 = 2 * Math.Atan2(q23.X, q23.W);

			double theta3 = 2 * Math.Atan2(q23.Z, q23.W);

			return new Vector3((float)theta1, (float)theta2, (float)theta3);
		}

		public static Vector3 unitX = new Vector3(1, 0, 0);
		public static Vector3 unitY = new Vector3(0, 1, 0);
		public static Vector3 unitZ = new Vector3(0, 0, 1);

		public static Vector3 vector3Interpolate(Vector3 a, Vector3 b, float aRatio)
		{
			return a * aRatio + b * (1 - aRatio);
		}

		public static Vector3 vector3TimeInterpolate(Vector3 a, Vector3 b, double timeA, double timeTarget, double timeB)
		{
			float aRatio = (float)((timeB - timeTarget) / (timeB - timeA));
			return a * aRatio + b * (1 - aRatio);
		}

		public static float scalarTimeInterpolate(float a, float b, double timeA, double timeTarget, double timeB)
		{
			float aRatio = (float)((timeB - timeTarget) / (timeB - timeA));
			return a * aRatio + b * (1 - aRatio);
		}

		public static Quaternion quaternionInterpolate_LERP(Quaternion a, Quaternion b, float aRatio)
		{
			if (a.W * b.W + a.X * b.X + a.Y * b.Y + a.Z * b.Z < 0)
			{
				b*=-1;
			}
			return (a * aRatio + b * (1 - aRatio)).Normalized();
		}
		public static Quaternion quaternionInterpolate_SLERP(Quaternion a, Quaternion b, float aRatio)
		{
			if (a.W * b.W + a.X * b.X + a.Y * b.Y + a.Z * b.Z < 0)
			{
				b *= -1;
			}
			return Quaternion.Slerp(a, b, 1-aRatio);
		}

		public static Quaternion quaternionTimeInterpolate_SLERP(Quaternion a, Quaternion b, double timeA, double timeTarget, double timeB)
		{
			float aRatio = (float)((timeB - timeTarget) / (timeB - timeA));
			return quaternionInterpolate_SLERP(a, b, aRatio);
		}

		public static double doubleInterpolate(double[] a,double targetIndex)
		{
			if(Math.Floor(targetIndex)==targetIndex)
				return a[(int)Math.Floor(targetIndex)];
			else
			{
				int f=(int)Math.Floor(targetIndex);
				double fraction = targetIndex - Math.Floor(targetIndex);
				return a[f]*(1-fraction)+a[f+1]*fraction;
			}
		}

		//if input is (ABq, BCq, CDq) the result will be ADq
		public static Quaternion quaternionMultiplySequence(params Quaternion[] sequence)
		{
			Quaternion result = new Quaternion(0, 0, 0, 1);
			foreach(Quaternion q in sequence)
			{
				result = Quaternion.Multiply(result, q);
			}
			return result;
		}

		public static int findTheHighestContinuousPeak(byte[] grayLine, int length, int filterRadius)
		{
			//1+2*radius must be less than length

			int maxSum = -1;
			int maxIndex = -1;
			for(int i=filterRadius;i<length-filterRadius;i++)
			{
				int sum = 0;
				for(int j=-filterRadius;j<=filterRadius;j++)
				{
					sum += grayLine[i + j];
				}
				if(sum>maxSum)
				{
					maxSum = sum;
					maxIndex = i;
				}
			}
			return maxIndex;
		}

		public static float calculateDistanceFromLine(Vector2 point, Vector2 lineBegin, Vector2 lineEnd)
		{
			Vector2 projectedPoint = projectPointOnLine(point, lineBegin, lineEnd);
			return (point - projectedPoint).Length;
		}

		public static Vector2 projectPointOnLine(Vector2 point, Vector2 lineBegin, Vector2 lineEnd)
		{
			return lineBegin + (lineEnd - lineBegin) * (Vector2.Dot(lineEnd - lineBegin, point - lineBegin) / (lineEnd - lineBegin).LengthSquared);
		}

		public static Vector3 projectPointOnLine(Vector3 point, Vector3 lineBegin, Vector3 lineEnd)
		{
			return lineBegin + (lineEnd - lineBegin) * (Vector3.Dot(lineEnd - lineBegin, point - lineBegin) / (lineEnd - lineBegin).LengthSquared);
		}

		public static bool isThisPointInCylinder(Vector3 point, Vector3 cylinderCoreStart, Vector3 cylinderCoreEnd, float cylinderRadius)
		{
			Vector3 projectedPoint = projectPointOnLine(point,cylinderCoreStart,cylinderCoreEnd);
			if((point-projectedPoint).Length>cylinderRadius)
				return false;
			else
			{
				Vector3 directionToStart = (cylinderCoreStart - projectedPoint).Normalized();
				Vector3 directionToEnd = (cylinderCoreEnd - projectedPoint).Normalized();
				if(Vector3.Dot(directionToStart,directionToEnd)>0)	//it will be around one
					return false;
				else
					return true;
			}
		}

		public static Vector3 projectVectorOnPlane(Vector3 normal, Vector3 v)
		{
			normal.Normalize();
			Vector3 tmp = normal*Vector3.Dot(normal, v);
			return v - tmp;
		}

		public static Vector3 intersectionPointOfTwoLines(Vector3 pointA, Vector3 directionA, Vector3 pointB, Vector3 directionB)
		{
			//direction a and b must be unit vector

			//return the point at the middle of the shortest connected line between 2 points
			//follow this method
			//http://geomalgorithms.com/a07-_distance.html
			Vector3 w0 = pointA - pointB;
			float a = 1;
			float b = Vector3.Dot(directionA, directionB);  //they must be unit vectors
			float c = 1;
			float d = Vector3.Dot(directionA, w0);
			float e = Vector3.Dot(directionB, w0);

			float Sc = (b * e - c * d) / (a * c - b * b);
			float Tc = (a * e - b * d) / (a * c - b * b);

			Vector3 bestA = pointA + directionA * Sc;
			Vector3 bestB = pointB + directionB * Tc;

			return (bestA + bestB) / 2;	//virtual intersection
		}

		public static float scope(float mean, float variationRadius, float input)
		{
			if (input > mean + variationRadius)
				return mean + variationRadius;

			if (input < mean - variationRadius)
				return mean - variationRadius;

			return input;
		}


		

		public static void linearRegression(double[] inX,double[] inY,int n, out double slope,out double intercept)	//n*2 matrix
		{
			double xx = 0, x = 0, xy = 0, y = 0;
			for(int i=0;i<n;i++)
			{
				xx += inX[i] * inX[i];
				x += inX[i];
				xy += inX[i] * inY[i];
				y += inY[i];
			}

			double det = xx * n - x * x;

			slope = (xy*n-x*y) / det;
			intercept = (-xy * x + y * xx) / det;

			//Debug.WriteLine("validation");

		}

		public static Vector4 getFrameRotationFrom_A_to_B_InAxisAngle(Quaternion A_B_Quaternion)
		{
			return A_B_Quaternion.ToAxisAngle();
		}

		public static Vector4 getPointRotationFrom_B_to_A_InAxisAngle(Quaternion A_B_Quaternion)
		{
			return A_B_Quaternion.ToAxisAngle();
		}


		public static Vector3 getBaryCentricFromCatesian(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
		{
			//http://gamedev.stackexchange.com/questions/23743/whats-the-most-efficient-way-to-find-barycentric-coordinates
			//not tested yet
			Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
			float d00 = Vector3.Dot(v0, v0);
			float d01 = Vector3.Dot(v0, v1);
			float d11 = Vector3.Dot(v1, v1);
			float d20 = Vector3.Dot(v2, v0);
			float d21 = Vector3.Dot(v2, v1);
			float denom = d00 * d11 - d01 * d01;
			float v = (d11 * d20 - d01 * d21) / denom;
			float w = (d00 * d21 - d01 * d20) / denom;
			float u = 1.0f - v - w;
			return new Vector3(u, v, w);
		}

		public static Vector3 getBaryCentricDirectionFromCatesianDirection(Vector3 direction, Vector3 a, Vector3 b, Vector3 c)
		{
			Vector3 v0 = b - a, v1 = c - a, v2 = direction;
			float d00 = Vector3.Dot(v0, v0);
			float d01 = Vector3.Dot(v0, v1);
			float d11 = Vector3.Dot(v1, v1);
			float d20 = Vector3.Dot(v2, v0);
			float d21 = Vector3.Dot(v2, v1);
			float denom = d00 * d11 - d01 * d01;
			float dv = (d11 * d20 - d01 * d21) / denom;
			float dw = (d00 * d21 - d01 * d20) / denom;
			float du = -dv - dw;
			return new Vector3(du, dv, dw);
		}

		public static Vector3 getShortestOrientationFrom2Directions(Vector3 unitDirectionBefore,Vector3 unitDirectionAfter)
		{
			return Vector3.Cross(unitDirectionBefore,unitDirectionAfter).Normalized() * Math.Abs(Vector3.CalculateAngle(unitDirectionBefore,unitDirectionAfter));
		}

		public static Matrix3 extractFromMatrix4(Matrix4 m)	//it works, do not change
		{
			return new Matrix3(m[0,0],m[0,1],m[0,2],m[1,0],m[1,1],m[1,2],m[2,0],m[2,1],m[2,2]);
			//return new Matrix3(m[0,0],m[1,0],m[2,0],m[0,1],m[1,1],m[2,1],m[0,2],m[1,2],m[2,2]);
		}

		public static Vector3 getVirtualGyroFrom2Orientation(Quaternion beforeQ,Quaternion afterQ,double timeBefore,double timeAfter)
		{
			//one will take shorter path, one will take longer path
			Quaternion qDiff1 = MyMath.quaternionMultiplySequence(afterQ.Inverted(), beforeQ);
			Quaternion qDiff2 = MyMath.quaternionMultiplySequence(afterQ.Inverted(), beforeQ*(-1));

			//quaternion can take both long and short path, so we do both and pick smaller angle
			OpenTK.Vector4 axisAngle1 = qDiff1.ToAxisAngle();
			OpenTK.Vector4 axisAngle2 = qDiff2.ToAxisAngle();
			OpenTK.Vector4 axisAngle;
			if (Math.Abs(axisAngle1.W) < Math.Abs(axisAngle2.W))
				axisAngle = axisAngle1;
			else
				axisAngle = axisAngle2;

			return axisAngle.Xyz * (axisAngle.W * 180 / (float)Math.PI * 1000 / (float)(timeAfter - timeBefore));
		}

		//https://www.geometrictools.com/Documentation/LeastSquaresFitting.pdf
		public static bool fitSphere(List<Vector3> points, out Vector3 center, out float radius)
		{
			double[,] A = new double[5,5];
			foreach(Vector3 e in points)
			{
				double x = e.X;
				double y = e.Y;
				double z = e.Z;
				double x2 = x*x;
				double y2 = y*y;
				double z2 = z*z;
				double xy = x*y;
				double xz = x*z;
				double yz = y*z;
				double r2 = x2 + y2 + z2;
				double xr2 = x*r2;
				double yr2 = y*r2;
				double zr2 = z*r2;
				double r4 = r2*r2;

				A[0, 1] += x;
				A[0, 2] += y;
				A[0, 3] += z;
				A[0, 4] += r2;
				A[1, 1] += x2;
				A[1, 2] += xy;
				A[1, 3] += xz;
				A[1, 4] += xr2;
				A[2, 2] += y2;
				A[2, 3] += yz;
				A[2, 4] += yr2;
				A[3, 3] += z2;
				A[3, 4] += zr2;
				A[4, 4] += r4;
			}

			A[0, 0] = points.Count;

			for (int row = 0; row < 5; ++row)
			{
				for (int col = 0; col < row; ++col)
				{
					A[row, col] = A[col, row];
				}
			}

			for (int row = 0; row < 5; ++row)
			{
				for (int col = 0; col < 5; ++col)
				{
					A[row, col] /= points.Count;
				}
			}

			double[][] eigenVectors = new double[5][];
			var eigenValues = StarMathLib.StarMath.GetEigenValuesAndVectors(A, out eigenVectors);
			
			//find index of the minimum eigenValues
			int minIndex=0;
			for (int i=1;i<5;i++)
			{
				if(eigenValues[0][i]<eigenValues[0][minIndex])
					minIndex=i;
			}

			double[] coefficients = new double[5] {
					eigenVectors[minIndex][0]/eigenVectors[minIndex][4],
					eigenVectors[minIndex][1]/eigenVectors[minIndex][4],
					eigenVectors[minIndex][2]/eigenVectors[minIndex][4],
					eigenVectors[minIndex][3]/eigenVectors[minIndex][4],
					eigenVectors[minIndex][4]/eigenVectors[minIndex][4]
			};

			center = new Vector3(
				(float)(-coefficients[1]/2),
				(float)(-coefficients[2]/2),
				(float)(-coefficients[3]/2)
			);

			radius = (float)Math.Sqrt(Math.Abs(Vector3.Dot(center,center)-coefficients[0]));
			return true;
		}

		public static double RMS(List<double> x)
		{
			double sum = 0;
			foreach(double e in x)
			{
				sum+=e*e;
			}
			return Math.Sqrt(sum/x.Count);
		}
	}
}
