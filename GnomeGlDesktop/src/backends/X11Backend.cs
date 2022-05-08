using GnomeGlDesktop.utils;
using GnomeGlDesktop.window;
using X11;

namespace GnomeGlDesktop.backends; 

public class X11Backend : AppBackend {
	private const string _windowTypeNormal = "_NET_WM_WINDOW_TYPE_NORMAL";
	private const string _windowTypeDesktop = "_NET_WM_WINDOW_TYPE_DESKTOP";
	

	public override void SetWindowType(GlfwWindow window, WindowType type) {
		string typeString = type switch {
			WindowType.normal => _windowTypeNormal,
			WindowType.desktop => _windowTypeDesktop,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
		
		IntPtr display = Xlib.XOpenDisplay(null);
		
		X11.Window x11Window = window.x11Window;
		Xlib.XLowerWindow(display, x11Window);
		long[] arr = {(long)XLibUtils.XInternAtom(display, typeString, true)};
		XLibUtils.XChangeProperty(display, x11Window, XLibUtils.XInternAtom(display, "_NET_WM_WINDOW_TYPE", true), Atom.Atom, 32, 0, arr, 1);
		
		Xlib.XCloseDisplay(display);
	}
}