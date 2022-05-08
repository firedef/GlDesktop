using GnomeGlDesktop.gl.render.attachments;
using GnomeGlDesktop.gl.render.postProcess;
using GnomeGlDesktop.gl.render.targets;
using GnomeGlDesktop.window;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GnomeGlDesktop.gl.render; 

public class RendererBase : IDisposable {
	public readonly RenderTarget general;
	public readonly PostProcessor postProcess;
	
	public float renderingQuality = 1f;
	public Vector2i targetResolution = new(1920, 1080);
	public Vector2i targetFramebufferResolution => (Vector2i)((Vector2)targetResolution * renderingQuality);
	public int samples = 0;
	public InternalFormat format = InternalFormat.Rgba16f;
	
	public float renderTimeDelta { get; private set; }
	public DateTime previousRenderTime { get; private set; } = DateTime.Now;
	public int renderFrequency = 60;
	public bool useThreadSleepForRenderDelay = true;

	public readonly List<GlfwWindow> windows = new();
	
	// public GlfwRenderer(int width, int height, int samples = 0, InternalFormat format = InternalFormat.Rgba) : base(width, height, samples, format) {
	// 	this.format = format;
	// 	this.samples = samples;
	// }
	
	public RendererBase() {
		GlContext.global.MakeCurrent();
		general = new(targetFramebufferResolution.X, targetFramebufferResolution.Y, samples, format, true);
		general.AddAttachment(0, AttachmentType.color, format);
		general.AddAttachment(0, AttachmentType.depthStencil, InternalFormat.Depth24Stencil8);
		
		postProcess = new(targetFramebufferResolution);
		postProcess.AddAttachment(AttachmentType.color, InternalFormat.Rgba16f);
		postProcess.AddAttachment(AttachmentType.depthStencil, InternalFormat.Depth24Stencil8);
		

		// frameBuffer = FrameBuffer.Create();
		// frameBuffer.Bind();
		// RecreateRenderBuffers(targetFramebufferResolution.X, targetFramebufferResolution.Y, samples, format);
	}

	public void RecreateRenderBuffers() {
		general.RecreateRenderBuffers(targetFramebufferResolution.X, targetFramebufferResolution.Y, samples);
		postProcess.RecreateTextures(targetFramebufferResolution);
	}

	public void End() {
		general.BlitToRenderTarget(postProcess.textures);
		general.BlitToRenderTarget(postProcess.downsampledTextures);
		general.BlitToRenderTarget(postProcess.downsampledTextures, ReadBufferMode.ColorAttachment0, DrawBufferMode.ColorAttachment1);
		
		PostProcess();
		
		foreach (GlfwWindow window in windows) {
			window.MakeCurrent();
			postProcess.BlitToScreen(window.size);
			window.Swap();
		}
	}
	
	protected virtual void PostProcess() {}

	public void Run() {
		OnLoad();
		while (!ShouldClose()) Render();
		OnUnload();
		Dispose();
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
	private void Render() {
		WaitForRenderDelay();
		general.Begin();
		GL.Viewport(0, 0, general.framebufferSize.X, general.framebufferSize.Y);
		OnRender();
		End();
	}
	
	protected virtual void OnLoad() { }
	protected virtual void OnUnload() { }

	protected virtual void OnRender() {
		
	}
	
	private unsafe bool ShouldClose() => windows.Any(win => GLFW.WindowShouldClose(win.windowPtr));

	private void ReleaseUnmanagedResources() {
		foreach (GlfwWindow window in windows) window.Dispose();
		general.Dispose();
		postProcess.Dispose();
	}
	protected virtual void Dispose(bool disposing) {
		ReleaseUnmanagedResources();
		if (disposing) { }
	}
	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	~RendererBase() => Dispose(false);
}