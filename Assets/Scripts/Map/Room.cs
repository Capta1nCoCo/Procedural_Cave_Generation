using System;
using System.Collections.Generic;

public class Room : IComparable<Room>
{
    public readonly List<Coord> tiles;
    public readonly List<Coord> edgeTiles;
    public readonly List<Room> connectedRooms;
    public readonly int roomSize;

    private bool isAccessibleFromMainRoom;
    private bool isMainRoom;

    public bool getIsAccessibleFromMainRoom { get { return isAccessibleFromMainRoom; } }
    public bool setIsAccessibleFromMainRoom { set { isAccessibleFromMainRoom = value; } }
    public bool setIsMainRoom { set { isMainRoom = value; } }

    public Room() { }

    public Room(List<Coord> roomTiles, int[,] map)
    {
        tiles = roomTiles;
        roomSize = tiles.Count;
        connectedRooms = new List<Room>();

        edgeTiles = new List<Coord>();
        foreach (Coord tile in tiles)
        {
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (x == tile.tileX || y == tile.tileY)
                    {
                        if (map[x, y] == 1)
                        {
                            edgeTiles.Add(tile);
                        }
                    }
                }
            }
        }
    }

    public void SetAccessibleFromMainRoom()
    {
        if (!isAccessibleFromMainRoom)
        {
            isAccessibleFromMainRoom = true;
            foreach (Room connectedRoom in connectedRooms)
            {
                connectedRoom.SetAccessibleFromMainRoom();
            }
        }
    }

    public static void ConnectRooms(Room roomA, Room roomB)
    {
        if (roomA.isAccessibleFromMainRoom)
        {
            roomB.SetAccessibleFromMainRoom();
        }
        else if (roomB.isAccessibleFromMainRoom)
        {
            roomA.SetAccessibleFromMainRoom();
        }
        roomA.connectedRooms.Add(roomB);
        roomB.connectedRooms.Add(roomA);
    }

    public bool IsConnected(Room otherRoom)
    {
        return connectedRooms.Contains(otherRoom);
    }

    public int CompareTo(Room otherRoom)
    {
        return otherRoom.roomSize.CompareTo(roomSize);
    }
}
