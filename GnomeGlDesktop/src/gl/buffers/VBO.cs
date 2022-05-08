using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GnomeGlDesktop.gl.buffers; 

public struct VBO {
	public static readonly VBO empty = new();

#if SINGLE_GL_CONTEXT_OR_SHARED
	private static VBO _boundBuffer = empty;
#endif

	private const BufferTargetARB _target = BufferTargetARB.ArrayBuffer;
	
	public uint handle;
	public int length = 0;
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

	public static VBO Generate() {
		VBO buffer = GL.CreateBuffer();
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