using GnomeGlDesktop.gl.render.attachments;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;

namespace GnomeGlDesktop.gl.render.targets; 

public class RenderTexturesTarget : IRenderTarget, IDisposable {
	private const int _textureCount = 2;
	
	public readonly FrameBuffer[] frameBuffers = new FrameBuffer[_textureCount];
	public readonly List<IFramebufferAttachment>[] framebufferAttachments = new List<IFramebufferAttachment>[_textureCount];
	public Vector2i framebufferSize { get; set; }
	public int framebufferIndex = 0;
	public uint colorAttachmentCount = 0;

	public FrameBuffer currentFramebuffer => frameBuffers[framebufferIndex];
	public FrameBuffer nextFramebuffer => frameBuffers[(framebufferIndex + 1) % _textureCount];
	
	public List<IFramebufferAttachment> currentAttachments => framebufferAttachments[framebufferIndex];
	public List<IFramebufferAttachment> nextAttachments => framebufferAttachments[(framebufferIndex + 1) % _textureCount];

	public RenderTexture nextRenderTexture => (RenderTexture)nextAttachments[0];
	
	public RenderTexturesTarget(int width, int height, int samples = 0) {
		BindContext();
		framebufferSize = new(width, height);

		for (int i = 0; i < _textureCount; i++) {
			frameBuffers[i] = FrameBuffer.Create();
			frameBuffers[i].Bind();
			framebufferAttachments[i] = new();
		}
	}
	
	public void BindContext() => GlContext.global.MakeCurrent();
	
	public virtual void Begin() {
		BindContext();
		currentFramebuffer.BindDraw();
	}
	
	public virtual void End() {}
	
	public void RecreateRenderBuffers(int width, int height, int samples = 0) {
		int attachmentCount = framebufferAttachments[0].Count;
		for (int t = 0; t < _textureCount; t++) {
			for (int i = 0; i < attachmentCount; i++) {
				FramebufferAttachment id = framebufferAttachments[t][i].attachmentId;
				InternalFormat format = framebufferAttachments[t][i].format;
				framebufferAttachments[t][i].Dispose();
				framebufferAttachments[t][i] = RenderTexture.Create(
					currentFramebuffer, 
					id, 
					new(width, height), 
					samples, 
					(SizedInternalFormat) format);
			}
		}
		
		framebufferSize = new(width, height);
	}
	
	public void AddAttachment(int samples = 0, AttachmentType type = AttachmentType.color, InternalFormat format = InternalFormat.Rgba, float quality = 1) {
		FramebufferAttachment id = type switch {
			AttachmentType.color => FramebufferAttachment.ColorAttachment0 + colorAttachmentCount,
			AttachmentType.depth => FramebufferAttachment.DepthAttachment,
			AttachmentType.stencil => FramebufferAttachment.StencilAttachment,
			AttachmentType.depthStencil => FramebufferAttachment.DepthStencilAttachment,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
		if (type == AttachmentType.color) colorAttachmentCount++;
		
		for (int t = 0; t < _textureCount; t++) {
			IFramebufferAttachment attachment = RenderTexture.Create(frameBuffers[t], id, (Vector2i) ((Vector2)framebufferSize * quality), samples, (SizedInternalFormat) format);
			
			framebufferAttachments[t].Add(attachment);
		}
	}
	
	public void BlitToScreen(Vector2i screenSize) {
		currentFramebuffer.BlitTo(FrameBuffer.screen, framebufferSize.X, framebufferSize.Y, screenSize.X, screenSize.Y, ClearBufferMask.ColorBufferBit);
	}
	
	public void BlitToRenderTarget(IRenderTarget renderTarget) {
		currentFramebuffer.BlitTo(renderTarget.currentFramebuffer, framebufferSize.X, framebufferSize.Y, renderTarget.framebufferSize.X, renderTarget.framebufferSize.Y, ClearBufferMask.ColorBufferBit);
	}
	
	public void BlitToRenderTarget(IRenderTarget renderTarget, ReadBufferMode srcAttachment, DrawBufferMode dstAttachment) {
		GL.ReadBuffer(srcAttachment);
		GL.DrawBuffer(dstAttachment);
		currentFramebuffer.BlitTo(renderTarget.currentFramebuffer, framebufferSize.X, framebufferSize.Y, renderTarget.framebufferSize.X, renderTarget.framebufferSize.Y, ClearBufferMask.ColorBufferBit);
		GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
		GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
	}

	public void Swap() => framebufferIndex = (framebufferIndex + 1) % _textureCount;

	public void Dispose() {
		for (int t = 0; t < _textureCount; t++) {
			foreach (IFramebufferAttachment attachment in framebufferAttachments[t]) attachment.Dispose();
			frameBuffers[t].Dispose();
		}
	}
}