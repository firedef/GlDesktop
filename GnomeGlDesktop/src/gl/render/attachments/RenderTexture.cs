using MathStuff.vectors;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GnomeGlDesktop.gl.render.attachments;

public readonly struct RenderTexture : IFramebufferAttachment {
	public readonly TextureHandle handle;
	public readonly int samples;
	public FramebufferAttachment attachmentId { get; init; }
	public InternalFormat format { get; init; }
	
	public RenderTexture(TextureHandle handle, int samples, FramebufferAttachment attachment, InternalFormat format) {
		this.handle = handle;
		this.samples = samples;
		attachmentId = attachment;
		this.format = format;
	}

	public static RenderTexture Create(FrameBuffer frameBuffer, FramebufferAttachment attachment, int2 size, int samples = 0, SizedInternalFormat format = SizedInternalFormat.Rgba16f, int levels = 1) {
		bool ms = samples > 0;
		RenderTexture tex = new(GL.CreateTexture(ms ? TextureTarget.Texture2dMultisample : TextureTarget.Texture2d), samples, attachment, (InternalFormat) format);
		
		if (ms) GL.TextureStorage2DMultisample(tex.handle, samples, format, size.x, size.y, 1);
		else GL.TextureStorage2D(tex.handle, levels, format, size.x, size.y);
		
		tex.AttachToFrameBuffer(frameBuffer, attachment);
		return tex;
	}

	public void Dispose() {
		if (handle != TextureHandle.Zero) GL.DeleteTexture(handle);
	}

	public void Bind(TextureTarget target) => GL.BindTexture(target, handle);

	public void Uniform(uint binding = 0) => GL.BindTextureUnit(binding, handle);

	public void AttachToFrameBuffer(FrameBuffer frameBuffer, FramebufferAttachment attachment = FramebufferAttachment.ColorAttachment0) =>
		AttachToFrameBuffer(frameBuffer, 0, attachment);
	
	public void AttachToFrameBuffer(FrameBuffer frameBuffer, int level, FramebufferAttachment attachment = FramebufferAttachment.ColorAttachment0) =>
		GL.NamedFramebufferTexture(frameBuffer.handle, attachment, handle, level);
}