using GnomeGlDesktop.gl.render.blit;
using OpenTK.Graphics.OpenGL;

namespace GnomeGlDesktop.gl.render.renderer; 

public class RendererBlitTargets {
	public readonly List<BlitTarget> targets = new();

	public void Blit(IBlitSrc blittable, ReadBufferMode srcAttachment = ReadBufferMode.ColorAttachment0) {
		foreach (BlitTarget trgt in targets) 
			blittable.Blit(trgt.dest, trgt.readMode ?? srcAttachment, trgt.drawMode);
	}

	public void AddTarget(IBlitDest target, DrawBufferMode attachment = DrawBufferMode.ColorAttachment0, ReadBufferMode? readAttachment = null) => targets.Add(new(target, attachment, readAttachment));
	public void RemoveTarget(IBlitDest target, DrawBufferMode attachment = DrawBufferMode.ColorAttachment0, ReadBufferMode? readAttachment = null) => targets.Remove(new(target, attachment, readAttachment));

	public record BlitTarget(IBlitDest dest, DrawBufferMode drawMode, ReadBufferMode? readMode) {
		public readonly IBlitDest dest = dest;
		public readonly DrawBufferMode drawMode = drawMode;
		public readonly ReadBufferMode? readMode = readMode;
	}
}