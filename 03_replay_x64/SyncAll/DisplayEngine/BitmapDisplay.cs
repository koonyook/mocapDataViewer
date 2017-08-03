using System;
using System.Collections.Generic;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SyncAll
{
	public class BitmapDisplay
	{
		Image targetImage;
		int width = 512;
		int height = 424;

		WriteableBitmap wbitmap;

		public BitmapDisplay(Image targetImage)
		{
			wbitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
			this.targetImage = targetImage;
			targetImage.Source = wbitmap;

			//init with black color
			byte[] table = new byte[424 * 512 * 3];
			for (int i = 0; i < 424; i++)
			{
				for (int j = 0; j < 512; j++)
				{
					table[(i * 512 + j) * 3 + 0] = (byte)((i + j) % 256);
					table[(i * 512 + j) * 3 + 1] = (byte)((i + j) % 256);
					table[(i * 512 + j) * 3 + 2] = (byte)((i + j) % 256);
				}
			}
			update(table);
		}

		public void updateDepthMap(float[] fmap)
		{
			if (fmap == null)
				return;

			byte[] table = new byte[424 * 512 * 3];
			for (int i = 0; i < 424; i++)
			{
				for (int j = 0; j < 512; j++)
				{
					float depth = fmap[i * 512 + j];
					if (depth > 4)
					{
						table[(i * 512 + j) * 3 + 0] = 0;
						table[(i * 512 + j) * 3 + 1] = 0;
						table[(i * 512 + j) * 3 + 2] = 0;
					}
					else if (depth > 3)
					{
						table[(i * 512 + j) * 3 + 0] = (byte)(256 * (4 - depth));
						table[(i * 512 + j) * 3 + 1] = 0;
						table[(i * 512 + j) * 3 + 2] = 0;
					}
					else if (depth > 2)
					{
						table[(i * 512 + j) * 3 + 0] = 255;
						table[(i * 512 + j) * 3 + 1] = (byte)(256 * (3 - depth)); ;
						table[(i * 512 + j) * 3 + 2] = 0;
					}
					else if (depth > 1)
					{
						table[(i * 512 + j) * 3 + 0] = 255;
						table[(i * 512 + j) * 3 + 1] = 255;
						table[(i * 512 + j) * 3 + 2] = (byte)(256 * (2 - depth));
					}
					else
					{
						table[(i * 512 + j) * 3 + 0] = 255;
						table[(i * 512 + j) * 3 + 1] = 255;
						table[(i * 512 + j) * 3 + 2] = 255;
					}
				}
			}
			update(table);
		}

		public void update(byte[] colorTable)
		{
			Int32Rect rect = new Int32Rect(0, 0, width, height);
			int stride = 3 * width;
			wbitmap.WritePixels(rect, colorTable, stride, 0);
		}
	}
}
