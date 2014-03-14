using System;
using System.IO;
using Android.Content;
using Android.Content.Res;
using Java.IO;

namespace GameStack {
	public static class Assets {
		const string AssetBasePath = "";
		static Context _context;

		public static void SetContext (Context context) {
			_context = context;
		}

		public static Stream ResolveStream (string path) {
			AssetManager am = _context.Assets;
			return am.Open (path);
		}

		public static AssetFileDescriptor ResolveFd (string path) {
			return _context.Assets.OpenFd (path);
		}
	}
}
