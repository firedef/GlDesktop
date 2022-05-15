using System.Collections;
using GlDesktop.utils;
using MathStuff;

namespace GlDesktop.structures; 

public class NativeList<T> : ICollection<T>, IDisposable where T : unmanaged {
#region fields

	public const int defaultSize = 1 << 20;
	public unsafe T* ptr;
	public int capacity;
	public int count;
	public int freeSpace => capacity - count;

	public unsafe T first => ptr[0];
	public unsafe T last => ptr[count - 1];

#endregion fields

#region ctors

	public NativeList() : this(defaultSize) {}

	public unsafe NativeList(int capacity) {
		ptr = (T*) MemoryAllocator.Allocate(capacity * sizeof(T));
		this.capacity = capacity;
	}
	
	/// <summary>data from ptr will be copied to new allocation</summary>
	public unsafe NativeList(T* other, int size) : this(size) {
		count = size;
		MemoryAllocator.MemCpy(ptr, other, size);
	}
	

#endregion ctors

#region memory

	protected unsafe void Resize(int newSize) {
		if (newSize == capacity) return;
		if (capacity == 0) ptr = (T*)MemoryAllocator.Allocate(newSize * sizeof(T));
		else ptr = (T*)MemoryAllocator.Resize(ptr, newSize * sizeof(T));
		capacity = newSize;
	}

	protected void Expand(int minSize) => Resize(capacity + math.max(minSize, capacity));
	public void EnsureFreeSpace(int space) { if (freeSpace < space) Expand(space); }
	public void EnsureCapacity(int c) { if (capacity < c) Expand(c - capacity); }
	
	public unsafe void CopyFrom(T* src, int c) {
		EnsureCapacity(c);
		count = c;
		MemoryAllocator.MemCpy(ptr, src, c);
	}
	
	public unsafe void CopyFrom(T* src, int c, int offset) {
		EnsureFreeSpace(c);
		count = c + offset;
		MemoryAllocator.MemCpy(ptr + offset, src, c);
	}

	public void Trim(int targetSize) {
		if (targetSize >= capacity) return;
		Resize(targetSize);
	}

	public void Trim() => Trim(count);

#endregion memory

#region elements

	public virtual unsafe int Add(T v) {
		EnsureFreeSpace(1);
		ptr[count++] = v;
		return count - 1;
	}
	
	public virtual unsafe void AddRange(T* v, int c) {
		EnsureFreeSpace(c);
		MemoryAllocator.MemCpy(ptr + count, v, c);
		count += c;
	}

	public virtual unsafe T Pop(int c = 1) {
		count = math.max(0, count - c);
		return ptr[count];
	}

	public unsafe void MoveElements(int src, int dest, int c) {
		EnsureCapacity(dest + c);
		MemoryAllocator.MemCpy(ptr + dest, ptr + src, c);
	}
	public void ShiftElements(int index, int amount) {
		count += amount;
		switch (amount) {
			case 0:   return;
			case < 0: MoveElements(index - amount, index, count - index + amount); break;
			default:  MoveElements(index, index + amount, count - index - amount); break;
		}
	}

	protected void CascadeRemove(int index, int c) => ShiftElements(index, -c);
	protected void CascadeInsert(int index, int c) => ShiftElements(index,  c);

	public void Insert(int index, T v) {
		if (index >= count) {
			Add(v);
			return;
		} 
		CascadeInsert(index, 1);
		this[index] = v;
	}
	
	public unsafe T* InsertSpace(int index, int c) {
		if (index + c >= count) {
			EnsureFreeSpace(c);
			count += c;
			return ptr + count - c;
		} 
		CascadeInsert(index, c);
		return ptr + index;
	}

	public virtual void RemoveAt(int index, int c = 1) {
		if (index + c == count) Pop(c);
		else CascadeRemove(index, c);
	}

	public unsafe T this[int index] { get => ptr[index]; set => ptr[index] = value; }

	public virtual int IndexOf(T v) { 
		for (int i = 0; i < count; i++) if (this[i].Equals(v)) return i; 
		return -1; 
	}
	
	public int IndexOf(Predicate<T> predicate) { 
		for (int i = 0; i < count; i++) if (predicate(this[i])) return i; 
		return -1; 
	}

	public unsafe void CopyTo(T* dest, int c) => MemoryAllocator.MemCpy(dest, ptr, math.min(c, count));

#endregion elements

#region icollection

	void ICollection<T>.Add(T v) => Add(v); 

	public virtual void Clear() => count = 0;
	public bool Contains(T item) => IndexOf(item) != -1;
	public unsafe void CopyTo(T[] array, int arrayIndex) { fixed(T* dest = array) CopyTo(dest, array.Length); }
	public virtual bool Remove(T item) {
		int ind = IndexOf(item);
		if (ind == -1) return false;
		RemoveAt(ind);
		return true;
	}
	public int Count => count;
	public bool IsReadOnly => false;

	public IEnumerator<T> GetEnumerator() => new Enumerator(this);
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#endregion icollection

#region enumerator
	
	public class Enumerator : IEnumerator<T> {
		public int pos = -1;
		public NativeList<T> owner;

		public Enumerator(NativeList<T> owner) => this.owner = owner;

		public bool MoveNext() => ++pos < owner.count;
		public void Reset() => pos = -1;
		public T Current => owner[pos];

		object IEnumerator.Current => Current;

		public void Dispose() { }
	}  

#endregion enumerator

#region dispose

	private unsafe void ReleaseUnmanagedResources() {
		MemoryAllocator.Free(ptr);
		ptr = null;
		capacity = 0;
	}
	public unsafe void Dispose() {
		if (ptr == null) return;
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}
	~NativeList() => ReleaseUnmanagedResources();

#endregion dispose
}
