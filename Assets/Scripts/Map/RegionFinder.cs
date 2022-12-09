using System.Collections.Generic;
using UnityEngine;

public class RegionFinder : MonoBehaviour
{
    private const int TILE_UNCHECKED = 0;
    private const int TILE_CHECKED = 1;

    private List<List<Coord>> regions;
    private Queue<Coord> queue;

    private int[,] mapRegionFlags;
    private int[,] mapRegionTileFlags;

    private MapGenerator mapGenerator;

    private void Awake()
    {
        mapGenerator = GetComponent<MapGenerator>();
    }

    public List<List<Coord>> GetRegions(int tileType)
    {
        regions = new List<List<Coord>>();
        mapRegionFlags = new int[mapGenerator.getWidth, mapGenerator.getHeight];

        for (int x = 0; x < mapGenerator.getWidth; x++)
        {
            for (int y = 0; y < mapGenerator.getHeight; y++)
            {
                CheckRegionTiles(tileType, x, y);
            }
        }

        return regions;
    }

    private void CheckRegionTiles(int tileType, int x, int y)
    {
        if (mapRegionFlags[x, y] == TILE_UNCHECKED && mapGenerator.getMap[x, y] == tileType)
        {
            List<Coord> newRegion = GetRegionTiles(x, y);
            regions.Add(newRegion);

            foreach (Coord tile in newRegion)
            {
                mapRegionFlags[tile.tileX, tile.tileY] = TILE_CHECKED;
            }
        }
    }

    private List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        mapRegionTileFlags = new int[mapGenerator.getWidth, mapGenerator.getHeight];
        int tileType = mapGenerator.getMap[startX, startY];

        queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapRegionTileFlags[startX, startY] = TILE_CHECKED;

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
                        if (mapRegionTileFlags[x, y] == TILE_UNCHECKED && mapGenerator.getMap[x, y] == tileType)
                        {
                            mapRegionTileFlags[x, y] = TILE_CHECKED;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }
}
