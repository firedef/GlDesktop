using GnomeGlDesktop.gl.render.attachments;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;

namespace GnomeGlDesktop.gl.render.targets; 

public class RenderTarget : IDisposable, IRenderTarget {
	public FrameBuffer frameBuffer;
	public List<IFramebufferAttachment> attachments = new();
	public Vector2i framebufferSize { get; set; }
	public bool useRenderBuffer;

	public uint colorAttachmentCount = 0;

	public FrameBuffer currentFramebuffer => frameBuffer;

	public RenderTarget() { }
	
	public RenderTarget(int width, int height, int samples = 0, InternalFormat format = InternalFormat.Rgba, bool useRenderBuffer = false) {
		this.useRenderBuffer = useRenderBuffer;
		
		BindContext();
		frameBuffer = FrameBuffer.Create();
		frameBuffer.Bind();
		
		framebufferSize = new(width, height);
		// for (int i = 0; i < attachmentCount; i++) AddAttachment(samples, format);
	}
	
	public void BindContext() => GlContext.global.MakeCurrent();

	public virtual void Begin() {
		BindContext();
		frameBuffer.BindDraw();
	}

	public virtual void End() {}

	public void RecreateRenderBuffers(int width, int height, int samples = 0) {
		int attachmentCount = attachments.Count;
		framebufferSize = new(width, height);
		for (int i = 0; i < attachmentCount; i++) {
			FramebufferAttachment id = attachments[i].attachmentId;
			InternalFormat format = attachments[i].format;
			attachments[i].Dispose();
			
			attachments[i] = useRenderBuffer 
				? RenderBuffer.Create(frameBuffer, id, framebufferSize, samples, format) 
				: RenderTexture.Create(frameBuffer, id, framebufferSize, samples, (SizedInternalFormat) format);
		}
		
	}

	public void AddAttachment(int samples = 0, AttachmentType type = AttachmentType.color, InternalFormat format = InternalFormat.Rgba) {
		FramebufferAttachment id = type switch {
			AttachmentType.color => FramebufferAttachment.ColorAttachment0 + colorAttachmentCount,
			AttachmentType.depth => FramebufferAttachment.DepthAttachment,
			AttachmentType.stencil => FramebufferAttachment.StencilAttachment,
			AttachmentType.depthStencil => FramebufferAttachment.DepthStencilAttachment,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
		if (type == AttachmentType.color) colorAttachmentCount++;
		
		IFramebufferAttachment attachment = useRenderBuffer
			? RenderBuffer.Create(frameBuffer, id, framebufferSize, samples, format) 
			: RenderTexture.Create(frameBuffer, id, framebufferSize, samples, (SizedInternalFormat) format);
		
		attachments.Add(attachment);
	}
	
	public void BlitToScreen(Vector2i screenSize, BlitFramebufferFilter filter = BlitFramebufferFilter.Linear) {
		frameBuffer.BlitTo(FrameBuffer.screen, framebufferSize.X, framebufferSize.Y, screenSize.X, screenSize.Y, ClearBufferMask.ColorBufferBit, filter);
	}
	
	public void BlitToRenderTarget(IRenderTarget renderTarget, BlitFramebufferFilter filter = BlitFramebufferFilter.Linear) {
		frameBuffer.BlitTo(renderTarget.currentFramebuffer, framebufferSize.X, framebufferSize.Y, renderTarget.framebufferSize.X, renderTarget.framebufferSize.Y, ClearBufferMask.ColorBufferBit, filter);
	}
	
	public void BlitToRenderTarget(IRenderTarget renderTarget, ReadBufferMode read, DrawBufferMode draw, BlitFramebufferFilter filter = BlitFramebufferFilter.Linear) {
		GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, currentFramebuffer.handle);
		GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, renderTarget.currentFramebuffer.handle);
		GL.ReadBuffer(read);
		GL.DrawBuffer(draw);
		frameBuffer.BlitTo(renderTarget.currentFramebuffer, framebufferSize.X, framebufferSize.Y, renderTarget.framebufferSize.X, renderTarget.framebufferSize.Y, ClearBufferMask.ColorBufferBit, filter);
		GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
		GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
	}

	public void Dispose() {
		foreach (IFramebufferAttachment attachment in attachments) attachment.Dispose();
		frameBuffer.Dispose();
	}
}