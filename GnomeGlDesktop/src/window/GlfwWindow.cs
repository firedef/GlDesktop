using GnomeGlDesktop.gl.render;
using GnomeGlDesktop.gl.render.blit;
using MathStuff.vectors;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;
using Window = OpenTK.Windowing.GraphicsLibraryFramework.Window;

namespace GnomeGlDesktop.window; 

public unsafe partial class GlfwWindow : IDisposable, IBlitDest {
	public readonly Window* windowPtr;
	public int2 size { get; protected set; }
	public readonly List<IWindowEvents> windowEventsListeners = new();

	public X11.Window x11Window => (X11.Window)GLFW.GetX11Window(windowPtr);

	private GlfwWindow(Window* windowPtr) {
		this.windowPtr = windowPtr;
		RegisterCallbacks();
	}
	
	public void MakeCurrent() => GLFW.MakeContextCurrent(windowPtr);
	public void Swap() => GLFW.SwapBuffers(windowPtr);

	public void SetPosition(int2 pos) => GLFW.SetWindowPos(windowPtr, pos.x, pos.y);

	public static GlfwWindow Create(int2 size, string name, Monitor* monitor, Window* share) {
		GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 5);
		GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
		GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
		GlfwWindow win = new(GLFW.CreateWindow(size.x, size.y, name, monitor, share));
		win.size = size;
		win.OnLoad();
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
	public int2 GetBlitDestFramebufferSize() => size;

	public void BeforeBlit(IBlitSrc src) {
		MakeCurrent();
		GLFW.PollEvents();
	}
	public void AfterBlit(IBlitSrc src) => Swap();
}