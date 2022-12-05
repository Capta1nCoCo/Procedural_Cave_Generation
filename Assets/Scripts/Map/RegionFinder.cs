using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionFinder : MonoBehaviour
{
    private const int tileUnchecked = 0;
    private const int tileChecked = 1;

    private MapGenerator mapGenerator;

    private void Awake()
    {
        mapGenerator = GetComponent<MapGenerator>();
    }

    public List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[mapGenerator.getWidth, mapGenerator.getHeight];

        for (int x = 0; x < mapGenerator.getWidth; x++)
        {
            for (int y = 0; y < mapGenerator.getHeight; y++)
            {
                if (mapFlags[x, y] == tileUnchecked && mapGenerator.getMap[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = tileChecked;
                    }
                }
            }
        }

        return regions;
    }

    private List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[mapGenerator.getWidth, mapGenerator.getHeight];
        int tileType = mapGenerator.getMap[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = tileChecked;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (mapGenerator.IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == tileUnchecked && mapGenerator.getMap[x, y] == tileType)
                        {
                            mapFlags[x, y] = tileChecked;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }
}
