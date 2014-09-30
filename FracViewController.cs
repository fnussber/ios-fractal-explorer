using System;
using MonoTouch.GLKit;
using MonoTouch.OpenGLES;
using System.Drawing;
using OpenTK.Graphics.ES20;
using System.Diagnostics;
using MonoTouch.UIKit;
using OpenTK;
using MonoTouch.CoreGraphics;
using System.Runtime.InteropServices;
using MonoTouch.CoreAnimation;

namespace Frax2
{
	public class FracViewController : GLKViewController
	{
		static uint renderbuffer;

		// vertices
		static readonly float[] vertices = {
			-1.0f, -1.0f,
			-1.0f,  1.0f,
			 1.0f, -1.0f,
			 1.0f,  1.0f
		};
		// texture coordinates
		static readonly float[] textureCoords = {
			0.0f, 0.0f,
			0.0f, 1.0f,
			1.0f, 0.0f,
			1.0f, 1.0f
		};


		// == the programs
		GLProgram program;
		GLProgram setupProgram;
		GLProgram iterationsProgram;
		GLProgram onScreenProgram;

		Stopwatch stopwatch = new Stopwatch();
		uint colorTextureId;
		uint tex0;
		uint tex1;
		uint fob0;
		uint fob1;

		static int minIter = 64;
		static int curIter = 64;
		static int maxIter = 128;

		static float scaleFactor = 1/256.0f;
		static float transX = 0.0f;
		static float transY = 0.0f;


		EAGLContext context;
		GLKView glkView;

		public FracViewController ()
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			context = new EAGLContext (EAGLRenderingAPI.OpenGLES2);
			glkView = (GLKView) View;
			glkView.Context = context;
			glkView.MultipleTouchEnabled = true;
			glkView.DrawInRect += Draw;

			PreferredFramesPerSecond = 10;
//			size = UIScreen.MainScreen.Bounds.Size.ToSize ();
//			View.ContentScaleFactor = UIScreen.MainScreen.Scale;

			AddGestureRecognizers (View);

			/* ** SETUP ** */
			EAGLContext.SetCurrentContext (context);
			GL.Enable (EnableCap.Texture2D);

			SetupFramebuffers (out fob0, out fob1);

			SetupPrograms ();
		}

		void Draw (object sender, GLKViewDrawEventArgs args)
		{

			stopwatch.Restart ();


			// do it in steps
			SetupIterations ();
//			RunIterations ();
			DrawIterations ();


			// do it in one go
//			DrawResult ();

			System.Console.WriteLine ("Total time for drawing cycle " + stopwatch.ElapsedMilliseconds + "ms");
		}

		void SetupIterations ()
		{

			stopwatch.Restart ();

		
			// == SET FRAMEBUFFER AND TEX0 AS OUTPUT FOR THIS STEP
			GL.BindFramebuffer (FramebufferTarget.Framebuffer, fob0);
			GL.Viewport (0, 0, 768, 1024);

			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			setupProgram.Use ();

			// setup the geometry.. (check if this can be done once only)
//			GL.VertexAttribPointer ((int) GLKVertexAttrib.Position, 2, VertexAttribPointerType.Float, false, 0, vertVertices);
//			GL.EnableVertexAttribArray ((int) GLKVertexAttrib.Position);
//			GL.VertexAttribPointer ((int) GLKVertexAttrib.TexCoord0, 2, VertexAttribPointerType.Float, false, 0, textureCoords);
//			GL.EnableVertexAttribArray ((int) GLKVertexAttrib.TexCoord0);
			GL.VertexAttribPointer (0, 2, VertexAttribPointerType.Float, false, 0, vertices);
			GL.EnableVertexAttribArray (0);
			GL.VertexAttribPointer (1, 2, VertexAttribPointerType.Float, false, 0, textureCoords);
			GL.EnableVertexAttribArray (1);

			// bind a texture to the texture register 0
//			GL.ActiveTexture (TextureUnit.Texture0);
//			GL.BindTexture (TextureTarget.Texture2D, tex0);

//			GL.ReadPixels(0, 0, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, ref data);


			GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);

