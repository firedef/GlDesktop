namespace GnomeGlDesktop.window.input; 

[Flags]
public enum MouseButtonFlags : byte {
	button0 = 1,
	button1 = 2,
	button2 = 4,
	button3 = 8,
	button4 = 16,
	button5 = 32,
	button6 = 64,
	button7 = 128,
	
	left = button0,
	right = button1,
	middle = button2,
	
	none = 0,
	all = 255,
}