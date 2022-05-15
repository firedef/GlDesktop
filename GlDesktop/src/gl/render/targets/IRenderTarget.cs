using MathStuff.vectors;
using OpenTK.Mathematics;

namespace GlDesktop.gl.render.targets; 

public interface IRenderTarget {
	public FrameBuffer currentFramebuffer { get; }
	public int2 framebufferSize { get; set; }
}