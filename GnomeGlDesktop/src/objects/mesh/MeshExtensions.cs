namespace GnomeGlDesktop.objects.mesh; 

public static class MeshExtensions {
	public static void AddTriangle<TVert>(this IDynamicMesh<TVert, ushort> mesh, TVert p0, TVert p1, TVert p2) where TVert : unmanaged, IVertex {
		int vC = mesh.verticesCount;
		mesh.AddVertex(p0);
		mesh.AddVertex(p1);
		mesh.AddVertex(p2);
		mesh.AddIndex((ushort)(vC + 0));
		mesh.AddIndex((ushort)(vC + 1));
		mesh.AddIndex((ushort)(vC + 2));
	}
	
	public static unsafe void AddQuad<TVert>(this IDynamicMesh<TVert, ushort> mesh, TVert p0, TVert p1, TVert p2, TVert p3) where TVert : unmanaged, IVertex {
		int vC = mesh.verticesCount;
		
		mesh.AddVertex(p0);
		mesh.AddVertex(p1);
		mesh.AddVertex(p2);
		mesh.AddVertex(p3);

		ushort* indices = stackalloc ushort[6] {
			(ushort)(vC + 0),
			(ushort)(vC + 1),
			(ushort)(vC + 2),
			(ushort)(vC + 0),
			(ushort)(vC + 2),
			(ushort)(vC + 3)
		};
		mesh.AddIndices(indices, 6);
	}
}