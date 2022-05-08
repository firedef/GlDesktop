using OpenTK.Mathematics;

namespace GnomeGlDesktop.utils; 

public static class MathUtils {
	public static Vector2i Mul(this Vector2i a, float b) => new((int)(a.X * b), (int)(a.Y * b));
}