using System.Globalization;
using Newtonsoft.Json;

namespace GnomeGlDesktop.imgui; 

public static class ImGuiGlyphs {
	public static readonly Dictionary<string, char> glyphs = new();

	public static HashSet<char> ParseGlyphsJson(string json) {
		Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;
		HashSet<char> addedGlyphs = new();

		foreach ((string name, string code) in dictionary) {
			char glyph = (char) ushort.Parse(code, NumberStyles.HexNumber);
			if (glyph < 0x1000 || glyphs.ContainsKey(name)) continue;
			glyphs.Add(name, glyph);
			addedGlyphs.Add(glyph);
		}
		return addedGlyphs;
	}
	
	public static char emptyFolder => glyphs["folder-blank"];
	public static char folder => glyphs["folder"];
	public static char github => glyphs["github"];
	
	public static char file => glyphs["file"];
}