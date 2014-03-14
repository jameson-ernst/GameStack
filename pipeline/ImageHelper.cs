using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace GameStack.Pipeline {
	public static class ImageHelper {
		public unsafe static Image PremultiplyAlpha (Image img) {
			var bmp = new Bitmap(img);
			var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			var p = (byte*)data.Scan0;
			var pEnd = p + data.Height * data.Stride;
			while (p < pEnd) {
				for (var i = 0; i < 3; i++) {
					int a = *(p + 3);
					if (a >= 128)
						a += 1;
					*(p + i) = (byte)((a * *(p + i) + 128) >> 8);
				}
				p += 4;
			}
			bmp.UnlockBits(data);
			return (Image)bmp;
		}
	}
}
