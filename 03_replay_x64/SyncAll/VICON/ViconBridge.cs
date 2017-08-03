using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SyncAll
{
    public class ViconBridge
    {
		float markerDiameter;	//m	14mm (always be this number)
		float markerBaseThickness; //m 0.6mm
						
		public double kinectFrameOffset = 37.5f;    //vicon starts after 37-38 frames of Kinect record
													//udp package is sent at the 30th frame (vicon took ~8 kinect frames to start the system)
		public double kinectTimestampFirstViconFrame;
		
		const int kinectFrameRate=30;	//do not use this
		public float dataRate;

		//for transformation
		public Quaternion kinect_vicon_q;
		public Vector3 viconOriginInKinectFrame;

		Dictionary<string,float> subjectParam=new Dictionary<string, float>();
		List<Dictionary<string,Vector3>> record;	//in kinect reference frame, with real and virtual markers

		string folderVicon;

		public ViconBridge(string folderMain,string subjectDatabase, OfflineBridge offlineBridge)
		{
			this.folderVicon = folderMain + "vicon/";

			//get frame transformation (vicon <-> kinect)
			Quaternion vicon_panel_q;
			Vector3 panelOriginInViconFrame;
			loadPanelParameterFromViconPerspective(folderVicon+"viconPanelFile.TRC",out vicon_panel_q, out panelOriginInViconFrame);

			Quaternion kinect_panel_q;
			Vector3 panelOriginInKinectFrame;
			SettingManager.loadPanelParameter(folderVicon+"kinectPanelFile.bin", out kinect_panel_q, out panelOriginInKinectFrame);

			kinect_vicon_q = MyMath.quaternionMultiplySequence(kinect_panel_q, vicon_panel_q.Inverted());
			viconOriginInKinectFrame = panelOriginInKinectFrame - MyMath.rotateVector(kinect_vicon_q, panelOriginInViconFrame);

			//load marker parameter
			Dictionary<string,float> markerParam = SettingManager.readDotMeasureFile(folderVicon+"marker.measure");
			markerDiameter=markerParam["diameter"];
			markerBaseThickness=markerParam["base"];

			//load subject name
			string subjectName=SettingManager.loadOneStringFromFile(folderMain+"subject.name");

			//load subject parameter

			subjectParam = SettingManager.readDotMeasureFile(subjectDatabase+"measure/"+subjectName+".measure");

			//load time offset
			if(File.Exists(folderVicon+"viconStartOffset.bin"))
				kinectFrameOffset = SettingManager.loadOneDoubleFromFile(folderVicon+"viconStartOffset.bin");
			else
				kinectFrameOffset = 37.5;

			kinectTimestampFirstViconFrame = MyMath.doubleInterpolate(offlineBridge.timestamp, kinectFrameOffset);

			//read groundtruth from vicon folder
			string viconRecordFile="";
			if(File.Exists(folderVicon+"viconRecord.TRC"))
				viconRecordFile=folderVicon+"viconRecord.TRC";
			else
			{
				//cut from this string
				//C:\GoogleDrive\research\movementRecord\2017-05-16_15-34-22\vicon\
				string[] ss = folderMain.Split(new char[] {'\\','/'});
				string dateString = ss[ss.Length-2]; 
				viconRecordFile=folderVicon+dateString+".TRC";
			}

			TRCreader viconRecord = new TRCreader(viconRecordFile);
			dataRate = viconRecord.dataRate;

			//create visual marker
			var recordInViconFrame = extractVirtualMarkers(viconRecord.record, subjectParam);

			//convert to kinect reference frame
			record = new List<Dictionary<string, Vector3>>();
			foreach(var e in recordInViconFrame)
			{
				Dictionary<string,Vector3> aFrame=new Dictionary<string, Vector3>();
				foreach(var name in e.Keys)
				{
					aFrame[name]=viconToKinect(e[name]);
				}
				record.Add(aFrame);
			}
		}

		//must set 2 parameters before
		public Vector3 viconToKinect(Vector3 V_vicon)
		{
			return viconOriginInKinectFrame + MyMath.rotateVector(kinect_vicon_q, V_vicon);
		}

		void loadPanelParameterFromViconPerspective(string filename, out Quaternion vicon_panel_q, out Vector3 panelOriginInViconFrame)
		{
			TRCreader panelRecord = new TRCreader(filename);

			//find average position of L0, L1, L2, L3
			Dictionary<string, Vector3> total = new Dictionary<string, Vector3>();
			Dictionary<string, int> count = new Dictionary<string, int>();

			total["L0"] = total["L1"] = total["L2"] = total["L3"] = new Vector3(0);
			count["L0"] = count["L1"] = count["L2"] = count["L3"] = 0;

			foreach (var sample in panelRecord.record)
			{
				foreach (string label in sample.Keys)
				{
					total[label] += sample[label];
					count[label] += 1;
				}
			}

			Vector3 L0 = total["L0"] / count["L0"];
			Vector3 L1 = total["L1"] / count["L1"];
			Vector3 L2 = total["L2"] / count["L2"];
			Vector3 L3 = total["L3"] / count["L3"];

			Vector3 panelX = (L1 - L0).Normalized();
			Vector3 panelZ = Vector3.Cross(panelX, (L2 - L0).Normalized()).Normalized();
			Vector3 panelY = Vector3.Cross(panelZ, panelX).Normalized();

			vicon_panel_q = MyMath.get_B_A_QuaternionFromThreeAxis(panelX, panelY, panelZ);
			panelOriginInViconFrame = L0;
		}

		

		public List<bool> groundTruthReady=new List<bool>();
		//TESTED
		List<Dictionary<string,Vector3>> extractVirtualMarkers(List<Dictionary<string, Vector3>> record, Dictionary<string,float> subjectMeasurement)
		{
			for(int i=0;i<record.Count;i++)
			{ 
				if(	!(	record[i].ContainsKey("STRN") &&
						record[i].ContainsKey("T10") &&
						record[i].ContainsKey("CLAV") &&
						record[i].ContainsKey("C7") &&
						record[i].ContainsKey("LSHO") &&
						record[i].ContainsKey("LELB") &&
						record[i].ContainsKey("LWRA") &&
						record[i].ContainsKey("LWRB") &&
						record[i].ContainsKey("RSHO") &&
						record[i].ContainsKey("RELB") &&
						record[i].ContainsKey("RWRA") &&
						record[i].ContainsKey("RWRB"))
					)
				{
					groundTruthReady.Add(false);
					continue;
				}
				else
				{
					groundTruthReady.Add(true);
				}

				//find thorax origin
				Vector3 thoraxZ = ((record[i]["STRN"]+record[i]["T10"])/2 - (record[i]["CLAV"]+record[i]["C7"])/2).Normalized();
				Vector3 thoraxX_tmp = ((record[i]["CLAV"]+record[i]["STRN"])/2 - (record[i]["C7"]+record[i]["T10"])/2).Normalized();
				Vector3 thoraxY = Vector3.Cross(thoraxZ,thoraxX_tmp).Normalized();
				Vector3 thoraxX = Vector3.Cross(thoraxY,thoraxZ).Normalized();
				Vector3 thoraxOrigin = record[i]["CLAV"] - thoraxX*(markerDiameter/2+markerBaseThickness);

				//find shoulder joint center (SJC)
				float shoulderOffset = subjectMeasurement["shoulderOffsetNoMarker"] + (markerDiameter/2+markerBaseThickness);

				Vector3 shoulderWandL = Vector3.Cross(thoraxX,record[i]["LSHO"]-thoraxOrigin).Normalized();
				Vector3 LSJC = chordFunction(record[i]["LSHO"],thoraxOrigin,shoulderOffset,shoulderWandL);

				Vector3 shoulderWandR = Vector3.Cross(thoraxX,record[i]["RSHO"]-thoraxOrigin).Normalized();;
				Vector3 RSJC = chordFunction(record[i]["RSHO"],thoraxOrigin,shoulderOffset,-shoulderWandR);

				//find elbow joint center (EJC)
				float elbowOffset = (subjectMeasurement["elbowWidth"]+markerDiameter)/2 + markerBaseThickness;

				Vector3 wristMidPointL =  (record[i]["LWRA"]+record[i]["LWRB"])/2 ;
				Vector3 elbowWandL = Vector3.Cross(record[i]["LELB"]-LSJC,record[i]["LELB"]-wristMidPointL).Normalized();
				Vector3 LEJC = chordFunction(record[i]["LELB"],LSJC,elbowOffset,elbowWandL);

				Vector3 wristMidPointR =  (record[i]["RWRA"]+record[i]["RWRB"])/2 ;
				Vector3 elbowWandR = Vector3.Cross(record[i]["RELB"]-RSJC,record[i]["RELB"]-wristMidPointR).Normalized();
				Vector3 REJC = chordFunction(record[i]["RELB"],RSJC,elbowOffset,-elbowWandR);

				//find wrist joint center (WJC) (no chord function)
				float wristOffset = (subjectMeasurement["wristThickness"]+markerDiameter)/2 + markerBaseThickness;

				Vector3 wristOffsetDirectionL = Vector3.Cross(wristMidPointL-LEJC,record[i]["LWRA"]-record[i]["LWRB"]).Normalized();
				Vector3 LWJC = wristMidPointL + wristOffsetDirectionL*wristOffset;

				Vector3 wristOffsetDirectionR = Vector3.Cross(wristMidPointR-REJC,record[i]["RWRB"]-record[i]["RWRA"]).Normalized();
				Vector3 RWJC = wristMidPointR + wristOffsetDirectionR*wristOffset;

				//save the record
				record[i]["LSJC"]=LSJC;
				record[i]["RSJC"]=RSJC;

				record[i]["LEJC"]=LEJC;
				record[i]["REJC"]=REJC;

				record[i]["LWJC"]=LWJC;
				record[i]["RWJC"]=RWJC;

				record[i]["THOO"]=thoraxOrigin;
			}

			return record;
		}

		static private Vector3 chordFunction(Vector3 targetMarker, Vector3 oppositeMarker, float offset, Vector3 constructionVector)
		{
			Vector3 rotationPoint = (targetMarker+oppositeMarker)/2;
			Vector3 vectorToBeRotated = targetMarker - rotationPoint;
			Vector3 rotationAxis = Vector3.Cross(constructionVector,vectorToBeRotated).Normalized();
			float rotationAngle = (float)(2*Math.Asin(offset/(targetMarker-oppositeMarker).Length));
			return rotationPoint + Matrix3.Transpose(Matrix3.CreateFromAxisAngle(rotationAxis,rotationAngle))*vectorToBeRotated;	   //need transpose because OpenGL is column first
		}

		//public Dictionary<string,Vector3> getFrame(int kinectFrameIndex,bool interpolate=false)
		public Dictionary<string,Vector3> getFrame(double kinectTimestamp,bool interpolate=false)
		{
			//it is buggy to calculate viconFrameIndex in this way
			//because I have found that sometimes kinect frame rate is not stable at 30 fps
			//I've found additional time of 100 ms during 50 sec of running in 2017-06-23_11-06-45
			//double targetViconFrameIndex=(double)(kinectFrameIndex-kinectFrameOffset)/kinectFrameRate *dataRate;

			double targetViconFrameIndex= (kinectTimestamp-kinectTimestampFirstViconFrame)/(1000/dataRate);

			if (!interpolate)
			{ 
				int target = (int)Math.Round(targetViconFrameIndex);

				if(target>=0 && target<record.Count)
				{
					return record[target];
				}
				else
				{
					return null;
				}
			}
			else
			{
				if(targetViconFrameIndex>=0 && targetViconFrameIndex<record.Count-1)    //make sure it can interpolate
				{
					int before = (int)Math.Floor(targetViconFrameIndex);
					int after = (int)Math.Ceiling(targetViconFrameIndex);

					float fraction = (float)targetViconFrameIndex-before;
					Dictionary<string,Vector3> interpolatedResult=new Dictionary<string, Vector3>();
					foreach (string k in record[before].Keys)
					{
						if(record[after].ContainsKey(k))
							interpolatedResult[k]=record[before][k]*(1-fraction)+record[after][k]*fraction;
					}
					return interpolatedResult;
				}
				else
				{
					return null;
				}
			}
		}

		public void saveCurrentFrameOffset()
		{
			SettingManager.saveOneDoubleToFile(folderVicon+"viconStartOffset.bin",kinectFrameOffset);
		}

		/*
		public bool isGroundTruthReady(int visualIndex)
		{
			return groundTruthReady()
		}
		*/
	}
}
