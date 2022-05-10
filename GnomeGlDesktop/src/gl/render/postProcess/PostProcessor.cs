using GnomeGlDesktop.debug.log;
using GnomeGlDesktop.gl.render.attachments;
using GnomeGlDesktop.gl.render.blit;
using GnomeGlDesktop.gl.render.targets;
using GnomeGlDesktop.gl.shaders;
using GnomeGlDesktop.objects.mesh;
using MathStuff;
using MathStuff.vectors;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GnomeGlDesktop.gl.render.postProcess; 

public class PostProcessor : IDisposable, IBlittable {
	public readonly RenderTexturesTarget textures;
	public readonly RenderTexturesTarget downsampledTextures;

	private int2 resolution;
	private int2 downsampleResolution;
	
	private readonly Mesh<Vertex, byte> _screenMesh;
	
	public FrameBuffer GetBlitDestFramebuffer() => textures.currentFramebuffer;
	public int2 GetBlitDestFramebufferSize() => textures.framebufferSize;

	public PostProcessor(int2 resolution, int2 downsampleResolution) {
		this.resolution = resolution;
		this.downsampleResolution = downsampleResolution;
		
		Log.Message($"Create postprocess render target {resolution.x}x{resolution.y}");
		textures = new(resolution.x, resolution.y);

		Log.Message($"Create postprocess downsampled render target {downsampleResolution.x}x{downsampleResolution.y}");
		downsampledTextures = new(downsampleResolution.x, downsampleResolution.y);

		Vertex[] vertices = {
			new(new(-1,-1), float3.front, color.white, new(0,0), new(0,0)),
			new(new(-1, 1), float3.front, color.white, new(0,1), new(0,1)),
			new(new( 1, 1), float3.front, color.white, new(1,1), new(1,1)),
			new(new( 1,-1), float3.front, color.white, new(1,0), new(1,0)),
		};
		byte[] indices = {0, 1, 2, 0, 2, 3};
		
		_screenMesh = new(vertices, indices);
		_screenMesh.Buffer();
	}

	public void RecreateTextures(int2 targetResolution, int2 targetDownsampleResolution) {
		resolution = targetResolution;
		downsampleResolution = targetDownsampleResolution;
		
		Log.Message($"Recreate postprocess render target {resolution.x}x{resolution.y}");
		textures.RecreateRenderBuffers(targetResolution.x, targetResolution.y);
		
		Log.Message($"Recreate postprocess downsampled render target {downsampleResolution.x}x{downsampleResolution.y}");
		downsampledTextures.RecreateRenderBuffers(targetDownsampleResolution.x, targetDownsampleResolution.y);
	}
	public void Swap() => textures.Swap();
	public void AddAttachment(AttachmentType type, InternalFormat format, float quality = 1) => textures.AddAttachment(0, type, format, quality);

	public void UniformTextureAttachment(int id, uint location) => ((RenderTexture)textures.currentAttachments[id]).Uniform(location);
	public void UniformDownsampledTextureAttachment(int id, uint location) => ((RenderTexture)downsampledTextures.currentAttachments[id]).Uniform(location);
	
	public void PostProcess(ShaderProgram shader, DrawBufferMode mode = DrawBufferMode.ColorAttachment0) {
		GL.Viewport(0, 0, resolution.x, resolution.y);
		shader.Bind();
		
		textures.Swap();
		textures.currentFramebuffer.BindDraw();
		GL.DrawBuffer(mode);
		
		_screenMesh.Draw();
		
		GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
	}
	
	public void DrawToDownsampledTexture(ShaderProgram shader, DrawBufferMode mode = DrawBufferMode.ColorAttachment0) {
		GL.Viewport(0, 0, downsampleResolution.x, downsampleResolution.y);
		shader.Bind();
		
		downsampledTextures.Swap();
		downsampledTextures.currentFramebuffer.BindDraw();
		GL.DrawBuffer(mode);
		
		_screenMesh.Draw();
		
		GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
	}

	public void BlitToScreen(int2 screenSize) {
		textures.BlitToScreen(screenSize);
	}
	
	public void Blit(IBlitDest target, ReadBufferMode srcAttachment = ReadBufferMode.ColorAttachment0, DrawBufferMode dstAttachment = DrawBufferMode.ColorAttachment0) {
		textures.Blit(target, srcAttachment, dstAttachment);
	}

	public void Dispose() {
		textures.Dispose();
		_screenMesh.Dispose();
	}
}