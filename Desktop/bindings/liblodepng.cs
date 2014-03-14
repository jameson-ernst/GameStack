using System;
using System.Runtime.InteropServices;

namespace GameStack.Bindings {
	public class LibLodePng {
		[DllImport ("lodepng", EntryPoint = "lodepng_decode32")]
		public static extern int Decode32 (out IntPtr image, out int width, out int height, byte[] data, int size);

		[DllImport ("lodepng", EntryPoint = "lodepng_error_text")]
		public static extern string ErrorText (int error);

		[DllImport ("libc", EntryPoint = "free")]
		public static extern int Free (IntPtr p);
	}
}
