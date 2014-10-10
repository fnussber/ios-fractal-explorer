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
		GLProgram setupProgram;
		GLProgram iterationsProgram;
		GLProgram onScreenProgram;

		NumberLabel reLabel;
		NumberLabel imLabel;
		NumberLabel scaleLabel;
		NumberLabel iterationsLabel;

		Stopwatch stopwatch = new Stopwatch();
		uint colorTextureId;
		uint colorBlueTextureId;

		GLFramebuffer fBuffer0Approx = null;
		GLFramebuffer fBuffer1Approx = null;
		GLFramebuffer fBuffer0 = null;
		GLFramebuffer fBuffer1 = null;

		static int pass = 0;
		static float curIter = 0.0f;
		static float steps = 8.0f;
		static float maxIter = 1024.0f;

		static float scaleFactor = 1.0f;
		static float transX = 0.0f;
		static float transY = 0.0f;
		static float rotation = 0.0f; // in radians

		Toolbar toolbar;

		EAGLContext context;
		GLKView glkView;

		public FracViewController ()
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Settings.SetupByPreferences ();

			context = new EAGLContext (EAGLRenderingAPI.OpenGLES2);
			glkView = (GLKView) View;
			glkView.Context = context;
			glkView.MultipleTouchEnabled = true;
			glkView.DrawInRect += Draw;

			PreferredFramesPerSecond = 20;
//			size = UIScreen.MainScreen.Bounds.Size.ToSize ();
//			View.ContentScaleFactor = UIScreen.MainScreen.Scale;

			AddGestureRecognizers (View);

			/* ** SETUP ** */
			EAGLContext.SetCurrentContext (context);
			GL.Enable (EnableCap.Texture2D);

			// SetupFramebuffers
			ResetImage ();
			ResetPosition ();

			SetupPrograms ();

			// some additional UI elements
			reLabel 	  = new NumberLabel("Re=", 0.0f, new RectangleF (10, 15, 200, 15));
			imLabel       = new NumberLabel("Im=", 0.0f, new RectangleF (10, 30, 200, 15));
			scaleLabel    = new NumberLabel("x=", 0.0f, new RectangleF (10, 45, 200, 15), "F1");
			iterationsLabel = new NumberLabel ("iterations=", 0.0f, new RectangleF (10, 60, 200, 15), "F0");

			View.AddSubview (reLabel);
			View.AddSubview (imLabel);
			View.AddSubview (scaleLabel);
			View.AddSubview (iterationsLabel);

//			toolbar = new Toolbar (View.Frame);
//			View.AddSubview (toolbar);

//			var settingsUI = new SettingsUI ();
//			UIWindow window = new UIWindow (UIScreen.MainScreen.Bounds);
//			window.MakeKeyAndVisible ();
//			window.RootViewController = settingsUI.navigation;
//			View.AddSubview (window);

		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			ResetImage ();
		}

		void ResetImage()
		{
			// delete existing framebuffers (if any)
			if (fBuffer0Approx != null)
				fBuffer0Approx.Delete();
			if (fBuffer1Approx != null)
				fBuffer1Approx.Delete();
			if (fBuffer0 != null)
				fBuffer0.Delete();
			if (fBuffer1 != null)
				fBuffer1.Delete();

			// create new framebuffers with appropriate dimensions
			float oversampling = 1.0f;
			float approxsampling = 0.125f;
			fBuffer0Approx = new GLFramebuffer (View.Frame.Width * approxsampling, View.Frame.Height * approxsampling);
			fBuffer1Approx = new GLFramebuffer (View.Frame.Width * approxsampling, View.Frame.Height * approxsampling);
			fBuffer0      = new GLFramebuffer (View.Frame.Width * oversampling, View.Frame.Height * oversampling);
			fBuffer1      = new GLFramebuffer (View.Frame.Width * oversampling, View.Frame.Height * oversampling);

			// reset iterations
			curIter = 0.0f;
		}

		class NumberLabel : UILabel
		{
			private String prefix;
			private String format;

			public NumberLabel(String prefix, float value, RectangleF frame, String format = "F10") 
			{
				this.prefix = prefix;
				Frame = frame;
				TextColor = UIColor.White;
				ShadowColor = UIColor.Black;
				Font = UIFont.FromName ("HelveticaNeue-Bold", 15);
				setText(value);
			}

			public void setText(float value)
			{
				Text = prefix + value.ToString (format);
			}
		}

		void Draw (object sender, GLKViewDrawEventArgs args)
		{

			if (curIter < maxIter) {
				stopwatch.Restart ();

				float[] matrix = UpdatePosition ();
				UpdateLabels (matrix);

				if (curIter == 0.0f) {
					pass = 0;
					SetupApproximation (matrix);
					SetupIterations (matrix);
				}

				if (pass % 2 == 0)
					RunIterations (matrix, fBuffer1, fBuffer0);
				else 
					RunIterations (matrix, fBuffer0, fBuffer1);

				curIter += steps;
				pass++;

				if (pass % 2 == 0)
					DrawIterations (matrix, fBuffer0);
				else
					DrawIterations (matrix, fBuffer1);

				System.Console.WriteLine ("Total time for drawing cycle " + stopwatch.ElapsedMilliseconds + "ms");
			}

		}

		void SetupApproximation (float[] matrix)
		{
			// reset
			fBuffer0Approx.Use ();

			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit);

			setupProgram.Use ();
			int attrPosition0 = GL.GetAttribLocation (setupProgram.Id(), "position");
			GL.VertexAttribPointer (attrPosition0, 2, VertexAttribPointerType.Float, false, 0, vertices);
			GL.EnableVertexAttribArray (attrPosition0);
			setupProgram.SetUniformMatrix ("matrix", matrix);
			GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);

			// calculate
			fBuffer1Approx.Use ();

