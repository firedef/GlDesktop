using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GnomeGlDesktop.gl.shaders; 

public struct ShaderProgram {
	public static readonly ShaderProgram empty = new();
#if SINGLE_GL_CONTEXT_OR_SHARED
	public static ProgramHandle boundShaderProgram { get; private set; } = ProgramHandle.Zero;
#endif

	private VertexShader _vertex;
	private FragmentShader _fragment;
	private GeometryShader _geometry;
	public bool isValid { get; private set; } = true;

	public ProgramHandle programHandle = ProgramHandle.Zero;

	public bool isGenerated => programHandle.Handle != 0;

	public ShaderProgram() {
		_vertex = new();
		_fragment = new();
		_geometry = new();
		programHandle = new();
	}

	public ShaderProgram(string vert, string frag, string? geom = null) : this() {
		_vertex = new(vert);
		_fragment = new(frag);
		_geometry = new(geom);
	}

	public ShaderLoadResult LoadVertexShader(string path) => LoadShader(path, ref _vertex);
	public ShaderLoadResult LoadFragmentShader(string path) => LoadShader(path, ref _fragment);
	public ShaderLoadResult LoadGeometryShader(string path) => LoadShader(path, ref _geometry);
	
	public ShaderLoadResult LoadVertexShaderString(string path) => LoadShaderString(path, out _vertex);
	public ShaderLoadResult LoadFragmentShaderString(string path) => LoadShaderString(path, out _fragment);
	public ShaderLoadResult LoadGeometryShaderString(string path) => LoadShaderString(path, out _geometry);

	private static ShaderLoadResult LoadShader<T>(string path, ref T v) where T : IShaderPart, new() {
		if (!File.Exists(path)) return ShaderLoadResult.fileNotFound;
		v = ShaderExtensions.FromFile<T>(path);
		return !ShaderExtensions.Compile(ref v) ? ShaderLoadResult.glCompilationError : ShaderLoadResult.success;
	}
	
	private static ShaderLoadResult LoadShaderString<T>(string str, out T v) where T : IShaderPart, new() {
		v = ShaderExtensions.FromString<T>(str);
		return !ShaderExtensions.Compile(ref v) ? ShaderLoadResult.glCompilationError : ShaderLoadResult.success;
	}

	public bool Compile() {
		if (isGenerated) return false;

		programHandle = GL.CreateProgram();
		
		if (ShaderExtensions.TryCompile(ref _vertex)) _vertex.Attach(programHandle);
		if (ShaderExtensions.TryCompile(ref _fragment)) _fragment.Attach(programHandle);
		if (ShaderExtensions.TryCompile(ref _geometry)) _geometry.Attach(programHandle);
		
		GL.LinkProgram(programHandle);
		
		_vertex.Delete();
		_fragment.Delete();
		_geometry.Delete();
		
		if (programHandle.Validate()) return true;
		isValid = false;
		
		Console.WriteLine("Shader program is not validated:");
		Console.WriteLine(programHandle.GetLog());
		return false;
	}

	public void Bind() {
		if (!isValid) return;
		if (!isGenerated) if (!Compile()) return;
	#if SINGLE_GL_CONTEXT_OR_SHARED
		if (boundShaderProgram == programHandle) return;
		boundShaderProgram = programHandle;
	#endif
		GL.UseProgram(programHandle);
	}
	
	public void Unbind() {
	#if SINGLE_GL_CONTEXT_OR_SHARED
		if (boundShaderProgram != programHandle) return;
		boundShaderProgram = ProgramHandle.Zero;
	#endif
		GL.UseProgram(ProgramHandle.Zero);
	}

	public int AttributeLocation(string name) {
		if (!isValid) return -1;
		if (!isGenerated) if (!Compile()) return -1;
		
		return GL.GetAttribLocation(programHandle, name);
	}
	
	public int UniformLocation(string name) {
		if (!isValid) return -1;
		if (!isGenerated) if (!Compile()) return -1;
		
		return GL.GetUniformLocation(programHandle, name);
	}

	public void Dispose() {
		if (!isGenerated || !isValid) return;
		Unbind();
		
		GL.DeleteProgram(programHandle);
		
		programHandle = ProgramHandle.Zero;
	}

	public static bool operator ==(ShaderProgram a, ShaderProgram b) => a.programHandle == b.programHandle;
	public static bool operator !=(ShaderProgram a, ShaderProgram b) => a.programHandle != b.programHandle;

	public bool Equals(ShaderProgram other) => programHandle.Equals(other.programHandle);
	public override bool Equals(object? obj) => obj is ShaderProgram other && Equals(other);
	public override int GetHashCode() => programHandle.GetHashCode();
}