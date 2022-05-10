using MathStuff.vectors;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GnomeGlDesktop.gl.render.attachments; 

public readonly struct RenderBuffer : IDisposable, IFramebufferAttachment {
	public readonly RenderbufferHandle handle;
	public FramebufferAttachment attachmentId { get; init; }
	public InternalFormat format { get; init; }

	public RenderBuffer(RenderbufferHandle handle, FramebufferAttachment attachmentId, InternalFormat format) {
		this.handle = handle;
		this.attachmentId = attachmentId;
		this.format = format;
	}

	public static RenderBuffer Create(FramebufferAttachment attachmentId, InternalFormat format) => new(GL.CreateRenderbuffer(), attachmentId, format);

	public static RenderBuffer Create(FrameBuffer frameBuffer, FramebufferAttachment attachment, int2 size, int samples = 0, InternalFormat format = InternalFormat.Rgba) {
		RenderBuffer buffer = Create(attachment, format);
		buffer.SetStorage(size.x, size.y, samples, format);
		buffer.AttachToFrameBuffer(frameBuffer, attachment);
		return buffer;
	}
	
	public void Bind() => GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, handle);

	public void AttachToFrameBuffer(FrameBuffer frameBuffer, FramebufferAttachment attachment = FramebufferAttachment.ColorAttachment0) {
		GL.NamedFramebufferRenderbuffer(frameBuffer.handle, attachment, RenderbufferTarget.Renderbuffer, handle);
	}

	public void SetStorage(int width, int height, int samples = 0, InternalFormat format = InternalFormat.Rgba) =>
		GL.NamedRenderbufferStorageMultisample(handle, samples, format, width, height);

	public void Dispose() {
		if (handle != RenderbufferHandle.Zero) GL.DeleteRenderbuffer(handle);
	}
}