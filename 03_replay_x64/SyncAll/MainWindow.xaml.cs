using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;
using OpenTK;
using System.IO;

namespace SyncAll
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml 
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
		public TickCore tickCore;
		public OfflineBridge bridge;

        //public Core core;
        //public KinectInput kinect;

        public GraphicWindow gwin;

        public string textToDisplay="none";

		public MainWindow()
        {
            InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            gwin = new GraphicWindow(RenderMode.SCREEN,wHost, null, true);	//depthImage
            while(!gwin.loaded)
            {
                Debug.Write("-");
            }
            On_GLControl_Load_Complete();
        }
        

        public async void On_GLControl_Load_Complete()
        {
			//gwin.actionList.Add(() => { detailText.Text = textToDisplay; });

            //don't use sensor at all
            gwin.startToEnableCameraControl();

			//find current path
			string upOneStep = System.AppDomain.CurrentDomain.BaseDirectory+"..\\";

			if(Directory.Exists(upOneStep+"measure") && Directory.Exists(upOneStep+"mocapRecord"))
			{
				Global.rootDirectory=upOneStep;
				textBlock.Text = upOneStep+"mocapRecord\\upperBody\\";
				listAllRecords(textBlock.Text);
				button.IsEnabled=false;
			}
			else 
			{
				if(Directory.Exists(Global.rootDirectory+"measure") && Directory.Exists(Global.rootDirectory+"mocapRecord"))
				{
					textBlock.Text = Global.rootDirectory+"mocapRecord\\upperBody\\";
					listAllRecords(textBlock.Text);
					button.IsEnabled=false;
				}
				else
				{
					textBlock1.Text = "Sorry, please browse the root directory.";
				}
			}
			
        }

		private void timelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			frameIndexText.Text = ((int)e.NewValue).ToString();
			tickCore.changeTime((int)e.NewValue);
		}

		public bool isPlaying = false;

		private void playButton_Click(object sender, RoutedEventArgs e)
		{
			if(comboBox.SelectedIndex>0);
				isPlaying = !isPlaying;
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if(gwin!=null)
				this.gwin.hasNewUpdate.state = true;
		}

		private void button5_Click(object sender, RoutedEventArgs e)
		{
			gwin.space.isGoingToTakeScreenShot=true;
			gwin.hasNewUpdate.state = true;
		}

		private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{

			//textBlock.Text = e.NewValue.ToString();
			bridge.vBridge.kinectFrameOffset = (float) e.NewValue;
			bridge.vBridge.kinectTimestampFirstViconFrame = MyMath.doubleInterpolate(bridge.timestamp, e.NewValue);

			tickCore.changeTime((int)timelineSlider.Value,true);
			//gwin.hasNewUpdate.set();
			
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
			{
				System.Windows.Forms.DialogResult result = dialog.ShowDialog();
				if(result.ToString()=="OK")
				{ 
					if(Directory.Exists(dialog.SelectedPath+"\\mocapRecord\\upperBody"))
					{ 
						Global.rootDirectory=dialog.SelectedPath+"\\";
						textBlock.Text = dialog.SelectedPath+"\\mocapRecord\\upperBody\\";
						listAllRecords(textBlock.Text);
						textBlock1.Text="ready";
						button.IsEnabled=false;
					}
					else
					{
						textBlock1.Text="Sorry, wrong directory.";
					}
				}
			}
		}

		public void listAllRecords(string path)
		{
			string[] dirList = Directory.GetDirectories(path);
			comboBox.Items.Clear();
			comboBox.Items.Add("");
			foreach(string e in dirList)
			{
				comboBox.Items.Add(e.Split(new char[] {'\\'}).Last());
			}
		}

		private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(comboBox.SelectedItem.ToString()!="")
			{ 
				string targetDirectory = textBlock.Text+"\\"+comboBox.SelectedItem.ToString()+"\\";

				if(tickCore==null)
				{ 
					//start a minimal core here
					bridge = new OfflineBridge(targetDirectory,Global.rootDirectory);
					timelineSlider.Maximum = bridge.visualCount-1;
					timelineSlider.Value=0;
					tickCore = new TickCore(this,33,bridge);
					tickCore.start();
				}
				else
				{
					tickCore.stop();
					bridge = new OfflineBridge(targetDirectory,Global.rootDirectory);
					timelineSlider.Maximum = bridge.visualCount-1;
					timelineSlider.Value=0;
					tickCore.bridge = bridge;
					tickCore.start();
				}
			}
			else
			{
				return;
			}
		}


	}
}
