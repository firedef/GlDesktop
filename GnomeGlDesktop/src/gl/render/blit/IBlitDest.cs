using MathStuff.vectors;

namespace GnomeGlDesktop.gl.render.blit; 

public interface IBlitDest {
	public FrameBuffer GetBlitDestFramebuffer();
	public int2 GetBlitDestFramebufferSize();
	public void BeforeBlit(IBlitSrc src) {}
	public void AfterBlit(IBlitSrc src) {}
}