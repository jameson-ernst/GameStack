using System;
using GameStack;
using OpenTK;
using GameStack.Graphics;

namespace GameStack.Gui {
	public class ImageView : View {
		public ImageView (Sprite sprite, LayoutSpec spec = null) : base(spec) {
			this.Sprite = sprite;
		}

		public Sprite Sprite { get; set; }

		public override void Layout () {
			base.Layout();
			var resizable = this.Sprite as SlicedSprite;
			if (resizable != null)
				resizable.Resize(this.Size.ToVector2());
		}

		protected override void OnDraw (ref Matrix4 transform) {
			this.Sprite.Draw(ref transform);
		}
	}
}
