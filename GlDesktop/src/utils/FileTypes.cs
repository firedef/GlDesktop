namespace GlDesktop.utils; 

public static class FileTypes {
	public static readonly Dictionary<string, FileType> types = new() {
		// https://www.computerhope.com/issues/ch001789.htm
		{"aif", FileType.audio},
		{"cda", FileType.audio},
		{"mid", FileType.audio},
		{"midi", FileType.audio},
		{"mp3", FileType.audio},
		{"mpa", FileType.audio},
		{"ogg", FileType.audio},
		{"wav", FileType.audio},
		{"wma", FileType.audio},
		{"wpl", FileType.audio},
		
		{"3g2", FileType.video},
		{"3gp", FileType.video},
		{"avi", FileType.video},
		{"flv", FileType.video},
		{"h264", FileType.video},
		{"m4v", FileType.video},
		{"mkv", FileType.video},
		{"mov", FileType.video},
		{"mp4", FileType.video},
		{"mpg", FileType.video},
		{"mpeg", FileType.video},
		{"rm", FileType.video},
		{"swf", FileType.video},
		{"vob", FileType.video},
		{"wmv", FileType.video},

		{"7z", FileType.archive},
		{"arj", FileType.archive},
		{"deb", FileType.archive},
		{"pkg", FileType.archive},
		{"rar", FileType.archive},
		{"rpm", FileType.archive},
		{"gz", FileType.archive},
		{"tar.gz", FileType.archive},
		{"z", FileType.archive},
		{"zip", FileType.archive},
		
		{"dmg", FileType.app},
		{"iso", FileType.app},
		{"toast", FileType.app},
		{"vcd", FileType.app},
		{"exe", FileType.app},
		{"apk", FileType.app},
		{"bat", FileType.app},
		{"sh", FileType.app},
		{"jar", FileType.app},
		{"msi", FileType.app},
		
		{"csv", FileType.database},
		{"xls", FileType.database},
		{"xlsx", FileType.database},
		{"xlsm", FileType.database},
		{"dat", FileType.database},
		{"data", FileType.database},
		{"cache", FileType.database},
		{"db", FileType.database},
		{"dbf", FileType.database},
		{"log", FileType.database},
		{"mdb", FileType.database},
		{"sav", FileType.database},
		{"sql", FileType.database},
		{"tar", FileType.database},
		{"xml", FileType.database},
		{"json", FileType.database},
		{"yml", FileType.database},
		
		{"ttf", FileType.font},
		{"otf", FileType.font},
		
		{"ai", FileType.image},
		{"bmp", FileType.image},
		{"gif", FileType.image},
		{"ico", FileType.image},
		{"jpg", FileType.image},
		{"jpeg", FileType.image},
		{"png", FileType.image},
		{"ps", FileType.image},
		{"psd", FileType.image},
		{"svg", FileType.image},
		{"tif", FileType.image},
		{"tiff", FileType.image},
		
		{"asp", FileType.code},
		{"aspx", FileType.code},
		{"css", FileType.code},
		{"htm", FileType.code},
		{"html", FileType.code},
		{"js", FileType.code},
		{"php", FileType.code},
		{"py", FileType.code},
		{"cs", FileType.code},
		{"cpp", FileType.code},
		{"cc", FileType.code},
		{"h", FileType.code},
		{"hpp", FileType.code},
		{"c", FileType.code},
		{"class", FileType.code},
		{"java", FileType.code},
		{"vb", FileType.code},
		{"lua", FileType.code},
		
		{"txt", FileType.text},
		{"doc", FileType.text},
		{"docx", FileType.text},
		{"odt", FileType.text},
		{"pdf", FileType.text},
		{"rtf", FileType.text},
		{"tex", FileType.text},
		{"wpd", FileType.text},
		{"md", FileType.text},
	};

	public static FileType GetTypeFromExtension(string ext) => types.TryGetValue(ext.ToLower(), out FileType t) ? t : FileType.normal;
	public static FileType GetTypeFromPath(string path) => GetTypeFromExtension(Path.GetExtension(path).Trim('.'));
}

public enum FileType {
	normal,
	audio,
	code,
	database,
	image,
	text,
	video,
	app,
	font,
	archive
}