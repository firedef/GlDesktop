using GlDesktop.utils;
using MathStuff.vectors;
using OpenTK.Mathematics;

namespace GlDesktop.gl.render.renderer; 

public class RendererResolution {
	public int2 targetResolution;
	
	public float renderQuality = 1f;
	public float postprocessQuality = 1f;
	public float postprocessDownsampledQuality = .2f;

	public int2 renderResolution => targetResolution.Mul(renderQuality);
	public int2 postProcessResolution => targetResolution.Mul(postprocessQuality);
	public int2 postProcessDownsampledResolution => targetResolution.Mul(postprocessDownsampledQuality);
	public int2 normalResolution => targetResolution;

	public RendererResolution(int2 targetResolution) {
		this.targetResolution = targetResolution;
	}
}