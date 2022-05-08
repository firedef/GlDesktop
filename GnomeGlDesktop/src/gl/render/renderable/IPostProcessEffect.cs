using GnomeGlDesktop.gl.render.renderer;

namespace GnomeGlDesktop.gl.render.renderable; 

public interface IPostProcessEffect {
	public void PostProcess(Renderer renderer);
}