namespace GnomeGlDesktop.gl; 

public interface IShaderPart {
	public string? source { get; set; }
	public int handle { get; set; }
}

public struct VertexShader : IShaderPart {
	public string? source { get; set; }
	public int handle { get; set; }

	public VertexShader(string? src) : this() => source = src;
}

public struct FragmentShader : IShaderPart {
	public string? source { get; set; }
	public int handle { get; set; }
	
	public FragmentShader(string? src) : this() => source = src;
}

public struct GeometryShader : IShaderPart {
	public string? source { get; set; }
	public int handle { get; set; }
	
	public GeometryShader(string? src) : this() => source = src;
}