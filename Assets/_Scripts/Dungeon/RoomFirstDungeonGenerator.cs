using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    PrepareDungeon prepDungeon;

    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;
    
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20;

    [SerializeField]
    [Range(0,10)]
    private int offset = 1;
    
    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

    private Dictionary<int, RoomType> finalRooms = new Dictionary<int, RoomType>();

    private enum RoomType
    {
        startRoom,
        bossRoom,
        merchentRoom,
        treasureRoom,
        generalRoom
    }

    RoomType[] roomTypeValues = (RoomType[])Enum.GetValues(typeof(RoomType));
    RoomType[] roomTypeRerollValues = {RoomType.treasureRoom, RoomType.generalRoom};

    public void CallPrep(Vector2Int roomCenter)
    {
       prepDungeon = GameObject.FindGameObjectWithTag("Prep").GetComponent<PrepareDungeon>();
       prepDungeon.PlayerToStart(roomCenter);
       prepDungeon.SpawnBoss();
    }

    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        floor = CreateRoomsRandomly(roomsList);
        
        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);
    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

       finalRooms.Add(0, RoomType.startRoom);
       ClearRoomInfo();
       finalRooms.Add(1, RoomType.bossRoom);
       ClearRoomInfo();
       finalRooms.Add(2, RoomType.merchentRoom);
       ClearRoomInfo();

        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
            foreach (var position in roomFloor)
            {
                if(position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) && position.y >= (roomBounds.yMin - offset) && position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
            if(i == 0)
                CallPrep(roomCenter);

            if(i >= 3)
                SetRoomType(roomFloor, roomCenter);
            
            SaveRoomData(roomCenter, roomFloor);
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destination.y)
        {
            if(destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if(destination.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position);
        }
        while (position.x != destination.x)
        {
            if (destination.x > position.x)
            {
                position += Vector2Int.right;
            }else if(destination.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
        }

        return corridor;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if(currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

   
    private void SaveRoomData(Vector2Int roomCenter, HashSet<Vector2Int> roomFloor)
    {
        roomsDictionary[roomCenter] = roomFloor;
    }

    public void SetRoomType(HashSet<Vector2Int> roomFloor, Vector2Int roomCenter)
    {
        int count = roomsDictionary.Count;

        RoomType randomRoomType = (RoomType)roomTypeRerollValues.GetValue(Random.Range(0, roomTypeRerollValues.Length));

        finalRooms.Add(count, randomRoomType);

        ClearRoomInfo();
    }
    public void ClearRoomInfo()
    {
        finalRooms = new Dictionary<int, RoomType>();
    }
}