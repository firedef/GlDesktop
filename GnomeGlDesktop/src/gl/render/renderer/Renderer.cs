using GnomeGlDesktop.debug.log;
using GnomeGlDesktop.gl.render.attachments;
using GnomeGlDesktop.gl.render.blit;
using GnomeGlDesktop.gl.render.postProcess;
using GnomeGlDesktop.gl.render.targets;
using GnomeGlDesktop.gl.shaders;
using GnomeGlDesktop.window;
using OpenTK.Graphics.OpenGL.Compatibility;
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

	private readonly List<GlfwWindow> _windows = new();
	private readonly RendererBlitTargets _generalBlitTargets = new();
	private readonly RendererBlitTargets _postprocessBlitTargets = new();
	private readonly RendererBlitTargets _finalBlitTargets = new();

	public Renderer() {
		GlContext.global.MakeCurrent();
		
		Log.Message($"Create general render target {_resolution.renderResolution.X}x{_resolution.renderResolution.Y} with {_samples} samples");
		_general = new(_resolution.renderResolution.X, _resolution.renderResolution.Y, _samples, InternalFormat.Rgba16f, true);
		
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
		RendererBlitTargets trgt = place switch {
			BlitDestPlace.afterRender => _generalBlitTargets,
			BlitDestPlace.afterPostProcess => _postprocessBlitTargets,
			BlitDestPlace.final => _finalBlitTargets,
			_ => throw new ArgumentOutOfRangeException(nameof(place), place, null)
		};
		trgt.AddTarget(dest, attachment);
	}
	
	public void RemoveBlitDestination(IBlitDest dest, BlitDestPlace place, DrawBufferMode attachment = DrawBufferMode.ColorAttachment0) {
		RendererBlitTargets trgt = place switch {
			BlitDestPlace.afterRender => _generalBlitTargets,
			BlitDestPlace.afterPostProcess => _postprocessBlitTargets,
			BlitDestPlace.final => _finalBlitTargets,
			_ => throw new ArgumentOutOfRangeException(nameof(place), place, null)
		};
		trgt.RemoveTarget(dest, attachment);
	}

	protected void Blit(IBlitSrc src, BlitDestPlace place, ReadBufferMode attachment = ReadBufferMode.ColorAttachment0) {
		RendererBlitTargets trgt = place switch {
			BlitDestPlace.afterRender => _generalBlitTargets,
			BlitDestPlace.afterPostProcess => _postprocessBlitTargets,
			BlitDestPlace.final => _finalBlitTargets,
			_ => throw new ArgumentOutOfRangeException(nameof(place), place, null)
		};
		trgt.Blit(src, attachment);
	}

	public GlfwWindow GetWindow(int i) => _windows[i];

	public void RecreateRenderBuffers() {
		_general.RecreateRenderBuffers(_resolution.renderResolution.X, _resolution.renderResolution.Y, _samples);
		_postProcess.RecreateTextures(_resolution.postProcessResolution, _resolution.postProcessDownsampledResolution);
	}

	private void End() {
		// blit general framebuffer (screen) to post process framebuffer (resolve MSAA)
		_general.Blit(_postProcess.textures);
		
		// blit post process framebuffer (resolved general framebuffer) to other blit targets (e.g. downsampled postprocess)
		Blit(_postProcess, BlitDestPlace.afterRender);
		
		// run post process
		PostProcess();
		
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
		_general.Begin();
		GL.Viewport(0, 0, _general.framebufferSize.X, _general.framebufferSize.Y);
		OnRender();
		End();
	}
	
	protected virtual void OnLoad() { }
	protected virtual void OnUnload() { }
	protected virtual void OnRender() { }
	
	private unsafe bool ShouldClose() => _windows.Any(win => GLFW.WindowShouldClose(win.windowPtr));

	public void UniformTexture(int id, uint loc) => _postProcess.UniformTextureAttachment(id, loc);
	public void UniformDownsampledTexture(int id, uint loc) => _postProcess.UniformDownsampledTextureAttachment(id, loc);

	public void PostProcess(ShaderProgram shader, DrawBufferMode attachment = DrawBufferMode.ColorAttachment0) => _postProcess.PostProcess(shader, attachment);
	public void ApplyToDownsampledTexture(ShaderProgram shader, DrawBufferMode attachment = DrawBufferMode.ColorAttachment0) => _postProcess.DrawToDownsampledTexture(shader, attachment);

	public void SetFramerate(int framerate) => _timer.frequency = framerate;
	public void SetSamples(int samples) => _samples = samples;
	public void SetResolution(Vector2i resolution) => _resolution.targetResolution = resolution;
	public void SetRenderingQuality(float quality) => _resolution.postprocessQuality = _resolution.renderQuality = quality;
	public void SetDownsampleQuality(float quality) => _resolution.postprocessDownsampledQuality = quality;
	
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