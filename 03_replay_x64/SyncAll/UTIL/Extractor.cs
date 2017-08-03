using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;
using Microsoft.Kinect;
using System.Diagnostics;

namespace SyncAll
{
	public static class Extractor
	{

		public static Quaternion calculateKinectUpperArmQuaternion(Vector3 upperArmPointingDirection, Vector3 forearmPointingDirection, bool forLeftSide)
		{
			upperArmPointingDirection.Normalize();
			forearmPointingDirection.Normalize();
			Vector3 zAxis = Vector3.Cross(upperArmPointingDirection, forearmPointingDirection).Normalized();
			if (forLeftSide)
				zAxis *= -1;
			Vector3 yAxis = upperArmPointingDirection;
			Vector3 xAxis = Vector3.Cross(yAxis, zAxis).Normalized();

			return MyMath.get_B_A_QuaternionFromThreeAxis(xAxis, yAxis, zAxis);
		}

		//tested
		public static float calculateRobustForearmPronation(Quaternion kinectUpperArmQuaternion, Quaternion kinectForearmQuaternion, bool forLeftHand)
		{
			Vector3 kinect_refY = MyMath.rotateVector(kinectForearmQuaternion, MyMath.unitY);
			Vector3 kinect_refX = MyMath.rotateVector(kinectUpperArmQuaternion, MyMath.unitZ); //refX of wrist is always equal to Z of upperarm (both left and right)
			Vector3 kinect_refZ = Vector3.Cross(kinect_refX, kinect_refY).Normalized();
			Quaternion refQuaternion = MyMath.get_B_A_QuaternionFromThreeAxis(kinect_refX, kinect_refY, kinect_refZ);

			Quaternion ref_measure_quaternion = MyMath.quaternionMultiplySequence(refQuaternion.Inverted(), kinectForearmQuaternion);
			OpenTK.Vector4 axisAngle = MyMath.getFrameRotationFrom_A_to_B_InAxisAngle(ref_measure_quaternion);
			float ansAngle = (float)(axisAngle.W * 180 / Math.PI);

			if (forLeftHand == true)
			{
				if (axisAngle.Xyz.Y < 0)
					ansAngle *= -1;
			}
			else
			{
				if (axisAngle.Xyz.Y >= 0)
					ansAngle *= -1;
			}

			//change it to the range of [-180,180]
			if (ansAngle > 180)
				ansAngle -= 360;
			else if (ansAngle < -180)
				ansAngle += 360;

			return ansAngle;    //start from thumb pointing along the triangle plane (shoulder-elbow-wrist) (natural position = 0)
								//go supination = negative
								//go pronation = positive
								//to make it go together with shoulder internal rotation
		}

		public static float calculateForearmPronation(Vector3 upperArmPointingDirection, Quaternion kinectForearmQuaternion, bool forLeftHand)
		{
			Vector3 kinect_refY = MyMath.rotateVector(kinectForearmQuaternion, MyMath.unitY);
			Vector3 kinect_refX = Vector3.Cross(upperArmPointingDirection, kinect_refY).Normalized();
			Vector3 kinect_refZ = Vector3.Cross(kinect_refX, kinect_refY).Normalized();
			Quaternion refQuaternion = MyMath.get_B_A_QuaternionFromThreeAxis(kinect_refX, kinect_refY, kinect_refZ);

			Quaternion ref_measure_quaternion = MyMath.quaternionMultiplySequence(refQuaternion.Inverted(), kinectForearmQuaternion);
			OpenTK.Vector4 axisAngle = MyMath.getFrameRotationFrom_A_to_B_InAxisAngle(ref_measure_quaternion);
			float ansAngle = (float)(axisAngle.W * 180 / Math.PI);

			if (forLeftHand == true)
			{
				if (axisAngle.Xyz.Y < 0)
					ansAngle *= -1;
			}
			else
			{
				if (axisAngle.Xyz.Y >= 0)
					ansAngle *= -1;
			}

			return ansAngle;    //start from thumb pointing along the triangle plane (shoulder-elbow-wrist) (natural position = 0)
								//go supination = negative
								//go pronation = positive
								//to make it go together with shoulder internal rotation
		}

