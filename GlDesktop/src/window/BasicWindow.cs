using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GlDesktop.window; 

public class BasicWindow : NativeWindow {
	public float renderTimeDelta { get; private set; }
	public DateTime previousRenderTime { get; private set; } = DateTime.Now;
	public int renderFrequency = 60;
	public bool useThreadSleepForRenderDelay = true;

	public List<BasicWindow> childs = new();

	public BasicWindow(NativeWindowSettings settings) : base(settings) {
		
	}

	public virtual unsafe void Run() {
		OnLoad();
		foreach (BasicWindow win in childs) win.OnLoad();
		OnResize(new(Size));
		while (!ShouldClose()) Render();
		OnUnload();
	}

	private unsafe bool ShouldClose() => GLFW.WindowShouldClose(WindowPtr) || childs.Any(win => GLFW.WindowShouldClose(win.WindowPtr));

	private void Render() {
		WaitForRenderDelay();
		UpdateCurrentWindow();
		foreach (BasicWindow win in childs) win.UpdateCurrentWindow();
	}

	private void WaitForRenderDelay() {
		TimeSpan delay = TimeSpan.FromSeconds(1.0 / renderFrequency);
		
		while (true) {
			DateTime currentTime = DateTime.Now;
			TimeSpan currentDelay = currentTime - previousRenderTime;
			if (currentDelay >= delay) {
				renderTimeDelta = (float)currentDelay.TotalMilliseconds;
				previousRenderTime = currentTime;
				return;
			}

			if (!useThreadSleepForRenderDelay) continue;
			int sleepMs = (int)((delay - currentDelay).TotalMilliseconds * .9);
			if (sleepMs > 1) Thread.Sleep(sleepMs);
		}
	}
	
	private void UpdateCurrentWindow() {
		Thread.Sleep(0);
		Context.MakeCurrent();

		ProcessEvents();
		OnRenderFrame();
		Context.SwapBuffers();
	}

	protected virtual void OnRenderFrame() { }
	protected virtual void OnLoad() { }
	protected virtual void OnUnload() { }
}