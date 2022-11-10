using System;
using System.Collections.Generic;
using UnityEngine;

public class PassageCreator : MonoBehaviour
{
    [SerializeField] private int passageRadius = 5;

    private int deltaX, deltaY, fromTileX, fromTileY, longest, shortest, step, gradientStep;
    private bool isInverted;

    private MapGenerator mapGenerator;

    private void Awake()
    {
        mapGenerator = GetComponent<MapGenerator>();
    }

    public void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);

    #if UNITY_EDITOR
        Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 30f);
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
                        int floor = 0;
                        mapGenerator.SetMapTile(drawX, drawY, floor);
                    }
                }
            }
        }
    }

    private List<Coord> GetLine(Coord from, Coord to)
    {
        SetLineParameters(from, to);

        if (longest < shortest)
        {
            InvertLineParamenters();
        }

        return BuildLine();
    }

    private void SetLineParameters(Coord from, Coord to)
    {
        deltaX = to.tileX - from.tileX;
        deltaY = to.tileY - from.tileY;

        fromTileX = from.tileX;
        fromTileY = from.tileY;

        isInverted = false;
        longest = Mathf.Abs(deltaX);
        shortest = Mathf.Abs(deltaY);

        step = Math.Sign(deltaX);
        gradientStep = Math.Sign(deltaY);
    }

    private void InvertLineParamenters()
    {
        isInverted = true;
        longest = Mathf.Abs(deltaY);
        shortest = Mathf.Abs(deltaX);

        step = Math.Sign(deltaY);
        gradientStep = Math.Sign(deltaX);
    }

    private List<Coord> BuildLine()
    {
        List<Coord> line = new List<Coord>();

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(fromTileX, fromTileY));

            AddStep();

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                AddGradientStep();
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    private void AddStep()
    {
        if (isInverted)
        {
            fromTileY += step;
        }
        else
        {
            fromTileX += step;
        }
    }

    private void AddGradientStep()
    {
        if (isInverted)
        {
            fromTileX += gradientStep;
        }
        else
        {
            fromTileY += gradientStep;
        }
    }

#if UNITY_EDITOR
    private Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-mapGenerator.getWidth / 2 + .5f + tile.tileX, 2f, -mapGenerator.getHeight / 2 + .5f + tile.tileY);
    }
#endif
}
