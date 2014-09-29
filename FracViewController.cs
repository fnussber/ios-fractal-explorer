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

		static readonly float[] squareVertices = {
			-2.0f, -1.0f,
			-2.0f,  1.0f,
			 1.0f, -1.0f,
			 1.0f,  1.0f,
		};
//		static readonly float[] squareTexture = {
//			0.0f, 0.0f,
//			1.0f, 0.0f,
//			1.0f,  1.0f,
//			0.0f,  1.0f,
//		};

		// == the programs
		GLProgram program;
		GLProgram setupProgram;
		GLProgram offScreenProgram;
		GLProgram onScreenProgram;

		Stopwatch stopwatch = new Stopwatch();
		uint tex0;
		uint tex1;
		uint fob0;
		uint fob1;

		static int scaleUniformIx;
		static int transUniformIx;
		static int maxItUniformIx;
		static int coltxUniformIx;
		static int matrixUniformIx;

		static int minIter = 64;
		static int curIter = 64;
		static int maxIter = 128;

		static float scaleFactor = 1/256.0f;
		static float transX = -2.0f;
		static float transY = -1.0f;

		float[] rotationMatrix = new float[16],
		translationMatrix = new float[16],
		modelViewMatrix = new float[16],
		projectionMatrix = new float[16],
		matrix = new float[16];

//		static readonly byte[] squareColors = {
//			255, 255,   0, 255,
//			0,   255, 255, 255,
//			0,     0,   0,   0,
//			255,   0, 255, 255,
//		};




		EAGLContext context;
		EAGLContext context2;
		GLKView glkView;

		public FracViewController ()
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			context = new EAGLContext (EAGLRenderingAPI.OpenGLES2);
			context2 = new EAGLContext (EAGLRenderingAPI.OpenGLES2, context.ShareGroup);
			glkView = (GLKView) View;
			glkView.Context = context;
			glkView.MultipleTouchEnabled = true;
			glkView.DrawInRect += Draw;
			//glkView.Delegate = new MyDelegate ();


			PreferredFramesPerSecond = 10;
//			size = UIScreen.MainScreen.Bounds.Size.ToSize ();
//			View.ContentScaleFactor = UIScreen.MainScreen.Scale;

			AddGestureRecognizers (View);

			/* ** SETUP ** */
			SetupGL ();

			SetupFramebuffers (out fob0, out fob1);
		}

		void Draw (object sender, GLKViewDrawEventArgs args)
		{

			stopwatch.Restart ();

//			DrawOffScreen ();

//			if (curIter <= maxIter) {

				program.Use ();

				GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
				GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

				// set camera
				UpdatePosition ();

				// setup the geometry.. (check if this can be done once only)
				GL.VertexAttribPointer ((int) GLKVertexAttrib.Position, 2, VertexAttribPointerType.Float, false, 0, squareVertices);
				GL.EnableVertexAttribArray ((int) GLKVertexAttrib.Position);
//				GL.VertexAttribPointer ((int) GLKVertexAttrib.TexCoord0, 2, VertexAttribPointerType.Float, false, 0, squareTexture);
//				GL.EnableVertexAttribArray ((int) GLKVertexAttrib.TexCoord0);

				// adapt parameters as needed
				GL.Uniform1 (maxItUniformIx, (float)curIter);
//				GL.Uniform1 (scaleUniformIx, scaleFactor);
//				GL.Uniform2 (transUniformIx, transX, transY);
				GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);

			//curIter *= 2;

