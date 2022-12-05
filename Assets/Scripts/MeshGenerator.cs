using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TriangleCalculator))]
[RequireComponent(typeof(OutlineCalculator))]
public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private MeshFilter walls;
    [SerializeField] private MeshFilter cave;

    [SerializeField] private float wallHeight = 5f;

    private SquareGrid squareGrid;

    private Mesh caveMesh;
    private Mesh wallMesh;

    private List<Vector3> wallVertices = new List<Vector3>();
    private List<int> wallTriangles = new List<int>();

    private TriangleCalculator triangleGenerator;
    private OutlineCalculator outlineGenerator;

    private void Awake()
    {
        triangleGenerator = GetComponent<TriangleCalculator>();
        outlineGenerator = GetComponent<OutlineCalculator>();
    }

    public void GenerateMesh(int[,] map, float squareSize)
    {
        InitializeMeshData(map, squareSize);
        TriangulateGridSquares();
        CreateCaveMesh();
        GenerateCaveMeshUV(map, squareSize);
        CreateWallMesh();
    }

    private void InitializeMeshData(int[,] map, float squareSize)
    {
        triangleGenerator.ClearTriangleData();
        outlineGenerator.ClearOutlineData();
        squareGrid = new SquareGrid(map, squareSize);
    }

    private void TriangulateGridSquares()
    {
        for (int x = 0; x < squareGrid.getSquares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.getSquares.GetLength(1); y++)
            {
                triangleGenerator.TriangulateSquare(squareGrid.getSquares[x, y]);
            }
        }
    }

    private void CreateCaveMesh()
    {
        caveMesh = new Mesh();
        cave.mesh = caveMesh;
        caveMesh.vertices = triangleGenerator.getVertices.ToArray();
        caveMesh.triangles = triangleGenerator.getTriangles.ToArray();
        caveMesh.RecalculateNormals();
    }

    private void GenerateCaveMeshUV(int[,] map, float squareSize)
    {
        int tileAmount = 10; // TODO: serialize in future texture related class
        Vector2[] uvs = new Vector2[triangleGenerator.getVertices.Count];
        for (int i = 0; i < triangleGenerator.getVertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, triangleGenerator.getVertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, triangleGenerator.getVertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        caveMesh.uv = uvs;
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