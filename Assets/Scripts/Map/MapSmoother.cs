using UnityEngine;

public class MapSmoother : MonoBehaviour
{
    private MapGenerator mapGenerator;

    private void Awake()
    {
        mapGenerator = GetComponent<MapGenerator>();
    }

    public void SmoothMap()
    {
        for (int x = 0; x < mapGenerator.getWidth; x++)
        {
            for (int y = 0; y < mapGenerator.getHeight; y++)
            {
                SmoothTile(x, y);
            }
        }
    }

    private void SmoothTile(int x, int y)
    {
        int neighbourWallTiles = GetSurroundingWallCount(x, y);
        SetTileTypeRelativeToWallNeighbours(x, y, neighbourWallTiles);
    }

    private void SetTileTypeRelativeToWallNeighbours(int x, int y, int neighbourWallTiles)
    {
        if (neighbourWallTiles > 4)
        {
            mapGenerator.SetMapTile(x, y, mapGenerator.getWALL);
        }
        else if (neighbourWallTiles < 4)
        {
            mapGenerator.SetMapTile(x, y, mapGenerator.getFLOOR);
        }
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                wallCount = CountWalls(gridX, gridY, wallCount, neighbourX, neighbourY);
            }
        }

        return wallCount;
    }

    private int CountWalls(int gridX, int gridY, int wallCount, int neighbourX, int neighbourY)
    {
        if (mapGenerator.IsInMapRange(neighbourX, neighbourY))
        {
            if (neighbourX != gridX || neighbourY != gridY)
            {
                wallCount += mapGenerator.getMap[neighbourX, neighbourY];
            }
        }
        else
        {
            wallCount++;
        }

        return wallCount;
    }
}
