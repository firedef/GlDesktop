using GnomeGlDesktop.gl.render.postProcess;
using GnomeGlDesktop.gl.render.renderable;

namespace GnomeGlDesktop.gl.render.renderer; 

public class RendererRenderables {
	public readonly List<IRenderable> renderables = new();
	public readonly List<IPostProcessEffect> postProcessEffects = new();

	public void Render(Renderer renderer) {
		foreach (IRenderable renderable in renderables) 
			renderable.Render(renderer);
	}
	
	public void PostProcess(Renderer renderer) {
		foreach (IPostProcessEffect postFx in postProcessEffects) 
			postFx.PostProcess(renderer);
	}

	public void AddRenderable(IRenderable renderable) => renderables.Add(renderable);
	public void RemoveRenderable(IRenderable renderable) => renderables.Remove(renderable);
	
	public void AddPostProcess(IPostProcessEffect postFx) => postProcessEffects.Add(postFx);
	public void RemovePostProcess(IPostProcessEffect postFx) => postProcessEffects.Remove(postFx);
}