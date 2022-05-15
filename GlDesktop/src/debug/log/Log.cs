using Spectre.Console;

namespace GlDesktop.debug.log; 

public static class Log {
	public static event PrintEventDelegate OnPrint;
	public static event Action<string> OnPrintRaw;

	public static LogLevel logLevel = LogLevel.minimal;

	static Log() {
		OnPrint += PrintToConsole;
		OnPrintRaw += PrintToConsole;
	}
	
	private static void PrintToConsole(string msg) => AnsiConsole.MarkupLine(msg);

	private static void PrintToConsole(string msg, string? form, LogLevel level, DateTime time) {
		string style = level switch {
			LogLevel.minimal => "gray i s",
			LogLevel.note => "gray",
			LogLevel.message => "",
			LogLevel.important => "blue",
			LogLevel.warning => "yellow",
			LogLevel.error => "red",
			LogLevel.fatal => "red bold",
			_ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
		};
		msg = $"[b][[{(form == null ? "" : form + " ")}{time:HH:mm:ss}]]:[/] {msg}";
		// msg = $"[b][[{level}{(form == null ? "" : " " + form)} {time:HH:mm:ss}]]:[/] {msg}";
		if (!string.IsNullOrEmpty(style)) msg = $"[{style}]{msg}[/]";
		
		AnsiConsole.MarkupLine(msg);
	}

	public static void Print(string msg) => OnPrintRaw(msg);
	
	public static void Print(string msg, string? form, LogLevel lvl) {
		if (logLevel <= lvl) OnPrint(msg, form, lvl, DateTime.Now);
	}
	
	// public static void Print(string msg, string style, string? form, LogLevel lvl) {
	// 	if (level <= lvl && (filter & form) != 0) Print($"[{style}]{msg}[/]");
	// }

	// public static void Minimal(string msg, string? form = null) => Print(msg, "gray i s", form, LogLevel.minimal);
	// public static void Note(string msg, string? form = null) => Print(msg, "gray", form, LogLevel.note);
	// public static void Message(string msg, string? form = null) => Print(msg, form, LogLevel.message);
	// public static void Important(string msg, string? form = null) => Print(msg, "blue", form, LogLevel.important);
	// public static void Warning(string msg, string? form = null) => Print(msg, "yellow", form, LogLevel.warning);
	// public static void Error(string msg, string? form = null) => Print(msg, "red", form, LogLevel.error);
	// public static void Fatal(string msg, string? form = null) => Print(msg, "red bold", form, LogLevel.fatal);
	
	public static void Minimal(string msg, string? form = null) => Print(msg, form, LogLevel.minimal);
	public static void Note(string msg, string? form = null) => Print(msg, form, LogLevel.note);
	public static void Message(string msg, string? form = null) => Print(msg, form, LogLevel.message);
	public static void Important(string msg, string? form = null) => Print(msg, form, LogLevel.important);
	public static void Warning(string msg, string? form = null) => Print(msg, form, LogLevel.warning);
	public static void Error(string msg, string? form = null) => Print(msg, form, LogLevel.error);
	public static void Fatal(string msg, string? form = null) => Print(msg, form, LogLevel.fatal);

	public delegate void PrintEventDelegate(string msg, string? form, LogLevel level, DateTime time);
}

public enum LogLevel {
	minimal,
	note,
	message,
	important,
	warning,
	error,
	fatal,
}