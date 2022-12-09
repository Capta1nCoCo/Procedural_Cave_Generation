using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PassageCreator))]
public class RoomConnector : MonoBehaviour
{
    private List<Room> allRooms;
    private bool forcedAccessibilityFromMainRoom;

    private List<Room> roomListA;
    private List<Room> roomListB;

    private Coord bestTileA;
    private Coord bestTileB;
    private Room bestRoomA;
    private Room bestRoomB;

    private int bestDistance;
    private bool possibleConnectionFound;

    private PassageCreator passageCreator;

    private void Awake()
    {
        passageCreator = GetComponent<PassageCreator>();
    }

    public void ConnectClosestRooms(List<Room> allRoomsList, bool forceAccessibilityFromMainRoom = false)
    {
        InitializeConnectorData(allRoomsList, forceAccessibilityFromMainRoom);
        FillRoomLists();
        LayClosestConnectionRoute();
        ApplyConnectionRouteRecursively();
    }

    private void InitializeConnectorData(List<Room> allRoomsList, bool forceAccessibilityFromMainRoom)
    {
        allRooms = allRoomsList;
        forcedAccessibilityFromMainRoom = forceAccessibilityFromMainRoom;

        roomListA = new List<Room>();
        roomListB = new List<Room>();

        bestTileA = new Coord();
        bestTileB = new Coord();
        bestRoomA = new Room();
        bestRoomB = new Room();
    }

    private void FillRoomLists()
    {
        if (forcedAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                DistributeRoomRelativeToItsAccesibilityFromMainRoom(room);
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }
    }

    private void DistributeRoomRelativeToItsAccesibilityFromMainRoom(Room room)
    {
        if (room.getIsAccessibleFromMainRoom)
        {
            roomListB.Add(room);
        }
        else
        {
            roomListA.Add(room);
        }
    }

    private void LayClosestConnectionRoute()
    {
        bestDistance = 0;
        possibleConnectionFound = false;
        foreach (Room roomA in roomListA)
        {
            if (!forcedAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                FindShortestPossibleConnection(roomA, roomB);
            }

            ApplyConnectionRoute();
        }
    }

    private void FindShortestPossibleConnection(Room roomA, Room roomB)
    {
        for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
        {
            for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
            {
                Coord tileA = roomA.edgeTiles[tileIndexA];
                Coord tileB = roomB.edgeTiles[tileIndexB];
                FindBestDistanceBetweenRooms(roomA, roomB, tileA, tileB);
            }
        }
    }

    private void FindBestDistanceBetweenRooms(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));
        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
        {
            bestDistance = distanceBetweenRooms;
            possibleConnectionFound = true;
            bestTileA = tileA;
            bestTileB = tileB;
            bestRoomA = roomA;
            bestRoomB = roomB;
        }
    }

    private void ApplyConnectionRoute()
    {
        if (possibleConnectionFound && !forcedAccessibilityFromMainRoom)
        {
            passageCreator.CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
        }
    }

    private void ApplyConnectionRouteRecursively()
    {
        if (possibleConnectionFound && forcedAccessibilityFromMainRoom)
        {
            passageCreator.CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        ConnectClosestRoomsWithForcedAcessibilityFromMainRoom();
    }

    private void ConnectClosestRoomsWithForcedAcessibilityFromMainRoom()
    {
        if (!forcedAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }
}
