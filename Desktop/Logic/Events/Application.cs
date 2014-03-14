using System;
using OpenTK;

namespace GameStack {
	public class Start : EventBase {
		public Vector2 Size { get; private set; }

		internal Start (Vector2 size) {
			this.Size = size;
		}
	}

	public class Pause : EventBase {
	}

	public class Resume : EventBase {
	}
}
