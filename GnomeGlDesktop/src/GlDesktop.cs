using GnomeGlDesktop.utils;
using GnomeGlDesktop.window;
using ImGuiNET;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using X11;
using Cursor = X11.Cursor;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;
using Window = X11.Window;

namespace GnomeGlDesktop; 

public static class GlDesktop {
	private static readonly List<GlWindow> _windows = new();

	private static unsafe void AddWindows() {
		Monitor*[] monitors = GLFW.GetMonitors();
		//for (int i = 0; i < 1; i++) _windows.Add(CreateWindow(monitors[i], i));
		for (int i = 0; i < monitors.Length; i++) _windows.Add(CreateWindow(monitors[i], i));
		_windows[0].childWindows = _windows.Skip(1).ToList();
		_windows[0].Run();
	}

	private static unsafe GlWindow CreateWindow(Monitor* monitor, int i) {
		GLFW.WindowHint(WindowHintBool.Decorated, false);
		GLFW.WindowHint(WindowHintBool.Maximized, true);
		GLFW.WindowHint(WindowHintBool.AutoIconify, true);
		GLFW.WindowHint(WindowHintBool.TransparentFramebuffer, true);
		GLFW.WindowHint(WindowHintBool.DoubleBuffer, false);
		GLFW.WindowHint(WindowHintString.X11InstanceName, "GlDesktop");
		
		GameWindowSettings winSettings = GameWindowSettings.Default;
		winSettings.RenderFrequency = 24;
		winSettings.IsMultiThreaded = false;
		
		NativeWindowSettings nativeSettings = NativeWindowSettings.Default;
		GLFW.GetMonitorPos(monitor, out int x, out int y);
		nativeSettings.Location = new(x, y);
		GlWindow win = new(winSettings, nativeSettings, i);
		
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
		AddWindows();
	}
}