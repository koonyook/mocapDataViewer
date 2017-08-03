using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SyncAll
{
	public class TRCreader
	{
		public float dataRate;
		public List<string> markerNameList = new List<string>();
		public List<Dictionary<string, Vector3>> record = new List<Dictionary<string, Vector3>>();

		char[] separator = new char[] {'\t'};

		public TRCreader(string filename)
		{
			int counter = 0;
			string line;

			// Read the file and display it line by line.
			System.IO.StreamReader file = new System.IO.StreamReader(filename);
			while((line = file.ReadLine()) != null)
			{
				//Console.WriteLine (line);
				counter++;
				if(counter==3)  //read dataRate
				{
					dataRate=float.Parse(line.Split(separator)[0]);
				}
				else if(counter==4) //read all marker's name
				{
					string[] longNames = line.Trim().Split(separator);
					foreach (string s in longNames)
					{
						if(s=="Frame#" || s=="Time" || s=="")
						{
							continue;
						}
						else if(s.Contains(":"))
						{
							markerNameList.Add(s.Split(new char[] {':'})[1]);	//pick sting after ':'
						}
						else
						{
							markerNameList.Add(s);
						}
					}
				}
				else if(counter>=6)	//read marker position
				{
					string[] numbers = line.Trim().Split(separator);
					Dictionary<string, Vector3> row = new Dictionary<string, Vector3>();
					for(int i=0;i<markerNameList.Count;i++)
					{
						if(numbers[2+i*3+0]=="" && numbers[2+i*3+1]=="" && numbers[2+i*3+2]=="")
						{
							continue;	//just skip this marker (normally happen at the early or late frame)
						}
						else
						{ 
							row[markerNameList[i]] = new Vector3(
								float.Parse(numbers[2+i*3+0])/1000,	//2 first columns for Frame# and Time 
								float.Parse(numbers[2+i*3+1])/1000,	//change from mm to m
								float.Parse(numbers[2+i*3+2])/1000
							);
						}
					}
					record.Add(row);
				}
			}
			file.Close();
		}
	}
}
