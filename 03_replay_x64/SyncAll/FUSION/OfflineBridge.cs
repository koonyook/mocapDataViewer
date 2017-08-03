using OpenTK;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

using Microsoft.Kinect;
using System.Diagnostics;
using Google.Protobuf.MocapRecord;
using Google.Protobuf.PoseFitting;
using System.Linq;

namespace SyncAll
{
    public class OfflineBridge
    {
		public string folder;
		private string folderKinect, folderEagle, folderVicon;
		private Dictionary<byte, string> folderIMU = new Dictionary<byte, string>();

		//public ColorRecordMode colorMode = ColorRecordMode.none;
		public bool irMode = false;

		public ViconBridge vBridge;

		public Dictionary<byte, EndDevice> deviceMap = new Dictionary<byte, EndDevice>();
		public byte baseID = 0;
		public Quaternion kinectBaseQuaternion;

		public Dictionary<byte, List<RawSample>> imuRaw = new Dictionary<byte, List<RawSample>>();
		public Dictionary<byte, List<ComputedSample>> imuCompute = new Dictionary<byte, List<ComputedSample>>();

		public int visualCount = 0;
		public double[] timestamp;
		//public List<OfflineBodyFrame> bodyData;
		public List<MyBodyFrame> bodyData;

		//public Dictionary<string, object> setting;
		public SettingProto settingProto;

		public ulong targetTrackingId = 0;
		int firstValidIndex;
		int lastValidIndex;

		public float avgRightForearmLength;
		public float avgRightUpperArmLength;
		
		public ExtractedSample[] exSample;


		//this might be replaced with pre-session measurement
		public static Quaternion rightImu_rightForearm_quaternion = Quaternion.FromAxisAngle(MyMath.unitY, -(float)(Math.PI / 2));

		public SessionProto fittingResult=null;
		//public SmoothFittingFrame[] smoothResult;
		//public CS_Renderer renderer;

		public MemoryMappedFile memMapDepth=null, memMapBodyIndex=null;

		public OfflineBridge(string folder,string subjectDatabase)
		{
			this.folder = folder;
			folderKinect = folder + "kinect/";
			folderVicon = folder + "vicon/";

			//folderEagle should contain
			//kinectPanelFile.bin (use 37_panel_frame_detection to generate file and copy to this folder)	//SettingManager.loadPanelParameter
			//eaglePanelFile.trb  (just a short record of panel with label)	//Eagle.loadPanelParameter
			//eagleRecordFile.trb (full record) //new TRBreader()


			//read setting
			//setting = MySerializer.load<Dictionary<string, object>>(folder + "setting.serial");

			//tempolary fix
			//SerialToProtoConverter.convertSettingAndSave(setting,folder);

			settingProto = ProtoTool.loadSettingProto(folder+"setting.SettingProto");

			//colorMode = (ColorRecordMode)settingProto.ColorMode;
			irMode = settingProto.IrMode;
			baseID = (byte)settingProto.BaseID;

			if(settingProto.MobileImuSamplingRate<=0)	//for old record
				settingProto.MobileImuSamplingRate=80;	//default sampling rate

			//colorMode = (ColorRecordMode)setting["colorMode"];
			//irMode = (bool)setting["irMode"];
			//baseID = (byte)setting["baseID"];

			memMapDepth = MemoryMappedFile.CreateFromFile(folderKinect + "depthBuffer.bin"); //, FileMode.Open, "depthFrame");				//not tested yet
			memMapBodyIndex = MemoryMappedFile.CreateFromFile(folderKinect + "bodyIndexBuffer.bin"); //, FileMode.Open, "bodyIndexFrame");	//not tested yet

			//long minFileSize = long.MaxValue;
			//search for imu folder
			if (baseID != 0)
			{
				foreach (string s in Directory.GetDirectories(folder, "imu*", SearchOption.TopDirectoryOnly))
				{
					int targetIndex = s.LastIndexOf("imu") + 3;
					byte id = byte.Parse(s.Substring(targetIndex));
					folderIMU[id] = folder + "imu" + id + "/";
				}
			
				foreach (byte id in folderIMU.Keys)
				{
					if (id == baseID) deviceMap[id] = new EndDevice(id, 0);
					else deviceMap[id] = new EndDevice(id, 1);

					deviceMap[id].loadMagCalParamsFromFile(folderIMU[id] + "magCalibrationParameters.bin");
					deviceMap[id].gyroBiasTracker.loadBiasStarterFromFile(folderIMU[id] + "gyroBiasStarter.bin");

					//imuRaw[id] = MySerializer.load<List<RawSample>>(folderIMU[id] + "RawSample.serial");

					//tempolary fix
					//SerialToProtoConverter.convertRawSampleListAndSave(imuRaw[id],folderIMU[id]);
					var loaded = ProtoTool.loadRawSampleListProto(folderIMU[id]+"RawSample.RawSampleListProto");
					imuRaw[id] = ProtoTool.convertRawSampleListProtoToRawSampleList(loaded);
				}
			
				kinectBaseQuaternion = SettingManager.loadRotationalOffset(folder + "kinectBaseQuaternion.bin");
			}

			OfflineCoordinateMapper.initForOfflineUsage(folderKinect);

			//bodyData = MySerializer.load<List<OfflineBodyFrame>>(folderKinect + "OfflineBodyframe.serial");
			bodyData = (new BodyFrameCollector()).load(folderKinect + "OfflineBodyframe.protobuf");
			//using (var file = File.OpenRead(folderKinect + "OfflineBodyframe.protobuf"))
			//{
			//	bodyData = Serializer.Deserialize<List<MyBodyFrame>>(file);
			//}

			//read timestamp
			using (BinaryReader b = new BinaryReader(File.OpenRead(folderKinect + "timestamp.bin")))
			{
				visualCount = (int)b.BaseStream.Length / 8;
				timestamp = new double[visualCount];

				for (int i = 0; i < visualCount; i++)
					timestamp[i] = b.ReadDouble();
			}

			string[] resultFiles = System.IO.Directory.GetFiles(folder, "result*.SessionProto", System.IO.SearchOption.TopDirectoryOnly);
			//if(File.Exists(folder+"result.SessionProto"))
			if(resultFiles.Length>0)
			{ 

				//read result from poseFitting
				fittingResult=ProtoTool.loadSessionProto(resultFiles[resultFiles.Length-1]);	//"result.SessionProto");
				//renderer=new CS_Renderer(coreModel,ProtoTool.getFloatArrayFromRepeatedField(fittingResult.Beta));
			}
			else
			{
				//renderer=new CS_Renderer(coreModel,new float[10] { 0,0,0,0,0,0,0,0,0,0});
			}


			//if (File.Exists(folderVicon+"viconRecordFile.TRC"))
			if(Directory.EnumerateFiles(folderVicon, "*.TRC").Any())
			{
				vBridge = new ViconBridge(folder,subjectDatabase,this);
			}

			//calculate dynamic stuff here
			if (baseID != 0)
			{
				preCompute();	//get imuCompute
				findTimeScopeAndNearestBody();	//get targetTrackingID firstValidIndex, lastValidIndex
				//visualJointEnhancement();
				//extractAllMetrics();

				if(fittingResult!=null)
				{
					//smoothResult=smoothFittingResult(fittingResult,1);	//wing=0 to see original result
					//extractAllMetricsFromIMUandFittingResult();
				}
			}
			else	//no imu
			{
				targetTrackingId=findProperTrackingID();

				if(fittingResult!=null)
				{
					//smoothResult=smoothFittingResult(fittingResult,1);
				}
			}
		}

