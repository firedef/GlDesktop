using GlDesktop.structures;

namespace GlDesktop.objects.mesh;

public class Mesh<TVert, TInd> : MeshBase, IDynamicMesh<TVert, TInd>
	where TVert : unmanaged, IVertex
	where TInd : unmanaged {
	public readonly NativeList<TVert> vertices;
	public readonly NativeList<TInd> indices;
	public int verticesCount => vertices.count;
	public int indicesCount => indices.count;

	public Mesh(NativeList<TVert> vertices, NativeList<TInd> indices) {
		this.vertices = vertices;
		this.indices = indices;
		Init();
	}
	
	public Mesh(int vertexCapacity = 16, int indexCapacity = 16) : this(new NativeList<TVert>(vertexCapacity), new(indexCapacity)) { }
	
	public unsafe Mesh(TVert[] vertices, TInd[] indices) {
		fixed(TVert* ptr = vertices) this.vertices = new(ptr, vertices.Length);
		fixed(TInd* ptr = indices) this.indices = new(ptr, indices.Length);
		Init();
	}

	protected override unsafe void* GetVboPtr() => vertices.ptr;
	protected override unsafe void* GetIboPtr() => indices.ptr;
	protected override int GetVboElementCount() => vertices.capacity;
	protected override int GetIboElementCount() => indices.capacity;
	protected override unsafe int GetVertexSize() => sizeof(TVert);
	protected override unsafe int GetIndexSize() => sizeof(TInd);
	protected override void GenerateVao() => new TVert().GenerateVao(vao);

	protected override int GetElementDrawCount() => indices.count;

	public void AddVertex(TVert v) => vertices.Add(v);
	public void AddIndex(TInd v) => indices.Add(v);

	public unsafe void AddVertices(TVert* v, int c) => vertices.AddRange(v, c);
	public unsafe void AddIndices(TInd* v, int c) => indices.AddRange(v, c);

	public void Clear() {
		vertices.Clear();
		indices.Clear();
	}
	public void Trim() {
		vertices.Trim();
		indices.Trim();
	}
}