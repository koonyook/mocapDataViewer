using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Kinect;
using OpenTK;
using System.Diagnostics;

namespace SyncAll
{
	//minimal core
	public class TickCore
	{
		private MainWindow mainWindow;
		public OfflineBridge bridge;

		private DispatcherTimer dispatcherTimer;

		public TickCore(MainWindow mainWindow,int ms, OfflineBridge bridge)
		{
			this.mainWindow = mainWindow;
			DispatcherTimerSetup(ms);
			this.bridge = bridge;
		}

		public void DispatcherTimerSetup(int ms)
		{
			dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += DispatcherTimer_Tick; ;
			dispatcherTimer.Interval = TimeSpan.FromMilliseconds(ms);
		}

		public void start()
		{
			dispatcherTimer.Start();
		}

		public void stop()
		{
			dispatcherTimer.Stop();
		}

		private void DispatcherTimer_Tick(object sender, EventArgs e)
		{
			//mainWindow.gwin.depthTableToDisplay = access.depthProjector.calculateDepthImage();
			if (mainWindow.isPlaying && mainWindow.timelineSlider.Value < mainWindow.timelineSlider.Maximum)
				mainWindow.timelineSlider.Value++;

			if (mainWindow.timelineSlider.Value == mainWindow.timelineSlider.Maximum)
				mainWindow.isPlaying = false;
		}

		private int currentVisualIndex = 0;

