using GnomeGlDesktop.gl.render.renderer;

namespace GnomeGlDesktop.gl.render.postProcess; 

public interface IPostProcessEffect {
	public void PostProcess(Renderer renderer);
}