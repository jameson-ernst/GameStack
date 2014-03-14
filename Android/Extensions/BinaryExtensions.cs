using System;
using System.IO;
using OpenTK;

namespace GameStack.Content {
	public static class BinaryExtensions {
		public static Vector2 ReadVector2 (this BinaryReader reader) {
			var result = new Vector2 ();
			result.X = reader.ReadSingle ();
			result.Y = reader.ReadSingle ();
			return result;
		}

		public static Vector3 ReadVector3 (this BinaryReader reader) {
			var result = new Vector3 ();
			result.X = reader.ReadSingle ();
			result.Y = reader.ReadSingle ();
			result.Z = reader.ReadSingle ();
			return result;
		}

		public static Vector4 ReadVector4 (this BinaryReader reader) {
			var result = new Vector4 ();
			result.X = reader.ReadSingle ();
			result.Y = reader.ReadSingle ();
			result.Z = reader.ReadSingle ();
			result.W = reader.ReadSingle ();
			return result;
		}
	}
}

