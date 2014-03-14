using System;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace GameStack {
	public class SoundChannel {
		int _source;
		SoundEffect _effect;

		public SoundChannel () {
			AL.GetError ();
			_source = AL.GenSource ();
			Sounds.CheckError ();
		}

		public bool IsPlaying {
			get { return AL.GetSourceState (_source) == ALSourceState.Playing; }
		}

		public SoundEffect Effect {
			get { return _effect; }
		}

		public float Volume {
			get {
				float vol;
				AL.GetSource (_source, ALSourcef.Gain, out vol);
				return vol;
			}
			set {
				AL.Source (_source, ALSourcef.Gain, value);
			}
		}

		public void PlayEffect (SoundEffect effect) {
			_effect = effect;

			AL.GetError ();
			AL.SourceStop (_source);
			AL.Source (_source, ALSourcei.Buffer, _effect.Buffer);
			AL.SourcePlay (_source);
			Sounds.CheckError ();
		}

		public void Play () {
			AL.SourcePlay (_source);
		}

		public void Pause () {
			AL.SourcePause (_source);
		}

		public void Stop () {
			AL.SourceStop (_source);
		}

		public void Dispose () {
			AL.DeleteSource (_source);
		}
	}
}
