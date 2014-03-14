using System;
using System.Threading;
using NLayer;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace GameStack {
	public class SDL2MusicChannel : IMusicChannel {
		Thread _thread;
		int _source;
		int[] _buffers;
		int _bufCursor;
		float[] _readBuffer;
		ALSourceState _userState;
		long _pcmCursor;

		volatile SDL2MusicTrack _music;
		volatile bool _isLooping;
		volatile bool _isDisposed;

		public SDL2MusicChannel () {
			_readBuffer = new float[5292 * 2];
			_music = null;

			_source = AL.GenSource();
			_buffers = AL.GenBuffers(4);

			_thread = new Thread(ThreadProc);
			_thread.Start();
		}

		void ThreadProc () {
			while (!_isDisposed) {
				var state = AL.GetSourceState(_source);
				if (state != _userState) {
					switch (_userState) {
						case ALSourceState.Playing:
							AL.SourcePlay(_source);
							break;
						case ALSourceState.Paused:
							AL.SourcePause(_source);
							break;
						case ALSourceState.Stopped:
							if (AL.GetSourceState(_source) != ALSourceState.Initial)
								AL.SourceStop(_source);
							break;
					}
				}

				if (state == ALSourceState.Playing)
					EnqueueBuffers();

				Thread.Sleep(120);
			}
		}

		bool EnqueueBuffers () {
			int nProcessed;
			AL.GetSource(_source, ALGetSourcei.BuffersProcessed, out nProcessed);
			for (int i = 0; i < nProcessed; i++)
				AL.SourceUnqueueBuffer(_source);

			int nQueued;
			var oldPcmCursor = _pcmCursor;
			AL.GetSource(_source, ALGetSourcei.BuffersQueued, out nQueued);
			for (int i = 0; i < 4 - nQueued; i++) {
				int read = _music.FillBuffer(_readBuffer, _pcmCursor, 5292);
				if (read == 0)
					if (_isLooping)
						_pcmCursor = 0;
					else
						break;
				_pcmCursor += read * 2;

				AL.BufferData(_buffers[_bufCursor], ALFormat.StereoFloat32Ext, _readBuffer, 5292 * sizeof(float), _music.SampleRate);
				AL.SourceQueueBuffer(_source, _buffers[_bufCursor]);
				_bufCursor = (_bufCursor + 1) & 3;
			}

			return _pcmCursor != oldPcmCursor;
		}

		public IMusicTrack CurrentMusic {
			get { return _music; }
		}

		public bool IsPlaying {
			get { return AL.GetSourceState(_source) == ALSourceState.Playing; }
		}

		public float Volume {
			get {
				float volume;
				AL.GetSource(_source, ALSourcef.Gain, out volume);
				return volume;
			}
			set {
				AL.Source(_source, ALSourcef.Gain, value);
			}
		}

		public bool Play (IMusicTrack music, bool loop) {
			var sdlMusic = music as SDL2MusicTrack;
			if (sdlMusic == null)
				throw new ArgumentException("Music must be an SDL2 music track.", "music");

			Stop();
			_music = sdlMusic;
			_isLooping = loop;
			Play();

			return false;
		}

		public bool Play () {
			if (_music == null)
				return false;

			if (!EnqueueBuffers())
				return false;

			_userState = ALSourceState.Playing;
			while (AL.GetSourceState(_source) != ALSourceState.Playing)
				Thread.Sleep(1);
			return true;
		}

		public void Pause () {
			_userState = ALSourceState.Paused;
			while (AL.GetSourceState(_source) != ALSourceState.Paused)
				Thread.Sleep(1);
		}

		public void Stop () {
			_userState = ALSourceState.Stopped;
			while (AL.GetSourceState(_source) != ALSourceState.Stopped && AL.GetSourceState(_source) != ALSourceState.Initial)
				Thread.Sleep(1);
		}

		public void Dispose () {
			if (_isDisposed)
				return;
			_isDisposed = true;
			_thread.Join();
			AL.DeleteBuffers(_buffers);
			AL.DeleteSource(_source);
		}
	}

	public class SDL2MusicTrack : IMusicTrack {
		string _path;
		MpegFile _mp3;

		internal SDL2MusicTrack (string path) {
			_path = path;
			_mp3 = new MpegFile(System.IO.Path.Combine("assets", path));
		}

		public string Path { get { return _path; } }
		public int SampleRate { get { return _mp3.SampleRate; } }

		public int FillBuffer (float[] buf, long pcmCursor, int count) {
			lock (_mp3) {
				if (pcmCursor == 0) // Hack to support looping with NLayer's broken seeking
					_mp3.Position = pcmCursor;
				return _mp3.ReadSamples(buf, 0, count);
			}
		}

		public void Dispose () {
			_mp3.Dispose();
		}
	}
}
