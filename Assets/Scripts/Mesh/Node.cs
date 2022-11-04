using UnityEngine;

public class Node
{
    public readonly Vector3 position;

    private int vertexIndex = -1;
    public int getVertexIndex { get { return vertexIndex; } }
    public int setVertexIndex { set { vertexIndex = value; } }

    public Node(Vector3 _pos)
    {
        position = _pos;
    }
}
