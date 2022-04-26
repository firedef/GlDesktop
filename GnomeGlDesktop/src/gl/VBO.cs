using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL.Compatibility;

namespace GnomeGlDesktop.gl; 

public struct VBO {
	public static readonly VBO empty = new();

	public static VBO boundBuffer = empty;
	public const BufferTargetARB target = BufferTargetARB.ArrayBuffer;
	
	public uint handle;
	public bool isGenerated => handle != 0;
	
	public VBO() => handle = 0;
	public VBO(uint handle) => this.handle = handle;
	public VBO(int handle) => this.handle = (uint)handle;

	public static implicit operator VBO(int v) => new(v);
	public static implicit operator VBO(uint v) => new(v);
	public static implicit operator int(VBO v) => (int) v.handle;
	public static implicit operator uint(VBO v) => v.handle;
	
	public static implicit operator VBO(BufferHandle v) => v.Handle;
	public static implicit operator BufferHandle(VBO v) => new(v);

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

	public static VBO Generate() {
		VBO buffer = GL.CreateBuffer();
		// VBO buffer = GL.GenBuffer();
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