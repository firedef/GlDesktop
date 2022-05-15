using GlDesktop.backends;
using GlDesktop.debug.log;
using GlDesktop.gl.render;
using GlDesktop.window;
using OpenTK.Graphics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using GL = OpenTK.Graphics.OpenGL.GL;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

namespace GlDesktop; 

public static class GlDesktop {
	private static CustomRenderer _renderer;

	private static unsafe void AddWindows() {
		_renderer = new();
		
		Monitor*[] monitors = GLFW.GetMonitors();
		Log.Note($"Found {monitors.Length} monitors");
		
		foreach (Monitor* m in monitors)
			_renderer.renderer.AddWindow(AppBackend.backend.CreateDesktopWindow("GlDesktop", m, GlContext.global.windowPtr));
		
		_renderer.AddImGui();
		
		Log.Important("Application loop start");
		_renderer.renderer.Run();
		Log.Important("Application loop exit");
	}

	// private static unsafe GlWindow CreateWindow(Monitor* monitor, int i, IGLFWGraphicsContext? sharedContext) {
	// 	GLFW.WindowHint(WindowHintBool.Decorated, false);
	// 	GLFW.WindowHint(WindowHintBool.Maximized, true);
	// 	GLFW.WindowHint(WindowHintBool.AutoIconify, true);
	// 	//GLFW.WindowHint(WindowHintBool.TransparentFramebuffer, true);
	// 	GLFW.WindowHint(WindowHintBool.DoubleBuffer, false);
	// 	GLFW.WindowHint(WindowHintString.X11InstanceName, "GlDesktop");
	// 	
	//
	// 	NativeWindowSettings nativeSettings = NativeWindowSettings.Default;
	// 	GLFW.GetMonitorPos(monitor, out int x, out int y);
	// 	nativeSettings.Location = new(x, y);
	// 	//nativeSettings.NumberOfSamples = 4;
	// 	nativeSettings.SharedContext = new GLFWGraphicsContext(GlContext.global.windowPtr);
	// 	// if (sharedContext != null) nativeSettings.SharedContext = sharedContext;
	// 	
	// 	GlWindow win = new(nativeSettings, i);
	// 	win.renderFrequency = 30;
	// 	
	// 	Window x11Window = (Window)GLFW.GetX11Window(win.WindowPtr);
	// 	IntPtr display = Xlib.XOpenDisplay(null);
	// 	Xlib.XLowerWindow(display, x11Window);
	// 	Xlib.XStoreName(display, x11Window, $"GlDesktop_{i}");
	// 	long[] arr = {(long)XLibUtils.XInternAtom(display, "_NET_WM_WINDOW_TYPE_DESKTOP", true)};
	// 	XLibUtils.XChangeProperty(display, x11Window, XLibUtils.XInternAtom(display, "_NET_WM_WINDOW_TYPE", true), Atom.Atom, 32, 0, arr, 1);
	// 	
	// 	Xlib.XCloseDisplay(display);
	//
	// 	return win;
	// }

	public static void Start() {
		Log.Important("Application start");
		
		Log.Message("Set thread priority to Lowest");
		Thread.CurrentThread.Priority = ThreadPriority.Lowest;
		
		Log.Message("GLFW Init");
		GLFW.Init();
		
		Log.Message("Load OpenGl bindings");
		GLLoader.LoadBindings(new GLFWBindingsContext());
		
		Log.Message("Adding windows");
		AddWindows();
	}
}