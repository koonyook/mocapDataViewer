using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace SyncAll
{

    public static class Global
    {
		//quick setting
		public const bool SHOW_IMU=false;

		public const bool USE_SUBJECT_PARAMETER = true;	//if true  -> load from file 
														//if false -> hard code (m,f) and quickBeta

		public static string rootDirectory = @"C:\GoogleDrive\research\publishedDataset\";

		public static TimeSpan FRAME_UPDATE_INTERVAL = new TimeSpan(0, 0, 0, 0, 30);    //if frame not update for more than 30 ms, it is time to update
		
        public const float NORMAL_RADIUS = 0.015f;   //15mm radius

		//////////// status variable
		public static bool isGoingToSaveOffset = false;
        public static bool isGoingToPrintDebug = false;

        ///////////////////////////////////////////////////
        //////////////////////////////////////////////////
        /////////////////////////////////////////////////
        //general setting

        //////////////////////////
        public const int MAGNETIC_DIP_WINDOW = 5 * 80;  //5 seconds

        ///////////////////////////
        public const int ARM_LENGTH_WINDOW = 5 * 30;    //5 seconds

        //////////////////////////

        public const double GRAVITATIONAL_ACCELERATION = 9.81;

        //sensor sensitivity setting
        public const double ACC_SENSITIVITY =  //g per unit
            //0.06/1000;    //for full scale = 2 g
            //0.12/1000;    //for full scale = 4 g
            0.18/1000;    //for full scale = 6 g
            //0.24/1000;    //for full scale = 8 g
            //0.73/1000;    //for full scale = 16 g

        public const double GYRO_SENSITIVITY =  //rad per sec per unit
            //8.75 / 1000 * Math.PI / 180;    //for full scale = 250 dps
            // 17.50/1000* Math.PI / 180;   //for full scale = 500 dps
             70.0/1000* Math.PI / 180;      //for full scale = 2000 dps

        public const double MAG_SENSITIVITY =  //mgauss per unit
            0.080;    //for full scale = 2 gauss
            //0.160;    //for full scale = 4 gauss
            //0.320;    //for full scale = 8 gauss
            //0.479;    //for full scale = 12 gauss
    }
}
