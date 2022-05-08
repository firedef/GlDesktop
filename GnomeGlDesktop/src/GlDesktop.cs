using GnomeGlDesktop.backends;
using GnomeGlDesktop.gl.render;
using GnomeGlDesktop.window;
using OpenTK.Graphics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

namespace GnomeGlDesktop; 

public static class GlDesktop {
	private static CustomRendererBase _rendererBase;

	private static unsafe void AddWindows() {
		_rendererBase = new();
		
		Monitor*[] monitors = GLFW.GetMonitors();
		foreach (Monitor* m in monitors)
			_rendererBase.windows.Add(AppBackend.backend.CreateDesktopWindow("GlDesktop", m, GlContext.global.windowPtr));
		_rendererBase.Run();
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
		Thread.CurrentThread.Priority = ThreadPriority.Lowest;
		GLFW.Init();
		GLLoader.LoadBindings(new GLFWBindingsContext());
		AddWindows();
	}
}