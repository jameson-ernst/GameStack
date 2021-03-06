using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace GameStack.Pipeline {
	public static class ImageHelper {
		public unsafe static Image PremultiplyAlpha (Image img) {
			// Mono on linux premultiplies images automatically, only do this if running on mac.
			if (!Extensions.IsRunningOnMac)
				return img;

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
			return bmp;
		}


		public static Image ResizeImage (Image img, string path, Size maxSize) {
			// Mono for mac uses a better system.drawing implementation; fall back to graphicsmagick on linux
			if (Extensions.IsRunningOnMac) {
				var maxAspect = (float)maxSize.Width / (float)maxSize.Height;
				var imgAspect = (float)img.Width / (float)img.Height;

				if (imgAspect > maxAspect)
					return new Bitmap(img, new Size(maxSize.Width, (int)Math.Round(maxSize.Width / imgAspect)));
				else
					return new Bitmap(img, new Size((int)Math.Round(maxSize.Height * imgAspect), maxSize.Height));
			} else {
				img.Dispose();
				var wand = GraphicsMagick.NewWand();
				GraphicsMagick.ReadImageBlob(wand, File.OpenRead(path));

				var maxAspect = (float)maxSize.Width / (float)maxSize.Height;
				var imgAspect = (float)GraphicsMagick.GetWidth(wand) / (float)GraphicsMagick.GetHeight(wand);

				if (imgAspect > maxAspect)
					GraphicsMagick.ResizeImage(wand, (IntPtr)maxSize.Width, (IntPtr)Math.Round(maxSize.Width / imgAspect), GraphicsMagick.Filter.Box, 1);
				else
					GraphicsMagick.ResizeImage(wand, (IntPtr)Math.Round(maxSize.Height * imgAspect), (IntPtr)maxSize.Height, GraphicsMagick.Filter.Box, 1);
				var newImgBlob = GraphicsMagick.WriteImageBlob(wand);

				using (var ms = new MemoryStream(newImgBlob)) {
					return Image.FromStream(ms);
				}
			}
		}
	}
}
