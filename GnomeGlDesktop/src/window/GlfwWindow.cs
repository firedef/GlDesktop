using GnomeGlDesktop.gl.render;
using GnomeGlDesktop.utils;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using X11;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;
using Window = OpenTK.Windowing.GraphicsLibraryFramework.Window;

namespace GnomeGlDesktop.window; 

public unsafe class GlfwWindow : IDisposable {
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

	//TODO: create backend and move all x11 functions
	public static GlfwWindow CreateDesktop(Vector2i size, string name, Monitor* monitor, int monitorId) {
		GLFW.WindowHint(WindowHintBool.Decorated, false);
		GLFW.WindowHint(WindowHintBool.Maximized, true);
		GLFW.WindowHint(WindowHintBool.AutoIconify, true);
		//GLFW.WindowHint(WindowHintBool.TransparentFramebuffer, true);
		GLFW.WindowHint(WindowHintBool.DoubleBuffer, false);
		GLFW.WindowHint(WindowHintString.X11InstanceName, "GlDesktop");
		
		GlfwWindow win = new(GLFW.CreateWindow(size.X, size.Y, name, monitor, GlContext.global.windowPtr));
		win.size = size;

		IntPtr display = Xlib.XOpenDisplay(null);
		
		X11.Window x11Window = win.x11Window;
		Xlib.XLowerWindow(display, x11Window);
		Xlib.XStoreName(display, x11Window, $"GlDesktop_{monitorId}");
		long[] arr = {(long)XLibUtils.XInternAtom(display, "_NET_WM_WINDOW_TYPE_DESKTOP", true)};
		XLibUtils.XChangeProperty(display, x11Window, XLibUtils.XInternAtom(display, "_NET_WM_WINDOW_TYPE", true), Atom.Atom, 32, 0, arr, 1);
		
		Xlib.XCloseDisplay(display);
		
		GLFW.GetMonitorPos(monitor, out int x, out int y);
		win.SetPosition(new(x,y));
		Console.WriteLine($"Move window #{monitorId} to ({x},{y})");
		
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
}