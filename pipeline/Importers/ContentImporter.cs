using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GameStack.Pipeline {
	public abstract class ContentImporter {
		static Dictionary<string, Type> _importers;
		// Find all classes marked with [ContentImporter] and catalog them by extension.
		static ContentImporter () {
			_importers = new Dictionary<string, Type>();
			foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(ContentImporter).IsAssignableFrom(t) && !t.IsAbstract)) {
				var attr = Attribute.GetCustomAttribute(type, typeof(ContentTypeAttribute)) as ContentTypeAttribute;
				if (attr != null) {
					foreach (var extension in attr.InExtension.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) {
						_importers.Add(extension, type);
					}
				}
			}
		}

		public static void Process (string inputFile, string outputFolder, IDictionary<string,string> opts) {
			if (!File.Exists(inputFile) && !Directory.Exists(inputFile))
				throw new ArgumentException("Input file does not exist: " + inputFile);
			if (!Directory.Exists(outputFolder))
				Directory.CreateDirectory(outputFolder);
			
			inputFile = Path.GetFullPath(inputFile);
			outputFolder = Path.GetFullPath(outputFolder);

			var extension = Path.GetExtension(inputFile).ToLower();
			var baseName = Path.GetFileNameWithoutExtension(inputFile);

			Type type;
			_importers.TryGetValue(extension, out type);
			if (type == null) {
				Console.WriteLine("No custom importer for {0}, copying as blob.", extension);
				
				string outputFile = Path.Combine(outputFolder, baseName + extension);
				if (File.Exists(outputFile) && File.GetLastWriteTime(outputFile) > File.GetLastWriteTime(inputFile))
					return;
				File.Copy(inputFile, outputFile, true);
			} else {
				var attr = (ContentTypeAttribute)Attribute.GetCustomAttribute(type, typeof(ContentTypeAttribute));

				var importer = (ContentImporter)Activator.CreateInstance(type);
				foreach (var kvp in opts) {
					var prop = type.GetProperty(kvp.Key, BindingFlags.IgnoreCase | BindingFlags.Public);
					if (prop == null)
						throw new ArgumentException("No such property found on importer: " + kvp.Key);
					prop.SetValue(importer, Convert.ChangeType(kvp.Value, prop.PropertyType), null);
				}

				string outputFile = Path.Combine(outputFolder, baseName + (attr.OutExtension == ".*" ? extension : attr.OutExtension));
				if (File.Exists(outputFile) && File.GetLastWriteTime(outputFile) > File.GetLastWriteTime(inputFile))
					return;
			
				var oldWorkingDir = Environment.CurrentDirectory;
				using (FileStream oStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write)) {
					if (Directory.Exists(inputFile)) {
						Environment.CurrentDirectory = inputFile;
						importer.Import(inputFile, oStream);
					} else {
						using (FileStream iStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read)) {
							var dir = Path.GetDirectoryName(inputFile);
							if (dir != string.Empty)
								Environment.CurrentDirectory = Path.GetDirectoryName(inputFile);
							importer.Import(iStream, oStream, Path.GetFileName(inputFile));
						}
					}
				}
				Environment.CurrentDirectory = oldWorkingDir;
			}
		}

		public virtual void Import (Stream iStream, Stream oStream, string filepath) {
			throw new NotImplementedException();
		}
		public virtual void Import (string iDir, Stream oStream) {
			throw new NotImplementedException();
		}
	}
}
