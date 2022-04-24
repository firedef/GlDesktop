using GnomeGlDesktop.window;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using X11;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;
using Window = X11.Window;

namespace GnomeGlDesktop; 

public static class GlDesktop {
	private static readonly List<GlWindow> _windows = new();

	private static unsafe void AddWindows() {
		//_windows.Add(CreateWindow(null));
		//_windows[0].Run();
		Monitor*[] monitors = GLFW.GetMonitors();
		foreach (Monitor* monitor in monitors) _windows.Add(CreateWindow(monitor));
		_windows[0].childWindows = _windows.Skip(1).ToList();
		_windows[0].Run();
		//foreach (GlWindow window in _windows) Task.Run(window.Run);
	}

	private static unsafe GlWindow CreateWindow(Monitor* monitor) {
		GLFW.WindowHint(WindowHintBool.Decorated, false);
		GLFW.WindowHint(WindowHintBool.Maximized, true);
		GLFW.WindowHint(WindowHintBool.AutoIconify, true);
		GLFW.WindowHint(WindowHintString.X11InstanceName, "GlDesktop");
		//GLFW.WindowHint(WindowHintBool.Floating, true);
		//Console.WriteLine(GLFW.GetVersionString());
		//GLFW.WindowHint(WindowHintBool.MousePassthrough, true);
		GLFW.WindowHint(WindowHintBool.TransparentFramebuffer, true);
		
		GameWindowSettings winSettings = GameWindowSettings.Default;
		winSettings.RenderFrequency = 30;
		
		NativeWindowSettings nativeSettings = NativeWindowSettings.Default;
		GLFW.GetMonitorPos(monitor, out int x, out int y);
		nativeSettings.Location = new(x, y);
		//nativeSettings.CurrentMonitor = new((IntPtr)monitor);
		//nativeSettings.
		GlWindow win = new(winSettings, nativeSettings);
		Window x11Window = (Window)GLFW.GetX11Window(win.WindowPtr);
		IntPtr display = Xlib.XOpenDisplay(null);
		Xlib.XLowerWindow(display, x11Window);
		//Xlib.XStoreName(display, x11Window, "GlDesktop");
		Xlib.XCloseDisplay(display);
		
		//win.WindowState = WindowState.Fullscreen;
		//win.CurrentMonitor = new((IntPtr)monitor);
		return win;
	}

	public static void Start() {
		GLFW.Init();
		AddWindows();
	}
}