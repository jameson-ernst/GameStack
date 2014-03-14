using System;

namespace GameStack {
	public interface IMusicChannel : IDisposable {
		IMusicTrack CurrentMusic { get; }

		bool IsPlaying { get; }

		float Volume { get; set; }

		bool Play (IMusicTrack music, bool loop = false);

		bool Play ();

		void Pause ();

		void Stop ();
	}
}
