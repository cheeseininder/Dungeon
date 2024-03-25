using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
[Serializable]
public class Room
{
    public int RoomNumber;
    public Vector3 RoomSize;
    public Vector3 RoomCenter;
}
[Serializable]
public class DungeonLayer
{
    [HideInInspector]
    public HashSet<Vector3> floors = new HashSet<Vector3>();
    [HideInInspector]
    public HashSet<Vector3> corridors = new HashSet<Vector3>();
    [HideInInspector]
    public HashSet<Vector3> doors = new HashSet<Vector3>();
    [HideInInspector]
    public HashSet<Vector3> Stairs = new HashSet<Vector3>();
    public List<Room> rooms = new List<Room>();
}
public class RoomFirstDungeonGenerator : DungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomLength = 4;
    [SerializeField]
    private int dungeonWidth = 20, dungeonLength = 20;
    [SerializeField]
    private int Layers = 1, LayerHeight = 40, RoomHeight = 20, CorridorHeight = 20, CorridorWidth;
    [SerializeField]
    [Range(0, 10)]
    private int offset = 1;
    [SerializeField]
    private bool randomWalkRooms = false;
    public bool debugOn = false;
    public List<DungeonLayer> dungeonLayers = new List<DungeonLayer>();
    private List<HashSet<Vector3>> flor = new List<HashSet<Vector3>>();
    private List<Vector3> corrido = new List<Vector3>();
    private List<HashSet<Vector3>> doors = new List<HashSet<Vector3>>();
    public List<Vector3> center = new List<Vector3>();
    public List<Room> roomDetail = new List<Room>();
    private List<List<Vector3>> stairP = new List<List<Vector3>>();
    private List<HashSet<Vector3>> stairs = new List<HashSet<Vector3>>();
    protected override void RunPCG()
    {
        createRooms();
    }
    private void createRooms()
    {
        CleanOBJ();
        roomDetail.Clear();
        center.Clear();
        doors.Clear();
        corrido.Clear();
        flor.Clear();
        stairP.Clear();
        dungeonLayers.Clear();
        stairs.Clear();
        for (int i = 0; i < Layers; i++)
        {
            
            DungeonLayer dungeonLayer = new DungeonLayer();
            dungeonLayers.Add(dungeonLayer);
            Vector3Int start = Vector3Int.FloorToInt(startP + new Vector3(0, i * RoomHeight, 0));
            var roomsList = PCGRoom.BinarySpacePartitioning(new BoundsInt(start, 
            new Vector3Int(dungeonWidth, 0, dungeonLength))
            , minRoomWidth, minRoomLength, i * LayerHeight);
            
            HashSet<Vector3> floor = new HashSet<Vector3>();            

            floor = createSimpleRooms(roomsList);

            List<Room> rooms = new List<Room>();
            
            List<Vector3> roomSize = new List<Vector3>();
            List<Vector3> roomCenters = new List<Vector3>();
            for (int e = 0; e < roomsList.Count; e++)
            {
                Room roo1 = new Room();
                roomCenters.Add(roomsList[e].center);
                roomSize.Add(roomsList[e].size);
                roo1.RoomCenter = roomsList[e].center;
                roo1.RoomSize = roomsList[e].size;
                roo1.RoomNumber = e + 1;
                rooms.Add(roo1);
            }
            dungeonLayers[i].rooms = rooms;
            
            HashSet<Vector3> corridors = ConnectRooms(roomCenters, roomSize);
            

            HashSet<Vector3> doorlist = creatDoor(corridors, floor);
            dungeonLayers[i].doors = doorlist;

            dungeonLayers[i].floors = floor;
            
            doors.Add(doorlist);
            flor.Add(floor);
            
            corridors.ExceptWith(floor);
            foreach (var item in corridors)
            {
                corrido.Add(item);
            }
            List<Vector3> stairPoint = new List<Vector3>();
            stairPoint = RandomStairPoint(corridors, floor, rooms);

            stairP.Add(stairPoint);
            

            HashSet<Vector3> stairfloor = new HashSet<Vector3>();

            dungeonLayers[i].corridors = corridors;
            if (i >= 1)
            {
                List<Vector3> roomsCenter = new List<Vector3>();
                foreach (var item in rooms)
                {
                    roomsCenter.Add(item.RoomCenter);
                }
                
                HashSet<Vector3> Stairs = ConnectStairToRooms(roomsCenter, stairP[i - 1], dungeonLayers[i - 1].corridors, dungeonLayers[i].corridors, dungeonLayers[i].floors);

                stairfloor.UnionWith(Stairs);  
                
                stairs.Add(stairfloor);

            }
            if(!debugOn)
            {
                CreateFloor("Corridors",corridors, FloorObject, parent);
                CreateFloor("floor", floor, FloorObject, parent);

                HashSet<Vector3> RoomCeiling = createCeiling(floor, RoomHeight);
                HashSet<Vector3> CorridorCeiling = createCeiling(corridors, CorridorHeight);
                CreateCeiling("CeilingRoom", RoomCeiling, CeilingObject, parent);
                CreateCeiling("CeilingCorridor", CorridorCeiling, CeilingObject, parent);

                Dictionary<Vector3, Quaternion> wallFloor = PCGWall.WallandDoor(floor, doorlist, RoomHeight);
                Dictionary<Vector3, Quaternion> wallcorridors = PCGWall.WallandDoor(corridors, doorlist, CorridorHeight);
                var f = wallFloor.ToHashSet();
                var c = wallcorridors.ToHashSet();
                f.UnionWith(c);
                var Combine = f.ToDictionary(i => i.Key, i => i.Value);
                CreateWallandDoor("wall", Combine, WallObject, parent);
                CreatePillar("Pillar", Combine, PillarObject, parent);
            }
        }
        
        //overlap.IntersectWith(corridors);
        //same = overlap;
        
        
    }
    private HashSet<Vector3> createCeiling(HashSet<Vector3> floor, int roomHeight)
    {
        HashSet<Vector3> ceiling = new HashSet<Vector3>();
        foreach (var item in floor)
        {
            var position = item + new Vector3(0, roomHeight, 0);
            ceiling.Add(position);
        }
        return ceiling;
    }

    private HashSet<Vector3> ConnectStairToRooms(List<Vector3> rooms, List<Vector3> stairPoint, HashSet<Vector3> Corridors, HashSet<Vector3> UpCorridors, HashSet<Vector3> UpFloor)
    {
        HashSet<Vector3> stairs = new HashSet<Vector3>();
        for (int i = 0; i < stairPoint.Count; i++)
        {
            Vector3 stairP = stairPoint[i];
            Vector3 closeCenter = FindClosePoint(stairP, rooms);
            HashSet<Vector3> newStairs = CreateStair(closeCenter, stairP, Corridors, UpCorridors, UpFloor);
            stairs.UnionWith(newStairs);
        }
        
        
        return stairs;
    }

    private HashSet<Vector3> CreateStair(Vector3 roomCenter, Vector3 stairStartPoint, HashSet<Vector3> Corridors, HashSet<Vector3> UpCorridors, HashSet<Vector3> UpFloor)
    {
        HashSet<Vector3> stairs = new HashSet<Vector3>();
        
        var position = stairStartPoint;
        var closet = Direction.AddRandomDireciton(roomCenter, 5);
        var start = 0;
        var NotCreate = 0;
        var Create = new int[4];
        var last = 1;
        while (start < Direction.StairList.Count)
        {
            var contains = position + Direction.StairList[start];
            if (UpCorridors.Contains(position))
            {
                return stairs;
            }
            if (Corridors.Contains(contains) | UpCorridors.Contains(contains + new Vector3(0, RoomHeight - 10, 0)))
            {
                Create[last - 1] = 0;
                NotCreate++;
            }
            if (!Corridors.Contains(contains) && !UpCorridors.Contains(contains + new Vector3(0, RoomHeight - 10, 0)))
            {
                Create[start] = 1;
            }
            last++;
            start++;
        }
        if (NotCreate >= 2)
        {   
            return stairs;
        }
        start = Array.IndexOf(Create, 1);
        while (position.y != closet.y)
        {
            
            position = position + Direction.StairList[start];
            position.y = position.y < closet.y ? position.y += 10 : position.y = closet.y;
            stairs.Add(position);
            if (start == 3)
            {
                start = 0;
            }
            start++;
        }
        
        while (position.x != closet.x && !UpCorridors.Contains(position) && !UpFloor.Contains(position))
        {
            position = Direction.CalculatePosition(position, closet, Direction.xPosition);
            
            stairs.Add(position);
        }
        while (position.z != closet.z && !UpCorridors.Contains(position) && !UpFloor.Contains(position))
        {
            position = Direction.CalculatePosition(position, closet, Direction.zPosition);
            
            stairs.Add(position);
        }
        

        return stairs;
    }

    private List<Vector3> RandomStairPoint(HashSet<Vector3> corridors, HashSet<Vector3> floor, List<Room> rooms)
    {
        List<Vector3> Hallway = new List<Vector3>();
        List<Vector3> Stairs = new List<Vector3>();

        int randomCorridors = 0;
        foreach (var corridor in corridors)
        {
            Hallway.Add(corridor);
        }
        int count = 0;
        Vector3 lastPosition;
        for (int i = 1; i < Hallway.Count; i++)
        {
            lastPosition = Hallway[i - 1];
            
            if (Vector3.Distance(lastPosition, Hallway[i]) == 10)
            {
                count += 1;
            }
            else if(count > 6)
            {
                int min = 4;
                int max = count - 3;
                int possiblePoint = max - min + 1;
                var position = Vector3.zero;
                bool IsMatch = false;
                randomCorridors = Random.value > 0.5f ? max : min;
                while (possiblePoint >= 0 || !IsMatch)
                {
                    
                    if (!FloorIsNearBy(floor, Hallway[i - randomCorridors], 1))
                    {
                        IsMatch = true;
                        int[] Near = new int[8];
                        
                        for (int e = 0; e < Direction.FullcardinalDirectionList.Count; e++)
                        {
                            Vector3 pos = Hallway[i - randomCorridors] + Direction.FullcardinalDirectionList[e];
                            if (Hallway.Contains(pos))
                            {
                                Near[e] = 1;
                            }
                        }
                        
                        if (Near[0] == 1 && Near[4] == 1 | (Near[5] == 1 || Near[7] == 1))
                        {
                            position = Hallway[i - randomCorridors] + new Vector3(0, 0 ,10); 
                        }
                        else if (Near[0] == 1 && Near[4] == 1 | (Near[1] == 1 || Near[3] == 1))
                        {
                            position = Hallway[i - randomCorridors] + new Vector3(0, 0 ,-10); 
                        }
                        else if (Near[2] == 1 && Near[6] == 1 | (Near[5] == 1 || Near[3] == 1))
                        {
                            position = Hallway[i - randomCorridors] + new Vector3(10, 0 ,0); 
                        }
                        else if (Near[2] == 1 && Near[6] == 1 | (Near[1] == 1 || Near[7] == 1))
                        {
                            position = Hallway[i - randomCorridors] + new Vector3(-10, 0 ,0); 
                        }

                        if (!FloorIsNearBy(floor, position, 1) && position != Vector3.zero)
                        {
                            Stairs.Add(position);
                            count = 0;
                            break;
                        }
                        else
                        {
                            count = 0;
                            break;
                        }
                    }
                    if (max - randomCorridors == 0)
                    {
                        max--;                        
                    }
                    else if(min - randomCorridors == 0)
                    {
                        min++;
                    }

                    possiblePoint--;

                    randomCorridors = Random.value > 0.5f ? max : min;
                }
            }
            else
            {
                count = 0;
            }

        }
        if (Stairs.Count <= 1)
        {

            IEnumerable<Room> BigRoom = rooms.Where(roomSize => roomSize.RoomSize.x >= 60 && roomSize.RoomSize.z >= 60 );
            int amount = 0;
            foreach (var room in BigRoom)
            {
                if (Random.value > 0.5f)
                {
                    var position = Direction.AddRandomDireciton(room.RoomCenter, 5);
                    Stairs.Add(position);
                    amount++;
                }
                if (amount == 3)
                {
                    return Stairs;
                }
            }
        }
        return Stairs;
    }

    private HashSet<Vector3> ConnectRooms(List<Vector3> roomCenters, List<Vector3> roomSize)
    {
        HashSet<Vector3> corridors = new HashSet<Vector3>();
        var roomsCenter = roomCenters;
        int RandomRoom = Random.Range(0, roomsCenter.Count);
        var roomsSize = roomSize;
        
        var currentRoomCenter = roomsCenter[RandomRoom];
        var currentRoomSize = roomsSize[RandomRoom];
        roomsCenter.Remove(currentRoomCenter);
        roomsSize.Remove(currentRoomSize);
        
        while (roomsCenter.Count > 0)
        {
            Vector3 closeCenter = FindClosePoint(currentRoomCenter, roomsCenter);
            Vector3 closeRoomSize = roomsSize[roomsCenter.IndexOf(closeCenter)];
            roomsCenter.Remove(closeCenter);
            roomsSize.Remove(closeRoomSize);
            HashSet<Vector3> newCorridor = createCorridor(currentRoomCenter, closeCenter, currentRoomSize, closeRoomSize);
            currentRoomCenter = closeCenter;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }
    private bool FloorIsNearBy(HashSet<Vector3> list, Vector3 floor, int CheckAmount)
    {
        int Amount = 0;
        for (int i = 0; i < Direction.cardinalDirectionList.Count; i++)
        {
            if (list.Contains(floor + Direction.cardinalDirectionList[i]))
            {
                Amount++;
            }
            if(Amount == CheckAmount)
            {
                return true;
            }
        } 
        return false; 
    }
    private HashSet<Vector3> creatDoor(HashSet<Vector3> corridors, HashSet<Vector3> floor)
    {
        HashSet<Vector3> doors = new HashSet<Vector3>();
        foreach (var corrido in corridors)
        {
            if (floor.Contains(corrido))
            {
                for (int i = 0; i < Direction.cardinalDirectionList.Count; i++)
                {
                    Vector3 position = corrido + Direction.cardinalDirectionList[i];
                    if(!floor.Contains(position) && corridors.Contains(position))
                    {
                        doors.Add(corrido + (Direction.cardinalDirectionList[i] / 2));
                    }
                }
            }
        }
        foreach (var door in doors)
        {
            int count = 0;
            int[] Near = new int[4]; 
            for (int i = 0; i < Direction.cardinalDirectionList.Count; i++)
            {
                Vector3 position = door + Direction.cardinalDirectionList[i];
                if (doors.Contains(position))
                {
                    count++;
                    Near[i] = count;
                    count = 0;
                }
            }
            if ((Near[1] == 1 && Near[0] == 1) || (Near[2] == 1 && Near[3] == 1))
            {
                doors.Remove(door);
            }
            
        }
        return doors;
    }
    private HashSet<Vector3> createCorridor(Vector3 currentRoomCenter, Vector3 closeCenter, Vector3 currentRoomSize, Vector3 closeRoomsize)
    {
        HashSet<Vector3> corridor = new HashSet<Vector3>();
        var position = Direction.AddRandomDireciton(currentRoomCenter, 5);
        var closet = Direction.AddRandomDireciton(closeCenter, 5);    
        
        center.Add(closet);
        corridor.Add(position);
        
        while (position.x != closet.x)
        {
            position = Direction.CalculatePosition(position, closet, Direction.xPosition);
            corridor.Add(position);
        }

        while (position.z != closet.z)
        {
            position = Direction.CalculatePosition(position, closet, Direction.zPosition);
            corridor.Add(position);
        }
        return corridor;
    }

    private Vector3 FindClosePoint(Vector3 currentRoomCenter, List<Vector3> roomCenters)
    {
        Vector3 closet = Vector3.zero;
        float Length = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector3.Distance(position, currentRoomCenter);
            if (currentDistance < Length)
            {
                Length = currentDistance;
                closet = position;
            }
        }
        return closet;
    }

    private HashSet<Vector3> createSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector3> floor = new HashSet<Vector3>();

        for (int i = 0; i < roomsList.Count; i++)
        {
            Room eachroom = new Room();
            eachroom.RoomNumber = i;
            eachroom.RoomSize = roomsList[i].size;
            eachroom.RoomCenter = roomsList[i].center;
            roomDetail.Add(eachroom);
        }
        foreach (var room in roomsList)
        {
            for (int col = offset ; col < room.size.x / 10; col++)
            {
                for (int row = offset; row < room.size.z / 10; row++)
                {
                    Vector3 position = (Vector3)room.min + new Vector3(col * 10, 0, row * 10);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }
    private void OnDrawGizmos() {
        if (debugOn)
        {
            for (int i = 0; i < flor.Count; i++)
            {
                foreach (var item in flor[i])
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(item, new Vector3(10, 0, 10));
                }
            }
            foreach (var item in center)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(item, new Vector3(10, 10, 10));
            }
            foreach (var item in corrido)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(item, new Vector3(10,0,10));
            }
            for (int i = 0; i < doors.Count; i++)
            {
                foreach (var item in doors[i])
                {
                    Gizmos.color = Color.blue;
                    Vector3 s = item + new Vector3(0, 5, 0);
                    Gizmos.DrawCube(s, new Vector3(1,1,1));
                }
            }
            for (int i = 0; i < stairP.Count; i++)
            {
                foreach (var item in stairP[i])
                {
                    Gizmos.color = Color.gray;
                    Gizmos.DrawCube(item, new Vector3(10,1,10));
                }
            }
            for (int i = 0; i < stairs.Count; i++)
            {
                foreach (var item in stairs[i])
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawCube(item, new Vector3(10,1,10));
                }
            }
        }
        
    }
}
