using GnomeGlDesktop.debug.log;
using GnomeGlDesktop.gl.render.attachments;
using GnomeGlDesktop.gl.render.blit;
using GnomeGlDesktop.gl.render.postProcess;
using GnomeGlDesktop.gl.render.renderable;
using GnomeGlDesktop.gl.render.targets;
using GnomeGlDesktop.gl.shaders;
using GnomeGlDesktop.window;
using MathStuff.vectors;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GnomeGlDesktop.gl.render.renderer; 

public class Renderer : IDisposable {
	private readonly RenderTarget _general;
	private readonly PostProcessor _postProcess;

	//TODO: get max resolution from monitors
	private readonly RendererResolution _resolution = new(new(1920, 1080));
	private readonly RendererTimer _timer = new(60);
	
	private int _samples = 4;

	private readonly RendererRenderables _renderables = new();

	private readonly List<GlfwWindow> _windows = new();
	private readonly RendererBlitTargets[] _blitTargets = {new(), new(), new()};

	public float renderTimeDelta => _timer.renderTimeDelta;

	public Renderer() {
		GlContext.global.MakeCurrent();
		
		Log.Message($"Create general render target {_resolution.renderResolution.x}x{_resolution.renderResolution.y} with {_samples} samples");
		_general = new(_resolution.renderResolution.x, _resolution.renderResolution.y, _samples, InternalFormat.Rgb16f, true);
		
		// add default color attachment
		AddGeneralAttachment(AttachmentType.color, InternalFormat.Rgba16f);
		
		// add depth and stencil attachment
		AddGeneralAttachment(AttachmentType.depthStencil, InternalFormat.Depth24Stencil8);
		
		_postProcess = new(_resolution.postProcessResolution, _resolution.postProcessDownsampledResolution);
		
		// add default color attachment
		AddPostProcessAttachment(AttachmentType.color, InternalFormat.Rgb16f);
		
		// add depth and stencil attachment
		AddPostProcessAttachment(AttachmentType.depthStencil, InternalFormat.Depth24Stencil8);
		
		// add blur attachment
		AddPostProcessDownsampledAttachment(AttachmentType.color, InternalFormat.Rgb16f);
		
		// add emissive blur attachment
		AddPostProcessDownsampledAttachment(AttachmentType.color, InternalFormat.Rgb16f);
		
		AddBlitDestination(_postProcess.downsampledTextures, BlitDestPlace.afterRender);
		AddBlitDestination(_postProcess.downsampledTextures, BlitDestPlace.afterRender, DrawBufferMode.ColorAttachment1);
	}

	public void AddGeneralAttachment(AttachmentType type, InternalFormat format) => _general.AddAttachment(_samples, type, format);
	public void AddPostProcessAttachment(AttachmentType type, InternalFormat format) => _postProcess.AddAttachment(type, format);
	public void AddPostProcessDownsampledAttachment(AttachmentType type, InternalFormat format) => _postProcess.downsampledTextures.AddAttachment(0, type, format);

	public void AddWindow(GlfwWindow win) {
		AddBlitDestination(win, BlitDestPlace.final);
		_windows.Add(win);
	}

	public void AddBlitDestination(IBlitDest dest, BlitDestPlace place, DrawBufferMode attachment = DrawBufferMode.ColorAttachment0) {
		_blitTargets[(int)place].AddTarget(dest, attachment);
	}
	
	public void RemoveBlitDestination(IBlitDest dest, BlitDestPlace place, DrawBufferMode attachment = DrawBufferMode.ColorAttachment0) {
		_blitTargets[(int)place].RemoveTarget(dest, attachment);
	}

	protected void Blit(IBlitSrc src, BlitDestPlace place, ReadBufferMode attachment = ReadBufferMode.ColorAttachment0) {
		_blitTargets[(int)place].Blit(src, attachment);
	}

	public GlfwWindow GetWindow(int i) => _windows[i];

	public void RecreateRenderBuffers() {
		_general.RecreateRenderBuffers(_resolution.renderResolution.x, _resolution.renderResolution.y, _samples);
		_postProcess.RecreateTextures(_resolution.postProcessResolution, _resolution.postProcessDownsampledResolution);
	}

	private void End() {
		// blit general framebuffer (screen) to post process framebuffer (resolve MSAA)
		_general.Blit(_postProcess.textures);
		
		// blit post process framebuffer (resolved general framebuffer) to other blit targets (e.g. downsampled postprocess)
		Blit(_postProcess, BlitDestPlace.afterRender);
		
		// run post process
		_renderables.PostProcess(this);
		
		// blit post-processed image to other blit targets (e.g. ImGui ui)
		Blit(_postProcess, BlitDestPlace.afterPostProcess);
		
		// blit final image to other blit targets (e.g. windows)
		Blit(_postProcess, BlitDestPlace.final);
	}
	
	protected virtual void PostProcess() {}

	public void Run() {
		Log.Message($"Renderer OnLoad()");
		OnLoad();
		
		Log.Message($"Renderer loop");
		while (!ShouldClose()) Render();
		
		Log.Message($"Renderer OnUnload()");
		OnUnload();
		
		Log.Message($"Renderer dispose");
		Dispose();
	}
	
	private void Render() {
		_timer.Wait();
		
		GlContext.global.MakeCurrent();
		_general.Begin();
		GL.Viewport(0, 0, _general.framebufferSize.x, _general.framebufferSize.y);
		_renderables.Render(this);
		
		End();
	}
	
	protected virtual void OnLoad() { }
	protected virtual void OnUnload() { }
	
	private unsafe bool ShouldClose() => _windows.Any(win => GLFW.WindowShouldClose(win.windowPtr));

	public void UniformTexture(int id, uint loc) => _postProcess.UniformTextureAttachment(id, loc);
	public void UniformDownsampledTexture(int id, uint loc) => _postProcess.UniformDownsampledTextureAttachment(id, loc);

	public void BindPostProcessFramebuffer() => _postProcess.textures.Begin();
	public void PostProcess(ShaderProgram shader, DrawBufferMode attachment = DrawBufferMode.ColorAttachment0) => _postProcess.PostProcess(shader, attachment);
	public void ApplyToDownsampledTexture(ShaderProgram shader, DrawBufferMode attachment = DrawBufferMode.ColorAttachment0) => _postProcess.DrawToDownsampledTexture(shader, attachment);

	public void SetFramerate(int framerate) => _timer.frequency = framerate;
	public void SetSamples(int samples) => _samples = samples;
	public void SetResolution(int2 resolution) => _resolution.targetResolution = resolution;
	public void SetRenderingQuality(float quality) => _resolution.postprocessQuality = _resolution.renderQuality = quality;
	public void SetDownsampleQuality(float quality) => _resolution.postprocessDownsampledQuality = quality;

	public void AddRenderable(IRenderable v) => _renderables.AddRenderable(v);
	public void RemoveRenderable(IRenderable v) => _renderables.RemoveRenderable(v);
	public void AddPostFx(IPostProcessEffect v) => _renderables.AddPostProcess(v);
	public void RemovePostFx(IPostProcessEffect v) => _renderables.RemovePostProcess(v);
	
	private void ReleaseUnmanagedResources() {
		foreach (GlfwWindow window in _windows) window.Dispose();
		_general.Dispose();
		_postProcess.Dispose();
	}
	protected virtual void Dispose(bool disposing) {
		ReleaseUnmanagedResources();
		if (disposing) { }
	}
	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	~Renderer() => Dispose(false);
}