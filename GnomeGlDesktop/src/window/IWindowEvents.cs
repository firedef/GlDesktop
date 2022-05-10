using MathStuff.vectors;
using OpenTK.Windowing.Common;

namespace GnomeGlDesktop.window;

//TODO: add more events
public interface IWindowEvents {
	public void OnTextInput(GlfwWindow win, TextInputEventArgs e) {}
	public void OnMouseWheel(GlfwWindow win, MouseWheelEventArgs e) {}
	public void OnResize(GlfwWindow win, int2 s) {}
	public void OnLoad(GlfwWindow win) {}
	public void OnUnload(GlfwWindow win) {}
}