			System.Console.WriteLine ("Setup Iterations in " + stopwatch.ElapsedMilliseconds + "ms");
		}

		void RunIterations()
		{
			stopwatch.Restart ();

//			EAGLContext.SetCurrentContext (context2);

			iterationsProgram.Use ();

			// == SET FRAMEBUFFER AND TEX1 AS OUTPUT FOR THIS STEP
			GL.BindFramebuffer (FramebufferTarget.Framebuffer, fob1);

			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// setup the geometry.. (check if this can be done once only)
			GL.VertexAttribPointer ((int) GLKVertexAttrib.Position, 2, VertexAttribPointerType.Float, false, 0, vertices);
			GL.EnableVertexAttribArray ((int) GLKVertexAttrib.Position);
			GL.VertexAttribPointer ((int) GLKVertexAttrib.TexCoord0, 2, VertexAttribPointerType.Float, false, 0, textureCoords);
			GL.EnableVertexAttribArray ((int) GLKVertexAttrib.TexCoord0);

			// bind a texture to the texture register 0
//			GL.ActiveTexture (TextureUnit.Texture0);
//			GL.BindTexture (TextureTarget.Texture2D, tex0);

			iterationsProgram.SetUniform ("inValues", tex0);

			GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);

//			GL.Flush (); // needed?

//			EAGLContext.SetCurrentContext (context);

			System.Console.WriteLine ("Do Iterations in " + stopwatch.ElapsedMilliseconds + "ms");
		}

		void DrawIterations()
		{
			stopwatch.Restart ();

			glkView.BindDrawable ();

			onScreenProgram.Use ();

			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// setup the geometry.. (check if this can be done once only)
			GL.VertexAttribPointer ((int) GLKVertexAttrib.Position, 2, VertexAttribPointerType.Float, false, 0, vertices);
			GL.EnableVertexAttribArray ((int) GLKVertexAttrib.Position);
//			GL.VertexAttribPointer ((int) GLKVertexAttrib.TexCoord0, 2, VertexAttribPointerType.Float, false, 0, textureCoords);
//			GL.EnableVertexAttribArray ((int) GLKVertexAttrib.TexCoord0);

			// activate coloring texture and input texture
			GL.ActiveTexture (TextureUnit.Texture0);
			GL.BindTexture (TextureTarget.Texture2D, colorTextureId);
			GL.ActiveTexture (TextureUnit.Texture1);
			GL.BindTexture (TextureTarget.Texture2D, tex0);

			onScreenProgram.SetUniform ("coltx", colorTextureId);
			onScreenProgram.SetUniform ("inValues", tex0);

			GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);

			System.Console.WriteLine ("Draw Iterations in " + stopwatch.ElapsedMilliseconds + "ms");
		}

		void DrawResult ()
		{

			stopwatch.Restart ();

			// render to screen
			glkView.BindDrawable ();

			program.Use ();

			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// setup the geometry.. (check if this can be done once only)
			GL.VertexAttribPointer ((int) GLKVertexAttrib.Position, 2, VertexAttribPointerType.Float, false, 0, vertices);
			GL.EnableVertexAttribArray ((int) GLKVertexAttrib.Position);
			GL.VertexAttribPointer ((int) GLKVertexAttrib.TexCoord0, 2, VertexAttribPointerType.Float, false, 0, textureCoords);
			GL.EnableVertexAttribArray ((int) GLKVertexAttrib.TexCoord0);

			// adapt parameters as needed
			program.SetUniform ("maxIter", curIter);
			program.SetUniform ("scale", scaleFactor);
			program.SetUniform2 ("trans", transX, transY);

			// activate coloring texture
			GL.ActiveTexture (TextureUnit.Texture0);
			GL.BindTexture (TextureTarget.Texture2D, colorTextureId);

			GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);


			System.Console.WriteLine ("Draw on screen in " + stopwatch.ElapsedMilliseconds + "ms");
		}


		float[] UpdatePosition ()
		{
//			float[] rotationMatrix = new float[16];
//			float[] translationMatrix = new float[16];
//			float[] modelViewMatrix = new float[16];
			float[] projectionMatrix = new float[16];

//			Vector3 rotationVector = new Vector3 (1.0f, 1.0f, 1.0f);
//			GLCommon.Matrix3DSetRotationByDegrees (ref rotationMatrix, 0.0f, rotationVector);
//			GLCommon.Matrix3DSetTranslation (ref translationMatrix, 0.0f, 0.0f, -3.0f);
//			modelViewMatrix = GLCommon.Matrix3DMultiply (translationMatrix, rotationMatrix);

//			GLCommon.Matrix3DSetPerspectiveProjectionWithFieldOfView (ref projectionMatrix, 45.0f, 0.1f, 100.0f,
//				View.Frame.Size.Width /
//				View.Frame.Size.Height);


			float aspectRation = View.Frame.Size.Width / View.Frame.Size.Height;
			GLCommon.Matrix3DSetOrthoProjection (ref projectionMatrix, -aspectRation, aspectRation, -1.0f, 1.0f, -1.0f, 1.0f);
			return projectionMatrix;
		}

