using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TriangleGenerator))]
public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private MeshFilter walls;
    [SerializeField] private MeshFilter cave;

    private SquareGrid squareGrid;

    private List<List<int>> outlines = new List<List<int>>();
    public HashSet<int> checkedVertices = new HashSet<int>(); // temp public

    private TriangleGenerator triangleGenerator;

    private void Awake()
    {
        triangleGenerator = GetComponent<TriangleGenerator>();
    }

    public void GenerateMesh(int[,] map, float squareSize)
    {
        triangleGenerator.ClearTriangleData();

        outlines.Clear();
        checkedVertices.Clear();

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
        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallHeight = 5f;

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(triangleGenerator.getVertices[outline[i]]); // left
                wallVertices.Add(triangleGenerator.getVertices[outline[i + 1]]); // right
                wallVertices.Add(triangleGenerator.getVertices[outline[i]] - Vector3.up * wallHeight); // bottom left
                wallVertices.Add(triangleGenerator.getVertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }

        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;

        GenerateColliders(walls.gameObject, wallMesh);
    }

    private void GenerateColliders(GameObject walls, Mesh wallMesh)
    {
        // BUG FIX: with each new generation during Play Mode old colliders remain
        ClearPreviousColliders(walls);

        MeshCollider wallCollider = walls.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }

    // Probably not needed for the real project, temporary bug fix for Play Mode [Consider Removal]
    private void ClearPreviousColliders(GameObject walls)
    {
        if (walls.GetComponent<MeshCollider>())
        {
            Destroy(walls.GetComponent<MeshCollider>());
        }
    }

    // Outlines
    private void CalculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < triangleGenerator.getVertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    private void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    private int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleGenerator.getTriangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    private bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleGenerator.getTriangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }

        return sharedTriangleCount == 1;
    }
}
