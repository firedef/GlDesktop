using GnomeGlDesktop.gl;
using GnomeGlDesktop.gl.buffers;
using OpenTK.Graphics.OpenGL.Compatibility;

namespace GnomeGlDesktop.objects.mesh; 

public abstract class MeshBase : IDisposable {
	protected abstract unsafe void* GetVboPtr();
	protected abstract unsafe void* GetIboPtr();
	protected abstract int GetVboElementCount();
	protected abstract int GetIboElementCount();
	protected abstract int GetVertexSize();
	protected abstract int GetIndexSize();

	protected virtual int GetElementDrawCount() => GetIboElementCount();
	
	protected virtual BufferUsageARB GetBufferUsage() => BufferUsageARB.DynamicDraw;
	protected virtual PrimitiveType GetPrimitiveType() => PrimitiveType.Triangles;

	protected VAO vao;
	protected VBO vbo;
	protected IBO ibo;
	protected bool isGenerated = false;

	protected void Init() {
	}

	protected abstract void GenerateVao();

	private void GenerateGlObjects() {
		vao = VAO.Generate();
		vao.Bind();
		
		vbo = VBO.Generate();
		vbo.Bind();
		
		ibo = IBO.Generate();
		ibo.Bind();
		
		GenerateVao();
		isGenerated = true;
	}

	public unsafe void Buffer() {
		Bind();
		vbo.Buffer(GetVboElementCount() * GetVertexSize(), GetVboPtr(), GetBufferUsage());
		ibo.Buffer(GetIboElementCount() * GetIndexSize(), GetIboPtr(), GetBufferUsage());
	}

	public void Bind() {
		if (!isGenerated) GenerateGlObjects();
		vao.Bind();
		vbo.Bind();
		ibo.Bind();
	}

	public virtual void Draw() {
		Bind();
		//Console.WriteLine($"{GetVboElementCount()} {GetIboElementCount()}");
		GL.DrawElements(GetPrimitiveType(), GetElementDrawCount(), GetIndexType(), 0);
	}
	
	protected virtual DrawElementsType GetIndexType() => GetIndexSize() switch {
		1 => DrawElementsType.UnsignedByte,
		2 => DrawElementsType.UnsignedShort,
		4 => DrawElementsType.UnsignedInt,
		_ => throw new NotSupportedException($"cannot draw mesh with index size of {GetIndexSize()}")
	};

	private void ReleaseUnmanagedResources() {
		vbo.Dispose();
		vao.Dispose();
		ibo.Dispose();
	}
	
	public void Dispose() { ReleaseUnmanagedResources(); GC.SuppressFinalize(this); }
	~MeshBase() => ReleaseUnmanagedResources();
}