//		float[] UpdatePosition ()
//		{
//			float[] rotationMatrix = new float[16];
//			float[] translationMatrix = new float[16];
//			float[] modelViewMatrix = new float[16];
//			float[] projectionMatrix = new float[16];
//
//			Vector3 rotationVector = new Vector3 (1.0f, 1.0f, 1.0f);
//			GLCommon.Matrix3DSetRotationByDegrees (ref rotationMatrix, 0.0f, rotationVector);
//			GLCommon.Matrix3DSetTranslation (ref translationMatrix, 0.0f, 0.0f, -3.0f);
//			modelViewMatrix = GLCommon.Matrix3DMultiply (translationMatrix, rotationMatrix);
//
//			GLCommon.Matrix3DSetPerspectiveProjectionWithFieldOfView (ref projectionMatrix, 45.0f, 0.1f, 100.0f,
//				View.Frame.Size.Width /
//				View.Frame.Size.Height);
//
//			return GLCommon.Matrix3DMultiply (projectionMatrix, modelViewMatrix);
//		}
			
		void SetupPrograms ()
		{
			colorTextureId = LoadTexture ("Shaders/colorsTexture.png");

			// ---------- PROGRAM A0
			program = new GLProgram ("Shaders/Shader.vsh", "Shaders/Shader.fsh");
			//program.AddAttribute ("position");
			if (!program.Link ()) {
				Console.WriteLine ("Link failed.");
				Console.WriteLine (String.Format ("Program Log: {0}", program.ProgramLog ()));
				Console.WriteLine (String.Format ("Fragment Log: {0}", program.FragmentShaderLog ()));
				Console.WriteLine (String.Format ("Vertex Log: {0}", program.VertexShaderLog ()));
			}
			program.AddUniform ("coltx");
			program.AddUniform ("maxIter");
			program.AddUniform ("scale");
			program.AddUniform ("trans");
			program.SetUniform ("coltx", colorTextureId);

			// ---------- PROGRAM A
			setupProgram = new GLProgram ("Shaders/SetupShader.vsh", "Shaders/SetupShader.fsh");
			//program.AddAttribute ("position");
			if (!setupProgram.Link ()) {
				Console.WriteLine ("Link failed.");
				Console.WriteLine (String.Format ("Program Log: {0}", setupProgram.ProgramLog ()));
				Console.WriteLine (String.Format ("Fragment Log: {0}", setupProgram.FragmentShaderLog ()));
				Console.WriteLine (String.Format ("Vertex Log: {0}", setupProgram.VertexShaderLog ()));
			}
			//setupProgram.AddUniform ("matrix");
//			setupProgram.AddUniform ("coltx");
//			setupProgram.SetUniform ("coltx", textureId);

			// ---------- PROGRAM B
			iterationsProgram = new GLProgram ("Shaders/OffScreenShader.vsh", "Shaders/OffScreenShader.fsh");
			if (!iterationsProgram.Link ()) {
				Console.WriteLine ("Link failed.");
				Console.WriteLine (String.Format ("Program Log: {0}", iterationsProgram.ProgramLog ()));
				Console.WriteLine (String.Format ("Fragment Log: {0}", iterationsProgram.FragmentShaderLog ()));
				Console.WriteLine (String.Format ("Vertex Log: {0}", iterationsProgram.VertexShaderLog ()));
			}
			iterationsProgram.AddUniform ("inValues");
			iterationsProgram.AddUniform ("steps");
			//iterationsProgram.AddUniform ("matrix");
			iterationsProgram.SetUniform ("steps", 50);
			iterationsProgram.SetUniform ("inValues", tex0);

			// ---------- PROGRAM C
			onScreenProgram = new GLProgram ("Shaders/OnScreenShader.vsh", "Shaders/OnScreenShader.fsh");
			if (!onScreenProgram.Link ()) {
				Console.WriteLine ("Link failed.");
				Console.WriteLine (String.Format ("Program Log: {0}", onScreenProgram.ProgramLog ()));
				Console.WriteLine (String.Format ("Fragment Log: {0}", onScreenProgram.FragmentShaderLog ()));
				Console.WriteLine (String.Format ("Vertex Log: {0}", onScreenProgram.VertexShaderLog ()));
			}
			onScreenProgram.AddUniform ("inValues");
			onScreenProgram.AddUniform ("coltx");
		}

		void TeardownGL ()
		{
		}

		// === Framebuffer

		static uint CreateTexture(int width, int height)
		{
			uint id;
			GL.Enable (EnableCap.Texture2D);
			GL.GenTextures (1, out id);
			GL.BindTexture (TextureTarget.Texture2D, id);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
			// set data here if init data is needed..
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr) 0);
			return id;
		}

