using OpenTK.Graphics.OpenGL;

namespace GlDesktop.gl.buffers; 

public static class VaoExtensions {
	public static void AttributeFloat(this VAO vao, uint loc, int stride, int offset) => vao.Attribute(loc, 1, VertexAttribPointerType.Float, false, stride, offset);
	public static void AttributeVec2(this VAO vao, uint loc, int stride, int offset) => vao.Attribute(loc, 2, VertexAttribPointerType.Float, false, stride, offset);
	public static void AttributeVec3(this VAO vao, uint loc, int stride, int offset) => vao.Attribute(loc, 3, VertexAttribPointerType.Float, false, stride, offset);
	public static void AttributeVec4(this VAO vao, uint loc, int stride, int offset) => vao.Attribute(loc, 4, VertexAttribPointerType.Float, false, stride, offset);
	
	public static void AttributeColor(this VAO vao, uint loc, int stride, int offset) => vao.Attribute(loc, 4, VertexAttribPointerType.UnsignedByte, true, stride, offset);
}