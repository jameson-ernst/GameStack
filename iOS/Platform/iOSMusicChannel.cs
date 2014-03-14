using System;
using MonoTouch.Foundation;
using MonoTouch.AVFoundation;

namespace GameStack {
	public class iOSMusicChannel : IMusicChannel {
		AVAudioPlayer _player;
		AvfMusicTrack _music;
		float _volume;

		public iOSMusicChannel () {
			_volume = 1.0f;
		}

		public IMusicTrack CurrentMusic {
			get { return _music; }
		}

		public bool IsPlaying {
			get { return _player != null && _player.Playing; }
		}

		public float Volume {
			get { return _volume; }
			set {
				_volume = value;
				if (_player != null)
					_player.Volume = _volume;
			}
		}

		public bool Play (IMusicTrack music, bool loop) {
			var avfMusic = (AvfMusicTrack)music;
			if (avfMusic == null)
				throw new ArgumentException ("Music must be an AvfBackgroundMusic object.", "music");
			_music = avfMusic;

			if (_player != null)
				_player.Dispose ();

			_player = AVAudioPlayer.FromUrl (new NSUrl (Assets.ResolvePath (avfMusic.Path)));
			_player.Volume = _volume;
			_player.NumberOfLoops = loop ? -1 : 0;
			return _player.Play ();
		}

		public bool Play () {
			if (_player == null)
				return false;

			return _player.Play ();
		}

		public void Pause () {
			if (!IsPlaying)
				return;

			_player.Pause ();
		}

		public void Stop () {
			if (!IsPlaying)
				return;

			_player.Stop ();
		}

		public void Dispose () {
			if (_player != null)
				_player.Dispose ();
			_player = null;
		}
	}

	public class AvfMusicTrack : IMusicTrack {
		string _path;

		internal AvfMusicTrack (string path) {
			_path = path;
		}

		public string Path { get { return _path; } }

		public void Dispose () {
		}
	}
}
