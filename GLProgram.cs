using System;
using OpenTK.Graphics.ES20;
using MonoTouch.Foundation;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Frax2
{
	public class GLProgram
	{
		int program,
		vertShader, 
		fragShader;

		List<string> attributes;
		Dictionary<string, int> uniforms;

		public GLProgram (string vShaderFilename, string fShaderFilename)
		{
			attributes = new List<string> ();
			uniforms = new Dictionary<string, int> ();
			program = GL.CreateProgram ();

			string vertShaderPathName = vShaderFilename; //NSBundle.MainBundle.PathForResource (vShaderFilename, "vsh");
			if (!compileShader (ref vertShader, ShaderType.VertexShader, vertShaderPathName))
				Console.WriteLine ("Failed to compile the vertex shader");

			string fragShaderPathName = fShaderFilename; //NSBundle.MainBundle.PathForResource (fShaderFilename, "fsh");
			if (!compileShader (ref fragShader, ShaderType.FragmentShader, fragShaderPathName))
				Console.WriteLine ("Failed to compile the fragment shader");

			GL.AttachShader (program, vertShader);
			GL.AttachShader (program, fragShader);
		}

		public int Id()
		{
			return program;
		}

		bool compileShader (ref int shader, ShaderType type, string file)
		{
			int status;
			string source;

			using (StreamReader sr = new StreamReader(file))
				source = sr.ReadToEnd();

			shader = GL.CreateShader (type);
			GL.ShaderSource (shader, source);
			GL.CompileShader (shader);

			GL.GetShader (shader, ShaderParameter.CompileStatus, out status);

			return status == (int) All.True;
		}

		public void AddAttribute (string attributeName)
		{
			if (!attributes.Contains (attributeName)) {
				attributes.Add (attributeName);
				GL.BindAttribLocation (program, attributes.IndexOf (attributeName), attributeName);
			}
		}

		public void AddUniform(string uniformName)
		{
			if (!uniforms.ContainsKey (uniformName)) {
				int ix = GL.GetUniformLocation (program, uniformName);
				uniforms.Add (uniformName, ix);
			}
		}

		public void SetUniform(string uniformName, int value)
		{
			int ix;
			if (uniforms.TryGetValue (uniformName, out ix)) {
				GL.Uniform1 (ix, value);
			} else {
				throw new Exception ("Unknown uniform value " + uniformName);
			}
		}

		public void SetUniform(string uniformName, float value)
		{
			int ix;
			if (uniforms.TryGetValue (uniformName, out ix)) {
				GL.Uniform1 (ix, value);
			} else {
				throw new Exception ("Unknown uniform value " + uniformName);
			}
		}

		public void SetUniform2(string uniformName, float value0, float value1)
		{
			int ix;
			if (uniforms.TryGetValue (uniformName, out ix)) {
				GL.Uniform2 (ix, value0, value1);
			} else {
				throw new Exception ("Unknown uniform value " + uniformName);
			}
		}


		public void SetUniformMatrix(string uniformName, float[] matrix)
		{
			int ix;
			if (uniforms.TryGetValue (uniformName, out ix)) {
				GL.UniformMatrix4 (ix, 1, false, matrix);
			} else {
				throw new Exception ("Unknown uniform value " + uniformName);
			}
		}

		public int GetAttributeIndex (string attributeName)
		{
			return attributes.IndexOf (attributeName);
		}

		public int GetUniformIndex (string uniformName)
		{
			return GL.GetUniformLocation (program, uniformName);
		}

		public void Link ()
		{
			int status = 0;

			GL.LinkProgram (program);
			GL.ValidateProgram (program);

			GL.GetProgram (program, ProgramParameter.LinkStatus, out status);
			if (status == (int)All.False) {
				Console.WriteLine ("Link failed.");
				Console.WriteLine (String.Format ("Program Log: {0}", ProgramLog ()));
				Console.WriteLine (String.Format ("Fragment Log: {0}", FragmentShaderLog ()));
				Console.WriteLine (String.Format ("Vertex Log: {0}", VertexShaderLog ()));
				return;
			}

			GL.DeleteShader (vertShader);
			GL.DeleteShader (fragShader);
		}

		public void Use ()
		{
			GL.UseProgram (program);
		}

		string GetLog (int obj) {
			string log;
			if (GL.IsShader (obj)) {
				log = GL.GetShaderInfoLog (obj);
			} else {
				log = GL.GetProgramInfoLog (obj);
			}

			return log;
		}

		public string VertexShaderLog ()
		{
			return GetLog (vertShader);
		}

		public string FragmentShaderLog ()
		{
			return GetLog (fragShader);
		}

		public string ProgramLog ()
		{
			return GetLog (program);
		}
	}
}
