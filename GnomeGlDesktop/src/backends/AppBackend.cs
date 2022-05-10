using GnomeGlDesktop.debug.log;
using GnomeGlDesktop.window;
using MathStuff.vectors;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

namespace GnomeGlDesktop.backends; 

public abstract class AppBackend {
	public static AppBackend backend = CreateBackend();

	private static AppBackend CreateBackend() => new X11Backend();
	
	public virtual unsafe GlfwWindow CreateWindow(int2 size, string name, Monitor* monitor, Window* share) => GlfwWindow.Create(size, name, monitor, share);

	public virtual unsafe GlfwWindow CreateDesktopWindow(string name, Monitor* monitor, Window* share) {
		const bool decorated = false;
		const bool maximized = true;
		const bool autoIconify = false;
		const bool doubleBuffer = false;
		
		GetMonitorSize(monitor).Deconstruct(out int sX, out int sY);
		
		Log.Note($"Create desktop window '{name}' {sX}x{sY}");
		Log.Minimal($"  Decorated:    {decorated}");
		Log.Minimal($"  Maximized:    {maximized}");
		Log.Minimal($"  AutoIconify:  {autoIconify}");
		Log.Minimal($"  DoubleBuffer: {doubleBuffer}");
		
		GLFW.WindowHint(WindowHintBool.Decorated, decorated);
		GLFW.WindowHint(WindowHintBool.Maximized, maximized);
		GLFW.WindowHint(WindowHintBool.AutoIconify, autoIconify);
		GLFW.WindowHint(WindowHintBool.DoubleBuffer, doubleBuffer);
		
		GlfwWindow window = CreateWindow(new(sX, sY), name, monitor, share);
		
		SetWindowType(window, WindowType.desktop);
		MoveWindowToMonitor(window, monitor);

		return window;
	}

	public virtual unsafe void MoveWindowToMonitor(GlfwWindow window, Monitor* monitor) {
		GLFW.GetMonitorPos(monitor, out int x, out int y);
		window.SetPosition(new(x,y));
		
		Log.Note($"Move window to ({x},{y})");
	}
	
	public virtual unsafe Vector2i GetMonitorSize(Monitor* monitor) {
		VideoMode* videoMode = GLFW.GetVideoMode(monitor);
		return new(videoMode->Width, videoMode->Height);
	}
	
	public virtual unsafe int GetRefreshRate(Monitor* monitor) => GLFW.GetVideoMode(monitor)->RefreshRate;

	public abstract void SetWindowType(GlfwWindow window, WindowType type);

	public abstract void UpdateWindowInput(GlfwWindow window);
}