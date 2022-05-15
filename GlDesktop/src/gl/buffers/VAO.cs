using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GlDesktop.gl.buffers; 

public struct VAO {
	public static readonly VAO empty = new();

	public uint handle;
	public bool isGenerated => handle != 0;
	
	public VAO() => handle = 0;
	public VAO(uint handle) => this.handle = handle;
	public VAO(int handle) => this.handle = (uint)handle;

	public static implicit operator VAO(int v) => new(v);
	public static implicit operator VAO(uint v) => new(v);
	public static implicit operator int(VAO v) => (int) v.handle;
	public static implicit operator uint(VAO v) => v.handle;
	
	public static implicit operator VAO(VertexArrayHandle v) => v.Handle;
	public static implicit operator VertexArrayHandle(VAO v) => new(v);

	public void Bind() {
		GL.BindVertexArray(this);
	}

	public static VAO Generate() {
		VAO buffer = GL.GenVertexArray();
		return buffer;
	}

	public static void Unbind() {
		GL.BindVertexArray(VertexArrayHandle.Zero);
	}

	public void Dispose() {
		if (!isGenerated) return;
		GL.DeleteVertexArray(this);
		handle = 0;
	}

	public void Attribute(uint loc, int size, VertexAttribPointerType type, bool normalized, int stride, int offset) {
		GL.VertexAttribPointer(loc, size, type, normalized, stride, offset);
		GL.EnableVertexAttribArray(loc);
	}
}