//			}

			System.Console.WriteLine ("Draw on screen in " + stopwatch.ElapsedMilliseconds + "ms");
		}

		void DrawOffScreen ()
		{

			stopwatch.Restart ();

			EAGLContext.SetCurrentContext (context2);

			offScreenProgram.Use ();

			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// setup the geometry.. (check if this can be done once only)
			GL.VertexAttribPointer ((int) GLKVertexAttrib.Position, 2, VertexAttribPointerType.Float, false, 0, squareVertices);
			GL.EnableVertexAttribArray ((int) GLKVertexAttrib.Position);
//			GL.VertexAttribPointer ((int) GLKVertexAttrib.TexCoord0, 2, VertexAttribPointerType.Float, false, 0, squareTexture);
//			GL.EnableVertexAttribArray ((int) GLKVertexAttrib.TexCoord0);

			// adapt parameters as needed
//			GL.Uniform1 (maxItUniformIx, (float)curIter);
//			GL.Uniform1 (scaleUniformIx, scaleFactor);
//			GL.Uniform2 (transUniformIx, transX, transY);
			GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);

			GL.Flush ();

			EAGLContext.SetCurrentContext (context);

			System.Console.WriteLine ("Draw off screen in " + stopwatch.ElapsedMilliseconds + "ms");
		}

		void UpdatePosition ()
		{
			Vector3 rotationVector = new Vector3 (1.0f, 1.0f, 1.0f);
			GLCommon.Matrix3DSetRotationByDegrees (ref rotationMatrix, 0.0f, rotationVector);
			GLCommon.Matrix3DSetTranslation (ref translationMatrix, 0.0f, 0.0f, -3.0f);
			modelViewMatrix = GLCommon.Matrix3DMultiply (translationMatrix, rotationMatrix);

			GLCommon.Matrix3DSetPerspectiveProjectionWithFieldOfView (ref projectionMatrix, 45.0f, 0.1f, 100.0f,
				View.Frame.Size.Width /
				View.Frame.Size.Height);

			matrix = GLCommon.Matrix3DMultiply (projectionMatrix, modelViewMatrix);
			GL.UniformMatrix4 (matrixUniformIx, 1, false, matrix);

		}
			
		void SetupGL ()
		{
//			GL.Enable (EnableCap.DepthTest);
//			GL.Enable (EnableCap.CullFace);
//			GL.Enable (EnableCap.Texture2D);
//			GL.Enable (EnableCap.Blend);

			EAGLContext.SetCurrentContext (context);

			uint textureId = LoadTexture ("Shaders/colorsTexture.png");

			// ---------- PROGRAM A0
			program = new GLProgram ("Shaders/Shader.vsh", "Shaders/Shader.fsh");
			//program.AddAttribute ("position");
			if (!program.Link ()) {
				Console.WriteLine ("Link failed.");
				Console.WriteLine (String.Format ("Program Log: {0}", program.ProgramLog ()));
				Console.WriteLine (String.Format ("Fragment Log: {0}", program.FragmentShaderLog ()));
				Console.WriteLine (String.Format ("Vertex Log: {0}", program.VertexShaderLog ()));
			}
			coltxUniformIx = program.GetUniformIndex ("coltx");
			maxItUniformIx = program.GetUniformIndex ("maxIter");
//			scaleUniformIx = program.GetUniformIndex ("scale");
//			transUniformIx = program.GetUniformIndex ("trans");
			matrixUniformIx = program.GetUniformIndex ("matrix");
			GL.Uniform1 (coltxUniformIx, textureId);

			// ---------- PROGRAM A
			setupProgram = new GLProgram ("Shaders/SetupShader.vsh", "Shaders/SetupShader.fsh");
			//program.AddAttribute ("position");
			if (!setupProgram.Link ()) {
				Console.WriteLine ("Link failed.");
				Console.WriteLine (String.Format ("Program Log: {0}", setupProgram.ProgramLog ()));
				Console.WriteLine (String.Format ("Fragment Log: {0}", setupProgram.FragmentShaderLog ()));
				Console.WriteLine (String.Format ("Vertex Log: {0}", setupProgram.VertexShaderLog ()));
			}
//			coltxUniformIx = setupProgram.GetUniformIndex ("coltx");
//			maxItUniformIx = setupProgram.GetUniformIndex ("maxIter");
//			scaleUniformIx = setupProgram.GetUniformIndex ("scale");
//			transUniformIx = setupProgram.GetUniformIndex ("trans");
//			GL.Uniform1 (coltxUniformIx, textureId);

			// ---------- PROGRAM B
			offScreenProgram = new GLProgram ("Shaders/OffScreenShader.vsh", "Shaders/OffScreenShader.fsh");
			if (!offScreenProgram.Link ()) {
				Console.WriteLine ("Link failed.");
				Console.WriteLine (String.Format ("Program Log: {0}", offScreenProgram.ProgramLog ()));
				Console.WriteLine (String.Format ("Fragment Log: {0}", offScreenProgram.FragmentShaderLog ()));
				Console.WriteLine (String.Format ("Vertex Log: {0}", offScreenProgram.VertexShaderLog ()));
			}
			var inValues1Id = offScreenProgram.GetUniformIndex ("inValues");
			var stepsId = offScreenProgram.GetUniformIndex ("steps");
			GL.Uniform1 (stepsId, 50);
			GL.Uniform1 (inValues1Id, tex0);

			// ---------- PROGRAM C
			onScreenProgram = new GLProgram ("Shaders/OnScreenShader.vsh", "Shaders/OnScreenShader.fsh");
			if (!onScreenProgram.Link ()) {
				Console.WriteLine ("Link failed.");
				Console.WriteLine (String.Format ("Program Log: {0}", onScreenProgram.ProgramLog ()));
				Console.WriteLine (String.Format ("Fragment Log: {0}", onScreenProgram.FragmentShaderLog ()));
				Console.WriteLine (String.Format ("Vertex Log: {0}", onScreenProgram.VertexShaderLog ()));
			}
//			coltxUniformIx = onScreenProgram.GetUniformIndex ("coltx");
//			maxItUniformIx = onScreenProgram.GetUniformIndex ("maxIter");
//			scaleUniformIx = onScreenProgram.GetUniformIndex ("scale");
//			transUniformIx = onScreenProgram.GetUniformIndex ("trans");
//			GL.Uniform1 (coltxUniformIx, textureId);
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
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
			// set data here if init data is needed..
			GL.TexImage2D(All.Texture2D, 0, (int) All.Rgba, width, height, 0, All.Rgba, All.UnsignedByte, (IntPtr) 0);
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
//		uint frameBuffer;
//		uint renderBuffer;
//		uint depthBuffer;
//		int backingWidth;
//		int backingHeight;
//
//		void createBuffers ()
//		{
//			GL.GenFramebuffers (1, out frameBuffer);
//			GL.GenRenderbuffers (1, out renderBuffer);
//			GL.BindFramebuffer (FramebufferTarget.Framebuffer, frameBuffer);
//			GL.BindRenderbuffer (RenderbufferTarget.Renderbuffer, renderBuffer);
//			context.RenderBufferStorage ((uint) All.Renderbuffer, (CAEAGLLayer) glkView.Layer);
//			GL.FramebufferRenderbuffer (FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, RenderbufferTarget.Renderbuffer, renderBuffer);
//			GL.GetRenderbufferParameter (RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth, out backingWidth);
//			GL.GetRenderbufferParameter (RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight, out backingHeight);
//
//			GL.GenRenderbuffers (1, out depthBuffer);
//			GL.BindRenderbuffer (RenderbufferTarget.Renderbuffer, depthBuffer);
//			GL.RenderbufferStorage (RenderbufferTarget.Renderbuffer, RenderbufferInternalFormat.DepthComponent16, backingWidth, backingHeight);
//			GL.FramebufferRenderbuffer (FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);
//
//			// sanity check
//			if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete) {
//				System.Console.WriteLine ("Framebuffer setup failed, sorry.");
//								if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferIncompleteAttachment)
//									System.Console.WriteLine ("1");
//								if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferIncompleteDimensions)
//									System.Console.WriteLine ("2");
//								if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferIncompleteMissingAttachment)
//									System.Console.WriteLine ("3");
//								if (GL.CheckFramebufferStatus (FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferUnsupported)
//									System.Console.WriteLine ("4");
//			}
//		}


		static uint CreateFramebuffer(uint inTexture, uint outTexture)
		{
			uint id;
			GL.GenFramebuffers (1, out id);
			GL.BindFramebuffer (FramebufferTarget.Framebuffer, id);
			GL.FramebufferTexture2D (FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, inTexture, 0);
			GL.FramebufferTexture2D (FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, outTexture, 0);

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
			EAGLContext.SetCurrentContext (context2);

			// -- create two textures which server alternatively as the data input/outputs
			tex0 = CreateTexture (768, 1024);
			tex1 = CreateTexture (768, 1024);

			// -- create two framebuffers two hold the textures
			fob0 = CreateFramebuffer (tex0, tex1);
			fob1 = CreateFramebuffer (tex1, tex0);

			GL.Flush ();

			EAGLContext.SetCurrentContext(context);
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

