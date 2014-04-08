using System;
using System.Drawing;
using OpenTK;
using GameStack.Graphics;

namespace GameStack.Gui {
	public class RootView : View, IUpdater, IHandler<Resize>, IHandler<Touch>, IHandler<Gesture> {
		float _depth, _maxDepth;
		Camera2D _camera;
		IPointerInput _focused, _current;
		long _pointerId;

		public RootView (Vector2 viewSize, float depth, float maxDepth = 1000f) {
			_maxDepth = maxDepth;
			_camera = new Camera2D(viewSize, _maxDepth);
			this.Layout(viewSize, depth);
			_pointerId = -1;
		}

		public void Layout (Vector2 viewSize, float depth) {
			_depth = depth;
			this.Frame = new RectangleF(0f, 0f, viewSize.X, viewSize.Y);
			this.Size = new SizeF(viewSize.X, viewSize.Y);
			this.ZDepth = depth;

			foreach (var view in this.Children)
				view.Layout();
		}

		public void Draw () {
			using (_camera.Begin()) {
				this.Draw(Matrix4.Identity);
			}
		}

		void IHandler<Resize>.Handle (FrameArgs frame, Resize e) {
			_camera.SetViewSize(e.Size, _maxDepth);
			this.Layout(e.Size, _depth);
		}

		void IHandler<Touch>.Handle (FrameArgs frame, Touch e) {
			Vector2 where;
			var src = this.FindByPoint(e.SurfacePoint, Matrix4.Identity, out where);
			switch (e.State) {
				case TouchState.Start:
					if (_pointerId >= 0)
						return;
					if (e.Index == _pointerId && _focused != null && src != _focused) {
						_focused.OnPointerExit(where);
						_focused.OnPointerUp(where);
					}
					if (src != null) {
						_focused = src;
						src.OnPointerDown(where);
						_pointerId = e.Index;
					}
					break;
				case TouchState.Move:
					if (e.Index != _pointerId)
						return;
					if (_focused != null) {
						if (_focused != src)
							_focused.OnPointerExit(where);
						else if (_current != _focused && _focused == src)
							_focused.OnPointerEnter(where);
						else
							_focused.OnPointerMove(where);
					}
					break;
				case TouchState.End:
					if (e.Index != _pointerId)
						return;
					if (_focused != null)
						_focused.OnPointerUp(where);
					_focused = null;
					_pointerId = -1;
					break;
				case TouchState.Cancel:
					if (e.Index != _pointerId)
						return;
					if (_focused != null) {
						_focused.OnPointerExit(where);
						_focused.OnPointerUp(where);
					}
					_focused = null;
					_pointerId = -1;
					break;
			}
			_current = src;
		}

		void IHandler<Gesture>.Handle (FrameArgs frame, Gesture e) {
			IGestureInput target = _focused as IGestureInput;
			if (target != null) {
				target.OnGesture(e);
			}
		}

		void IUpdater.Update (FrameArgs e) {
			this.Update(e);
		}
	}
}

