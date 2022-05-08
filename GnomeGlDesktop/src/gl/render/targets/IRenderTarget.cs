using OpenTK.Mathematics;

namespace GnomeGlDesktop.gl.render.targets; 

public interface IRenderTarget {
	public FrameBuffer currentFramebuffer { get; }
	public Vector2i framebufferSize { get; set; }
}