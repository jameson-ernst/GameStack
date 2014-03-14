using System;
using Android.Content.Res;
using Android.Media;
using Java.IO;

namespace GameStack {
	public class AndroidMusicChannel : IMusicChannel {
		MediaPlayer _player;
		AndroidMusicTrack _music;
		float _volume;

		public AndroidMusicChannel () {
			_volume = 1.0f;
		}

		public IMusicTrack CurrentMusic {
			get { return _music; }
		}

		public bool IsPlaying {
			get { return _player != null && _player.IsPlaying; }
		}

		public float Volume {
			get { return _volume; }
			set {
				_volume = value;
				if (_player != null)
					_player.SetVolume (_volume, _volume);
			}
		}

		void Reset () {
			if (_player == null)
				_player = new MediaPlayer ();
			else
				_player.Reset ();
			_player.SetAudioStreamType (Stream.Music);
			_player.SetVolume (_volume, _volume);
		}

		public bool Play (IMusicTrack music, bool loop = false) {

			var aMusic = music as AndroidMusicTrack;
			if (aMusic == null)
				throw new ArgumentException ("Music must be an AndroidMusicTrack.", "music");
			_music = aMusic;

			Reset ();
			_player.Looping = loop;
			var asset = aMusic.Asset;
			_player.SetDataSource (asset.FileDescriptor, asset.StartOffset, asset.Length);
			_player.Prepare ();
			_player.Start ();

			return _player.IsPlaying;
		}

		public bool Play () {
			_player.Start ();
			return _player.IsPlaying;
		}

		public void Pause () {
			_player.Pause ();
		}

		public void Stop () {
			_player.Stop ();
		}

		public void Dispose () {
			_player.Dispose ();
		}
	}

	public class AndroidMusicTrack : IMusicTrack {
		public AssetFileDescriptor Asset { get; private set; }

		public AndroidMusicTrack (string path) {
			this.Asset = Assets.ResolveFd (path);
		}

		public void Dispose () {
		}
	}
}
