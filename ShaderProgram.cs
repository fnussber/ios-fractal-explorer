using System;
using OpenTK.Graphics.ES20;

namespace Fractals
{
	public class ShaderProgram
	{
		public bool valid;
		public int program;

		public ShaderProgram (String vertexShader, String fragmentShader)
		{
			valid = LoadShaders (vertexShader, fragmentShader);
		}

		bool LoadShaders (String vertexShader, String fragmentShader)
		{
			int vertShader, fragShader;

			program = GL.CreateProgram ();
			if (CompileShader (out vertShader, All.VertexShader, vertexShader)){
				if (CompileShader (out fragShader, All.FragmentShader, fragmentShader)){
					// Attach shaders
					GL.AttachShader (program, vertShader);
					GL.AttachShader (program, fragShader);

					// Bind attribute locations
//					GL.BindAttribLocation (program, ATTRIB_VERTEX, "position");
//					GL.BindAttribLocation (program, ATTRIB_TEXCOORD, "texCoord");

					if (LinkProgram (program)){
						// Get uniform locations
//						uniforms [UNIFORM_Y] = GL.GetUniformLocation (program, "SamplerY");
//						uniforms [UNIFORM_UV] = GL.GetUniformLocation (program, "SamplerUV");

						// Delete these ones, we do not need them anymore
						GL.DeleteShader (vertShader);
						GL.DeleteShader (fragShader);
						return true;
					} else {
						Console.WriteLine ("Failed to link the shader programs");
						GL.DeleteProgram (program);
						program = 0;
					}
				} else
					Console.WriteLine ("Failed to compile fragment shader");
				GL.DeleteShader (vertShader);
			} else 
				Console.WriteLine ("Failed to compile vertex shader");
			GL.DeleteProgram (program);
			return false;
		}

		bool CompileShader (out int shader, All type, string path)
		{
			string shaderProgram = System.IO.File.ReadAllText (path);
			int len = shaderProgram.Length, status = 0;
			shader = GL.CreateShader (type);

			GL.ShaderSource (shader, 1, new string [] { shaderProgram }, ref len);
			GL.CompileShader (shader);
			GL.GetShader (shader, All.CompileStatus, ref status);
			if (status == 0){
				GL.DeleteShader (shader);
				return false;
			}
			return true;
		}

		bool LinkProgram (int program)
		{
			GL.LinkProgram (program);
			int status = 0;
			int len = 0;
			GL.GetProgram (program, ProgramParameter.LinkStatus, out status);
			if (status == 0){
				GL.GetProgram (program, ProgramParameter.InfoLogLength, out len);
				var sb = new System.Text.StringBuilder (len);
				GL.GetProgramInfoLog (program, len, out len, sb);
				Console.WriteLine ("Link error: {0}", sb);
			}
			return status != 0;
		}
	}
}

