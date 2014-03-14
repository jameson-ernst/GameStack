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
			if (!File.Exists(inputFile))
				throw new ArgumentException("Input file does not exist: " + inputFile);
			if (!Directory.Exists(outputFolder))
				Directory.CreateDirectory(outputFolder);

			var extension = Path.GetExtension(inputFile);
			var baseName = Path.GetFileNameWithoutExtension(inputFile);

			Type type;
			_importers.TryGetValue(extension, out type);
			if (type == null) {
				Console.WriteLine("No custom importer for {0}, skipping.", extension);
				return;
			}
			var attr = (ContentTypeAttribute)Attribute.GetCustomAttribute(type, typeof(ContentTypeAttribute));

			var importer = (ContentImporter)Activator.CreateInstance(type);
			foreach (var kvp in opts) {
				var prop = type.GetProperty(kvp.Key, BindingFlags.IgnoreCase | BindingFlags.Public);
				if (prop == null)
					throw new ArgumentException("No such property found on importer: " + kvp.Key);
				prop.SetValue(importer, Convert.ChangeType(kvp.Value, prop.PropertyType), null);
			}

			var oldWorkingDir = Environment.CurrentDirectory;
			using (FileStream iStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read),
			       oStream = new FileStream(Path.Combine(outputFolder, baseName + (attr.OutExtension == ".*" ? extension : attr.OutExtension)),
				                          FileMode.Create, FileAccess.Write)) {
				var dir = Path.GetDirectoryName(inputFile);
				if(dir != string.Empty)
					Environment.CurrentDirectory = Path.GetDirectoryName(inputFile);
				importer.Import(iStream, oStream, extension);
				Environment.CurrentDirectory = oldWorkingDir;
			}
		}

		public abstract void Import (Stream input, Stream output, string extension);
	}
}
