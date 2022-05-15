using System.Runtime.InteropServices;
using X11;

namespace GlDesktop.utils; 

public static class XLibUtils {
	[DllImport("libX11.so.6")]
	public static extern Atom XInternAtom(IntPtr display, [MarshalAs(UnmanagedType.LPStr)] string atomName, bool onlyIfExists);
	
	[DllImport("libX11.so.6")]
	public static extern void XChangeProperty(IntPtr display, X11.Window win, Atom property, Atom type, int format, int mode, [MarshalAs(UnmanagedType.LPStr)] string data, int elementCount);
	
	[DllImport("libX11.so.6")]
	public static extern unsafe void XChangeProperty(IntPtr display, X11.Window win, Atom property, Atom type, int format, int mode, void* data, int elementCount);
	
	[DllImport("libX11.so.6")]
	public static extern void XChangeProperty(IntPtr display, X11.Window win, Atom property, Atom type, int format, int mode, sbyte[] data, int elementCount);
	
	[DllImport("libX11.so.6")]
	public static extern void XChangeProperty(IntPtr display, X11.Window win, Atom property, Atom type, int format, int mode, short[] data, int elementCount);
	
	[DllImport("libX11.so.6")]
	public static extern void XChangeProperty(IntPtr display, X11.Window win, Atom property, Atom type, int format, int mode, long[] data, int elementCount);
}