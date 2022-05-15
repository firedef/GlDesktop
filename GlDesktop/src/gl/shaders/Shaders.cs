using OpenTK.Graphics.OpenGL;

namespace GlDesktop.gl.shaders; 

public interface IShaderPart {
	public ShaderType type { get; }
	public string? source { get; set; }
	public int handle { get; set; }
}

public struct VertexShader : IShaderPart {
	public string? source { get; set; }
	public int handle { get; set; }
	public ShaderType type => ShaderType.VertexShader;

	public VertexShader(string? src) : this() => source = src;
}

public struct FragmentShader : IShaderPart {
	public string? source { get; set; }
	public int handle { get; set; }
	public ShaderType type => ShaderType.FragmentShader;
	
	public FragmentShader(string? src) : this() => source = src;
}

public struct GeometryShader : IShaderPart {
	public string? source { get; set; }
	public int handle { get; set; }
	public ShaderType type => ShaderType.GeometryShader;
	
	public GeometryShader(string? src) : this() => source = src;
}