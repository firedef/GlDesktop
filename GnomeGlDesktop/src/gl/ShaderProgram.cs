using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL.Compatibility;

namespace GnomeGlDesktop.gl; 

public struct ShaderProgram {
	public static readonly ShaderProgram empty = new();

	public VertexShader vertex;
	public FragmentShader fragment;
	public GeometryShader geometry;
	
	public ProgramHandle programHandle;

	public bool isGenerated => programHandle.Handle != 0;

	public ShaderProgram(VertexShader vert, FragmentShader frag, GeometryShader geom) : this() {
		vertex = vert;
		fragment = frag;
		geometry = geom;
	}
	
	public ShaderProgram(string vert, string frag, string? geom = null) : this() {
		vertex = new(vert);
		fragment = new(frag);
		geometry = new(geom);
	}

	public bool Compile() {
		if (isGenerated) return false;

		programHandle = GL.CreateProgram();
		
		vertex.handle = GL.CreateShader(ShaderType.VertexShader).Handle;
		GL.ShaderSource((ShaderHandle)vertex.handle, vertex.source!);
		GL.CompileShader((ShaderHandle)vertex.handle);
		GL.AttachShader(programHandle, (ShaderHandle) vertex.handle);
		
		fragment.handle = GL.CreateShader(ShaderType.FragmentShader).Handle;
		GL.ShaderSource((ShaderHandle)fragment.handle, fragment.source!);
		GL.CompileShader((ShaderHandle)fragment.handle);
		GL.AttachShader(programHandle, (ShaderHandle) fragment.handle);

		if (geometry.source != null) {
			geometry.handle = GL.CreateShader(ShaderType.GeometryShader).Handle;
			GL.ShaderSource((ShaderHandle)geometry.handle, geometry.source!);
			GL.CompileShader((ShaderHandle)geometry.handle);
			GL.AttachShader(programHandle, (ShaderHandle) geometry.handle);
		}
		
		GL.LinkProgram(programHandle);

		//Log.Note($"compile shader {programHandle.Handle}");

		return true;
		
		/*
		 *
		 * vertexHandle = GL.CreateShader(ShaderType.VertexShader);
		fragmentHandle = GL.CreateShader(ShaderType.FragmentShader);
		programHandle = GL.CreateProgram();

		GL.ShaderSource(vertexHandle, vertexSrc!);
		GL.CompileShader(vertexHandle);
		
		GL.ShaderSource(fragmentHandle, fragmentSrc!);
		GL.CompileShader(fragmentHandle);
		
		GL.AttachShader(programHandle, vertexHandle);
		GL.AttachShader(programHandle, fragmentHandle);
		GL.LinkProgram(programHandle);
		 */
	}

	public void Bind() => OpenGl.BindShader(ref this);

	public int AttributeLocation(string name) {
		if (!isGenerated) Compile();
		return GL.GetAttribLocation(programHandle, name);
	}
	
	public int UniformLocation(string name) {
		if (!isGenerated) Compile();
		return GL.GetUniformLocation(programHandle, name);
	}
	
	public void ProcessAttribute(string name, int size, VertexAttribPointerType type, bool normalized, int stride, int offset) {
		if (!isGenerated) Compile();
		uint loc = (uint) GL.GetAttribLocation(programHandle, name);
		
		GL.VertexAttribPointer(loc, size, type, normalized, stride, offset);
		GL.EnableVertexAttribArray(loc);
	}
	
	public void Dispose() {
		if (!isGenerated) return;
		if (OpenGl.boundShaderProgram == this) OpenGl.UnbindShader();
		
		GL.DeleteProgram(programHandle);
		GL.DeleteShader((ShaderHandle)fragment.handle);
		GL.DeleteShader((ShaderHandle)vertex.handle);
		if (geometry.handle != 0) GL.DeleteShader((ShaderHandle)geometry.handle);

		vertex.handle = 0;
		fragment.handle = 0;
		geometry.handle = 0;
		programHandle = ProgramHandle.Zero;
	}

	public static bool operator ==(ShaderProgram a, ShaderProgram b) => a.programHandle == b.programHandle;
	public static bool operator !=(ShaderProgram a, ShaderProgram b) => a.programHandle != b.programHandle;

	public bool Equals(ShaderProgram other) => programHandle.Equals(other.programHandle);
	public override bool Equals(object? obj) => obj is ShaderProgram other && Equals(other);
	public override int GetHashCode() => programHandle.GetHashCode();
}