		public Vector3 getRightForearmPointingDirectionFromIMU(int visualIndex)
		{
			Dictionary<byte, int> nearestImuIndex = searchForNearestImuIndex(timestamp[visualIndex]);

			byte rightID=(byte)settingProto.RightID;
			if(imuCompute.ContainsKey(rightID))
			{
				Quaternion kinectRightImuOriginalQuaternion = MyMath.quaternionMultiplySequence(
					kinectBaseQuaternion,
					imuCompute[baseID][nearestImuIndex[baseID]].MagneticSensorQuaternion.Inverted(),
					imuCompute[rightID][nearestImuIndex[rightID]].MagneticSensorQuaternion
					//bridge.imuRaw[rightID][nearestImuIndex[rightID]].orientation
				);

				return MyMath.rotateVector(kinectRightImuOriginalQuaternion, Vector3.UnitY);	//use Y axis for right IMU
			}
			else
			{
				return new Vector3(0,0,0);
			}
		}

		public Vector3 getLeftForearmPointingDirectionFromIMU(int visualIndex)
		{
			Dictionary<byte, int> nearestImuIndex = searchForNearestImuIndex(timestamp[visualIndex]);

			byte leftID=(byte)settingProto.LeftID;
			if(imuCompute.ContainsKey(leftID))
			{
				Quaternion kinectLeftImuOriginalQuaternion = MyMath.quaternionMultiplySequence(
					kinectBaseQuaternion,
					imuCompute[baseID][nearestImuIndex[baseID]].MagneticSensorQuaternion.Inverted(),
					imuCompute[leftID][nearestImuIndex[leftID]].MagneticSensorQuaternion
					//bridge.imuRaw[leftID][nearestImuIndex[leftID]].orientation
				);

				return MyMath.rotateVector(kinectLeftImuOriginalQuaternion, -Vector3.UnitY);	//use negative Y for left IMU
			}
			else
			{
				return new Vector3(0,0,0);
			}
		}

		

		public void readBodyIndexFrame(Int64 index, ref byte[] targetArray)   //targetArray must have size of [424 * 512]
		{
			if(memMapBodyIndex!=null)
			{
				using (var accessor = memMapBodyIndex.CreateViewAccessor(index * (424 * 512), 424 * 512))
				{
					accessor.ReadArray<byte>(0, targetArray, 0, 424 * 512);
				}
			}
			else
			{ 
				using (var mmf = MemoryMappedFile.CreateFromFile(folderKinect + "bodyIndexBuffer.bin", FileMode.Open, "bodyIndexFrame"))
				{
					using (var accessor = mmf.CreateViewAccessor(index * (424 * 512), 424 * 512))
					{
						accessor.ReadArray<byte>(0, targetArray, 0, 424 * 512);
					}
				}
			}
		}

