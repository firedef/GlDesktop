using GlDesktop.debug.log;
using GlDesktop.gl.render.attachments;
using GlDesktop.gl.render.blit;
using MathStuff.vectors;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GlDesktop.gl.render.targets; 

public class RenderTarget : IDisposable, IRenderTarget, IBlittable {
	public FrameBuffer frameBuffer;
	public List<IFramebufferAttachment> attachments = new();
	public int2 framebufferSize { get; set; }
	public bool useRenderBuffer;

	public uint colorAttachmentCount = 0;

	public FrameBuffer currentFramebuffer => frameBuffer;

	public FrameBuffer GetBlitDestFramebuffer() => frameBuffer;
	public int2 GetBlitDestFramebufferSize() => framebufferSize;

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
		Log.Message($"Recreate render target {width}x{height} with {samples} samples");
		
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
		
		Log.Note($"Add {id} {format} to render target {frameBuffer.handle.Handle} with {samples} samples");
	}
	
	public void BlitToScreen(int2 screenSize) {
		frameBuffer.BlitTo(FrameBuffer.screen, framebufferSize.x, framebufferSize.y, screenSize.x, screenSize.y, ClearBufferMask.ColorBufferBit);
	}
	
	public void Blit(IBlitDest renderTarget, ReadBufferMode srcAttachment = ReadBufferMode.ColorAttachment0, DrawBufferMode dstAttachment = DrawBufferMode.ColorAttachment0) {
		renderTarget.BeforeBlit(this);
		
		GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, currentFramebuffer.handle);
		GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, renderTarget.GetBlitDestFramebuffer().handle);
		GL.ReadBuffer(srcAttachment);
		GL.DrawBuffer(dstAttachment);
		frameBuffer.BlitTo(renderTarget.GetBlitDestFramebuffer(), framebufferSize.x, framebufferSize.y, renderTarget.GetBlitDestFramebufferSize().x, renderTarget.GetBlitDestFramebufferSize().y, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
		GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
		GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
		
		renderTarget.AfterBlit(this);
	}

	public void Dispose() {
		foreach (IFramebufferAttachment attachment in attachments) attachment.Dispose();
		frameBuffer.Dispose();
	}
}