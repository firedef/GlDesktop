using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GlDesktop.gl.buffers; 

public struct IBO {
	public static readonly IBO empty = new();

#if SINGLE_GL_CONTEXT_OR_SHARED
	private static IBO _boundBuffer = empty;
#endif
	
	private const BufferTargetARB _target = BufferTargetARB.ElementArrayBuffer;
	
	public uint handle { get; private set; }
	public int length { get; private set; } = 0;
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
	#if SINGLE_GL_CONTEXT_OR_SHARED
		if (_boundBuffer.handle == handle) return;
		_boundBuffer = handle;
	#endif
		GL.BindBuffer(_target, this);
	}
	
	public unsafe void Alloc(int size, BufferUsageARB usage = BufferUsageARB.DynamicDraw) => BufferData(size, null, usage);
	
	public unsafe void Buffer(int size, void* ptr, BufferUsageARB usage = BufferUsageARB.DynamicDraw) {
		if (size == length) BufferSubData(0, size, ptr);
		else BufferData(size, ptr, usage);
	}
	
	public unsafe void Buffer<T>(T[] arr, BufferUsageARB usage = BufferUsageARB.DynamicDraw) where T : unmanaged {
		fixed(T* ptr = arr) Buffer(arr.Length * sizeof(T), ptr, usage);
	}

	public unsafe void BufferRange(Range range, void* ptr) => BufferSubData(range.Start.Value, range.End.Value, ptr);

	public unsafe void BufferRange<T>(Range range, T[] arr) where T : unmanaged {
		fixed(T* ptr = arr) BufferSubData(range.Start.Value * sizeof(T), range.End.Value * sizeof(T), ptr);
	}
	
	public unsafe void BufferData(int size, void* ptr, BufferUsageARB usage = BufferUsageARB.DynamicDraw) {
		length = size;
		if (OpenGl.supportNamedBuffers) {
			GL.NamedBufferData((BufferHandle)(int)handle, size, ptr, (VertexBufferObjectUsage) usage);
			return;
		}
		Bind();
		GL.BufferData(_target, size, ptr, usage);
	}
	
	public unsafe void BufferSubData(int offset, int size, void* ptr) {
		if (OpenGl.supportNamedBuffers) {
			GL.NamedBufferSubData((BufferHandle)(int)handle, (IntPtr)offset, size, ptr);
			return;
		}
		
		Bind();
		GL.BufferSubData(_target, (IntPtr)offset, size, ptr);
	}

	public static IBO Generate() {
		IBO buffer = GL.CreateBuffer();
		return buffer;
	}

	public static void Unbind() {
	#if SINGLE_GL_CONTEXT_OR_SHARED
		if (_boundBuffer.handle == 0) return;
		_boundBuffer = empty;
	#endif
		GL.BindBuffer(_target, BufferHandle.Zero);
	}

	public void Dispose() {
		if (!isGenerated) return;
	#if SINGLE_GL_CONTEXT_OR_SHARED
		if (_boundBuffer.handle == handle) Unbind();
	#endif
		GL.DeleteBuffer(this);
		handle = 0;
	}
}