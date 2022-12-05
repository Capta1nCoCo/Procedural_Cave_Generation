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
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                {
                    mapGenerator.SetMapTile(x, y, 1);
                }
                else if (neighbourWallTiles < 4)
                {
                    mapGenerator.SetMapTile(x, y, 0);
                }
            }
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
