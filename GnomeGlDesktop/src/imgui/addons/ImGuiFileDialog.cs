using GnomeGlDesktop.utils;
using ImGuiNET;
using MathStuff;

namespace GnomeGlDesktop.imgui.addons; 

public class ImGuiFileDialog {
	private static readonly Dictionary<FileType, char> _fileIcons = new() {
		{FileType.normal, ImGuiGlyphs.glyphs["file"]},
		{FileType.audio, ImGuiGlyphs.glyphs["file-audio"]},
		{FileType.code, ImGuiGlyphs.glyphs["file-code"]},
		{FileType.database, ImGuiGlyphs.glyphs["calendar"]},
		{FileType.image, ImGuiGlyphs.glyphs["file-image"]},
		{FileType.text, ImGuiGlyphs.glyphs["file-text"]},
		{FileType.video, ImGuiGlyphs.glyphs["file-video"]},
		{FileType.app, ImGuiGlyphs.glyphs["box"]},
		{FileType.font, ImGuiGlyphs.glyphs["font"]},
		{FileType.archive, ImGuiGlyphs.glyphs["file-archive"]}
	};

	private string _path;
	private string[] _pathParts;
	private CachedDirectory[] _directories;
	private string[] _files;
	private string[] _logicalDrives;

	public string path {
		get => _path;
		set {
			if (_path == value) return;
			_path = value;
			UpdatePath();
		}
	}

	public ImGuiFileDialog(string path) => this.path = path;

	private void UpdatePath() {
		if (string.IsNullOrEmpty(_path)) {
			_pathParts = Array.Empty<string>();
			_files = Array.Empty<string>();
			_directories = _logicalDrives.Select(v => new CachedDirectory(v)).ToArray();
			return;
		}
		List<string> parts = new();
		DirectoryInfo? dir = new(_path);
		while (true) {
			if (dir == null) break;
			parts.Add(dir.Name);
			dir = dir.Parent;
		}
		parts.Reverse();
		_pathParts = parts.ToArray();

		//_pathParts = _path.Split(new[]{'\\', '/'}, StringSplitOptions.RemoveEmptyEntries);
		_directories = Directory.GetDirectories(_path).Select(v => new CachedDirectory(v)).ToArray();
		_files = Directory.GetFiles(_path);
		_logicalDrives = Directory.GetLogicalDrives();
	}
	
	public void Layout() {
		ImGuiStylePtr style = ImGui.GetStyle();
		
		ImGui.Begin("choose file");
		ImGuiElements.ButtonPurple(ImGuiGlyphs.glyphs["search"].ToString());
		ImGui.SameLine();
		LayoutPathParts();

		ImGui.BeginChild("view");
		LayoutFolders();
		
		LayoutFiles();
		ImGui.EndChild();
		
		ImGui.End();
	}

	private void LayoutPathParts() {
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.BeginChild("path parts", new(ImGui.GetWindowWidth() - style.ItemSpacing.X * 2, 24));
		
		if (ImGuiElements.ButtonRed($"drives")) path = "";
		
		if (!string.IsNullOrEmpty(path)) {
			for (int i = 0; i < _pathParts.Length; i++) {
				string part = _pathParts[i];
				ImGui.SameLine();
				if (!ImGuiElements.ButtonYellow(part)) continue;
				path = Path.Join(_pathParts.Take(i + 1).ToArray());
			}
		}
		ImGui.EndChild();
	}

	private void LayoutFolders() {
		foreach (CachedDirectory directory in _directories) {
			if (!ImGuiElements.ButtonColored(directory.display, directory.col)) continue;
			if (!LinuxFileExtensions.IsReadable(directory.path)) continue;
			path = directory.path;
		}
	}
	
	private void LayoutFiles() {
		foreach (string file in _files) {
			//Console.WriteLine($"{FileTypes.GetTypeFromPath(file)} {file}");
			if (!ImGuiElements.ButtonGray($"{GetFileIcon(file)} {file}")) continue;
		}
	}

	private static char GetFileIcon(string path) => _fileIcons[FileTypes.GetTypeFromPath(path)];
}

public class CachedDirectory {
	public string path;
	public string display;
	public color col;

	public CachedDirectory(string path, string display, color col) {
		this.path = path;
		this.display = display;
		this.col = col;
	}

	public CachedDirectory(string path) {
		this.path = path;
		display = $"{ImGuiGlyphs.folder} {path}";
		bool isReadable = LinuxFileExtensions.IsReadable(path);
		col = isReadable ? ImGuiElements.yellow : ImGuiElements.red;
	}
}