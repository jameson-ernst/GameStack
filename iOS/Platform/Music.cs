using System;

namespace GameStack {
	public static class Music {
		public static IMusicChannel CreateMusicChannel () {
			return new iOSMusicChannel ();
		}

		public static IMusicTrack LoadTrack (string path) {
			return new AvfMusicTrack (path);
		}
	}
}
