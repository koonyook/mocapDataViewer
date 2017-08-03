using System;
using System.Collections.Generic;
using System.Text;

using Google.Protobuf;
using Google.Protobuf.MocapRecord;
using Google.Protobuf.PoseFitting;
using System.IO;
using OpenTK;

namespace SyncAll
{
    static class ProtoTool
    {
		public static SettingProto loadSettingProto(string filePath)
		{
			SettingProto ans;
			using (var input = File.OpenRead(filePath))
			{
				ans=SettingProto.Parser.ParseFrom(input);
			}
			return ans;
		}

		public static RawSampleListProto loadRawSampleListProto(string filePath)
		{
			RawSampleListProto ans;
			using (var input = File.OpenRead(filePath))
			{
				ans=RawSampleListProto.Parser.ParseFrom(input);
			}
			return ans;
		}

		public static SessionProto loadSessionProto(string filePath)
		{
			SessionProto ans;
			using (var input = File.OpenRead(filePath))
			{
				ans=SessionProto.Parser.ParseFrom(input);
			}
			return ans;
		}

		public static void saveSettingProto(SettingProto input, string filePath)
		{
			using (var output = File.Create(filePath))
			{
				input.WriteTo(output);
			}
		}

		public static void saveRawSampleListProto(RawSampleListProto input, string filePath)
		{
			using (var output = File.Create(filePath))
			{
				input.WriteTo(output);
			}
		}

		public static List<RawSample> convertRawSampleListProtoToRawSampleList(RawSampleListProto input)
		{
			List<RawSample> ans = new List<RawSample>();
			foreach(var e in input.List)
			{
				ans.Add(new RawSample(e));
			}
			return ans;
		}

		public static RawSampleListProto convertRawSampleListToRawSampleListProto(List<RawSample> rawSampleList)
		{
			RawSampleListProto ans = new RawSampleListProto();
			foreach (var e in rawSampleList)
			{
				ans.List.Add(e.getRawSampleProto());
			}

			return ans;
		}

		public static Vector3[] getVector3FromRepeatedField(Google.Protobuf.Collections.RepeatedField<float> input)
		{
			int n=input.Count/3;
			Vector3[] ans=new Vector3[n];
			for(int i=0;i<n;i++)
			{
				ans[i]=new Vector3(input[i*3],input[i*3+1],input[i*3+2]);
			}
			return ans;
		}

		public static float[] getFloatArrayFromRepeatedField(Google.Protobuf.Collections.RepeatedField<float> input)
		{
			float[] ans=new float[input.Count];
			for(int i=0;i<input.Count;i++)
			{
				ans[i]=input[i];
			}
			return ans;
		}
    }
}
