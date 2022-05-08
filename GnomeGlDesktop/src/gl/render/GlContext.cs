using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GnomeGlDesktop.gl.render; 

public readonly unsafe struct GlContext : IDisposable {
	public static readonly GlContext global = Create();
	
	public readonly Window* windowPtr;

	public GlContext(Window* windowPtr) => this.windowPtr = windowPtr;

	public static GlContext CreateWithHints() => new(GLFW.CreateWindow(1, 1, "", null, null));
	
	public static GlContext Create() {
		GLFW.WindowHint(WindowHintBool.DoubleBuffer, false);
		GLFW.WindowHint(WindowHintBool.TransparentFramebuffer, false);
		GLFW.WindowHint(WindowHintBool.Visible, false);
		return CreateWithHints();
	}

	public void MakeCurrent() => GLFW.MakeContextCurrent(windowPtr);

	public void Dispose() => GLFW.DestroyWindow(windowPtr);
}