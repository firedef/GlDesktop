using GlDesktop.backends;
using GlDesktop.window.input;
using MathStuff.vectors;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GlDesktop.window;

public unsafe partial class GlfwWindow {
	public readonly WindowInput input = new();
	
	private void RegisterCallbacks() {
		GLFW.SetCharCallback(windowPtr, (_, i) => OnTextInput(new((int)i)));
		GLFW.SetScrollCallback(windowPtr, (_, x, y) => OnMouseWheel(new((float)x, (float)y)));
		GLFW.SetFramebufferSizeCallback(windowPtr, (_, x, y) => OnResize(new(x, y)));
		GLFW.SetMouseButtonCallback(windowPtr, MouseButtonCallback);
	}
	
	private void MouseButtonCallback(Window* window, MouseButton button, InputAction action, KeyModifiers mods) {
		switch (action) {
			case InputAction.Release:
				OnMouseUp(button, mods);
				break;
			case InputAction.Press: 
				OnMouseDown(button, mods);
				break;
			case InputAction.Repeat: 
				OnMouseRepeat(button, mods);
				break;
			default:                  throw new ArgumentOutOfRangeException(nameof(action), action, null);
		}
	}

	protected virtual void OnMouseUp(MouseButton button, KeyModifiers mods) {
		input.SetMouseButton(button, false);
	}
	
	protected virtual void OnMouseDown(MouseButton button, KeyModifiers mods) {
		input.SetMouseButton(button, true);
	}
	
	protected virtual void OnMouseRepeat(MouseButton button, KeyModifiers mods) {
		
	}

	protected virtual void OnTextInput(TextInputEventArgs e) {
		foreach (IWindowEvents listener in windowEventsListeners) listener.OnTextInput(this, e);
	}
	
	protected virtual void OnMouseWheel(MouseWheelEventArgs e) {
		foreach (IWindowEvents listener in windowEventsListeners) listener.OnMouseWheel(this, e);
	}
	
	protected virtual void OnResize(int2 s) {
		size = s;
		foreach (IWindowEvents listener in windowEventsListeners) listener.OnResize(this, s);
	}
	
	protected virtual void OnLoad() {
		foreach (IWindowEvents listener in windowEventsListeners) listener.OnLoad(this);
	}
	
	protected virtual void OnUnload() {
		foreach (IWindowEvents listener in windowEventsListeners) listener.OnUnload(this);
	}

	public void UpdateInput() {
		AppBackend.backend.UpdateWindowInput(this);
		
		//if (input.IsAnyMouseButtonDown())
	}
}