		public static Quaternion getUpperArmRollZeroReferenceQuaternion(Vector3 torso_upperArmPointingDirection, bool forLeftSide)
		{
			Vector3 pointingDirection = torso_upperArmPointingDirection;

			//build quaternion at the equator ring
			//Y is pointing direction projected to ZX plane
			Vector3 yAxisEquator = (new Vector3(pointingDirection.X, 0, pointingDirection.Z)).Normalized();
			//X axis just point upward
			Vector3 xAxisEquator = new Vector3(0, -1, 0);   //X point down
			if (forLeftSide)
				xAxisEquator *= -1;
			Vector3 zAxisEquator = Vector3.Cross(xAxisEquator, yAxisEquator).Normalized();
			Quaternion torso_equatorQuaternion = MyMath.get_B_A_QuaternionFromThreeAxis(xAxisEquator, yAxisEquator, zAxisEquator);

			//build quaternion at the pole
			Vector3 xAxisPole,yAxisPole,zAxisPole;
			Quaternion torso_poleQuaternion;
			if (pointingDirection.Y >= 0)
			{
				xAxisPole = new Vector3(-1, 0, 0);
				//if (forLeftSide)
				//	xAxisPole *= -1;
				yAxisPole = new Vector3(0, 1, 0);
				zAxisPole = Vector3.Cross(xAxisPole, yAxisPole).Normalized();
				torso_poleQuaternion = MyMath.get_B_A_QuaternionFromThreeAxis(xAxisPole, yAxisPole, zAxisPole);
			}
			else
			{
				xAxisPole = new Vector3(1, 0, 0);
				//if (forLeftSide)
				//	xAxisPole *= -1;
				yAxisPole = new Vector3(0, -1, 0);
				zAxisPole = Vector3.Cross(xAxisPole, yAxisPole).Normalized();
				torso_poleQuaternion = MyMath.get_B_A_QuaternionFromThreeAxis(xAxisPole, yAxisPole, zAxisPole);
			}

			double angleToPoleRad = Math.Acos(Math.Abs(pointingDirection.Y));
			double angleToEquatorRad = Math.PI / 2 - angleToPoleRad;

			float polePortion = (float)((Math.PI/2 - angleToPoleRad) / (Math.PI/2));
			//float equatorPortion = 1f - polePortion;

			//rotate both quaternion to the meeting spot
			Vector3 rotationAxis = Vector3.Cross(yAxisPole, yAxisEquator).Normalized();
			//pole will move with positive angle (always)

			Quaternion poleToSpot_rotate = Quaternion.FromAxisAngle(rotationAxis, (float)angleToPoleRad);
			Quaternion torso_spotQuaternion_forPole = MyMath.quaternionMultiplySequence(poleToSpot_rotate, torso_poleQuaternion);	//confused but correct

			//equator will move with negative angle (always)
			Quaternion equatorToSpot_rotate = Quaternion.FromAxisAngle(rotationAxis, -(float)angleToEquatorRad);
			Quaternion torso_spotQuaternion_forEquator = MyMath.quaternionMultiplySequence(equatorToSpot_rotate,torso_equatorQuaternion); //confused but correct

			//fuse 2 quaternions that share the same Y axis
			Quaternion torso_refQuaternion = MyMath.quaternionInterpolate_SLERP(torso_spotQuaternion_forPole, torso_spotQuaternion_forEquator, polePortion);

			/*
			//use SLERP to calculate reference orientation (Y axis will be wrong)
			//this reference quaternion should has the same Y axis as torso_upperArm_q has
			Quaternion torso_refQuaternion = MyMath.quaternionInterpolate_SLERP(torso_poleQuaternion, torso_equatorQuaternion, polePortion);
			*/

			/*
			Debug.WriteLine("===");
			Debug.WriteLine(pointingDirection);

			Debug.WriteLine(MyMath.rotateVector(torso_spotQuaternion_forPole, MyMath.unitY));
			Debug.WriteLine(MyMath.rotateVector(torso_spotQuaternion_forEquator, MyMath.unitY));

			Debug.WriteLine(MyMath.rotateVector(torso_refQuaternion, MyMath.unitY));
			*/

			return torso_refQuaternion.Normalized();
		}

