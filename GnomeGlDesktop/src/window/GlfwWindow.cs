using GnomeGlDesktop.gl.render;
using GnomeGlDesktop.gl.render.blit;
using GnomeGlDesktop.utils;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using X11;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;
using Window = OpenTK.Windowing.GraphicsLibraryFramework.Window;

namespace GnomeGlDesktop.window; 

public unsafe class GlfwWindow : IDisposable, IBlitDest {
	public readonly Window* windowPtr;
	public Vector2i size { get; protected set; }

	public X11.Window x11Window => (X11.Window)GLFW.GetX11Window(windowPtr);

	private GlfwWindow(Window* windowPtr) => this.windowPtr = windowPtr;
	
	public void MakeCurrent() => GLFW.MakeContextCurrent(windowPtr);
	public void Swap() => GLFW.SwapBuffers(windowPtr);

	public void SetPosition(Vector2i pos) => GLFW.SetWindowPos(windowPtr, pos.X, pos.Y);

	public static GlfwWindow Create(Vector2i size, string name, Monitor* monitor, Window* share) {
		GlfwWindow win = new(GLFW.CreateWindow(size.X, size.Y, name, monitor, share));
		win.size = size;
		return win;
	}

	private void ReleaseUnmanagedResources() {
		GLFW.DestroyWindow(windowPtr);
	}
	public void Dispose() {
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}
	~GlfwWindow() => ReleaseUnmanagedResources();
	
	public FrameBuffer GetBlitDestFramebuffer() => FrameBuffer.screen;
	public Vector2i GetBlitDestFramebufferSize() => size;
	
	public void BeforeBlit(IBlitSrc src) => MakeCurrent();
	public void AfterBlit(IBlitSrc src) => Swap();
}