#pragma warning disable 0618

using System;
using OpenTK;
using OpenTK.Graphics;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
using BufferUsage = OpenTK.Graphics.OpenGL.BufferUsageHint;

#else
using OpenTK.Graphics.ES20;
#endif
namespace GameStack.Graphics {
	public class IndexBuffer : IDisposable {
		int[] _data;
		uint _handle;
		#if __ANDROID__
		All _mode;

#else
		BeginMode _mode;
		#endif
		#if __ANDROID__
		public IndexBuffer (int[] vertices = null, All mode = All.Triangles) {

#else
		public IndexBuffer (int[] vertices = null, BeginMode mode = BeginMode.Triangles) {
#endif
			_mode = mode;
			ThreadContext.Current.EnsureGLContext();
			_data = vertices;

			var buffers = new uint[1];
			GL.GenBuffers(1, buffers);
			_handle = buffers[0];

			if (_data != null)
				this.Commit();
		}

		public int[] Data { get { return _data; } set { _data = value; } }

		public void Commit () {
			if (_data == null)
				_data = new int[0];

#if __ANDROID__
            GL.BindBuffer(All.ElementArrayBuffer, _handle);
            GL.BufferData(All.ElementArrayBuffer, (IntPtr)(sizeof(int) * _data.Length), _data, All.StaticDraw);
            GL.BindBuffer(All.ElementArrayBuffer, 0);
#else
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _handle);
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(int) * _data.Length), _data, BufferUsage.StaticDraw);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
#endif
		}

		public void Draw (int offset = 0, int count = -1) {
			if (count < 0)
				count = _data.Length;
			if (count == 0)
				return;
#if __ANDROID__
            GL.BindBuffer(All.ElementArrayBuffer, _handle);
			GL.DrawElements(_mode, count, All.UnsignedInt, (IntPtr)(offset * sizeof(int)));
            GL.BindBuffer(All.ElementArrayBuffer, 0);
#else
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _handle);
			GL.DrawElements(_mode, count, DrawElementsType.UnsignedInt, (IntPtr)(offset * sizeof(int)));
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
#endif
		}

		public void Dispose () {
			GL.DeleteBuffers(1, new uint[_handle]);
		}
	}
}