		public static float calculateShoulderInternalRotation(Quaternion torso_upperArm_q, bool forLeftSide)    //more internal rotation==>more positive
		{
			//1. get reference quaternion, 2. use it to calculate forearm roll

			Vector3 pointingDirection = MyMath.rotateVector(torso_upperArm_q, MyMath.unitY);
			Quaternion torso_refQuaternion = getUpperArmRollZeroReferenceQuaternion(pointingDirection, forLeftSide);
			/*
			Debug.WriteLine("==");
			Debug.WriteLine(pointingDirection);
			Debug.WriteLine(MyMath.rotateVector(torso_refQuaternion, MyMath.unitY));
			*/
			//compare torso_upperArm_q to refQuaternion to see how much X has rotated around Y axis
			Quaternion ref_measure_quaternion = MyMath.quaternionMultiplySequence(torso_refQuaternion.Inverted(), torso_upperArm_q);
			OpenTK.Vector4 axisAngle = MyMath.getFrameRotationFrom_A_to_B_InAxisAngle(ref_measure_quaternion);    //axisAngle from ref to measure
			float ansAngle = (float)(axisAngle.W * 180 / Math.PI);

			//Debug.WriteLine(axisAngle.Xyz);
			//Debug.WriteLine(ansAngle);

			if (axisAngle.Xyz.Y < 0)
				ansAngle *= -1;     //make sure ansAngle is rotating around Y axis

			

			if (forLeftSide == false)	//for right side, shoulder internal rotation is backward
			{
					ansAngle *= -1;
			}

			//clean up the value to range [-90,270]
			if (ansAngle < -90)
				ansAngle = ansAngle + 360;
			else if (ansAngle > 270)
				ansAngle = ansAngle - 360;

			//debug: convert it back to measure_ref_quaternion
			//Quaternion measure_ref_quaternion2 = Quaternion.FromAxisAngle()

			//debug: convert it back
			//Quaternion q=calculateTorsoUpperArmQuaternion(pointingDirection, ansAngle, false);
			//Debug.WriteLine("==");
			//Debug.WriteLine(torso_upperArm_q);
			//Debug.WriteLine(q);
			//Debug.WriteLine(MyMath.calculateOrientationDifferenceInOneRotationAngle(torso_upperArm_q, q));

			//Debug.WriteLine(ansAngle);
			//Debug.WriteLine(MyMath.calculateOrientationDifferenceInOneRotationAngle(torso_refQuaternion, torso_upperArm_q));


			return ansAngle;
			//return axisAngle;

			//return refQuaternion;
			//return equatorQuaternion; //OK
			//return refQuaternion;
		}

		// inverse version of calculateShoulderInternalRotation
		//not tested yet
		public static Quaternion calculateTorsoUpperArmQuaternion(Vector3 torso_upperArmPointingDirection, float shoulderInternalRotationDeg, bool forLeftSide)
		{
			Quaternion torso_refQuaternion = getUpperArmRollZeroReferenceQuaternion(torso_upperArmPointingDirection, forLeftSide);
			if(!forLeftSide)
			{
				shoulderInternalRotationDeg *= -1;
			}

			//here, I will rotate refQuaternion around its Y-axis with angle of shoulderInternalRotation
			float angleRad = (float)(shoulderInternalRotationDeg*Math.PI/180);

			Quaternion ref_measure_quaternion = Quaternion.FromAxisAngle(MyMath.unitY, angleRad);	//rotate "frame" from ref to measure
			Quaternion torso_upperArmMeasure_quaternion = MyMath.quaternionMultiplySequence(torso_refQuaternion, ref_measure_quaternion);

			/*
			Debug.WriteLine("==");
			Debug.WriteLine(torso_upperArmPointingDirection);
			Debug.WriteLine(MyMath.rotateVector(torso_upperArmMeasure_quaternion, MyMath.unitY));
			*/

			return torso_upperArmMeasure_quaternion;
		}
	}
}