//		uint CreateRenderbuffer()
//		{
//			uint id;
//			GL.GenRenderbuffers (1, out id);
//			GL.BindRenderbuffer (RenderbufferTarget.Renderbuffer, id);
//			context.RenderBufferStorage ((uint) All.Renderbuffer, (CAEAGLLayer) glkView.Layer);
//			GL.FramebufferRenderbuffer (FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, RenderbufferTarget.Renderbuffer, id);
//			return id;
//		}
//

		uint CreateFramebuffer2 (out uint texture, int backingWidth, int backingHeight)
		{
			uint frameBuffer;
//			uint renderBuffer;
			uint depthBuffer;

			GL.GenFramebuffers (1, out frameBuffer);
			GL.BindFramebuffer (FramebufferTarget.Framebuffer, frameBuffer);

			GL.GenTextures (1, out texture);
			GL.BindTexture (TextureTarget.Texture2D, texture);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
			// set data here if init data is needed..
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, backingWidth, backingHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr) 0);

			GL.FramebufferTexture2D (FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, texture, 0);


			GL.GenRenderbuffers (1, out depthBuffer);
			GL.BindRenderbuffer (RenderbufferTarget.Renderbuffer, depthBuffer);
			GL.RenderbufferStorage (RenderbufferTarget.Renderbuffer, RenderbufferInternalFormat.DepthComponent16, backingWidth, backingHeight);
			GL.FramebufferRenderbuffer (FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);

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

			return frameBuffer;
		}


		static uint CreateFramebuffer(uint outTexture)
		{
			uint id;
			GL.GenFramebuffers (1, out id);
			GL.BindFramebuffer (FramebufferTarget.Framebuffer, id);
			GL.FramebufferTexture2D (FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, outTexture, 0);
			//GL.FramebufferTexture2D (FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment1, TextureTarget.Texture2D, outTexture, 0);

			// sanity check
			if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete) {
				System.Console.WriteLine ("Framebuffer setup failed, sorry.");
				//				if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferIncompleteAttachment)
				//					System.Console.WriteLine ("1");
				//				if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferIncompleteDimensions)
				//					System.Console.WriteLine ("2");
				//				if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferIncompleteMissingAttachment)
				//					System.Console.WriteLine ("3");
				//				if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferUnsupported)
				//					System.Console.WriteLine ("4");
			}

			return id;
		}

	    void SetupFramebuffers(out uint fob0, out uint fob1)
		{
//			EAGLContext.SetCurrentContext (context2);

			// -- create two textures which server alternatively as the data input/outputs
//			tex0 = CreateTexture (768, 1024);
//			tex1 = CreateTexture (768, 1024);

			// -- create two framebuffers two hold the textures
			fob0 = CreateFramebuffer2 (out tex0, 768, 1024);
			fob1 = CreateFramebuffer2 (out tex1, 768, 1024);

//			GL.Flush ();

//			EAGLContext.SetCurrentContext(context);
		}

		// === Textures

		uint LoadTexture(String path) 
		{
			// store the ID of the OpenGL texture
			uint id;

			// safely load the image from disc
			using(var image = UIImage.FromBundle(path).CGImage) {
				// allocate enough memory to store the image data
				// each pixel requires a byte for red, green, blue and alpha components
				IntPtr data = Marshal.AllocHGlobal(image.Height * image.Width * 4);

				using(
					var context = new CGBitmapContext(data, image.Width, image.Height, 8,
						image.Width * 4, image.ColorSpace, CGImageAlphaInfo.PremultipliedLast)) {
					// draw the image to the bitmap context
					// this fills the data variable with the image data
					context.DrawImage(new RectangleF(0, 0, image.Width, image.Height), image);

					// get an id from OpenGL
					GL.GenTextures(1, out id);

					// let OpenGL know we're dealing with this image id
					GL.BindTexture(TextureTarget.Texture2D, id);

					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);

					// generate the OpenGL texture
					GL.TexImage2D(All.Texture2D, 0, (int) All.Rgba, image.Width, image.Height, 0,
						All.Rgba, All.UnsignedByte, data);

					// free the allocated memory
					Marshal.FreeHGlobal(data);
				}
			}

			return id;
		}

		// === Gestures

		void AddGestureRecognizers (UIView image)
		{
			image.UserInteractionEnabled = true;

//			var rotationGesture = new UIRotationGestureRecognizer (RotateImage);
//			image.AddGestureRecognizer (rotationGesture);

			var pinchGesture = new UIPinchGestureRecognizer (ScaleImage);
			image.AddGestureRecognizer (pinchGesture);

			var panGesture = new UIPanGestureRecognizer (PanImage);
			panGesture.MaximumNumberOfTouches = 2;
			image.AddGestureRecognizer (panGesture);

//			var longPressGesture = new UILongPressGestureRecognizer (ShowResetMenu);
//			image.AddGestureRecognizer (longPressGesture);
		}


		void PanImage (UIPanGestureRecognizer gestureRecognizer)
		{
			var image = gestureRecognizer.View;
			if (gestureRecognizer.State == UIGestureRecognizerState.Began || gestureRecognizer.State == UIGestureRecognizerState.Changed) {
				var translation = gestureRecognizer.TranslationInView (View);
				transX -= translation.X * scaleFactor;
				transY += translation.Y * scaleFactor;
				curIter = minIter;
				// Reset the gesture recognizer's translation to {0, 0} - the next callback will get a delta from the current position.
				gestureRecognizer.SetTranslation (PointF.Empty, image);

				System.Console.WriteLine ("Panning: (x,y)=" + translation);
			}
		}

		// Scales the image by the current scale
		void ScaleImage (UIPinchGestureRecognizer gestureRecognizer)
		{
			if (gestureRecognizer.State == UIGestureRecognizerState.Began || gestureRecognizer.State == UIGestureRecognizerState.Changed) {
				var loc = gestureRecognizer.LocationInView (View);
				var oldScaleFactor = scaleFactor;
				scaleFactor /= gestureRecognizer.Scale;
				curIter = minIter;
				transX += loc.X*(oldScaleFactor - scaleFactor);;
				transY += (View.Frame.Height - loc.Y)*(oldScaleFactor - scaleFactor);
				// Reset the gesture recognizer's scale - the next callback will get a delta from the current scale.
				gestureRecognizer.Scale = 1;
				System.Console.WriteLine ("Pinching: (scale)=" + gestureRecognizer.Scale);
			}
		}

	}
}