		public void readIrFrameInUshort(Int64 index, ref ushort[] targetArray)   //targetArray must have size of [424 * 512]
		{
			if (irMode == true)
			{
				using (var mmf = MemoryMappedFile.CreateFromFile(folderKinect + "irBuffer.bin", FileMode.Open, "irFrame"))
				{
					using (var accessor = mmf.CreateViewAccessor(index * (2 * 424 * 512), 2 * 424 * 512))
					{
						accessor.ReadArray<ushort>(0, targetArray, 0, 424 * 512);
					}
				}
			}
		}

		public void readIrFrameInByte(Int64 index, ref byte[] targetArray)   //targetArray must have size of [424 * 512*2]
		{
			if (irMode == true)
			{
				using (var mmf = MemoryMappedFile.CreateFromFile(folderKinect + "irBuffer.bin", FileMode.Open, "irFrame"))
				{
					using (var accessor = mmf.CreateViewAccessor(index * (2 * 424 * 512), 2 * 424 * 512))
					{
						accessor.ReadArray<byte>(0, targetArray, 0, 424 * 512*2);
					}
				}
			}
		}
		public void sendIrFrame(int index, WriteableBitmap irBitmap)
		{
			byte[] tmpArray = new byte[2 * 424 * 512];
			readIrFrameInByte(index, ref tmpArray);
			irBitmap.Lock();
			Marshal.Copy(tmpArray, 0, irBitmap.BackBuffer, 2 *424 * 512);
			//Marshal.Copy(tmpArray, 0, irBitmap.BackBuffer, 424 * 512);

			irBitmap.AddDirtyRect(new Int32Rect(0, 0, 512, 424));
			irBitmap.Unlock();
		}

		public void loadDepthFrame(Int64 frameIndex, ushort[] targetArray)
		{
			//target array must be ushort[424 * 512]
			if(memMapDepth!=null)
			{
				using (var accessor = memMapDepth.CreateViewAccessor(frameIndex * (2 * 424 * 512), 2 * 424 * 512))
				{
					accessor.ReadArray<ushort>(0, targetArray, 0, 424 * 512);
				}
			}
			else
			{
				using (var mmf = MemoryMappedFile.CreateFromFile(folderKinect + "depthBuffer.bin", FileMode.Open, "depthFrame"))
				{
					using (var accessor = mmf.CreateViewAccessor(frameIndex * (2 * 424 * 512), 2 * 424 * 512))
					{
						accessor.ReadArray<ushort>(0, targetArray, 0, 424 * 512);
					}
				}
			}
		}

		public void sendDepthFrame(int index, IntPtr targetIntPtr)
		{
			ushort[] tmp = new ushort[424 * 512];
			loadDepthFrame(index, tmp);
			/*
			using (var mmf = MemoryMappedFile.CreateFromFile(folderKinect + "depthBuffer.bin", FileMode.Open, "depthFrame"))
			{
				using (var accessor = mmf.CreateViewAccessor(index * (2 * 424 * 512), 2 * 424 * 512))
				{
					accessor.ReadArray<ushort>(0, tmp, 0, 424 * 512);
				}
			}
			*/
			OfflineCoordinateMapper.MapDepthFrameToCameraSpaceUsingIntPtr(tmp, targetIntPtr);
		}
		

		

		bool fixMagnetometerOffset=false;	//can only apply to mobile IMU (base IMU is too still)

