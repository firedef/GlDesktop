using ImGuiNET;
using MathStuff;

namespace GlDesktop.imgui; 

public static class ImGuiElements {
	public static readonly color yellow = "#f59b42";
	public static readonly color purple = "#7842f5";
	public static readonly color red = "#f54263";
	public static readonly color gray = "#464a57";
	
	public static bool ButtonColored(string txt, color col) {
		color texCol = col.WithLightness(col.lightness * 1.3f);
		float opacity = col.aF;
		
		ImGui.PushStyleColor(ImGuiCol.Button, col.WithAlpha((byte)math.min(140 * opacity, 255)).ToVec4());
		ImGui.PushStyleColor(ImGuiCol.ButtonHovered, col.WithAlpha((byte)math.min(210 * opacity, 255)).ToVec4());
		ImGui.PushStyleColor(ImGuiCol.ButtonActive, col.WithAlpha((byte)math.min(255 * opacity, 255)).ToVec4());
		ImGui.PushStyleColor(ImGuiCol.Text, texCol.ToVec4());
		bool b = ImGui.Button(txt);
		ImGui.PopStyleColor(4);
		return b;
	}
	
	public static bool ButtonColored(string txt, color col, float opacity) {
		color texCol = col.WithLightness(col.lightness * 1.3f);
		
		ImGui.PushStyleColor(ImGuiCol.Button, col.WithAlpha((byte)math.min(140 * opacity, 255)).ToVec4());
		ImGui.PushStyleColor(ImGuiCol.ButtonHovered, col.WithAlpha((byte)math.min(210 * opacity, 255)).ToVec4());
		ImGui.PushStyleColor(ImGuiCol.ButtonActive, col.WithAlpha((byte)math.min(255 * opacity, 255)).ToVec4());
		ImGui.PushStyleColor(ImGuiCol.Text, texCol.ToVec4());
		bool b = ImGui.Button(txt);
		ImGui.PopStyleColor(4);
		return b;
	}
	
	public static bool ButtonYellow(string txt, float opacity = 1) => ButtonColored(txt, yellow, opacity);
	public static bool ButtonPurple(string txt, float opacity = 1) => ButtonColored(txt, purple, opacity);
	public static bool ButtonRed(string txt, float opacity = 1) => ButtonColored(txt, red, opacity);
	public static bool ButtonGray(string txt, float opacity = 1) => ButtonColored(txt, gray, opacity);
}