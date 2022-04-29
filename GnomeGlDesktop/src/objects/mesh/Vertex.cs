using System.Runtime.InteropServices;
using GnomeGlDesktop.gl;
using MathStuff;
using MathStuff.vectors;

namespace GnomeGlDesktop.objects.mesh; 

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct Vertex(float3 position, float3 normal, color color, float2 uv0, float2 uv1) : IVertex {
	public float3 position = position;
	public float3 normal = normal;
	public color color = color;
	public float2 uv0 = uv0;
	public float2 uv1 = uv1;
	
	public unsafe void GenerateVao(VAO vao) {
		int stride = sizeof(Vertex);

		const int offset0 = 0;
		int offset1 = offset0 + sizeof(float3);
		int offset2 = offset1 + sizeof(float3);
		int offset3 = offset2 + sizeof(color);
		int offset4 = offset3 + sizeof(float2);
		
		vao.AttributeVec3(0, stride, offset0);
		vao.AttributeVec3(1, stride, offset1);
		vao.AttributeColor(2, stride, offset2);
		vao.AttributeVec2(3, stride, offset3);
		vao.AttributeVec2(4, stride, offset4);
	}

}