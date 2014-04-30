using System;
using OpenTK;
#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#endif
using GameStack.Graphics;


namespace GameStack
{
	public class SceneCrossfader : Scene, IUpdater, IHandler<Start>, IDisposable
	{
		bool fading;
		Scene _scene;
		float _duration;
		bool _freezeNext;
		float _t;
		Texture _prevTexture, _nextTexture;
		FrameBuffer _prevFBO, _nextFBO;
		
		SpriteMaterial _mat;
		Quad _quad;
		Camera _cam;
		
		public SceneCrossfader (IGameView view = null)
			: base(view)
		{
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e)
		{
			_cam = new Camera2D(e.Size, 2);
			_quad = new Quad(new Vector4(0, 0, e.Size.X, e.Size.Y), Vector4.One);
			_mat = new SpriteMaterial(new SpriteShader(), null);
		}
		
		public void Init (Scene initialScene)
		{
			if (_scene != null)
				throw new InvalidOperationException("Fader has already been initialized!");
			
			_scene = initialScene;
		}
		
		public void FadeTo (Scene nextScene, Start startArgs, FrameArgs frameArgs, float duration = 0.5f, bool freezeNext = true)
		{
			if (_scene == null)
				throw new InvalidOperationException("No current scene to crossfade from!");
			
			var prevScene = _scene;
			_scene = nextScene;
			_duration = duration;
			_freezeNext = freezeNext;
			_t = 0;
			
			fading = true;
			
			if (_duration <= 0)
				Skip();
			else {
				if (_prevTexture == null)
					_prevTexture = new Texture(new System.Drawing.Size((int)startArgs.Size.X, (int)startArgs.Size.Y));
				if (_nextTexture == null)
					_nextTexture = new Texture(new System.Drawing.Size((int)startArgs.Size.X, (int)startArgs.Size.Y));
				if (_prevFBO == null)
					_prevFBO = new FrameBuffer(_prevTexture);
				if (_nextFBO == null)
					_nextFBO = new FrameBuffer(_nextTexture);
				
				using (_prevFBO.Begin()) {
					if (prevScene != null)
						prevScene.OnRender(this, frameArgs);
					else
						base.OnDraw(frameArgs);
				}
				
				View.RenderNow();
			}
			
			if (prevScene != null)
				prevScene.Dispose();
			
			View.LoadFrame();
			_scene.Start(this, startArgs);
		}
		
		public void Skip ()
		{
			if (!fading)
				return;
			
			FreeResources();
			fading = false;
		}

		void FreeResources ()
		{
			if (_prevFBO != null)
				_prevFBO.Dispose();
			if (_nextFBO != null)
				_nextFBO.Dispose();
			if (_prevTexture != null)
				_prevTexture.Dispose();
			if (_nextTexture != null)
				_nextTexture.Dispose();
			
			_prevFBO = null;
			_nextFBO = null;
			_prevTexture = null;
			_nextTexture = null;
		}
		
		public void Update (FrameArgs e)
		{
			if (!fading)
				_scene.OnUpdate(this, e);
			else {
				if (!_freezeNext)
					_scene.OnUpdate(this, e);

				_t += e.DeltaTime / _duration;
				if (_t > 1f)
					Skip();
			}
		}

		protected sealed override void OnDraw (FrameArgs e)
		{
			if (_scene == null) {
				base.OnDraw(e);
				return;
			}
			
			if (!fading)
				_scene.OnRender(this, e);
			else {
				if (_t > 0) {
					using (_nextFBO.Begin()) {
						_scene.OnRender(this, e);
					}
				}

				OnComposeScenes(e, _prevTexture, _nextTexture, _t);
			}
		}
		
		protected virtual void OnComposeScenes (FrameArgs e, Texture prevTexture, Texture nextTexture, float t)
		{
			using (_cam.Begin()) {
				var origColor = ClearColor;
				ClearColor = System.Drawing.Color.Black;
				base.OnDraw(e);
				ClearColor = origColor;
				
				_mat.Texture = prevTexture;
				_mat.Color = Vector4.One;
				using (_mat.Begin()) {
					_quad.Draw(0, 0, 0);
				}
				
				if (_t > 0) {
					_mat.Texture = nextTexture;
					_mat.Color = new Vector4(t, t, t, t);
					using (_mat.Begin()) {
						_quad.Draw(0, 0, 1);
					}
				}
			}
		}

		public override void Dispose ()
		{
			FreeResources();
			
			if (_scene != null)
				_scene.Dispose();
			
			base.Dispose();
		}
	}
}
