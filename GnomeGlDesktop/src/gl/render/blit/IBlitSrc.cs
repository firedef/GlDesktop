using GnomeGlDesktop.gl.render.targets;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;

namespace GnomeGlDesktop.gl.render.blit; 

public interface IBlitSrc {
	public void Blit(IBlitDest target, ReadBufferMode srcAttachment = ReadBufferMode.ColorAttachment0, DrawBufferMode dstAttachment = DrawBufferMode.ColorAttachment0);
	public void BlitToScreen(Vector2i screenSize);
}