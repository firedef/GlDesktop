using MathStuff.vectors;
using OpenTK.Mathematics;

namespace GlDesktop.utils; 

public static class MathUtils {
	public static int2 Mul(this int2 a, float b) => new((int)(a.x * b), (int)(a.y * b));
}