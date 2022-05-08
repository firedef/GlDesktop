using GnomeGlDesktop.gl.render;
using GnomeGlDesktop.utils;
using GnomeGlDesktop.window;
using OpenTK.Graphics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using X11;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;
using Window = X11.Window;

namespace GnomeGlDesktop; 

public static class GlDesktop {
	private static readonly List<GlWindow> _windows = new();
	private static CustomRendererBase _rendererBase;

	private static unsafe void AddWindows() {
		// Monitor*[] monitors = GLFW.GetMonitors();
		// for (int i = 0; i < monitors.Length; i++) 
		// 	_windows.Add(CreateWindow(monitors[i], i, i == 0 ? null : _windows[0].Context));
		// _windows[0].childs = _windows.Skip(1).Cast<BasicWindow>().ToList();
		// _windows[0].Run();

		_rendererBase = new();
		
		Monitor*[] monitors = GLFW.GetMonitors();
		for (int i = 0; i < monitors.Length; i++)
			_rendererBase.windows.Add(GlfwWindow.CreateDesktop(new(1920,1080), "GlDesktop", monitors[i], i));
		Console.WriteLine(monitors.Length);
		_rendererBase.Run();
	}

	private static unsafe GlWindow CreateWindow(Monitor* monitor, int i, IGLFWGraphicsContext? sharedContext) {
		GLFW.WindowHint(WindowHintBool.Decorated, false);
		GLFW.WindowHint(WindowHintBool.Maximized, true);
		GLFW.WindowHint(WindowHintBool.AutoIconify, true);
		//GLFW.WindowHint(WindowHintBool.TransparentFramebuffer, true);
		GLFW.WindowHint(WindowHintBool.DoubleBuffer, false);
		GLFW.WindowHint(WindowHintString.X11InstanceName, "GlDesktop");
		

		NativeWindowSettings nativeSettings = NativeWindowSettings.Default;
		GLFW.GetMonitorPos(monitor, out int x, out int y);
		nativeSettings.Location = new(x, y);
		//nativeSettings.NumberOfSamples = 4;
		nativeSettings.SharedContext = new GLFWGraphicsContext(GlContext.global.windowPtr);
		// if (sharedContext != null) nativeSettings.SharedContext = sharedContext;
		
		GlWindow win = new(nativeSettings, i);
		win.renderFrequency = 30;
		
		Window x11Window = (Window)GLFW.GetX11Window(win.WindowPtr);
		IntPtr display = Xlib.XOpenDisplay(null);
		Xlib.XLowerWindow(display, x11Window);
		Xlib.XStoreName(display, x11Window, $"GlDesktop_{i}");
		long[] arr = {(long)XLibUtils.XInternAtom(display, "_NET_WM_WINDOW_TYPE_DESKTOP", true)};
		XLibUtils.XChangeProperty(display, x11Window, XLibUtils.XInternAtom(display, "_NET_WM_WINDOW_TYPE", true), Atom.Atom, 32, 0, arr, 1);
		
		Xlib.XCloseDisplay(display);

		return win;
	}

	public static void Start() {
		Thread.CurrentThread.Priority = ThreadPriority.Lowest;
		GLFW.Init();
		GLLoader.LoadBindings(new GLFWBindingsContext());
		AddWindows();
	}
}