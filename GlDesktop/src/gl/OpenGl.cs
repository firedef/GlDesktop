using GlDesktop.gl.shaders;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;

namespace GlDesktop.gl; 

public class OpenGl {
	public static Version glVersion;
	public static ShaderProgram boundShaderProgram = ShaderProgram.empty;

	public static bool supportNamedBuffers => true || glVersion >= new Version(4, 5);

	static OpenGl() {
		GLFW.GetVersion(out int major, out int minor, out int revision);
		glVersion = new(major, minor, revision);
	}
	
	public static void BindShader(ref ShaderProgram v) {
		if (!v.isGenerated) v.Compile();
		//if (boundShaderProgram == v) return;
		boundShaderProgram = v;
		//Log.Minimal($"bound shader {v.programHandle.Handle}", LogForm.rendererCore);
		GL.UseProgram(v.programHandle);
	}
	
	public static void UnbindShader() {
		if (boundShaderProgram == ShaderProgram.empty) return;
		//Log.Minimal($"unbound shader {boundShaderProgram.programHandle.Handle}", LogForm.rendererCore);
		boundShaderProgram = ShaderProgram.empty;
		GL.UseProgram(ProgramHandle.Zero);
	}
	
	public static void CheckErrors() {
		while (true) {
			ErrorCode errorCode = GL.GetError();
			if (errorCode == ErrorCode.NoError) return;
			//Log.Error($"OpenGl error {(int) errorCode}: {errorCode}");
		}
	}
	
	public static void CheckErrors(string partName) {
		while (true) {
			ErrorCode errorCode = GL.GetError();
			if (errorCode == ErrorCode.NoError) return;
			Console.WriteLine($"OpenGl error {(int) errorCode}: {errorCode} ({partName})");
			//Log.Error($"OpenGl error {(int) errorCode}: {errorCode} ({partName})");
		}
	}
}