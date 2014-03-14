using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameStack.Pipeline;

namespace GameStack.Tools.Import {
	class MainClass {
		static readonly char[] SplitChar = new char[] { ' ' };

		public static int Usage () {
			Console.WriteLine(string.Format(@"Usage:
    {0} <inputFile|inputFolder> <outputFolder> [PROPERTY=VAL]...",
				Assembly.GetExecutingAssembly().GetName().Name));
			return -1;
		}

		public static int Main (string[] args) {
			if (args.Length < 2) {
				return Usage();
			}
			var opts = new Dictionary<string,string>();
			for (var i = 2; i < args.Length; i++) {
				var parts = args[i].Split(SplitChar, 2);
				if (parts.Length < 2)
					return Usage();
				opts[parts[0]] = parts[1];
			}

			string[] paths = new[] { args[0] };
			if (Directory.Exists(args[0])) {
				paths = Directory.GetFiles(args[0]).Where(f => !Path.GetFileName(f).StartsWith(".")).ToArray();
			}

			try {
				foreach (var path in paths) {
					ContentImporter.Process(path, args[1], opts);
				}
			} catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				return -1;
			}
			return 0;
		}
	}
}
