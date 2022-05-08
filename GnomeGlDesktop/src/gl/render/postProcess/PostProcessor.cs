using GnomeGlDesktop.gl.render.attachments;
using GnomeGlDesktop.gl.render.targets;
using GnomeGlDesktop.gl.shaders;
using GnomeGlDesktop.objects.mesh;
using MathStuff;
using MathStuff.vectors;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;

namespace GnomeGlDesktop.gl.render.postProcess; 

public class PostProcessor : IDisposable {
	public readonly RenderTexturesTarget textures;
	public readonly RenderTexturesTarget downsampledTextures;
	private const int _downsample = 2;
	
	private readonly Mesh<Vertex, byte> _screenMesh;

	public PostProcessor(Vector2i targetResolution) {
		//targetResolution = new(800, 800);
		textures = new(targetResolution.X, targetResolution.Y);

		downsampledTextures = new(targetResolution.X>>_downsample, targetResolution.Y>>_downsample);
		downsampledTextures.AddAttachment(0, AttachmentType.color, InternalFormat.Rgb16f); // blur
		downsampledTextures.AddAttachment(0, AttachmentType.color, InternalFormat.Rgb16f); // emissive blur

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
	
	public void RecreateTextures(Vector2i targetResolution) => textures.RecreateRenderBuffers(targetResolution.X, targetResolution.Y);
	public void Swap() => textures.Swap();
	public void AddAttachment(AttachmentType type, InternalFormat format, float quality = 1) => textures.AddAttachment(0, type, format, quality);

	public void UniformTextureAttachment(int id, uint location) => ((RenderTexture)textures.currentAttachments[id]).Uniform(location);
	public void UniformDownsampledTextureAttachment(int id, uint location) => ((RenderTexture)downsampledTextures.currentAttachments[id]).Uniform(location);
	
	public void PostProcess(ShaderProgram shader, DrawBufferMode mode = DrawBufferMode.ColorAttachment0) {
		GL.Viewport(0, 0, 1920, 1080);
		shader.Bind();
		
		textures.Swap();
		textures.currentFramebuffer.BindDraw();
		GL.DrawBuffer(mode);
		
		_screenMesh.Draw();
		
		GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
	}
	
	public void DrawToDownsampledTexture(ShaderProgram shader, DrawBufferMode mode = DrawBufferMode.ColorAttachment0) {
		GL.Viewport(0, 0, 1920>>_downsample, 1080>>_downsample);
		shader.Bind();
		
		downsampledTextures.Swap();
		downsampledTextures.currentFramebuffer.BindDraw();
		GL.DrawBuffer(mode);
		
		_screenMesh.Draw();
		
		GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
	}

	public void BlitToScreen(Vector2i screenSize) {
		//Swap();
		
		//GL.NamedFramebufferReadBuffer(FramebufferHandle.Zero, );
		textures.BlitToScreen(screenSize);
		//GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
		
		// GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, downsampledTextures.currentFramebuffer.handle);
		// GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FramebufferHandle.Zero);
		// GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
		// GL.BlitFramebuffer(0, 0, 1920>>_downsample, 1080>>_downsample, 0, 0, screenSize.X, screenSize.Y, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
		// GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
	}
	
	public void BlitToRenderTarget(IRenderTarget target, ReadBufferMode srcAttachment = ReadBufferMode.ColorAttachment0, DrawBufferMode dstAttachment = DrawBufferMode.ColorAttachment0) {
		//Swap();
		textures.BlitToRenderTarget(target, srcAttachment, dstAttachment);
	}

	public void Dispose() {
		textures.Dispose();
		_screenMesh.Dispose();
	}
}