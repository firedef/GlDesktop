using GnomeGlDesktop.gl.render.blit;
using OpenTK.Graphics.OpenGL.Compatibility;

namespace GnomeGlDesktop.gl.render.renderer; 

public class RendererBlitTargets {
	public readonly List<(IBlitDest, DrawBufferMode)> targets = new();

	public void Blit(IBlitSrc blittable, ReadBufferMode srcAttachment = ReadBufferMode.ColorAttachment0) {
		foreach ((IBlitDest blitDest, DrawBufferMode mode) in targets) 
			blittable.Blit(blitDest, srcAttachment, mode);
	}

	public void AddTarget(IBlitDest target, DrawBufferMode attachment = DrawBufferMode.ColorAttachment0) => targets.Add((target, attachment));
	
	public void RemoveTarget(IBlitDest target, DrawBufferMode attachment = DrawBufferMode.ColorAttachment0) => targets.Remove((target, attachment));
}