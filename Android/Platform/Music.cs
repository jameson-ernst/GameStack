using System;
using Android.Content.Res;

namespace GameStack {
	public static class Music {
		public static IMusicChannel CreateMusicChannel () {
			return new AndroidMusicChannel ();
		}

		public static IMusicTrack LoadTrack (string path) {
			return new AndroidMusicTrack (path);
		}
	}
}