//			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
//			GL.Clear (ClearBufferMask.ColorBufferBit);

			iterationsProgram.Use ();
			int attrPosition = GL.GetAttribLocation (iterationsProgram.Id(), "position");
			int attrTexture  = GL.GetAttribLocation (iterationsProgram.Id(), "texture");
			GL.VertexAttribPointer (attrPosition, 2, VertexAttribPointerType.Float, false, 0, vertices);
			GL.EnableVertexAttribArray (attrPosition);
			GL.VertexAttribPointer (attrTexture, 2, VertexAttribPointerType.Float, false, 0, textureCoords);
			GL.EnableVertexAttribArray (attrTexture);

			// bind a texture to the texture register 0
			GL.ActiveTexture (TextureUnit.Texture0);
			GL.BindTexture (TextureTarget.Texture2D, fBuffer0Approx.TextureId);

			// low resulotion but full iteration count!
			iterationsProgram.SetUniform ("iterations", maxIter/3);
			iterationsProgram.SetUniform ("inValues", 0);
			iterationsProgram.SetUniformMatrix ("matrix", matrix);

			GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);

		}
			
		void SetupIterations (float[] matrix)
		{

			stopwatch.Restart ();

			// == SET FRAMEBUFFER AND TEX0 AS OUTPUT FOR THIS STEP
			fBuffer0.Use ();
			setupProgram.Use ();

			// setup the geometry.. (check if this can be done once only)
			int attrPosition = GL.GetAttribLocation (setupProgram.Id(), "position");
			GL.VertexAttribPointer (attrPosition, 2, VertexAttribPointerType.Float, false, 0, vertices);
			GL.EnableVertexAttribArray (attrPosition);

			setupProgram.SetUniformMatrix ("matrix", matrix);

			GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);

			System.Console.WriteLine ("Setup Iterations in " + stopwatch.ElapsedMilliseconds + "ms");
		}

		void RunIterations(float[] matrix, GLFramebuffer fobOut, GLFramebuffer fobIn)
		{
			stopwatch.Restart ();

			// == SET FRAMEBUFFER AND TEX1 AS OUTPUT FOR THIS STEP
			fobOut.Use ();

			iterationsProgram.Use ();

			// setup the geometry.. (check if this can be done once only)
			int attrPosition = GL.GetAttribLocation (iterationsProgram.Id(), "position");
			int attrTexture  = GL.GetAttribLocation (iterationsProgram.Id(), "texture");
			GL.VertexAttribPointer (attrPosition, 2, VertexAttribPointerType.Float, false, 0, vertices);
			GL.EnableVertexAttribArray (attrPosition);
			GL.VertexAttribPointer (attrTexture, 2, VertexAttribPointerType.Float, false, 0, textureCoords);
			GL.EnableVertexAttribArray (attrTexture);

			// bind a texture to the texture register 0
			GL.ActiveTexture (TextureUnit.Texture0);
			GL.BindTexture (TextureTarget.Texture2D, fobIn.TextureId);

			iterationsProgram.SetUniform ("iterations", steps);
			iterationsProgram.SetUniform ("inValues", 0);
			iterationsProgram.SetUniformMatrix ("matrix", matrix);

			GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);

			System.Console.WriteLine ("Do Iterations in " + stopwatch.ElapsedMilliseconds + "ms");
		}

		void DrawIterations(float[] matrix, GLFramebuffer fobIn)
		{
			stopwatch.Restart ();

			glkView.BindDrawable ();

			onScreenProgram.Use ();

			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit);

			// setup the geometry.. (check if this can be done once only)
			int attrPosition = GL.GetAttribLocation (onScreenProgram.Id(), "position");
			int attrTexture  = GL.GetAttribLocation (onScreenProgram.Id(), "texture");
			GL.VertexAttribPointer (attrPosition, 2, VertexAttribPointerType.Float, false, 0, vertices);
			GL.EnableVertexAttribArray (attrPosition);
			GL.VertexAttribPointer (attrTexture, 2, VertexAttribPointerType.Float, false, 0, textureCoords);
			GL.EnableVertexAttribArray (attrTexture);

			// activate coloring texture and input texture
			GL.ActiveTexture (TextureUnit.Texture0);
			GL.BindTexture (TextureTarget.Texture2D, colorTextureId);
			GL.ActiveTexture (TextureUnit.Texture1);
			GL.BindTexture (TextureTarget.Texture2D, fobIn.TextureId);
			GL.ActiveTexture (TextureUnit.Texture2);
			GL.BindTexture (TextureTarget.Texture2D, fBuffer1Approx.TextureId);

			// assign the texture slot to use to the uniform input params
			onScreenProgram.SetUniform ("maxIterations", maxIter);
			onScreenProgram.SetUniform ("curIterations", curIter);
			onScreenProgram.SetUniform ("coltx", 0);
			onScreenProgram.SetUniform ("inValues", 1);
			onScreenProgram.SetUniform ("approx", 2);
			onScreenProgram.SetUniformMatrix ("matrix", matrix);

			GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);

			System.Console.WriteLine ("Draw Iterations in " + stopwatch.ElapsedMilliseconds + "ms");
		}
			
		void ResetPosition ()
		{
			// make mid point of screen = (-0.5, 0.0) and width cover the range [-2.5,1.5]
			// this will resize everything in such a way that the whole Mandelbrot set is visible
			scaleFactor = 2.0f; // * View.Frame.Width / View.Frame.Height;
			transX = -0.5f;
			transY = 0.0f;
			rotation = 0.0f; // in radians
		}

		float[] UpdatePosition ()
		{
			float[] rotationMatrix = new float[16];
			float[] translationMatrix = new float[16];
			float[] scaleMatrix = new float[16];
			float[] matrix;

//			Vector3 rotationVector = new Vector3 (1.0f, 1.0f, 1.0f);
//			GLCommon.Matrix3DSetRotationByDegrees (ref rotationMatrix, 0.0f, rotationVector);
//			GLCommon.Matrix3DSetTranslation (ref translationMatrix, 0.0f, 0.0f, -3.0f);
//			modelViewMatrix = GLCommon.Matrix3DMultiply (translationMatrix, rotationMatrix);

//			GLCommon.Matrix3DSetPerspectiveProjectionWithFieldOfView (ref projectionMatrix, 45.0f, 0.1f, 100.0f,
//				View.Frame.Size.Width /
//				View.Frame.Size.Height);
			//GLCommon.Matrix3DSetOrthoProjection (ref projectionMatrix, -aspectRation, aspectRation, -1.0f, 1.0f, -1.0f, 1.0f);
			//return projectionMatrix;


			float aspectRatio = View.Frame.Size.Width / View.Frame.Size.Height;
			Vector3 rotationVector = new Vector3 (1.0f, 1.0f, 1.0f);
			GLCommon.Matrix3DSetRotationByRadians (ref rotationMatrix, rotation, ref rotationVector);
			GLCommon.Matrix3DSetScaling (ref scaleMatrix, scaleFactor * aspectRatio, scaleFactor, 1.0f);
			GLCommon.Matrix3DSetTranslation (ref translationMatrix, transX, transY, 0.0f);
			matrix = GLCommon.Matrix3DMultiply (rotationMatrix, scaleMatrix);
			matrix = GLCommon.Matrix3DMultiply (translationMatrix, matrix);
			return matrix;

		}

		void UpdateLabels (float[] matrix)
		{
			var upperLeft = GLCommon.MatrixVectorMultiply (matrix, new float[4] { -1.0f, 1.0f, 0.0f, 1.0f });
			var lowerRight = GLCommon.MatrixVectorMultiply (matrix, new float[4] { 1.0f, -1.0f, 0.0f, 1.0f });

			reLabel.setText ((upperLeft [0] + lowerRight [0]) / 2.0f);
			imLabel.setText ((upperLeft [1] + lowerRight [1]) / 2.0f);
			scaleLabel.setText (2.0f / (lowerRight [0] - upperLeft [0]));
			iterationsLabel.setText (curIter);
		}


		void SetupPrograms ()
		{
			colorTextureId = LoadTexture ("Shaders/colorsTexture.png");
			colorBlueTextureId = LoadTexture ("Shaders/colorsBlueTexture.png");

			// ---------- PROGRAM A
			setupProgram = new GLProgram ("Shaders/SetupShader.vsh", "Shaders/SetupShader.fsh");
			//program.AddAttribute ("position");
			setupProgram.Link ();
			setupProgram.AddUniform ("matrix");

			// ---------- PROGRAM B
			iterationsProgram = new GLProgram ("Shaders/OffScreenShader.vsh", "Shaders/OffScreenShader.fsh");
			iterationsProgram.Link ();
			iterationsProgram.AddUniform ("matrix");
			iterationsProgram.AddUniform ("inValues");
			iterationsProgram.AddUniform ("iterations");

			// ---------- PROGRAM C
			onScreenProgram = new GLProgram ("Shaders/OnScreenShader.vsh", "Shaders/OnScreenShader.fsh");
			onScreenProgram.Link ();
			onScreenProgram.AddUniform ("matrix");
			onScreenProgram.AddUniform ("inValues");
			onScreenProgram.AddUniform ("maxIterations");
			onScreenProgram.AddUniform ("curIterations");
			onScreenProgram.AddUniform ("coltx");
			onScreenProgram.AddUniform ("approx");
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

					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);

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

			var rotationGesture = new UIRotationGestureRecognizer (RotateImage);
			image.AddGestureRecognizer (rotationGesture);

			var pinchGesture = new UIPinchGestureRecognizer (ScaleImage);
			image.AddGestureRecognizer (pinchGesture);

			var panGesture = new UIPanGestureRecognizer (PanImage);
			panGesture.MaximumNumberOfTouches = 2;
			image.AddGestureRecognizer (panGesture);

			//var longPressGesture = new UILongPressGestureRecognizer (ResetImage);
			//image.AddGestureRecognizer (longPressGesture);
		}

		void ResetImage (UILongPressGestureRecognizer gesture) 
		{
			if (gesture.State == UIGestureRecognizerState.Began) {
				ResetPosition ();
				curIter = 0.0f;
			}
		}

		void RotateImage (UIRotationGestureRecognizer gesture)
		{
			if (gesture.State == UIGestureRecognizerState.Began || gesture.State == UIGestureRecognizerState.Changed) {
				rotation += gesture.Rotation;
				// Reset the gesture recognizer's rotation - the next callback will get a delta from the current rotation.
				gesture.Rotation = 0;
				curIter = 0.0f;
			}
		}


		void PanImage (UIPanGestureRecognizer gesture)
		{
			var image = gesture.View;
			if (gesture.State == UIGestureRecognizerState.Began || gesture.State == UIGestureRecognizerState.Changed) {
				var translation = gesture.TranslationInView (View);
				transX -= (translation.X/View.Bounds.Width*1.5f) * scaleFactor;
				transY += (translation.Y/View.Bounds.Height*1.5f) * scaleFactor;
				curIter = 0.0f;
				// Reset the gesture recognizer's translation to {0, 0} - the next callback will get a delta from the current position.
				gesture.SetTranslation (PointF.Empty, image);

				System.Console.WriteLine ("Panning: (x,y)=" + translation);
			}
		}

		// Scales the image by the current scale
		void ScaleImage (UIPinchGestureRecognizer gesture)
		{
			if (gesture.State == UIGestureRecognizerState.Began || gesture.State == UIGestureRecognizerState.Changed) {
				var loc = gesture.LocationInView (View);
				var oldScaleFactor = scaleFactor;
				scaleFactor /= gesture.Scale;
				curIter = 0.0f;
				//transX += loc.X*(oldScaleFactor - scaleFactor);;
				//transY += (View.Frame.Height - loc.Y)*(oldScaleFactor - scaleFactor);
				// Reset the gesture recognizer's scale - the next callback will get a delta from the current scale.
				gesture.Scale = 1;
				System.Console.WriteLine ("Pinching: (scale)=" + gesture.Scale);
			}
		}

		public class Toolbar : UIToolbar {

			public UIBarButtonItem barBtn;

			public Toolbar(RectangleF rect)
			{
				barBtn = new UIBarButtonItem (new UIImage("timer-32.png"), UIBarButtonItemStyle.Plain, ClickAction);
					
				Frame = new RectangleF(0, rect.Bottom - 44, rect.Width, 44); 
				Items = new UIBarButtonItem[] {barBtn};

			}

			void ClickAction(object o, EventArgs EventArgs) {
				Console.WriteLine ("Button clicked");
				var settingsUI = new SettingsUI ();
				UIPopoverController popover = new UIPopoverController (settingsUI.navigation);
				popover.PopoverContentSize = new SizeF (400, 600);
				popover.PresentFromBarButtonItem (barBtn, UIPopoverArrowDirection.Any, true);
			}

		}
	}

}

