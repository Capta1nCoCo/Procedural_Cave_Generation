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
        int wall = 1;
        int floor = 0;

        if (neighbourWallTiles > 4)
        {
            mapGenerator.SetMapTile(x, y, wall);
        }
        else if (neighbourWallTiles < 4)
        {
            mapGenerator.SetMapTile(x, y, floor);
        }
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
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
            }
        }

        return wallCount;
    }
}
