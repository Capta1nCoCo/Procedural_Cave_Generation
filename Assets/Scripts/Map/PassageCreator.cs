using System;
using System.Collections.Generic;
using UnityEngine;

public class PassageCreator : MonoBehaviour
{
    [SerializeField] private int passageRadius = 5;

    private MapGenerator mapGenerator;

    private void Awake()
    {
        mapGenerator = GetComponent<MapGenerator>();
    }

    public void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);

    #if UNITY_EDITOR
        Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 100f);
    #endif

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord coord in line)
        {
            DrawCircle(coord, passageRadius);
        }
    }

    private void DrawCircle(Coord coord, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int drawX = coord.tileX + x;
                    int drawY = coord.tileY + y;
                    if (mapGenerator.IsInMapRange(drawX, drawY))
                    {
                        mapGenerator.SetMap(drawX, drawY, 0);
                    }
                }
            }
        }
    }

    private List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int deltaX = to.tileX - from.tileX;
        int deltaY = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(deltaX);
        int gradientStep = Math.Sign(deltaY);

        int longest = Mathf.Abs(deltaX);
        int shortest = Mathf.Abs(deltaY);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(deltaY);
            shortest = Mathf.Abs(deltaX);

            step = Math.Sign(deltaY);
            gradientStep = Math.Sign(deltaX);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

#if UNITY_EDITOR
    private Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-mapGenerator.getWidth / 2 + .5f + tile.tileX, 2f, -mapGenerator.getHeight / 2 + .5f + tile.tileY);
    }
#endif
}
