namespace GlDesktop.gl.shaders; 

public enum ShaderLoadResult {
	success,
	fileNotFound,
	cannotLoadFile,
	glCompilationError,
	glValidationError,
}