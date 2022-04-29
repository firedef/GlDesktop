using System.Numerics;
using MathStuff;

namespace GnomeGlDesktop.imgui; 

public static class ImGuiColors {
	public static Vector4 header = color.softYellow.ToVec4();
	public static Vector4 ok = "#7869ff".ToVec4();
	public static Vector4 warn = "#f59b42".ToVec4();
	public static Vector4 error = "#f5424e".ToVec4();
	public static Vector4 disabled = "#676b70".ToVec4();
	public static Vector4 text0 = color.steelBlueLighter.ToVec4();
	public static Vector4 text1 = color.steelBlueLightest.ToVec4();
	public static Vector4 text2 = color.steelBlueLightest2.ToVec4();

	public static Vector4 ToVec4(this color col) => new(col.rF, col.gF, col.bF, col.aF);
	public static Vector4 ToVec4(this string col) => ((color) col).ToVec4();
	
	public static color ToColor(this Vector4 col) => new(col.X, col.Y, col.Z, col.W);
}