		public void changeTime(int visualIndex, bool forceUpdate=false)
		{
			if(visualIndex != currentVisualIndex || forceUpdate)
			{
				currentVisualIndex = visualIndex;

				//TODO:update components in access (from playback)
				//mainWindow.gwin.space.deviceUpwardDirection = KinectUtil.convertVector4ToQuaternion(bridge.bodyData[visualIndex].FloorClipPlane).Xyz;
				mainWindow.gwin.space.deviceUpwardDirection = bridge.bodyData[visualIndex].FloorClipPlane.getQuaternion().Xyz;
				access.upwardLine.end = mainWindow.gwin.space.deviceUpwardDirection;
				bridge.sendDepthFrame(visualIndex, access.kinectPointCloud.cloudVertexPtr);

				byte[] bdi=new byte[424 * 512];
				bridge.readBodyIndexFrame(visualIndex,ref bdi);
				access.kinectPointCloud.changeColorByBodyIndex(bdi);



				if(visualIndex==0)
				{
					//just simply ignore
				}
				else
				{
					//TODO: search and display 2 forearms (don't have to interpolate)

					/////////////////////////
					double targetTimestamp = bridge.timestamp[visualIndex];

					//display kinect skeleton
					MyBody body = bridge.bodyData[visualIndex].getBodyByTrackingID(bridge.targetTrackingId);
					if (body != null)
					{
						access.kinectSkeleton.updateVertexFromCore(body);
						access.kinectSkeleton.visible=true;
					}
					else
					{
						access.kinectSkeleton.visible=false;
					}

					if (bridge.baseID != 0)
					{
						Dictionary<byte, int> nearestImuIndex = bridge.searchForNearestImuIndex(targetTimestamp);

						//byte rightID = (byte)bridge.setting["rightID"];
						//byte leftID = (byte)bridge.setting["leftID"];
						byte rightID = (byte)bridge.settingProto.RightID;
						byte leftID = (byte)bridge.settingProto.LeftID;

						//Debug.WriteLine(targetTimestamp);
						//Debug.WriteLine("right ktime:"+bridge.imuCompute[rightID][nearestImuIndex[rightID]].ktime);
						//Debug.WriteLine(" left ktime:"+bridge.imuCompute[leftID][nearestImuIndex[leftID]].ktime);

						//Debug.WriteLine(nearestImuIndex[rightID] + ":" + bridge.imuCompute[rightID][nearestImuIndex[rightID]].MagneticSensorQuaternion);

						/*
						if(bridge.imuCompute.ContainsKey(rightID))
						{ 
							//right forearm
							Quaternion kinectRightForearmOriginalQuaternion = MyMath.quaternionMultiplySequence(
								bridge.kinectBaseQuaternion,
								bridge.imuCompute[bridge.baseID][nearestImuIndex[bridge.baseID]].MagneticSensorQuaternion.Inverted(),
								bridge.imuCompute[rightID][nearestImuIndex[rightID]].MagneticSensorQuaternion
								//bridge.imuRaw[rightID][nearestImuIndex[rightID]].orientation //use calculation at record time
							);
							access.rightForearmOriginal.updateFromCore(bridge.bodyData[visualIndex].getBodyByTrackingID(bridge.targetTrackingId),kinectRightForearmOriginalQuaternion);

							//access.rightForearm.updateFromCore(bridge.bodyData[visualIndex].getBodyByTrackingID(bridge.targetTrackingId), bridge.exSample[visualIndex].kRightForearmQuaternion);
							//access.rightUpperArm.updateFromCore(bridge.bodyData[visualIndex].getBodyByTrackingID(bridge.targetTrackingId), bridge.exSample[visualIndex].kRightUpperArmQuaternion);
						}

						if(bridge.imuCompute.ContainsKey(leftID))
						{
							//left forearm
							Quaternion kinectLeftImuQuaternion = MyMath.quaternionMultiplySequence(
								bridge.kinectBaseQuaternion, 
								bridge.imuCompute[bridge.baseID][nearestImuIndex[bridge.baseID]].MagneticSensorQuaternion.Inverted(), 
								bridge.imuCompute[leftID][nearestImuIndex[leftID]].MagneticSensorQuaternion
								//bridge.imuRaw[leftID][nearestImuIndex[leftID]].orientation //use calculation at record time
							);

							//must do one rotation to flip 180 degree around left-IMU-Z-axis
							Quaternion leftImu_leftForearm_quaternion = Quaternion.FromAxisAngle(MyMath.unitZ, (float)Math.PI);
							Quaternion kinectLeftForearmOriginalQuaternion = MyMath.quaternionMultiplySequence(kinectLeftImuQuaternion,leftImu_leftForearm_quaternion);

							access.leftForearmOriginal.updateFromCore(bridge.bodyData[visualIndex].getBodyByTrackingID(bridge.targetTrackingId), kinectLeftForearmOriginalQuaternion);
						}
						*/

						ExtractedSample ex=bridge.exSample[visualIndex];
						if(ex.valid)
						{ 
							if(Global.SHOW_IMU)
							{ 
								//access.rightUpperArm.updateFromCore(ex.jointPosition[17],ex.jointPosition[19],ex.kRightUpperArmQuaternion_tmp);// 
								access.rightUpperArm.updateFromCore(ex.jointPosition[17],ex.jointPosition[19],ex.kRightUpperArmQuaternion);
								access.rightForearm.updateFromCore(ex.jointPosition[19],ex.jointPosition[21],ex.kRightForearmQuaternion);
							}
							else
							{
								access.rightUpperArm.visible = false;
								access.rightForearm.visible = false;
							}

						}

						access.upperTorso.coordinatePosition = bridge.exSample[visualIndex].kTorsoPosition;
						access.upperTorso.orientation = bridge.exSample[visualIndex].kTorsoQuaternion;

						access.rightShoulder.position = bridge.exSample[visualIndex].kRightShoulderPosition;
						
						/*
						EnhanceBody e = bridge.enhance[visualIndex];
						if (e.spineShoulder.haveData())
						{
							access.enSpineShoulder.position = e.spineShoulder.position;
							access.enSpineShoulder.changeColor(e.spineShoulder.getColor());
							access.enSpineShoulder.visible = true;
						}
						else
						{
							access.enSpineShoulder.visible = false;
						}

						if (e.shoulderLeft.haveData())
						{
							access.enShoulderLeft.position = e.shoulderLeft.position;
							access.enShoulderLeft.changeColor(e.shoulderLeft.getColor());
							access.enShoulderLeft.visible = true;
						}
						else
						{
							access.enShoulderLeft.visible = false;
						}

						if (e.shoulderRight.haveData())
						{
							access.enShoulderRight.position = e.shoulderRight.position;
							access.enShoulderRight.changeColor(e.shoulderRight.getColor());
							access.enShoulderRight.visible = true;
						}
						else
						{
							access.enShoulderRight.visible = false;
						}

						if (e.elbowRight.haveData())
						{
							access.enElbowRight.position = e.elbowRight.position;
							access.enElbowRight.changeColor(e.elbowRight.getColor());
							access.enElbowRight.visible = true;
						}
						else
						{
							access.enElbowRight.visible = false;
						}

						if (e.wristRight.haveData())
						{
							access.enWristRight.position = e.wristRight.position;
							access.enWristRight.changeColor(e.wristRight.getColor());
							access.enWristRight.visible = true;
						}
						else
						{
							access.enWristRight.visible = false;
						}
						*/
						//left forearm
						/*
						Quaternion kinectLeftForearmQuaternion = MyMath.quaternionMultiplySequence(
							bridge.kinectBaseQuaternion,
							bridge.imuCompute[bridge.baseID][nearestImuIndex[bridge.baseID]].MagneticSensorQuaternion.Inverted(),
							bridge.imuCompute[leftID][nearestImuIndex[leftID]].MagneticSensorQuaternion
						);
						access.leftForearm.updateFromCore(bridge.bodyData[visualIndex].bodies, kinectLeftForearmQuaternion);
						*/

						//mainWindow.textToDisplay = bridge.exSample[visualIndex].torso_rightWristPosition.ToString();
						//mainWindow.textToDisplay = bridge.exSample[visualIndex].rightWristSpeed.ToString();
						//mainWindow.textToDisplay = bridge.exSample[visualIndex].rightElbowAngle.ToString();
						//mainWindow.textToDisplay = bridge.exSample[visualIndex].rightElbowAngleVelocity.ToString();
						//mainWindow.textToDisplay = bridge.exSample[visualIndex].torso_rightForearmPointingDirection.ToString();
						//mainWindow.textToDisplay = bridge.exSample[visualIndex].torso_rightUpperArmPointingDirection.ToString();
						//mainWindow.textToDisplay = bridge.exSample[visualIndex].rightForearmPronation.ToString();
						//mainWindow.textToDisplay = bridge.exSample[visualIndex].rightShoulderInternalRotation.ToString();
						//mainWindow.textToDisplay = bridge.exSample[visualIndex].rightUpperArmVirtualGyro.ToString();
						//mainWindow.textToDisplay = bridge.exSample[visualIndex].trunkFrontalTilt.ToString();
						//mainWindow.textToDisplay = bridge.exSample[visualIndex].trunkLateralTilt.ToString();

						if (body != null)
						{
							/*
							mainWindow.textToDisplay = "Spine: " + body.Joints[JointType.SpineShoulder].TrackingState;
							mainWindow.textToDisplay += "\nShoulderRight: " + body.Joints[JointType.ShoulderRight].TrackingState;
							mainWindow.textToDisplay += "\nElbowRight: " + body.Joints[JointType.ElbowRight].TrackingState;
							mainWindow.textToDisplay += "\nWristRight: " + body.Joints[JointType.WristRight].TrackingState;

							mainWindow.textToDisplay += "\n\nShoulderLeft: " + body.Joints[JointType.ShoulderLeft].TrackingState;
							mainWindow.textToDisplay += "\nhead: " + body.Joints[JointType.Head].TrackingState;
							mainWindow.textToDisplay += "\nneck: " + body.Joints[JointType.Neck].TrackingState;
							*/
						}
					}
					else
					{
						//access.rightForearm.updateFromCore(bridge.bodyData[visualIndex].getNearestBodyToKinect(), new Quaternion(0, 0, 0, 1));
					}

					

					if(bridge.vBridge!=null)
					{ 
						var groundTruth = bridge.vBridge.getFrame(bridge.timestamp[visualIndex],true);
						if(groundTruth!=null)
						{
							access.viconSkeleton.updateVertexFromCore(groundTruth);
							access.viconSkeleton.visible=true;

							if (bridge.baseID != 0)
							{
								Dictionary<byte, int> nearestImuIndex = bridge.searchForNearestImuIndex(targetTimestamp);
								//display IMU info on vBridge position
								byte rightID = (byte)bridge.settingProto.RightID;
								byte leftID = (byte)bridge.settingProto.LeftID;

								if(bridge.imuCompute.ContainsKey(rightID) && groundTruth.ContainsKey("REJC"))
								{ 
									//right forearm
									Quaternion kinectRightForearmOriginalQuaternion = MyMath.quaternionMultiplySequence(
										bridge.kinectBaseQuaternion,
										bridge.imuCompute[bridge.baseID][nearestImuIndex[bridge.baseID]].MagneticSensorQuaternion.Inverted(),
										bridge.imuCompute[rightID][nearestImuIndex[rightID]].MagneticSensorQuaternion
										//bridge.imuRaw[rightID][nearestImuIndex[rightID]].orientation //use calculation at record time
									);
									

									if(Global.SHOW_IMU)
										access.rightForearmOriginal.updateFromCore(groundTruth["REJC"],kinectRightForearmOriginalQuaternion,Vector3.UnitY,0.3f);
									else
										access.rightForearmOriginal.visible=false;
									//access.rightForearmOriginal.updateFromCore(bridge.bodyData[visualIndex].getBodyByTrackingID(bridge.targetTrackingId),kinectRightForearmOriginalQuaternion);

									//access.rightForearm.updateFromCore(bridge.bodyData[visualIndex].getBodyByTrackingID(bridge.targetTrackingId), bridge.exSample[visualIndex].kRightForearmQuaternion);
									//access.rightUpperArm.updateFromCore(bridge.bodyData[visualIndex].getBodyByTrackingID(bridge.targetTrackingId), bridge.exSample[visualIndex].kRightUpperArmQuaternion);
								}

								if(bridge.imuCompute.ContainsKey(leftID) && groundTruth.ContainsKey("LEJC"))
								{
									//left forearm
									Quaternion kinectLeftImuQuaternion = MyMath.quaternionMultiplySequence(
										bridge.kinectBaseQuaternion, 
										bridge.imuCompute[bridge.baseID][nearestImuIndex[bridge.baseID]].MagneticSensorQuaternion.Inverted(), 
										bridge.imuCompute[leftID][nearestImuIndex[leftID]].MagneticSensorQuaternion
										//bridge.imuRaw[leftID][nearestImuIndex[leftID]].orientation //use calculation at record time
									);

									//must do one rotation to flip 180 degree around left-IMU-Z-axis
									Quaternion leftImu_leftForearm_quaternion = Quaternion.FromAxisAngle(MyMath.unitZ, (float)Math.PI);
									Quaternion kinectLeftForearmOriginalQuaternion = MyMath.quaternionMultiplySequence(kinectLeftImuQuaternion,leftImu_leftForearm_quaternion);

									//access.leftForearmOriginal.updateFromCore(bridge.bodyData[visualIndex].getBodyByTrackingID(bridge.targetTrackingId), kinectLeftForearmOriginalQuaternion);
									if(Global.SHOW_IMU)
									{ 
										access.leftForearmOriginal.updateFromCore(groundTruth["LEJC"],kinectLeftForearmOriginalQuaternion,Vector3.UnitY,0.3f);
									}
									else
									{
										access.leftForearmOriginal.visible=false;
									}
								}
							}
						}
						else
						{
							access.viconSkeleton.visible=false;
						}
					}
					else
					{
						access.viconSkeleton.visible=false;
					}

					/*
					//for upper arm
					Vector3 elbowPosition = interpolate.getElbowPosition();
					Vector3 upperArmUnitVector = (elbowPosition - interpolate.interpolatedShoulderPosition).Normalized();
					Vector3 forearmUnitVector = MyMath.rotateVector(interpolate.kinect_forearm_q, MyMath.unitY);
					//the whole arm is not too straight, it is posible to recover upperarm orientation easily
					//Y-axis is easy
					Vector3 upperArmY = upperArmUnitVector;
					//Z-axis from cross product
					Vector3 upperArmZ = Vector3.Cross(upperArmUnitVector, forearmUnitVector).Normalized();
					//X-axis
					Vector3 upperArmX = Vector3.Cross(upperArmY, upperArmZ).Normalized();

					//get quaternion from 3 axis
					Quaternion kinect_rightUpperArm_q = MyMath.getQuaternionFromThreeAxis(upperArmX, upperArmY, upperArmZ);

					access.rightUpperArm.updateFromCore(interpolate.interpolatedShoulderPosition, elbowPosition, kinect_rightUpperArm_q);

					access.rightUpperArm.visible = true;
					*/
				}

				

				mainWindow.gwin.hasNewUpdate.state = true;	//this will invoke update automatically soon
			}
		}

	
	}
}