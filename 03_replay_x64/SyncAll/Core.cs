using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

using OpenTK;
using Microsoft.Kinect;

namespace SyncAll
{
    //don't look at this core, I use TickCore (minimal core)
	/*
    public class Core
    {
        private MainWindow mainWindow;
        private Space space;
		CancellationTokenSource cts = new CancellationTokenSource();
		private Thread myThread;

		public Core(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            this.space = mainWindow.gwin.space;
        }

        public void start()
        {
            myThread = new Thread(new ThreadStart(mainLoop));
            myThread.Start();
        }

        //bool mainLoopIsDone = false;
        public void stop()
        {
            cts.Cancel();
            myThread.Join();
        }

        public delegate void UpdateFrameDelegate();
        public void updateFrame()
        {
			mainWindow.gwin.updateFrame();
        }

		public void mainLoop()
		{
			while (!cts.IsCancellationRequested)
			{
				UpdateFrameDelegate updateFrameDelegate = new UpdateFrameDelegate(this.updateFrame);
				mainWindow.Dispatcher.BeginInvoke(updateFrameDelegate);
			}
		}                   
    }
	*/
}
