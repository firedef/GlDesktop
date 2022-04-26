using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GnomeGlDesktop.gl; 

public struct IBO {
	public static readonly IBO empty = new();

	public static IBO boundBuffer = empty;
	public const BufferTargetARB target = BufferTargetARB.ElementArrayBuffer;
	
	public uint handle;
	public bool isGenerated => handle != 0;
	
	public IBO() => handle = 0;
	public IBO(uint handle) => this.handle = handle;
	public IBO(int handle) => this.handle = (uint)handle;

	public static implicit operator IBO(int v) => new(v);
	public static implicit operator IBO(uint v) => new(v);
	public static implicit operator int(IBO v) => (int) v.handle;
	public static implicit operator uint(IBO v) => v.handle;
	
	public static implicit operator IBO(BufferHandle v) => v.Handle;
	public static implicit operator BufferHandle(IBO v) => new(v);

	public void Bind() {
		//if (boundBuffer.handle == handle) return;
		boundBuffer = handle;
		GL.BindBuffer(target, this);
	}
	
	public unsafe void Alloc(int size, BufferUsageARB usage = BufferUsageARB.DynamicDraw) => BufferData(size, null, usage);
	
	public unsafe void BufferData(int size, void* ptr, BufferUsageARB usage = BufferUsageARB.DynamicDraw) {
		if (OpenGl.supportNamedBuffers) {
			GL.NamedBufferData((BufferHandle)(int)handle, size, ptr, (VertexBufferObjectUsage) usage);
			return;
		}
		Bind();
		GL.BufferData(target, size, ptr, usage);
	}
	
	public unsafe void BufferSubData(int offset, int size, void* ptr) {
		if (OpenGl.supportNamedBuffers) {
			GL.NamedBufferSubData((BufferHandle)(int)handle, (IntPtr)offset, size, ptr);
			return;
		}
		
		Bind();
		GL.BufferSubData(target, (IntPtr)offset, size, ptr);
	}

	public static IBO Generate() {
		IBO buffer = GL.CreateBuffer();
		// IBO buffer = GL.GenBuffer();
		return buffer;
	}

	public static void Unbind() {
		if (boundBuffer.handle == 0) return;
		boundBuffer = empty;
		GL.BindBuffer(target, BufferHandle.Zero);
	}

	public void Dispose() {
		if (!isGenerated) return;
		if (boundBuffer.handle == handle) Unbind();
		GL.DeleteBuffer(this);
		handle = 0;
	}
}