using GnomeGlDesktop.utils;
using OpenTK.Mathematics;

namespace GnomeGlDesktop.gl.render.renderer; 

public class RendererResolution {
	public Vector2i targetResolution;
	
	public float renderQuality = 1f;
	public float postprocessQuality = 1f;
	public float postprocessDownsampledQuality = .2f;

	public Vector2i renderResolution => targetResolution.Mul(renderQuality);
	public Vector2i postProcessResolution => targetResolution.Mul(postprocessQuality);
	public Vector2i postProcessDownsampledResolution => targetResolution.Mul(postprocessDownsampledQuality);
	public Vector2i normalResolution => targetResolution;

	public RendererResolution(Vector2i targetResolution) {
		this.targetResolution = targetResolution;
	}
}