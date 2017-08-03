using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SyncAll
{
	//this class is complete, do not need to config anything
    public static class ViconControl
    {
		static string ip="192.168.0.1";
		//static string ip="169.254.213.199";	//loopback address

		static string FOLDER_PATH="";	//this will save the record to the current directory (on the remote machine)

		static int PACKET_ID=0;				//this value must continue from previous run
		public static string CAPTURE_NAME="";	//this will be a timestamp
		public static string DESCRIPTION="Mr.Abc Sequence0";
		public static string SUBJECTNAME="";

		static int port=30;

		const int targetLength=256;	//cannot be more than 259 (to work with NIE mocap lab)

		static public void resetPacketID()
		{
			PACKET_ID=0;
		}

		static public void start()	
		{
			IPEndPoint RemoteEndPoint= new IPEndPoint(IPAddress.Parse(ip), port);
			Socket server = new Socket(AddressFamily.InterNetwork,SocketType.Dgram, ProtocolType.Udp);
			string messageStart = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?><CaptureStart><Name VALUE=\"" + CAPTURE_NAME + "\"/><Notes VALUE=\"\"/><Description VALUE=\""+ DESCRIPTION +"\"/><DatabasePath VALUE=\"" + FOLDER_PATH + "\"/><Delay VALUE=\"\"/><PacketID VALUE=\"" + PACKET_ID.ToString() + "\"/></CaptureStart>";
			//Debug.WriteLine(messageStart.Length);
			var data = Encoding.ASCII.GetBytes(messageStart.PadRight(targetLength));
			server.SendTo(data, data.Length, SocketFlags.None, RemoteEndPoint);
			server.Close();

			Debug.WriteLine("UDP Start:"+PACKET_ID + " length:"+messageStart.Length);
			PACKET_ID++;
		}

		
		static public void stop()	
		{
			IPEndPoint RemoteEndPoint= new IPEndPoint(IPAddress.Parse(ip), port);
			Socket server = new Socket(AddressFamily.InterNetwork,SocketType.Dgram, ProtocolType.Udp);
			string messageStop = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?><CaptureStop RESULT=\"SUCCESS\"><Name VALUE=\"" + CAPTURE_NAME + "\"/><DatabasePath VALUE=\"" + FOLDER_PATH + "\"/><Delay VALUE=\"\"/><PacketID VALUE=\"" + PACKET_ID.ToString() + "\"/></CaptureStop>";
			var data = Encoding.ASCII.GetBytes(messageStop.PadRight(targetLength));
			server.SendTo(data, data.Length, SocketFlags.None, RemoteEndPoint);
			server.Close();
			
			Debug.WriteLine("UDP Stop:"+PACKET_ID + " length:"+messageStop.Length);
			PACKET_ID+=1;
		}
		
		
		static public void loadCurrentPacketID()
		{
			if(File.Exists("packetID.txt"))
			{
				PACKET_ID=int.Parse(File.ReadAllText("packetID.txt"));
			}
			else
			{
				PACKET_ID=0;
			}
		}

		static public void saveCurrentPacketID()
		{
			File.WriteAllText("packetID.txt",PACKET_ID.ToString());
		}


    }
}