		public void preCompute()	//can be called after all raw data are loaded to calculate imuCompute
		{
			//calculate magneticSensorQuaternion for baseDevice
			imuCompute[baseID] = new List<ComputedSample>();
			EndDevice baseDevice = deviceMap[baseID];

			foreach (RawSample r in imuRaw[baseID])
			{
				ComputedSample c = new ComputedSample();
				c.ktime = r.ktime;
				//c.gyro = baseDevice.transformRawGyro(r.gyro);	//not used
				c.acc = baseDevice.transformRawAcc(r.acc);
				c.mag = baseDevice.transformRawMag(r.mag);

				c.MagneticSensorQuaternion = baseDevice.baseOrientationTracker.update(c.acc, c.mag);

				imuCompute[baseID].Add(c);
			}

			//calculate gyroBias and Kalman filter for each mobile device
			foreach(byte id in imuRaw.Keys)
			{
				if(id!=baseID)
				{
					Vector3 magCenter=new Vector3(0);
					if(fixMagnetometerOffset)
					{
						List<Vector3> points = new List<Vector3>();
						foreach (RawSample r in imuRaw[id])
						{
							points.Add(deviceMap[id].transformRawMag(r.mag));
						}

						float radius;
						MyMath.fitSphere(points,out magCenter,out radius);
					}

					imuCompute[id] = new List<ComputedSample>();

					deviceMap[id].gyroBiasTracker.reset();
					deviceMap[id].gyroBiasTracker.loadBiasStarterFromFile(folderIMU[id] + "gyroBiasStarter.bin");
					deviceMap[id].kalmanFilter.initialize(settingProto.MobileImuSamplingRate); //TODO: make it better
					foreach (RawSample r in imuRaw[id])
					{
						ComputedSample c = new ComputedSample();

						deviceMap[id].gyroBiasTracker.updateBias(r.gyro);

						c.ktime = r.ktime;
						//c.gyro = deviceMap[id].transformRawGyro(r.gyro);	//from raw data
						c.gyro = deviceMap[id].transformRawGyro(r.gyro,r.gyroBias);	//similar to record
						c.acc = deviceMap[id].transformRawAcc(r.acc);
						c.mag = deviceMap[id].transformRawMag(r.mag)-magCenter;

						deviceMap[id].kalmanFilter.filterUpdate(c.gyro, c.acc, c.mag);
						c.MagneticSensorQuaternion = deviceMap[id].kalmanFilter.getMagneticSensorQuaternion();

						//c.MagneticSensorQuaternion = deviceMap[id].quickMagneticSensorQuaternion(r);	//for debugging
						
						imuCompute[id].Add(c);
					}
				}
			}
		}

		public ulong findProperTrackingID()
		{
			//get the right trackingID (body that are close to the camera for a longer period of time)
			Dictionary<ulong, int> trackingID_count = new Dictionary<ulong, int>();
			for (int i = 0; i < visualCount; i++)
			{
				MyBody nearestBody = bodyData[i].getNearestBodyToKinect();
				if (nearestBody != null)
				{
					if (trackingID_count.ContainsKey(nearestBody.TrackingId))
						trackingID_count[nearestBody.TrackingId]++;
					else
						trackingID_count[nearestBody.TrackingId] = 0;
				}
			}

			ulong targetTrackingId = 0;
			foreach (ulong key in trackingID_count.Keys)
			{
				if (targetTrackingId == 0 || trackingID_count[key] > trackingID_count[targetTrackingId])
					targetTrackingId = key;
			}

			return targetTrackingId;
		}

		public void findTimeScopeAndNearestBody()
		{
			exSample = new ExtractedSample[visualCount];

			//call this before running visualJointEnhancement and extractAllMetric

			//byte rightID = (byte)setting["rightID"];
			byte rightID = (byte)settingProto.RightID;

			targetTrackingId=findProperTrackingID();

			//create a scope by checking the overlapping timestamp
			double startTimeIMU = Math.Max(imuCompute[rightID][0].ktime, imuCompute[baseID][0].ktime);
			double stopTimeIMU = Math.Min(imuCompute[rightID][imuCompute[rightID].Count - 1].ktime, imuCompute[baseID][imuCompute[baseID].Count - 1].ktime);

			double startTimeVisual = timestamp[0];
			double stopTimeVisual = timestamp[visualCount - 1];

			firstValidIndex = 0;
			lastValidIndex = visualCount - 1;
			//find first visual timestamp that is 1.contain that body 2.greater than startTimeIMU
			while (timestamp[firstValidIndex] < startTimeIMU || bodyData[firstValidIndex].getBodyByTrackingID(targetTrackingId) == null)
			{
				exSample[firstValidIndex].valid = false;
				firstValidIndex++;
			}
			//find last visual timestamp that is lesser than stopTimeIMU
			while (timestamp[lastValidIndex] > stopTimeIMU || bodyData[lastValidIndex].getBodyByTrackingID(targetTrackingId) == null)
			{
				exSample[lastValidIndex].valid = false;
				lastValidIndex--;
			}

			//from now on, I should process data from firstValidIndex to lastValidIndex
		}

