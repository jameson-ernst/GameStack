using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using GameStack.Pipeline.Atlas;
using GameStack.Pipeline.Tar;

namespace GameStack.Pipeline {
	[ContentType(".sprites", ".atlas")]
	public class SpriteImporter : ContentImporter {
		public override void Import (Stream input, Stream output, string extension) {
			var ser = new JsonSerializer();
			Metadata metadata = null;
			using (var sr = new StreamReader(input)) {
				metadata = ser.Deserialize<Metadata>(new JsonTextReader(sr));
			}

			var lp = new LayoutProperties {
				inputFilePaths = Directory.GetFiles(metadata.Path).Where(p => Path.GetExtension(p).ToLower() == ".png").ToArray(),
				distanceBetweenImages = metadata.Padding,
				marginWidth = metadata.Margin,
				powerOfTwo = !metadata.NoPowerOfTwo
			};
			var sheetMaker = new SpriteBuilder(lp);

			using (var s = output) {
				using (var tw = new TarWriter(s)) {
					using (MemoryStream atlasStream = new MemoryStream(), sheetStream = new MemoryStream()) {
						using (var bw = new BinaryWriter(atlasStream)) {
							sheetMaker.Create(bw, sheetStream, ImageFormat.Png, metadata.NoPreMultiply);
							bw.Flush();
							atlasStream.Position = 0;
							sheetStream.Position = 0;
							tw.Write(atlasStream, atlasStream.Length, "atlas.bin");
							tw.Write(sheetStream, sheetStream.Length, "sheet.png");
						}
					}
				}
			}
		}

		class Metadata {
			[JsonProperty("path")]
			public string Path { get; set; }

			[JsonProperty("padding")]
			public int Padding { get; set; }

			[JsonProperty("margin")]
			public int Margin { get; set; }

			[JsonProperty("noPreMultiply")]
			public bool NoPreMultiply { get; set; }

			[JsonProperty("noPowerOfTwo")]
			public bool NoPowerOfTwo { get; set; }
		}
	}
}
