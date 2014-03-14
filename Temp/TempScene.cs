using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Temp {
	public class TempScene : Scene, IUpdater, IHandler<Start> {
		Model _model;
		Matrix4 _world;
		Camera _cam;
		float _rot;
		//Animation _anim;
		Lighting _lights;

		public TempScene (IGameView view) : base(view) {
		}

		unsafe void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_model = new Model("sphere.model");
			_cam = new Camera();
			_cam.SetTransforms(
				Matrix4.LookAt(new Vector3(0f, 0f, 50f), new Vector3(0f, 0f, 0f), Vector3.UnitY),
				Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), e.Size.X / e.Size.Y, 0.1f, 100f));

			//_anim = _model.Animations[""];
			//_anim.Start(frame.Time);

			var lightpos = new Vector3(-500f, 0f, 50f);
			_lights = new Lighting(new PointLight(lightpos, new Vector3(0.4f, 0.4f, 1f), Vector3.One, Vector3.One, 0, 0, 2f));
		}

		void IUpdater.Update (FrameArgs e) {
			_rot += e.DeltaTime * 45f;
			_world = Matrix4.Scale(1500f) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_rot));
			//_anim.Update(e);
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) {
				using (_lights.Begin()) {
					_model.Draw(ref _world);
				}
			}
		}
	}
}
