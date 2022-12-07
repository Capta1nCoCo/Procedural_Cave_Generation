using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshGenerator))]
[RequireComponent(typeof(RoomConnector))]
[RequireComponent(typeof(RegionFinder))]
[RequireComponent(typeof(MapSmoother))]
public class MapGenerator : MonoBehaviour
{
    [Header("Map Size")]
    [SerializeField] private int width;
    [SerializeField] private int height;

    [Header("Random Generation")]
    [SerializeField] private string seed;
    [SerializeField] private bool useRandomSeed;
    [Range(0,100)]
    [SerializeField] private int randomFillPercent;

    [Header("Regions")]
    [Tooltip("Bellow 50 allows small pillars to be spawned")]
    [SerializeField] private int wallThresholdSize = 50;
    [Tooltip("Bellow 50 allows small rooms(unconnected) to be spawned")]
    [SerializeField] private int roomThresholdSize = 50;


    private const int WALL = 1;
    private const int FLOOR = 0;
    private const int BORDER_SIZE = 1;
    private const float SQUARE_SIZE = 1f;

    private int[,] map;
    private int[,] borderedMap;

    private MeshGenerator meshGenerator;
    private RoomConnector roomConnector;
    private RegionFinder regionFinder;
    private MapSmoother mapSmoother;

    private void Awake()
    {
        meshGenerator = GetComponent<MeshGenerator>();
        roomConnector = GetComponent<RoomConnector>();
        regionFinder = GetComponent<RegionFinder>();
        mapSmoother = GetComponent<MapSmoother>();
    }

    private void Start()
    {
        GenerateMap();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }
#endif

    private void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();
        SmoothMapXTimes(5);
        ProcessMap();
        CalculateBorderedMap();
    }

    private void RandomFillMap()
    {
        UseRandomSeed();
        PesudoRandomFillMap();
    }

    private void UseRandomSeed()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
    }

    private void PesudoRandomFillMap()
    {
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateRandomMapWithEdgeWalls(pseudoRandom, x, y);
            }
        }
    }

    private void CreateRandomMapWithEdgeWalls(System.Random pseudoRandom, int x, int y)
    {
        if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
        {
            map[x, y] = WALL;
        }
        else
        {
            map[x, y] = pseudoRandom.Next(0, 100) < randomFillPercent ? WALL : FLOOR;
        }
    }

    private void SmoothMapXTimes(int x)
    {
        for (int i = 0; i < x; i++)
        {
            mapSmoother.SmoothMap();
        }
    }

    private void ProcessMap()
    {
        AllocateWalls();
        AllocateRooms();
    }

    private void AllocateWalls()
    {
        List<List<Coord>> wallRegions = regionFinder.GetRegions(WALL);
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                RemovePillar(wallRegion);
            }
        }
    }

    private void RemovePillar(List<Coord> wallRegion)
    {
        foreach (Coord tile in wallRegion)
        {
            map[tile.tileX, tile.tileY] = FLOOR;
        }
    }

    private void AllocateRooms()
    {
        List<List<Coord>> roomRegions = regionFinder.GetRegions(FLOOR);
        List<Room> survivingRooms = new List<Room>();
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                RemoveCheeseHole(roomRegion);
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        ConnectSurvivingRooms(survivingRooms);
    }

    private void RemoveCheeseHole(List<Coord> roomRegion)
    {
        foreach (Coord tile in roomRegion)
        {
            map[tile.tileX, tile.tileY] = WALL;
        }
    }

    private void ConnectSurvivingRooms(List<Room> survivingRooms)
    {
        survivingRooms.Sort();
        survivingRooms[0].setIsMainRoom = true;
        survivingRooms[0].setIsAccessibleFromMainRoom = true;
        roomConnector.ConnectClosestRooms(survivingRooms);
    }

    private void CalculateBorderedMap()
    {
        borderedMap = new int[width + BORDER_SIZE * 2, height + BORDER_SIZE * 2];
        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                FillBorderedMapTileWithWall(x, y);
            }
        }
        meshGenerator.GenerateMesh(borderedMap, SQUARE_SIZE);
    }

    private void FillBorderedMapTileWithWall(int x, int y)
    {
        if (x >= BORDER_SIZE && x < width + BORDER_SIZE && y >= BORDER_SIZE && y < height + BORDER_SIZE)
        {
            borderedMap[x, y] = map[x - BORDER_SIZE, y - BORDER_SIZE];
        }
        else
        {
            borderedMap[x, y] = WALL;
        }
    }

    public bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public void SetMapTile(int rowIndex, int columnIndex, int value)
    {
        map[rowIndex, columnIndex] = value;
    }

    public int getWidth { get { return width; } }

    public int getHeight { get { return height; } }

    public int[,] getMap { get { return map; } }
}
