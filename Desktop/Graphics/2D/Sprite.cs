using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#endif

namespace GameStack.Graphics {
	// A sprite is a simple quad with a texture applied. A region may be specified
	// (in pixels, from the top left) so that a texture may hold multiple sprites
	// (a.k.a atlasing).
	public class Sprite : Drawable2D, IBatchable {
		Material _material;
		Vector2 _size;
		VertexBuffer _vbuffer;
		IndexBuffer _ibuffer;
		int _ioffset, _icount;

		internal Sprite (Material material, Vector2 size, VertexBuffer vbuffer = null, IndexBuffer ibuffer = null, int ioffset = 0, int icount = 0) {
			_material = material;
			_size = size;
			_vbuffer = vbuffer;
			_ibuffer = ibuffer;
			_ioffset = ioffset;
			_icount = icount;
		}

		public Material Material { get { return _material; } }

		public virtual Vector2 Size { get { return _size; } }

		protected void SetBuffers (VertexBuffer vbuffer, IndexBuffer ibuffer, int ioffset, int icount) {
			_vbuffer = vbuffer;
			_ibuffer = ibuffer;
			_ioffset = ioffset;
			_icount = icount;
		}

		protected override void OnDraw (ref Matrix4 world) {
			var cam = ScopedObject.Find<Camera> ();
			if (cam == null)
				throw new InvalidOperationException ("There is no active camera.");

			var mat = ScopedObject.Find<Material>();
			if (mat == null) {
				using (_material.Begin()) {
					cam.Apply(ref world);
					using (_vbuffer.Begin()) {
						_ibuffer.Draw(_ioffset, _icount);
					}
				}
			} else {
				cam.Apply(ref world);
				using (_vbuffer.Begin()) {
					_ibuffer.Draw(_ioffset, _icount);
				}
			}
		}

		#region IBatchable implementation

		VertexBuffer IBatchable.VertexBuffer {
			get {
				return _vbuffer;
			}
		}

		IndexBuffer IBatchable.IndexBuffer {
			get {
				return _ibuffer;
			}
		}

		int IBatchable.IndexOffset {
			get {
				return _ioffset;
			}
		}

		int IBatchable.IndexCount {
			get {
				return _icount;
			}
		}

		Material IBatchable.Material {
			get {
				return _material;
			}
		}

		#endregion

	}
}
