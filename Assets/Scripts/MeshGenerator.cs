using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TriangleGenerator))]
[RequireComponent(typeof(OutlineGenerator))]
public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private MeshFilter walls;
    [SerializeField] private MeshFilter cave;

    [SerializeField] private float wallHeight = 5f;

    private SquareGrid squareGrid;

    private List<Vector3> wallVertices = new List<Vector3>();
    private List<int> wallTriangles = new List<int>();
    private Mesh wallMesh;

    private TriangleGenerator triangleGenerator;
    private OutlineGenerator outlineGenerator;

    private void Awake()
    {
        triangleGenerator = GetComponent<TriangleGenerator>();
        outlineGenerator = GetComponent<OutlineGenerator>();
    }

    public void GenerateMesh(int[,] map, float squareSize)
    {
        triangleGenerator.ClearTriangleData();
        outlineGenerator.ClearOutlineData();

        squareGrid = new SquareGrid(map, squareSize);

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                triangleGenerator.TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        cave.mesh = mesh;

        mesh.vertices = triangleGenerator.getVertices.ToArray();
        mesh.triangles = triangleGenerator.getTriangles.ToArray();
        mesh.RecalculateNormals();

        int tileAmount = 10;
        Vector2[] uvs = new Vector2[triangleGenerator.getVertices.Count];
        for (int i = 0; i < triangleGenerator.getVertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, triangleGenerator.getVertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, triangleGenerator.getVertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        mesh.uv = uvs;

        CreateWallMesh();
    }

    private void CreateWallMesh()
    {
        outlineGenerator.CalculateMeshOutlines();
        InitializeWallMesh();
        CalculateWallMeshParameters();
        GenerateWallMesh();
        GenerateColliders();
    }

    private void InitializeWallMesh()
    {
        wallVertices.Clear();
        wallTriangles.Clear();
        wallMesh = new Mesh();
    }

    private void CalculateWallMeshParameters()
    {
        foreach (List<int> outline in outlineGenerator.getOutlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                AddWallVertices(outline, i);
                AddWallTriangles(startIndex);
            }
        }
    }

    private void GenerateWallMesh()
    {
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;
    }

    private void GenerateColliders()
    {
#if UNITY_EDITOR
        ClearPreviousColliders();
#endif
        MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }

    private void AddWallVertices(List<int> outline, int index)
    {
        wallVertices.Add(triangleGenerator.getVertices[outline[index]]); // left
        wallVertices.Add(triangleGenerator.getVertices[outline[index + 1]]); // right
        wallVertices.Add(triangleGenerator.getVertices[outline[index]] - Vector3.up * wallHeight); // bottom left
        wallVertices.Add(triangleGenerator.getVertices[outline[index + 1]] - Vector3.up * wallHeight); // bottom right
    }

    private void AddWallTriangles(int startIndex)
    {
        wallTriangles.Add(startIndex + 0);
        wallTriangles.Add(startIndex + 2);
        wallTriangles.Add(startIndex + 3);

        wallTriangles.Add(startIndex + 3);
        wallTriangles.Add(startIndex + 1);
        wallTriangles.Add(startIndex + 0);
    }

#if UNITY_EDITOR
    private void ClearPreviousColliders()
    {
        if (walls.GetComponent<MeshCollider>())
        {
            Destroy(walls.GetComponent<MeshCollider>());
        }
    }
#endif
}
