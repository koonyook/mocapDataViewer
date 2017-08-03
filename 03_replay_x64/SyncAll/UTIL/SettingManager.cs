using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace SyncAll
{
    public static class SettingManager
    {
        public static void saveRotationalOffset(string filename, Quaternion KinectBaseQuaternion)
        {
            using (BinaryWriter binWriter = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {
                binWriter.Write(KinectBaseQuaternion.X);
                binWriter.Write(KinectBaseQuaternion.Y);
                binWriter.Write(KinectBaseQuaternion.Z);
                binWriter.Write(KinectBaseQuaternion.W);
            }
        }

		public static void saveVector3(string filename, Vector3 v)
		{
			using (BinaryWriter binWriter = new BinaryWriter(File.Open(filename, FileMode.Create)))
			{
				binWriter.Write(v.X);
				binWriter.Write(v.Y);
				binWriter.Write(v.Z);
			}
		}

		public static Vector3 loadVector3(string filename)
		{
			BinaryReader binReader = new BinaryReader(File.Open(filename, FileMode.Open));
			try
			{

				// If the file is not empty, 
				// read the application settings.

				//if (binReader.PeekChar() != -1)
				{
					return new Vector3(binReader.ReadSingle(), binReader.ReadSingle(), binReader.ReadSingle());

					//lookupDir = binReader.ReadString();
					//autoSaveTime = binReader.ReadInt32();
					//showStatusBar = binReader.ReadBoolean();
				}

			}

			// If the end of the stream is reached before reading
			// the four data values, ignore the error and use the
			// default settings for the remaining values.
			catch (EndOfStreamException e)
			{
				Console.WriteLine("{0} caught and ignored. " +
					"Using default values.", e.GetType().Name);

				return new Vector3(0, 0, 0);
			}
			finally
			{
				binReader.Close();
			}
		}

		public static void savePanelParameter(string filename, Quaternion KinectPanelQuaternion, Vector3 panelOriginInKinectFrame)
		{
			using (BinaryWriter binWriter = new BinaryWriter(File.Open(filename, FileMode.Create)))
			{
				binWriter.Write(KinectPanelQuaternion.X);
				binWriter.Write(KinectPanelQuaternion.Y);
				binWriter.Write(KinectPanelQuaternion.Z);
				binWriter.Write(KinectPanelQuaternion.W);

				binWriter.Write(panelOriginInKinectFrame.X);
				binWriter.Write(panelOriginInKinectFrame.Y);
				binWriter.Write(panelOriginInKinectFrame.Z);
			}

			Debug.WriteLine("SAVE: "+filename);
			//Global.latestPanelFilename = filename;
		}

		public static void loadPanelParameter(string filename, out Quaternion kinect_panel_q, out Vector3 panelOriginInKinectFrame)
		{
			BinaryReader binReader = new BinaryReader(File.Open(filename, FileMode.Open));
			try
			{
				kinect_panel_q = new Quaternion(binReader.ReadSingle(),binReader.ReadSingle(),binReader.ReadSingle(),binReader.ReadSingle());
				panelOriginInKinectFrame = new Vector3(binReader.ReadSingle(), binReader.ReadSingle(), binReader.ReadSingle());

				//Debug.WriteLine(kinectPanelQuaternion);
				//Debug.WriteLine(panelOriginInKinectFrame);
			}
			catch (EndOfStreamException e)
			{
				Console.WriteLine("{0} caught and ignored. " +
					"Using default values.", e.GetType().Name);

				kinect_panel_q = new Quaternion(0, 0, 0, 1);
				panelOriginInKinectFrame = new Vector3(0);
			}
			finally
			{
				binReader.Close();
			}
		}

		public static double loadOneDoubleFromFile(string filename)
		{
			BinaryReader binReader = new BinaryReader(File.Open(filename, FileMode.Open));
			try
			{
				return binReader.ReadDouble();
			}
			catch (EndOfStreamException e)
			{
				Console.WriteLine("{0} caught and ignored. " +
					"Using default values.", e.GetType().Name);

				return 0;
			}
			finally
			{
				binReader.Close();
			}
		}

		public static void saveOneDoubleToFile(string filename, double value)
		{
			using (BinaryWriter binWriter = new BinaryWriter(File.Open(filename, FileMode.Create)))
			{
				binWriter.Write(value);
			}

			Debug.WriteLine("SAVE: "+filename);
		}

		public static void saveOneStringToFile(string filename, string content)
		{
			File.WriteAllText(filename,content);
		}

		public static string loadOneStringFromFile(string filename)
		{
			return File.ReadAllText(filename);
		}

		//KinectBaseQuaternion
		public static Quaternion loadRotationalOffset(string filename)
        {
            float x = 0, y = 0, z = 0, w = 0;
            BinaryReader binReader = new BinaryReader(File.Open(filename, FileMode.Open));
            try
            {

                // If the file is not empty, 
                // read the application settings.

                //if (binReader.PeekChar() != -1)
                {
                    x = binReader.ReadSingle();
                    y = binReader.ReadSingle();
                    z = binReader.ReadSingle();
                    w = binReader.ReadSingle();

                    //lookupDir = binReader.ReadString();
                    //autoSaveTime = binReader.ReadInt32();
                    //showStatusBar = binReader.ReadBoolean();
                }

            }

            // If the end of the stream is reached before reading
            // the four data values, ignore the error and use the
            // default settings for the remaining values.
            catch (EndOfStreamException e)
            {
                Console.WriteLine("{0} caught and ignored. " +
                    "Using default values.", e.GetType().Name);
            }
            finally
            {
                binReader.Close();
            }

            return new Quaternion(x, y, z, w);
        }


		public static void saveMagneticCalibrationToFile(string filename, float[] centerXYZ, float[,] transformationMatrix)
		{
			using (BinaryWriter binWriter = new BinaryWriter(File.Open(filename, FileMode.Create)))
			{
				binWriter.Write(centerXYZ[0]);
				binWriter.Write(centerXYZ[1]);
				binWriter.Write(centerXYZ[2]);

				binWriter.Write(transformationMatrix[0, 0]);
				binWriter.Write(transformationMatrix[0, 1]);
				binWriter.Write(transformationMatrix[0, 2]);
				binWriter.Write(transformationMatrix[1, 0]);
				binWriter.Write(transformationMatrix[1, 1]);
				binWriter.Write(transformationMatrix[1, 2]);
				binWriter.Write(transformationMatrix[2, 0]);
				binWriter.Write(transformationMatrix[2, 1]);
				binWriter.Write(transformationMatrix[2, 2]);
			}

			Debug.WriteLine("SAVE: " + filename);
			//Global.latestPanelFilename = filename;
		}

		/// <summary>
		/// read magnetometer calibration setting from a file
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="centerXYZ"></param>
		/// <param name="transformationMatrix"></param>
		/// <returns></returns>
		public static void loadMagneticCalibrationFromFile(string filename, ref float[] centerXYZ, ref float[,] transformationMatrix)
        {
            
            BinaryReader binReader = new BinaryReader(File.Open(filename, FileMode.Open));
            try
            {
                // If the file is not empty, 
                // read the application settings.

                //if (binReader.PeekChar() != -1)
                {
                    centerXYZ[0] = binReader.ReadSingle();
                    centerXYZ[1] = binReader.ReadSingle();
                    centerXYZ[2] = binReader.ReadSingle();

                    transformationMatrix[0,0] = binReader.ReadSingle();
                    transformationMatrix[0,1] = binReader.ReadSingle();
                    transformationMatrix[0,2] = binReader.ReadSingle();
                    transformationMatrix[1,0] = binReader.ReadSingle();
                    transformationMatrix[1,1] = binReader.ReadSingle();
                    transformationMatrix[1,2] = binReader.ReadSingle();
                    transformationMatrix[2,0] = binReader.ReadSingle();
                    transformationMatrix[2,1] = binReader.ReadSingle();
                    transformationMatrix[2,2] = binReader.ReadSingle();

                }

            }

            // If the end of the stream is reached before reading
            // the all data values, ignore the error and use the
            // default settings for the remaining values.
            catch (EndOfStreamException e)
            {
                Console.WriteLine("{0} caught and ignored. " +
                    "Using default values.", e.GetType().Name);
            }
            finally
            {
                binReader.Close();
            }
        }

		/* something like this in subject.measure or marker.measure text file
		shoulderOffsetNoMarker=50.5
		elbowWidth=65
		wristThickness=40
		handThickness=26
		*/
		public static Dictionary<string,float> readDotMeasureFile(string filename)	//something like "koon.measurement"
		{
			Dictionary<string,float> param = new Dictionary<string, float>();
			string line;

			// Read the file and display it line by line.
			System.IO.StreamReader file = new System.IO.StreamReader(filename);
			while((line = file.ReadLine()) != null)
			{
				string[] s=line.Split(new char[] {'='});
				param[s[0]]=float.Parse(s[1])/1000;	//mm to m
			}
			file.Close();
			return param;
		}

		
		public static Tuple<float[],char> readDotBetaFile(string filename)
		{
			System.IO.StreamReader file = new System.IO.StreamReader(filename);
			string line1=file.ReadLine();
			line1=line1.Replace("f","").Replace(" ","");

			var s=line1.Split(new char[] {','});
			float[] beta=new float[10];
			for(int i=0;i<10;i++)
			{
				beta[i]=float.Parse(s[i]);
			}
			string line2=file.ReadLine();
			file.Close();
			return new Tuple<float[], char>(beta,line2[0]);
		}
		
    }
}
