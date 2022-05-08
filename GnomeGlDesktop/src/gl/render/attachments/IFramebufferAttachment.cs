using OpenTK.Graphics.OpenGL.Compatibility;

namespace GnomeGlDesktop.gl.render.attachments; 

public interface IFramebufferAttachment : IDisposable {
	public FramebufferAttachment attachmentId { get; init; }
	public InternalFormat format { get; init; }
	public void AttachToFrameBuffer(FrameBuffer frameBuffer, FramebufferAttachment attachment = FramebufferAttachment.ColorAttachment0);
}