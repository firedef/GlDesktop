using System.Runtime.InteropServices;

namespace GlDesktop.utils; 

public static class MemoryAllocator {
	// public static ulong currentAllocated => QuartzNative.GetCurrentAllocatedBytes();
	// public static ulong totalAllocated => QuartzNative.GetTotalAllocatedBytes();
	// public static ulong allocatedSinceLastCleanup => QuartzNative.GetAllocatedBytesSinceLastCleanup();
	// public static int allocatedPerRareUpdate;

	public static unsafe void* Allocate(int bytes) {
		//allocatedPerRareUpdate += bytes;
		
		void* ptr = NativeMemory.Alloc((uint) bytes);
		return ptr;
	}

	public static unsafe void Free(void* ptr) {
		NativeMemory.Free(ptr);
	}

	public static unsafe void* Resize(void* ptr, int newSizeBytes) {
		if (ptr == null) return Allocate(newSizeBytes);
		//allocatedPerRareUpdate += newSizeBytes;

		void* ptrNew = NativeMemory.Realloc(ptr, (uint)newSizeBytes);
		return ptrNew;
	}

	public static unsafe void MemCpy<T>(T* dest, T* src, int count) where T : unmanaged => Buffer.MemoryCopy(src, dest, count * sizeof(T), count * sizeof(T)); 

	// public static string ToStringMarkup() => $"\n[b]" +
	//                                          $"current: {ToStringDataVal(currentAllocated)}\n" +
	//                                          $"total: {ToStringDataVal(totalAllocated)}\n" +
	//                                          $"per rare update: {ToStringDataVal((ulong)allocatedPerRareUpdate)}\n" +
	//                                          $"[/]";
	//
	// private static string ToStringDataVal(ulong v) => $"[yellow]{v.ToStringData()}[/]";
}