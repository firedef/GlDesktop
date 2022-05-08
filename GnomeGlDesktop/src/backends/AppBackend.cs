using GnomeGlDesktop.window;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

namespace GnomeGlDesktop.backends; 

public abstract class AppBackend {
	public static AppBackend backend = CreateBackend();

	private static AppBackend CreateBackend() => new X11Backend();
	
	public virtual unsafe GlfwWindow CreateWindow(Vector2i size, string name, Monitor* monitor, Window* share) => GlfwWindow.Create(size, name, monitor, share);

	public virtual unsafe GlfwWindow CreateDesktopWindow(string name, Monitor* monitor, Window* share) {
		GLFW.WindowHint(WindowHintBool.Decorated, false);
		GLFW.WindowHint(WindowHintBool.Maximized, true);
		GLFW.WindowHint(WindowHintBool.AutoIconify, true);
		GLFW.WindowHint(WindowHintBool.DoubleBuffer, false);

		Vector2i size = GetMonitorSize(monitor);

		GlfwWindow window = CreateWindow(size, name, monitor, share);
		
		SetWindowType(window, WindowType.desktop);
		MoveWindowToMonitor(window, monitor);

		return window;
	}

	public virtual unsafe void MoveWindowToMonitor(GlfwWindow window, Monitor* monitor) {
		GLFW.GetMonitorPos(monitor, out int x, out int y);
		window.SetPosition(new(x,y));
	}
	
	public virtual unsafe Vector2i GetMonitorSize(Monitor* monitor) {
		VideoMode* videoMode = GLFW.GetVideoMode(monitor);
		return new(videoMode->Width, videoMode->Height);
	}
	
	public virtual unsafe int GetRefreshRate(Monitor* monitor) => GLFW.GetVideoMode(monitor)->RefreshRate;

	public abstract void SetWindowType(GlfwWindow window, WindowType type);
}