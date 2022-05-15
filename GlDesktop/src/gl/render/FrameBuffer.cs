using MathStuff;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GlDesktop.gl.render; 

public readonly struct FrameBuffer : IDisposable {
	public static readonly FrameBuffer screen = new(FramebufferHandle.Zero);
	
	public readonly FramebufferHandle handle;

	public FrameBuffer(FramebufferHandle handle) => this.handle = handle;

	public static FrameBuffer Create() => new(GL.CreateFramebuffer());

	public void Bind(FramebufferTarget target = FramebufferTarget.Framebuffer) => GL.BindFramebuffer(target, handle);
	
	public void BindDraw() => GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, handle);
	public void BindRead() => GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, handle);

	public void BlitTo(FrameBuffer draw, int wSrc, int hSrc, int wDest, int hDest, ClearBufferMask mask, BlitFramebufferFilter filter = BlitFramebufferFilter.Linear) {
		GL.BlitNamedFramebuffer(
			// src
			handle,
			
			// dest
			draw.handle,
			
			// src rect
			0, 
			0, 
			wSrc,
			hSrc,
			
			// dest rect
			0,
			0,
			wDest,
			hDest,
			
			mask,
			filter);
	}
	
	public void BlitTo(int wSrc, int hSrc, int wDest, int hDest, ClearBufferMask mask, BlitFramebufferFilter filter = BlitFramebufferFilter.Linear) {
		GL.BlitFramebuffer(
			// src rect
			0, 
			0, 
			wSrc,
			hSrc,
			
			// dest rect
			0,
			0,
			wDest,
			hDest,
			
			mask,
			filter);
	}
	
	public void BlitTo(rect readRect, rect drawRect, ClearBufferMask mask, BlitFramebufferFilter filter = BlitFramebufferFilter.Linear) {
		GL.BlitFramebuffer(
			// src rect
			(int)readRect.left, 
			(int)readRect.bottom, 
			(int)readRect.right,
			(int)readRect.top,
			
			// dest rect
			(int)drawRect.left, 
			(int)drawRect.bottom, 
			(int)drawRect.right, 
			(int)drawRect.top,
			
			mask,
			filter);
	}
	
	public void BlitTo(FrameBuffer draw, rect readRect, rect drawRect, ClearBufferMask mask, BlitFramebufferFilter filter = BlitFramebufferFilter.Linear) {
		GL.BlitNamedFramebuffer(
			// src
			handle,
			
			// dest
			draw.handle,
			
			// src rect
			(int)readRect.left, 
			(int)readRect.bottom, 
			(int)readRect.right,
			(int)readRect.top,
			
			// dest rect
			(int)drawRect.left, 
			(int)drawRect.bottom, 
			(int)drawRect.right, 
			(int)drawRect.top,
			
			mask,
			filter);
	}

	public void Dispose() => GL.DeleteFramebuffer(handle);
}