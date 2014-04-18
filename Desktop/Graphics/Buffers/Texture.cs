#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK;
using GameStack.Content;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
using GenerateMipmapTarget = OpenTK.Graphics.ES20.All;
#endif

namespace GameStack.Graphics {
	public class TextureSettings {
		public TextureFilter MagFilter { get; set; }
		public TextureFilter MinFilter { get; set; }

		public TextureWrap WrapS { get; set; }
		public TextureWrap WrapT { get; set; }

		public TextureSettings () {
			this.MagFilter = this.MinFilter = TextureFilter.Linear;
			this.WrapS = this.WrapT = TextureWrap.Clamp;
		}
	}

	public enum TextureFilter {
		Nearest = All.Nearest,
		Linear = All.Linear,
		Trilinear = All.LinearMipmapLinear,
	}

	public enum TextureWrap {
		Clamp = All.ClampToEdge,
		Repeat = All.Repeat
	}

	public class Texture : IDisposable {
		uint _handle = 0;
		Size _size;
		PixelFormat _format;
		Vector2 _texelSize;
		TextureSettings _settings;

		public Texture (Size size, PixelFormat format = PixelFormat.Rgba, TextureSettings settings = null) {
			_size = size;
			_format = format;

			this.Initialize(null, settings);
		}

		public Texture (string path, TextureSettings settings = null)
			: this(Assets.ResolveStream(path), Path.GetExtension(path), settings, false)
		{
		}

		public Texture (Stream stream, string format = ".png", TextureSettings settings = null, bool leaveOpen = true) {
			byte[] data = null;
			switch (format.ToLower()) {
			case ".png":
				data = PngLoader.Decode(stream, out _size, out _format);
				break;
			default:
				throw new NotSupportedException("Unsupported image format: " + format);
			}
			this.Initialize(data, settings);

			if (!leaveOpen)
				stream.Dispose();
		}

		public uint Handle { get { return _handle; } }

		public Size Size { get { return _size; } }

		public Vector2 TexelSize { get { return _texelSize; } }

		public TextureSettings Settings { get { return _settings; } }

		public void SetData (IntPtr buf) {
			GL.BindTexture(TextureTarget.Texture2D, _handle);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _size.Width, _size.Height, 0, _format, PixelType.UnsignedByte, buf);
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		void Initialize (byte[] buf, TextureSettings settings) {
			_settings = settings ?? new TextureSettings();

			ThreadContext.Current.EnsureGLContext();

			// upload the texture into a texture buffer
			_handle = (uint)GL.GenTexture();

			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, _handle);
			if (buf == null) {
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _size.Width, _size.Height, 0, _format, PixelType.UnsignedByte, IntPtr.Zero);
			} else {
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _size.Width, _size.Height,
					0, _format, PixelType.UnsignedByte, buf);
				if (_settings.MinFilter == TextureFilter.Trilinear)
					GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			}
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)_settings.MagFilter);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)_settings.MinFilter);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)_settings.WrapS);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)_settings.WrapT);
			GL.BindTexture(TextureTarget.Texture2D, 0);

			_texelSize = new Vector2(1f / _size.Width, 1f / _size.Height);
		}

		public void Dispose () {
			GL.DeleteTexture((int)this.Handle);
		}
	}
}

