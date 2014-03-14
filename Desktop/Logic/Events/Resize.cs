using System;
using OpenTK;

namespace GameStack {
	public class Resize : EventBase {
		public Vector2 Size { get; private set; }

		internal Resize (Vector2 size) {
			this.Size = size;
		}
	}
}
