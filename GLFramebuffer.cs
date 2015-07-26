using System;
using OpenTK.Graphics.ES20;

namespace Fractals
{
	public class GLFramebuffer
	{
		private readonly float width;
		private readonly float height;
		private int frameBuffer;
		private int texture;


		public GLFramebuffer (float width, float height)
		{
			this.width = width;
			this.height = height;
			init ();
		}

		public float Width { get { return width; } }
		public float Height { get { return width; } }

		public int FramebufferId { get { return frameBuffer; } }
		public int TextureId { get { return texture; } }

		public void Use()
		{
			GL.BindFramebuffer (FramebufferTarget.Framebuffer, frameBuffer);
			GL.Viewport (0, 0, (int) width, (int) height);
			GL.ClearColor (1.0f, 0.0f, 0.0f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit);
		}

		public void Delete()
		{
			// TODO: do we need to delete textures separately or are they deleted together with framebuffer?
			GL.DeleteFramebuffers (1, ref frameBuffer);
		}

		private void init()
		{

			GL.GenFramebuffers (1, out frameBuffer);
			GL.BindFramebuffer (FramebufferTarget.Framebuffer, frameBuffer);

			// -- TEXTURE
			GL.GenTextures (1, out texture);
			GL.BindTexture (TextureTarget.Texture2D, texture);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
			// IMPORTANT: Set the wrap mode to clamp or Non-POT (power of two) texture WILL NOT work! 
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
			GL.TexImage2D(All.Texture2D, 0, (int) All.Rgba, (int)width, (int)height, 0, All.Rgba, All.HalfFloatOes, (IntPtr) 0);
			//https://www.khronos.org/registry/gles/extensions/OES/OES_texture_float.txt
//			GL.TexImage2D(All.Texture2D, 0, (int) All.Rgba, (int)width, (int)height, 0, All.Rgba, (All) 0x8D61, (IntPtr) 0);

//			// -- DEPTH BUFFER
//			GL.GenRenderbuffers (1, out depthBuffer);
//			GL.BindRenderbuffer (RenderbufferTarget.Renderbuffer, depthBuffer);
//			GL.RenderbufferStorage (RenderbufferTarget.Renderbuffer, RenderbufferInternalFormat.DepthComponent16, (int)width, (int)height);

			// -- ATTACH
			GL.FramebufferTexture2D (FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, texture, 0);
//			GL.FramebufferRenderbuffer (FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);


			// sanity check
			if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete) {
				System.Console.WriteLine ("Framebuffer setup failed, sorry.");
				if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferIncompleteAttachment)
					System.Console.WriteLine ("1");
				if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferIncompleteDimensions)
					System.Console.WriteLine ("2");
				if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferIncompleteMissingAttachment)
					System.Console.WriteLine ("3");
				if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferUnsupported)
					System.Console.WriteLine ("4");
			}

			// TODO: is this good practice, should i reset framebuffer and texture??
			//			GL.BindTexture (TextureTarget.Texture2D, 0);
			//			GL.BindFramebuffer (FramebufferTarget.Framebuffer, 0);

		}
	}
}

