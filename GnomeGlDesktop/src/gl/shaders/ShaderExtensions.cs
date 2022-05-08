using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL.Compatibility;

namespace GnomeGlDesktop.gl.shaders;

public static class ShaderExtensions {
	public static bool TryCompile<T>(ref T shader) where T : IShaderPart => shader.source != null && Compile(ref shader);
	
	public static bool Compile<T>(ref T @this) where T : IShaderPart  {
		if (@this.handle != 0) return true;
		
		ShaderHandle shader = GL.CreateShader(@this.type);
		GL.ShaderSource(shader, @this.source!);
		GL.CompileShader(shader);

		int isCompiled = 0;
		GL.GetShaderi(shader, ShaderParameterName.CompileStatus, ref isCompiled);
		if (isCompiled == 0) {
			GL.GetShaderInfoLog(shader, out string log);
			Console.WriteLine($"Failed to compile {@this.type}:");
			Console.WriteLine(log);
			GL.DeleteShader(shader);
			return false;
		}
		@this.handle = shader.Handle;
		return true;
	}

	public static void Attach<T>(this T shader, ProgramHandle programHandle) where T : IShaderPart => GL.AttachShader(programHandle, new(shader.handle));

	public static bool Validate(this ProgramHandle handle) {
		GL.ValidateProgram(handle);
		int validateStatus = 0;
		GL.GetProgrami(handle, ProgramPropertyARB.ValidateStatus, ref validateStatus);
		return validateStatus == 1;
	}

	public static string GetLog(this ProgramHandle handle) {
		GL.GetProgramInfoLog(handle, out string log);
		return log;
	}
	
	public static string GetLog(this ShaderHandle handle) {
		GL.GetShaderInfoLog(handle, out string log);
		return log;
	}

	public static void Delete(this IShaderPart @this) {
		if (@this.handle != 0) GL.DeleteShader(new(@this.handle));
		@this.handle = 0;
	}
	
	public static void Delete(this ProgramHandle @this) {
		if (@this.Handle != 0) GL.DeleteProgram(@this);
	}

	public static T FromString<T>(string str) where T : IShaderPart, new() => new() {source = str};
	
	public static T FromFile<T>(string path) where T : IShaderPart, new() {
		if (!File.Exists(path)) throw new FileNotFoundException($"Shader file not found: '{path}'");
		return FromString<T>(File.ReadAllText(path));
	}
}