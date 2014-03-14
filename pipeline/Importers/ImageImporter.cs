using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameStack.Pipeline {
	[ContentType (".image", ".png")]
	public class ImageImporter : ContentImporter {
		public override void Import (Stream input, Stream output, string extension) {
			var ser = new JsonSerializer();
			Metadata metadata = null;
			using (var sr = new StreamReader(input)) {
				metadata = ser.Deserialize<Metadata>(new JsonTextReader(sr));
			}

			var img = Image.FromFile(metadata.Path);
			if (metadata.PreMultiply) {
				img = ImageHelper.PremultiplyAlpha(img);
			}
			img.Save(output, ImageFormat.Png);
		}
	}

	class Metadata {
		[JsonProperty("path")]
		public string Path { get; set; }

		[JsonProperty("preMultiply")]
		public bool PreMultiply { get; set; }
	}
}
