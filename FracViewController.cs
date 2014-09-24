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

namespace Frax2
{
	public class FracViewController : GLKViewController
	{
		static readonly float[] squareVertices = {
			-2.0f, -1.0f,
			2.0f, -1.0f,
			-2.0f,  1.0f,
			2.0f,  1.0f,
		};

		int scaleUniformIx;
		int transUniformIx;
		int maxItUniformIx;
		int coltxUniformIx;

		int minIter = 64;
		int curIter = 64;
		int maxIter = 512;

		float scaleFactor = 1/256.0f;
		float transX = -2.0f;
		float transY = -1.0f;

//		float[] rotationMatrix = new float[16],
//		translationMatrix = new float[16],
//		modelViewMatrix = new float[16],
//		projectionMatrix = new float[16],
//		matrix = new float[16];

//		static readonly byte[] squareColors = {
//			255, 255,   0, 255,
//			0,   255, 255, 255,
//			0,     0,   0,   0,
//			255,   0, 255, 255,
//		};

		const int ATTRIB_VERTEX = 0;
//		const int ATTRIB_COLOR = 1;



		Stopwatch stopwatch = new Stopwatch();



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

			PreferredFramesPerSecond = 60;
//			size = UIScreen.MainScreen.Bounds.Size.ToSize ();
//			View.ContentScaleFactor = UIScreen.MainScreen.Scale;

			AddGestureRecognizers (View);

			/* ** SETUP ** */
			SetupGL ();

		}

//		public override void ViewDidUnload ()
//		{
//			base.ViewDidUnload ();
//
//			/* ** TEAR DOWN ** */
//			TeardownGL ();
//
//			if (EAGLContext.CurrentContext == context)
//				EAGLContext.SetCurrentContext (null);
//		}

		void Draw (object sender, GLKViewDrawEventArgs args)
		{
			stopwatch.Restart ();

			if (curIter <= maxIter) {
				GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
				GL.Clear (ClearBufferMask.ColorBufferBit);
				GL.VertexAttribPointer (ATTRIB_VERTEX, 2, VertexAttribPointerType.Float, false, 0, squareVertices);
				GL.EnableVertexAttribArray (ATTRIB_VERTEX);
				GL.Uniform1 (maxItUniformIx, (float)curIter);
				GL.Uniform1 (scaleUniformIx, scaleFactor);
				GL.Uniform2 (transUniformIx, transX, transY);
				GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);
				curIter *= 2;
			}

			System.Console.WriteLine ("Drawn in " + stopwatch.ElapsedMilliseconds + "ms");
		}

		void UpdatePosition ()
		{
//			Vector3 rotationVector = new Vector3 (1.0f, 1.0f, 1.0f);
//			GLCommon.Matrix3DSetRotationByDegrees (ref rotationMatrix, 0.0f, rotationVector);
//			GLCommon.Matrix3DSetTranslation (ref translationMatrix, 0.0f, 0.0f, -3.0f);
//			modelViewMatrix = GLCommon.Matrix3DMultiply (translationMatrix, rotationMatrix);
//
//			GLCommon.Matrix3DSetPerspectiveProjectionWithFieldOfView (ref projectionMatrix, 45.0f, 0.1f, 100.0f,
//				View.Frame.Size.Width /
//				View.Frame.Size.Height);
//
//			matrix = GLCommon.Matrix3DMultiply (projectionMatrix, modelViewMatrix);
//			GL.UniformMatrix4 (matrixUniform, 1, false, matrix);

		}
			
		void SetupGL ()
		{
			EAGLContext.SetCurrentContext (context);

			uint textureId = LoadTexture ("Shaders/colorsTexture.png");

			GLProgram program = new GLProgram ("Shaders/Shader.vsh", "Shaders/Shader.fsh");
			//program.AddAttribute ("position");
			if (!program.Link ()) {
				Console.WriteLine ("Link failed.");
				Console.WriteLine (String.Format ("Program Log: {0}", program.ProgramLog ()));
				Console.WriteLine (String.Format ("Fragment Log: {0}", program.FragmentShaderLog ()));
				Console.WriteLine (String.Format ("Vertex Log: {0}", program.VertexShaderLog ()));
			}
			coltxUniformIx = program.GetUniformIndex ("coltx");
			maxItUniformIx = program.GetUniformIndex ("maxIter");
			scaleUniformIx = program.GetUniformIndex ("scale");
			transUniformIx = program.GetUniformIndex ("trans");
			program.Use ();

			GL.Uniform1 (coltxUniformIx, textureId);

		}

		void TeardownGL ()
		{
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
					GL.BindTexture(All.Texture2D, id);

					GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int) All.Nearest);
					GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int) All.Nearest);

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
			pinchGesture.Delegate = new GestureDelegate ();
			image.AddGestureRecognizer (pinchGesture);

			var panGesture = new UIPanGestureRecognizer (PanImage);
			panGesture.MaximumNumberOfTouches = 2;