		private bool isReliablePoint(float pointDepth, float surfaceDepth)
		{
			if (surfaceDepth == 0 || pointDepth < surfaceDepth || pointDepth - surfaceDepth > 0.15f)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public EnhanceBody[] enhance;
		public void visualJointEnhancement()
		{
			int availableIndex;

			//input: everything from visual data
			//todo: delete joint positions that are likely to be wrong and interpolate 
			enhance = new EnhanceBody[visualCount];
			for (int i = 0; i < visualCount; i++)
				enhance[i].clear();

			//round1: copy and apply 2 general rules about distance from the surface
			//joint position that is too far behind the point cloud surface is being occluded (unreliable)
			for (int i = firstValidIndex; i <= lastValidIndex; i++)
			{
				//load up the depth frame
				ushort[] depthFrame = new ushort[424 * 512];
				loadDepthFrame(i, depthFrame);

				//get the body
				MyBody body = bodyData[i].getBodyByTrackingID(targetTrackingId);
				if (body == null)
				{
					enhance[i].clear();  //just set everything void
				}
				else
				{
					//check each part one-by-one

					//spineShoulder
					if (body.Joints[JointType.SpineShoulder].TrackingState == TrackingState.Tracked)
					{
						Vector3 p = body.Joints[JointType.SpineShoulder].Position.getVector3();
						float surfaceDepth = OfflineCoordinateMapper.pointCloudSurfaceDepth(depthFrame, p);

						if (isReliablePoint(p.Z, surfaceDepth))
						{
							//first pass, just copy
							enhance[i].spineShoulder.isVoid = false;
							enhance[i].spineShoulder.position = p;
							enhance[i].spineShoulder.orientation = body.JointOrientations[JointType.SpineShoulder].getQuaternion();
						}
						else
						{
							//do not pass
							enhance[i].spineShoulder.isVoid = true;
							//Debug.WriteLine("point:{0} , surface:{1}", p.Z, surfaceDepth);
						}
					}
					else
					{
						enhance[i].spineShoulder.isVoid = true;
					}

					//shoulderLeft
					if (body.Joints[JointType.ShoulderLeft].TrackingState == TrackingState.Tracked)
					{
						Vector3 p = body.Joints[JointType.ShoulderLeft].Position.getVector3();
						float surfaceDepth = OfflineCoordinateMapper.pointCloudSurfaceDepth(depthFrame, p);

						if (isReliablePoint(p.Z, surfaceDepth))
						{
							//first pass, just copy
							enhance[i].shoulderLeft.isVoid = false;
							enhance[i].shoulderLeft.position = p;
						}
						else
						{
							//do not pass
							enhance[i].shoulderLeft.isVoid = true;
							
						}
					}
					else
					{
						enhance[i].shoulderLeft.isVoid = true;
					}

					//shoulderRight
					if (body.Joints[JointType.ShoulderRight].TrackingState == TrackingState.Tracked)
					{
						Vector3 p = body.Joints[JointType.ShoulderRight].Position.getVector3();
						float surfaceDepth = OfflineCoordinateMapper.pointCloudSurfaceDepth(depthFrame, p);

						if (isReliablePoint(p.Z, surfaceDepth))
						{
							//first pass, just copy
							enhance[i].shoulderRight.isVoid = false;
							enhance[i].shoulderRight.position = p;
							
						}
						else
						{
							//do not pass
							enhance[i].shoulderRight.isVoid = true;
						}
					}
					else
					{
						enhance[i].shoulderRight.isVoid = true;
					}

					//elbowRight
					if (body.Joints[JointType.ElbowRight].TrackingState == TrackingState.Tracked)
					{
						Vector3 p = body.Joints[JointType.ElbowRight].Position.getVector3();
						float surfaceDepth = OfflineCoordinateMapper.pointCloudSurfaceDepth(depthFrame, p);

						if (isReliablePoint(p.Z, surfaceDepth))
						{
							//first pass, just copy
							enhance[i].elbowRight.isVoid = false;
							enhance[i].elbowRight.position = p;
						}
						else
						{
							//do not pass
							enhance[i].elbowRight.isVoid = true;
						}
					}
					else
					{
						enhance[i].elbowRight.isVoid = true;
					}

					//wristRight
					if (body.Joints[JointType.WristRight].TrackingState == TrackingState.Tracked)
					{
						Vector3 p = body.Joints[JointType.WristRight].Position.getVector3();
						float surfaceDepth = OfflineCoordinateMapper.pointCloudSurfaceDepth(depthFrame, p);

						if (isReliablePoint(p.Z, surfaceDepth))
						{
							//first pass, just copy
							enhance[i].wristRight.isVoid = false;
							enhance[i].wristRight.position = p;
						}
						else
						{
							//do not pass
							enhance[i].wristRight.isVoid = true;
						}
					}
					else
					{
						enhance[i].wristRight.isVoid = true;
					}
				}
			}//end of the first round

			//round2: 
			//2.1: spineShoulder single frame jumping
			List<int> toVoid = new List<int>();
			for (int i = firstValidIndex; i <= lastValidIndex; i++)
			{
				if (!enhance[i - 1].spineShoulder.isVoid && !enhance[i].spineShoulder.isVoid && !enhance[i + 1].spineShoulder.isVoid)
				{
					//try to detect a jump in the middle and just remove

					//check a jump for position
					float AB_distance = (enhance[i].spineShoulder.position - enhance[i - 1].spineShoulder.position).Length;
					float BC_distance = (enhance[i + 1].spineShoulder.position - enhance[i].spineShoulder.position).Length;
					float AC_distance = (enhance[i + 1].spineShoulder.position - enhance[i - 1].spineShoulder.position).Length;

					//check a jump for orientation
					float AB_angle = MyMath.calculateOrientationDifferenceInOneRotationAngle(enhance[i].spineShoulder.orientation, enhance[i - 1].spineShoulder.orientation);
					float BC_angle = MyMath.calculateOrientationDifferenceInOneRotationAngle(enhance[i].spineShoulder.orientation, enhance[i + 1].spineShoulder.orientation);
					float AC_angle = MyMath.calculateOrientationDifferenceInOneRotationAngle(enhance[i + 1].spineShoulder.orientation, enhance[i - 1].spineShoulder.orientation);

					if ((AC_distance < AB_distance && AC_distance < BC_distance)|| (AC_angle < AB_angle && AC_angle < BC_angle) )
					{
						toVoid.Add(i);
					}
				}
			}
			foreach (int i in toVoid)
			{
				enhance[i].spineShoulder.isVoid = true;
			}

			//2.2: elbowRight single frame jumping
			toVoid = new List<int>();
			for (int i = firstValidIndex; i <= lastValidIndex; i++)
			{
				if (!enhance[i - 1].elbowRight.isVoid && !enhance[i].elbowRight.isVoid && !enhance[i + 1].elbowRight.isVoid)
				{
					//try to detect a jump in the middle and just remove

					//check a jump for position
					Vector3 AB = enhance[i].elbowRight.position - enhance[i - 1].elbowRight.position;
					Vector3 BC = enhance[i + 1].elbowRight.position - enhance[i].elbowRight.position;
					Vector3 AC = enhance[i + 1].elbowRight.position - enhance[i - 1].elbowRight.position;

					if (AC.Length < AB.Length && AC.Length < BC.Length)	//jump for sure
					{
						toVoid.Add(i);
					}
					else if(Vector3.CalculateAngle(-AB,BC) < Math.PI/2) //angle is too sharp
					{
						toVoid.Add(i);
					}
				}
			}
			foreach (int i in toVoid)
			{
				enhance[i].elbowRight.isVoid = true;
			}

			//2.3 wrist right single frame jumping
			toVoid = new List<int>();
			for (int i = firstValidIndex; i <= lastValidIndex; i++)
			{
				if (!enhance[i - 1].wristRight.isVoid && !enhance[i].wristRight.isVoid && !enhance[i + 1].wristRight.isVoid)
				{
					//try to detect a jump in the middle and just remove

					//check a jump for position
					Vector3 AB = enhance[i].wristRight.position - enhance[i - 1].wristRight.position;
					Vector3 BC = enhance[i + 1].wristRight.position - enhance[i].wristRight.position;
					Vector3 AC = enhance[i + 1].wristRight.position - enhance[i - 1].wristRight.position;

					if (AC.Length < AB.Length && AC.Length < BC.Length) //jump for sure
					{
						toVoid.Add(i);
					}
					else if (Vector3.CalculateAngle(-AB, BC) < Math.PI / 2) //angle is too sharp
					{
						toVoid.Add(i);
					}
				}
			}
			foreach (int i in toVoid)
			{
				enhance[i].wristRight.isVoid = true;
			}

			//end of round 2

			//round?: interpolate spineShoulder only
			//must do this before the next round, so weird shoulder position can be detected
			availableIndex = -1;
			for (int i = firstValidIndex; i <= lastValidIndex; i++)
			{
				if (!enhance[i].spineShoulder.isVoid)
				{
					if (availableIndex != -1 && availableIndex < i - 1)
					{
						//there is a gap to fill in (linear time interpolation)
						Vector3 beforeP = enhance[availableIndex].spineShoulder.position;
						Quaternion beforeQ = enhance[availableIndex].spineShoulder.orientation;
						Vector3 afterP = enhance[i].spineShoulder.position;
						Quaternion afterQ = enhance[i].spineShoulder.orientation;

						for (int j = availableIndex + 1; j < i; j++)
						{
							enhance[j].spineShoulder.position = MyMath.vector3TimeInterpolate(beforeP, afterP, timestamp[availableIndex], timestamp[j], timestamp[i]);
							enhance[j].spineShoulder.orientation = MyMath.quaternionTimeInterpolate_SLERP(beforeQ, afterQ, timestamp[availableIndex], timestamp[j], timestamp[i]);
							enhance[j].spineShoulder.isInterpolated = true;
						}
					}

					availableIndex = i;
				}
			}
			//end of round?

			//round 3: consider spineShoulder and both shoulders together
			//when a bad thing is detected, at least 2 be removed (one shoulder might be okay) and hope that interpolation will do better job 
			const float clavicleAngleThreshold = 120f;
			const float shoulderHeightThreshold = -0.08f;
			for (int i = firstValidIndex; i <= lastValidIndex; i++)
			{
				//first we always need spineShoulder to detect a bad thing
				if (enhance[i].spineShoulder.haveData())
				{
					Vector3 spineAxis = MyMath.rotateVector(enhance[i].spineShoulder.orientation, MyMath.unitY);

					//check left clavicle direction
					//and projected distance to torso core axis
					if (enhance[i].shoulderLeft.isVoid == false)
					{
						Vector3 p = enhance[i].shoulderLeft.position;
						float clavicleAngle = (float)(Vector3.CalculateAngle(p - enhance[i].spineShoulder.position, spineAxis) * 180 / Math.PI);
						
						Vector3 torso_shoulderPosition = MyMath.rotateVector(enhance[i].spineShoulder.orientation.Inverted(), p - enhance[i].spineShoulder.position);
						float shoulderHeight = torso_shoulderPosition.Y;

						//Debug.WriteLine("{2}: Angle:{0} ,Height:{1}", clavicleAngle, shoulderHeight,i);

						if (clavicleAngle > clavicleAngleThreshold || shoulderHeight < shoulderHeightThreshold)
						{
							//delete both of them
							enhance[i].spineShoulder.isVoid = true;
							enhance[i].spineShoulder.isInterpolated = false;

							enhance[i].shoulderLeft.isVoid = true;
						}
					}

					//check right clavicle direction
					//and projected distance to torso core axis
					if (enhance[i].shoulderRight.isVoid == false)
					{
						Vector3 p = enhance[i].shoulderRight.position;
						float clavicleAngle = (float)(Vector3.CalculateAngle(p - enhance[i].spineShoulder.position, spineAxis) * 180 / Math.PI);

						Vector3 torso_shoulderPosition = MyMath.rotateVector(enhance[i].spineShoulder.orientation.Inverted(), p - enhance[i].spineShoulder.position);
						float shoulderHeight = torso_shoulderPosition.Y;

						Debug.WriteLine("{2}: Angle:{0} ,Height:{1}", clavicleAngle, shoulderHeight,i);

						if (clavicleAngle > clavicleAngleThreshold || shoulderHeight < shoulderHeightThreshold)
						{
							//delete both of them
							enhance[i].spineShoulder.isVoid = true;
							enhance[i].spineShoulder.isInterpolated = false;

							enhance[i].shoulderRight.isVoid = true;
						}
					}
				}
			}//end of round 3

			
			//round 4: interpolate every gap
			//4.1 spineShoulder
			availableIndex = -1;
			for (int i = firstValidIndex; i <= lastValidIndex; i++)
			{
				if(enhance[i].spineShoulder.haveData())
				{
					if (availableIndex != -1 && availableIndex < i - 1)
					{
						//there is a gap to fill in (linear time interpolation)
						Vector3 beforeP = enhance[availableIndex].spineShoulder.position;
						Quaternion beforeQ = enhance[availableIndex].spineShoulder.orientation;
						Vector3 afterP = enhance[i].spineShoulder.position;
						Quaternion afterQ = enhance[i].spineShoulder.orientation;

						for (int j = availableIndex + 1; j < i; j++)
						{
							enhance[j].spineShoulder.position = MyMath.vector3TimeInterpolate(beforeP, afterP, timestamp[availableIndex], timestamp[j], timestamp[i]);
							enhance[j].spineShoulder.orientation = MyMath.quaternionTimeInterpolate_SLERP(beforeQ, afterQ, timestamp[availableIndex], timestamp[j], timestamp[i]);
							enhance[j].spineShoulder.isInterpolated = true;
						}
					}

					availableIndex = i;
				}
			}

			//4.2 shoulderLeft
			availableIndex = -1;
			for (int i = firstValidIndex; i <= lastValidIndex; i++)
			{
				if (!enhance[i].shoulderLeft.isVoid)
				{
					if (availableIndex != -1 && availableIndex < i - 1)
					{
						//there is a gap to fill in (linear time interpolation)
						Vector3 beforeP = enhance[availableIndex].shoulderLeft.position;
						Vector3 afterP = enhance[i].shoulderLeft.position;

						for (int j = availableIndex + 1; j < i; j++)
						{
							enhance[j].shoulderLeft.position = MyMath.vector3TimeInterpolate(beforeP, afterP, timestamp[availableIndex], timestamp[j], timestamp[i]);
							enhance[j].shoulderLeft.isInterpolated = true;
						}
					}

					availableIndex = i;
				}
			}

			//4.3 shoulderRight
			availableIndex = -1;
			for (int i = firstValidIndex; i <= lastValidIndex; i++)
			{
				if (!enhance[i].shoulderRight.isVoid)
				{
					if (availableIndex != -1 && availableIndex < i - 1)
					{
						//there is a gap to fill in (linear time interpolation)
						Vector3 beforeP = enhance[availableIndex].shoulderRight.position;
						Vector3 afterP = enhance[i].shoulderRight.position;

						for (int j = availableIndex + 1; j < i; j++)
						{
							enhance[j].shoulderRight.position = MyMath.vector3TimeInterpolate(beforeP, afterP, timestamp[availableIndex], timestamp[j], timestamp[i]);
							enhance[j].shoulderRight.isInterpolated = true;
						}
					}

					availableIndex = i;
				}
			}

			//4.4 elbowRight
			availableIndex = -1;
			for (int i = firstValidIndex; i <= lastValidIndex; i++)
			{
				if (!enhance[i].elbowRight.isVoid)
				{
					if (availableIndex != -1 && availableIndex < i - 1)
					{
						//there is a gap to fill in (linear time interpolation)
						Vector3 beforeP = enhance[availableIndex].elbowRight.position;
						Vector3 afterP = enhance[i].elbowRight.position;

						for (int j = availableIndex + 1; j < i; j++)
						{
							enhance[j].elbowRight.position = MyMath.vector3TimeInterpolate(beforeP, afterP, timestamp[availableIndex], timestamp[j], timestamp[i]);
							enhance[j].elbowRight.isInterpolated = true;
						}
					}

					availableIndex = i;
				}
			}

			//4.5 wristRight
			availableIndex = -1;
			for (int i = firstValidIndex; i <= lastValidIndex; i++)
			{
				if (!enhance[i].wristRight.isVoid)
				{
					if (availableIndex != -1 && availableIndex < i - 1)
					{
						//there is a gap to fill in (linear time interpolation)
						Vector3 beforeP = enhance[availableIndex].wristRight.position;
						Vector3 afterP = enhance[i].wristRight.position;

						for (int j = availableIndex + 1; j < i; j++)
						{
							enhance[j].wristRight.position = MyMath.vector3TimeInterpolate(beforeP, afterP, timestamp[availableIndex], timestamp[j], timestamp[i]);
							enhance[j].wristRight.isInterpolated = true;
						}
					}

					availableIndex = i;
				}
			}
			//end of round 4 (interpolation)

			//change firstValidIndex and lastValidIndex
			while (!enhance[firstValidIndex].haveAllData())
				firstValidIndex++;

			while (!enhance[lastValidIndex].haveAllData())
				lastValidIndex--;
			
		}

	
		public Dictionary<byte,int> searchForNearestImuIndex(double ktime)	//BUG: it give negative values
		{
			Dictionary<byte, int> ans = new Dictionary<byte, int>();
			foreach(byte id in imuCompute.Keys)
			{
				int n = imuCompute[id].Count;
				//assume that there is no missing frame in IMU
				int estIndex=(int)Math.Round(n*(ktime-imuCompute[id][0].ktime)/(imuCompute[id][n-1].ktime-imuCompute[id][0].ktime));
				if (estIndex < 0)
					estIndex = 0;
				else if (estIndex > n-2)
					estIndex = n - 2;
			
				while(estIndex>=0 && estIndex<=n-2)
				{
					if(ktime<imuCompute[id][estIndex].ktime)
					{
						estIndex--;
					}
					else if(imuCompute[id][estIndex+1].ktime<ktime)
					{
						estIndex++;
					}
					else
					{
						//this case is when imuCompute[id][estIndex].ktime<ktime<imuCompute[id][estIndex+1].ktime
						break;
					}
				}

				if (estIndex < 0)
					estIndex = 0;
				else if (estIndex > n - 2)
					estIndex = n - 2;

				//check which one is closer
				if (Math.Abs(ktime - imuCompute[id][estIndex].ktime) > Math.Abs(imuCompute[id][estIndex + 1].ktime - ktime))
					ans[id]=estIndex+1;
				else
					ans[id]=estIndex;
			}

			return ans;
		}

		

		void writeToCSV(ExtractedSample[] ex)
		{
			string filename = String.Format("25features_{0}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(folder+filename + ".csv"))
			{
				file.WriteLine("time,index,accX,accY,accZ,gyroX,gyroY,gyroZ,wristX,wristY,wristZ,wristSpd,elbowAng,elbowAngVel,forePointX,forePointY,forePointZ,forePronate,upPointX,upPointY,upPointZ,shoIntRot,upGyroX,upGyroY,upGyroZ,trunkFTilt,trunkLTilt");

				//foreach(var e in ex)
				for(int i=firstValidIndex+1;i<=lastValidIndex-1;i++)
				{
					var e=ex[i];
					file.Write(e.ktime+",");
					file.Write(e.visualIndex+",");
					file.Write(e.rightWristAcc.X+",");
					file.Write(e.rightWristAcc.Y+",");
					file.Write(e.rightWristAcc.Z+",");
					file.Write(e.rightWristGyro.X+",");
					file.Write(e.rightWristGyro.Y+",");
					file.Write(e.rightWristGyro.Z+",");
					file.Write(e.torso_rightWristPosition.X+",");
					file.Write(e.torso_rightWristPosition.Y+",");
					file.Write(e.torso_rightWristPosition.Z+",");
					file.Write(e.rightWristSpeed+",");
					file.Write(e.rightElbowAngle+",");
					file.Write(e.rightElbowAngleVelocity+",");
					file.Write(e.torso_rightForearmPointingDirection.X+",");
					file.Write(e.torso_rightForearmPointingDirection.Y+",");
					file.Write(e.torso_rightForearmPointingDirection.Z+",");
					file.Write(e.rightForearmPronation+",");
					file.Write(e.torso_rightUpperArmPointingDirection.X+",");
					file.Write(e.torso_rightUpperArmPointingDirection.Y+",");
					file.Write(e.torso_rightUpperArmPointingDirection.Z+",");
					file.Write(e.rightShoulderInternalRotation+",");
					file.Write(e.rightUpperArmVirtualGyro.X+",");
					file.Write(e.rightUpperArmVirtualGyro.Y+",");
					file.Write(e.rightUpperArmVirtualGyro.Z+",");
					file.Write(e.trunkFrontalTilt+",");
					file.Write(e.trunkLateralTilt+",");
					file.WriteLine();
				}
			}
		}
	}
}
