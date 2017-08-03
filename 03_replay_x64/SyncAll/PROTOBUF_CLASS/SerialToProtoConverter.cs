using System;
using System.Collections.Generic;
using System.Text;

using Google.Protobuf;
using Google.Protobuf.MocapRecord;
using System.Linq;
using System.IO;

namespace SyncAll
{
    public static class SerialToProtoConverter
    {
		public static void convertSettingAndSave(Dictionary<string,object> setting, string savingDirectory)
		{
			SettingProto ans = new SettingProto();
			if(setting.Keys.Contains("baseID"))		ans.BaseID=(byte)setting["baseID"];
			if(setting.Keys.Contains("leftID"))		ans.LeftID=(byte)setting["leftID"];
			if(setting.Keys.Contains("rightID"))	ans.RightID=(byte)setting["rightID"];

			ans.BodyIndexBuffer=(bool)setting["bodyIndexBuffer"];
			ans.DepthBuffer=(bool)setting["depthBuffer"];
			ans.ColorMode=(SettingProto.Types.ColorRecordModeProto)setting["colorMode"];	//not sure
			ans.IrMode=(bool)setting["irMode"];

			using (var output = File.Create(savingDirectory+"setting.SettingProto"))
			{
				ans.WriteTo(output);
			}
		}

		public static void convertRawSampleListAndSave(List<RawSample> rawSampleList, string savingDirectory)
		{
			RawSampleListProto ans = new RawSampleListProto();
			foreach (var e in rawSampleList)
			{
				ans.List.Add(e.getRawSampleProto());
			}

			using (var output = File.Create(savingDirectory+"RawSample.RawSampleListProto"))
			{
				ans.WriteTo(output);
			}
		}

		/*
		public static RawSample convertRawSampleProtoToRawSample(RawSampleProto input)
		{
			RawSample ans = new RawSample();
			ans.blockNumber=(byte)input.BlockNumber;
			ans.deviceID=(byte)input.DeviceID;
			ans.timestamp=input.Timestamp;
			ans.orientation.W=input.Orientation.W;
			ans.orientation.X=input.Orientation.X;
			ans.orientation.Y=input.Orientation.Y;
			ans.orientation.Z=input.Orientation.Z;
			ans.forearmTilt=input.ForearmTilt;
			ans.pressure=input.Pressure;
			ans.capTouchUp=(UInt16)input.CapTouchUp;
			ans.capTouchDown=(UInt16)input.CapTouchDown;
			ans.acc.x=(Int16)input.Acc.X;
			ans.acc.y=(Int16)input.Acc.Y;
			ans.acc.z=(Int16)input.Acc.Z;
			ans.gyro.x=(Int16)input.Gyro.X;
			ans.gyro.y=(Int16)input.Gyro.Y;
			ans.gyro.z=(Int16)input.Gyro.Z;
			ans.mag.x=(Int16)input.Mag.X;
			ans.mag.y=(Int16)input.Mag.Y;
			ans.mag.z=(Int16)input.Mag.Z;
			ans.button=(byte)input.Button;
			ans.battery=(byte)input.Battery;
			ans.worn=(byte)input.Worn;
			ans.btime=input.Btime;
			ans.ktime=input.Ktime;
			return ans;
		}
		*/

		
    }
}
