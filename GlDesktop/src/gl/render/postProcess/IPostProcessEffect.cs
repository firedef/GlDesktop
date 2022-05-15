using GlDesktop.gl.render.renderer;

namespace GlDesktop.gl.render.postProcess; 

public interface IPostProcessEffect {
	public void PostProcess(Renderer renderer);
}