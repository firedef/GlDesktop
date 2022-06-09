using System.Diagnostics;
using GlDesktop.debug.log;
using GlDesktop.external;
using GlDesktop.utils;
using GlDesktop.window;
using OpenTK.Windowing.GraphicsLibraryFramework;
using X11;
using KeySym = GlDesktop.external.KeySym;
using Window = X11.Window;

namespace GlDesktop.backends; 

public class X11Backend : AppBackend {
	private const string _windowTypeNormal = "_NET_WM_WINDOW_TYPE_NORMAL";
	private const string _windowTypeDesktop = "_NET_WM_WINDOW_TYPE_DESKTOP";

	private static bool inputInit = false;

	public override void SetWindowType(GlfwWindow window, WindowType type) {
		Log.Note($"Set window mode to {type}");
		
		string typeString = type switch {
			WindowType.normal => _windowTypeNormal,
			WindowType.desktop => _windowTypeDesktop,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
		
		IntPtr display = Xlib.XOpenDisplay(null);
		
		Window x11Window = window.x11Window;
		Xlib.XLowerWindow(display, x11Window);
		long[] arr = {(long)XLibUtils.XInternAtom(display, typeString, true)};
		XLibUtils.XChangeProperty(display, x11Window, XLibUtils.XInternAtom(display, "_NET_WM_WINDOW_TYPE", true), Atom.Atom, 32, 0, arr, 1);
		
		Xlib.XCloseDisplay(display);
	}

	public override unsafe void UpdateWindowInput(GlfwWindow window) {
		if (!inputInit) CoreApi.StartXInput();
		inputInit = true;
		CoreApi.UpdateXInput();

		XInputData* inputDataPtr = CoreApi.GetInputDataPtr();
		
		window.input.Reset();
		
		window.input.mousePosition = new((float)inputDataPtr->position_x, (float)inputDataPtr->position_y);
		window.input.scroll = new((float)inputDataPtr->scroll_x, (float)inputDataPtr->scroll_y);
		
		window.input.SetMouseButton(MouseButton.Left, (inputDataPtr->mouseButtons & XInputDataMouseButtons.left) != 0);
		window.input.SetMouseButton(MouseButton.Right, (inputDataPtr->mouseButtons & XInputDataMouseButtons.right) != 0);
		window.input.SetMouseButton(MouseButton.Middle, (inputDataPtr->mouseButtons & XInputDataMouseButtons.middle) != 0);
		window.input.SetMouseButton(MouseButton.Button4, (inputDataPtr->mouseButtons & XInputDataMouseButtons.back) != 0);
		window.input.SetMouseButton(MouseButton.Button5, (inputDataPtr->mouseButtons & XInputDataMouseButtons.forward) != 0);

		int keysPressedCount = CoreApi.GetKeySymCount();
		KeySym* keySymArr = stackalloc KeySym[keysPressedCount];
		CoreApi.WriteKeySymTo(keySymArr, keysPressedCount);

		for (int i = 0; i < keysPressedCount; i++) {
			window.input.characters.Add((char) CoreApi.KeySymToUnicode(keySymArr[i], inputDataPtr->modifiers));
			//Keys
			//window.input.SetKey();
		}
	}
}