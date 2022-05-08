using OpenTK.Mathematics;

namespace GnomeGlDesktop.gl.render.blit; 

public interface IBlitDest {
	public FrameBuffer GetBlitDestFramebuffer();
	public Vector2i GetBlitDestFramebufferSize();
	public void BeforeBlit(IBlitSrc src) {}
	public void AfterBlit(IBlitSrc src) {}
}