using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncAll
{
	public class EnableSetting
	{
		bool eTimestamp = false;
		bool eQuaternionFull = false;
		bool eQuaternionCompact = false;
		bool eForearmTilt = false;
		bool ePressure = false;
		bool eCapTouch = false;
		bool eAcc = false;
		bool eGyro = false;
		bool eMag = false;
		bool eButton = false;
		bool eBattery = false;
		bool eWorn = false;

		//offset
		int oTimestamp = 0;
		int oQuaternionFull = 0;
		int oQuaternionCompact = 0;
		int oForearmTilt = 0;
		int oPressure = 0;
		int oCapTouch = 0;
		int oAcc = 0;
		int oGyro = 0;
		int oMag = 0;
		int oButton = 0;
		int oBattery = 0;
		int oWorn = 0;

		public byte sampleSize = 0;
		public byte en1 = 0, en2 = 0;


		public EnableSetting(bool eTimestamp, bool eQuaternionFull, bool eQuaternionCompact, bool eForearmTilt, bool ePressure, bool eCapTouch, bool eAcc, bool eGyro, bool eMag, bool eButton, bool eBattery, bool eWorn)
		{
			this.eTimestamp = eTimestamp;
			this.eQuaternionFull = eQuaternionFull;
			this.eQuaternionCompact = eQuaternionCompact;
			this.eForearmTilt = eForearmTilt;
			this.ePressure = ePressure;
			this.eCapTouch = eCapTouch;
			this.eAcc = eAcc;
			this.eGyro = eGyro;
			this.eMag = eMag;
			this.eButton = eButton;
			this.eBattery = eBattery;
			this.eWorn = eWorn;

			sampleSize = 0;

			en1 = 0x00;
			if (eTimestamp) {		en1 |= (1 << 7); oTimestamp = sampleSize; sampleSize += 4; }
			if (eQuaternionFull) {	en1 |= (1 << 6); oQuaternionFull = sampleSize; sampleSize += 16; }
			if (eQuaternionCompact){en1 |= (1 << 5); oQuaternionCompact = sampleSize; sampleSize += 12; }
			if (eForearmTilt) {		en1 |= (1 << 4); oForearmTilt = sampleSize; sampleSize += 4; }
			if (ePressure) {		en1 |= (1 << 3); oPressure = sampleSize; sampleSize += 4; }
			if (eCapTouch) {		en1 |= (1 << 2); oCapTouch = sampleSize; sampleSize += 4; }
			if (eAcc) {				en1 |= (1 << 1); oAcc = sampleSize; sampleSize += 6; }
			if (eGyro) {			en1 |= (1 << 0); oGyro = sampleSize; sampleSize += 6; }

			en2 = 0x00;
			if (eMag) {		en2 |= (1 << 7); oMag = sampleSize; sampleSize += 6; }
			if (eButton) {	en2 |= (1 << 6); oButton = sampleSize; sampleSize += 1; }
			if (eBattery) { en2 |= (1 << 5); oBattery = sampleSize; sampleSize += 1; }
			if (eWorn) {	en2 |= (1 << 4); oWorn = sampleSize; sampleSize += 1; }

			if(sampleSize<=1)
			{
				sampleSize = 2;	//sample size cannot be 1
			}

			//sampleSize cannot be larger than 29 bytes with limitation of buffer in CC2530
		}

		public RawSample extractFromByteArray(byte[] buffer, int offset)
		{
			RawSample r = new RawSample();
			if (eTimestamp)
			{
				//Console.WriteLine("{0:x}-{1:x}-{2:x}-{3:x}", buffer[offset + oTimestamp], buffer[offset + oTimestamp+1], buffer[offset + oTimestamp+2], buffer[offset + oTimestamp + 3]);
				r.timestamp=BitConverter.ToUInt32(buffer,offset+oTimestamp);
			}
			if (eQuaternionFull)
			{
				Quaternion q=new Quaternion();
				q.W = BitConverter.ToSingle(buffer, offset + oQuaternionFull);
				q.X = BitConverter.ToSingle(buffer, offset + oQuaternionFull+4);
				q.Y = BitConverter.ToSingle(buffer, offset + oQuaternionFull+8);
				q.Z = BitConverter.ToSingle(buffer, offset + oQuaternionFull+12);

				r.orientation = q;
			}
			if (eQuaternionCompact)
			{
				Quaternion q = new Quaternion();
				q.X = BitConverter.ToSingle(buffer, offset + oQuaternionFull);
				q.Y = BitConverter.ToSingle(buffer, offset + oQuaternionFull + 4);
				q.Z = BitConverter.ToSingle(buffer, offset + oQuaternionFull + 8);
				q.W = 1f - (q.X * q.X + q.Y * q.Y + q.Z * q.Z);
				
				r.orientation = q;
			}
			if (eForearmTilt)
			{
				r.forearmTilt = BitConverter.ToSingle(buffer, offset + oForearmTilt);
			}
			if (ePressure)
			{
				r.pressure = BitConverter.ToInt32(buffer, offset + oPressure);
			}
			if (eCapTouch)
			{
				r.capTouchUp = BitConverter.ToUInt16(buffer, offset + oCapTouch);
				r.capTouchDown = BitConverter.ToUInt16(buffer, offset + oCapTouch+2);
			}
			if (eAcc)
			{
				r.acc.x = BitConverter.ToInt16(buffer, offset + oAcc);
				r.acc.y = BitConverter.ToInt16(buffer, offset + oAcc+2);
				r.acc.z = BitConverter.ToInt16(buffer, offset + oAcc+4);
			}
			if (eGyro)
			{
				r.gyro.x = BitConverter.ToInt16(buffer, offset + oGyro);
				r.gyro.y = BitConverter.ToInt16(buffer, offset + oGyro + 2);
				r.gyro.z = BitConverter.ToInt16(buffer, offset + oGyro + 4);
			}
			if (eMag)
			{
				r.mag.x = BitConverter.ToInt16(buffer, offset + oMag);
				r.mag.y = BitConverter.ToInt16(buffer, offset + oMag + 2);
				r.mag.z = BitConverter.ToInt16(buffer, offset + oMag + 4);
			}
			if (eButton)
			{
				r.button = buffer[offset + oButton];
			}
			if (eBattery)
			{
				r.battery = buffer[offset + oBattery];
			}
			if (eWorn)
			{
				r.worn = buffer[offset + oWorn];
			}

			return r;
		}
	}
}
