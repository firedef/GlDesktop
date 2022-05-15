namespace GlDesktop.objects.mesh; 

public interface IDynamicMesh<TVert, TInd>
	where TVert : unmanaged, IVertex
	where TInd : unmanaged {
	public int verticesCount { get; }
	public int indicesCount { get; }
	public void AddVertex(TVert v);
	public unsafe void AddVertices(TVert* v, int c);
	public void AddIndex(TInd v);
	public unsafe void AddIndices(TInd* v, int c);
	public void Clear();
	public void Trim();
}