//			panGesture.Delegate = new GestureDelegate ();
			image.AddGestureRecognizer (panGesture);

//			var longPressGesture = new UILongPressGestureRecognizer (ShowResetMenu);
//			image.AddGestureRecognizer (longPressGesture);
		}


		void PanImage (UIPanGestureRecognizer gestureRecognizer)
		{
//			AdjustAnchorPointForGestureRecognizer (gestureRecognizer);
			var image = gestureRecognizer.View;
			if (gestureRecognizer.State == UIGestureRecognizerState.Began || gestureRecognizer.State == UIGestureRecognizerState.Changed) {
				var translation = gestureRecognizer.TranslationInView (View);
				//image.Center = new PointF (image.Center.X + translation.X, image.Center.Y + translation.Y);
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
//			AdjustAnchorPointForGestureRecognizer (gestureRecognizer);
			if (gestureRecognizer.State == UIGestureRecognizerState.Began || gestureRecognizer.State == UIGestureRecognizerState.Changed) {
				var loc = gestureRecognizer.LocationInView (gestureRecognizer.View);
//				gestureRecognizer.View.Transform *= CGAffineTransform.MakeScale (gestureRecognizer.Scale, gestureRecognizer.Scale);
				scaleFactor /= gestureRecognizer.Scale;
				curIter = minIter;
				// TODO: also move lower left corner in case of scaling...
//				transX += gestureRecognizer.Scale >= 1.0 ? -loc.X*scaleFactor : +loc.X*scaleFactor;
//				transY += gestureRecognizer.Scale >= 1.0 ? -loc.Y*scaleFactor : +loc.Y*scaleFactor;
				// Reset the gesture recognizer's scale - the next callback will get a delta from the current scale.
				gestureRecognizer.Scale = 1;
				System.Console.WriteLine ("Pinching: (scale)=" + gestureRecognizer.Scale);
			}
		}

		// *** NEEDED???
		// Scale and rotation transforms are applied relative to the layer's anchor point.
		// This method moves a UIGestureRecognizer's view anchor point between the user's fingers
//		void AdjustAnchorPointForGestureRecognizer (UIGestureRecognizer gestureRecognizer)
//		{
//			if (gestureRecognizer.State == UIGestureRecognizerState.Began) {
//				var image = gestureRecognizer.View;
//				var locationInView = gestureRecognizer.LocationInView (image);
//				var locationInSuperview = gestureRecognizer.LocationInView (image.Superview);
//
//				image.Layer.AnchorPoint = new PointF (locationInView.X / image.Bounds.Size.Width, locationInView.Y / image.Bounds.Size.Height);
//				image.Center = locationInSuperview;
//			}
//		}



		// do we need this??
		class GestureDelegate : UIGestureRecognizerDelegate
		{

			public GestureDelegate ()
			{
			}

			public override bool ShouldReceiveTouch(UIGestureRecognizer aRecogniser, UITouch aTouch)
			{
				return true;
			}

			public override bool ShouldRecognizeSimultaneously (UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
			{
				return true;
			}
		}


	}
}

