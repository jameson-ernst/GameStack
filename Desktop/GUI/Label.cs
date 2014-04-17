using System;
using System.Drawing;
using OpenTK;
using GameStack.Graphics;

namespace GameStack.Gui {
	public enum VerticalAlignment {
		Top,
		Bottom,
		Middle
	}

	public class Label : View {
		TextBlock _text;

		public Label (string text, BitmapFont font, LayoutSpec spec = null) : base(spec) {
			_text = new TextBlock (font, 0f, text);
			this.VerticalAlignment = VerticalAlignment.Middle;
		}

		public string Text { get { return _text.Text; } set { _text.Text = value; } }

		public BitmapFont Font { get { return _text.Font; } set { _text.Font = value; } }

		public float Kerning { get { return _text.Kerning; } set { _text.Kerning = value; } }

		public float Leading { get { return _text.Leading; } set { _text.Leading = value; } }

		public Color Color { get { return _text.Color.ToColor (); } set { _text.Color = value.ToVector4 (); } }

		public HorizontalAlignment HorizontalAlignment { get { return _text.HorizontalAlignment; } set { _text.HorizontalAlignment = value; } }

		public VerticalAlignment VerticalAlignment { get; set; }

		public override void Layout () {
			base.Layout ();

			_text.Width = this.Size.Width;
		}

		protected override void OnDraw (ref Matrix4 transform) {
			var height = _text.ActualSize.Y;
			var textTransform = transform;
			switch (this.VerticalAlignment) {
				case VerticalAlignment.Top:
					textTransform.M42 += (int)(this.Size.Height - height);
					break;
				case VerticalAlignment.Middle:
					textTransform.M42 += (int)((this.Size.Height - height) / 2f);
					break;
				case VerticalAlignment.Bottom:
					break;
			}
			_text.Draw (ref textTransform);
		}
	}
}

