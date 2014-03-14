using System;

namespace GameStack {
	public static class Music {
		public static IMusicChannel CreateMusicChannel () {
			return new SDL2MusicChannel ();
		}

		public static IMusicTrack LoadTrack (string path) {
			return new SDL2MusicTrack (path);
		}
	}
}
