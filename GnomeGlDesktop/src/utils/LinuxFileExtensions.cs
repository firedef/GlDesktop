using System.Runtime.InteropServices;

namespace GnomeGlDesktop.utils;

public static class LinuxFileExtensions {
	[DllImport("libc", CharSet = CharSet.Unicode)]
	public static extern int chmod(string path, int mode);
	
	[DllImport("libc", CharSet = CharSet.Ansi)]
	public static extern int stat(string path, out Stat stat);
	
	[DllImport("libc", CharSet = CharSet.Ansi)]
    public static extern int access([MarshalAs(UnmanagedType.LPStr)]string path, int mode);

	public static bool CheckAccessRights(string path, AccessRights rights) => access(path, (int)rights) == 0;

	public static bool IsReadable(string path) => CheckAccessRights(path, AccessRights.r);
}

public struct Stat {
	public ulong st_dev;
	public ulong st_ino;
	public ulong st_nlink;
	public uint st_mode;
	public uint st_uid;
	public uint st_gid;
	public ulong st_rdev;
	public long st_size;
	public long st_blksize;
	public long st_blocks;
	public long st_atime;
	public long st_atime_nsec;
	public long st_ctime;
	public long st_ctime_nsec;
}

public enum AccessRights {
	f = 0,
	x = 1,
	w = 2,
	r = 4,
}

// // user -> group -> other
// [Flags]
// public enum AccessRights {
// 	ur = 0x400,
// 	gr = 0x040,
// 	or = 0x004,
// 	
// 	uw = 0x200,
// 	gw = 0x020,
// 	ow = 0x002,
// 	
// 	ux = 0x100,
// 	gx = 0x010,
// 	ox = 0x001,
// }