using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
public static class PCGFloor
{
    public static HashSet<Vector3> SimpleRandomWalk(Vector3 startP, int walkLength)
    {
        HashSet<Vector3> path = new HashSet<Vector3>();

        path.Add(startP);
        var previousP = startP;

        for (int i = 0; i < walkLength; i++)
        {
            var newP = previousP + Direction.GetRandomCardinalDireciotn();
            path.Add(newP);
            previousP = newP;
        }

        return path;
    }

    public static List<Vector3> RandomWalkCorridor(Vector3 startP, int corridorLength)
    {
        List<Vector3> corridor = new List<Vector3>();

        var directoin = Direction.GetRandomCardinalDireciotn();
        var currentP = startP;
        corridor.Add(currentP);

        for (int i = 0; i < corridorLength; i++)
        {
            currentP += directoin;
            corridor.Add(currentP);
        }

        return corridor;
    }
}
public static class PCGRoom
{
    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWitdth, int minLength, int height)
    {
        
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit);
        while(roomsQueue.Count > 0)
        {
            var room = roomsQueue.Dequeue();
            room.y = height;
            if(room.size.z >= minLength && room.size.x >= minWitdth)
            {
                if (Random.value < 0.5f)
                {
                    //Split it by Horizontally
                    if(room.size.z >= minLength * 2)
                    {
                        SplitHorizontally(minLength, roomsQueue, room);
                    }
                    else if(room.size.x >= minWitdth * 2)
                    {
                        SpliVertically(minWitdth, roomsQueue, room);
                    }
                    else if(room.size.x >= minWitdth && room.size.z >= minLength)
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {
                    //Split it by Vertically
                    if(room.size.x >= minWitdth * 2)
                    {
                        SpliVertically(minWitdth, roomsQueue, room);
                    }
                    else if(room.size.z >= minLength * 2)
                    {
                        SplitHorizontally(minLength, roomsQueue, room);
                    }
                    else if(room.size.x >= minWitdth && room.size.z >= minLength)
                    {
                        roomsList.Add(room);
                    }
                }
            } 
        }
        return roomsList;
    }
    
    private static void SpliVertically(int minWitdth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        int split = (room.size.x / 10) - 2;

        int[] RandomSplits = new int[split];
        RandomSplits[0] = 10;
        for (int i = 1; i < RandomSplits.Length - 2; i++)
        {
            RandomSplits[i] = RandomSplits[0] + 10 * i;
        }
        
        var xSplit = RandomSplits[Random.Range(0, RandomSplits.Length - 2)];
        
        

        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(xSplit, 0, room.size.z));
        roomsQueue.Enqueue(room1);

        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
            new Vector3Int(room.size.x - xSplit, 0, room.size.z));
        roomsQueue.Enqueue(room2);

    }

    private static void SplitHorizontally(int minLength, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        int split = (room.size.z / 10) - 2;

        int[] RandomSplits = new int[split];
        RandomSplits[0] = 10;
        for (int i = 1; i < RandomSplits.Length - 2; i++)
        {
            RandomSplits[i] = RandomSplits[0] + 10 * i;
        }
        
        var zSplit = RandomSplits[Random.Range(0, RandomSplits.Length - 2)];
        
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, 0, zSplit));
        roomsQueue.Enqueue(room1);
        

        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y, room.min.z + zSplit),
            new Vector3Int(room.size.x, 0, room.size.z  - zSplit));
        roomsQueue.Enqueue(room2);
    }
}
public static class PCGWall
{
    public static Dictionary<Vector3, Quaternion> Walls(HashSet<Vector3> path)
    {
        Dictionary<Vector3, Quaternion> wall = new Dictionary<Vector3, Quaternion>();
        
        foreach (var item in path)
        {
            for (int i = 0; i < Direction.cardinalDirectionList.Count; i++)
            {
                Vector3 EmptyFloor = item + Direction.cardinalDirectionList[i];
                if (!path.Contains(EmptyFloor))
                {
                    Vector3 WallP = item + WallPlace[i];
                    wall.Add(WallP, WallRotate[i]);
                }
            }
        }

        return wall;
    }
    public static Dictionary<Vector3, Quaternion> WallandDoor(HashSet<Vector3> path, HashSet<Vector3> doors, int Height)
    {
        Dictionary<Vector3, Quaternion> wall = new Dictionary<Vector3, Quaternion>();
        int count = Height / 10;
        foreach (var item in path)
        {
            for (int i = 0; i < Direction.cardinalDirectionList.Count; i++)
            {
                Vector3 EmptyFloor = item + Direction.cardinalDirectionList[i];
                Vector3 DoorP = item + (Direction.cardinalDirectionList[i] / 2);
                if (!path.Contains(EmptyFloor) && !doors.Contains(DoorP))
                {
                    for (int e = 0; e < count; e++)
                    {
                        Vector3 WallP = item + WallPlace[i] + new Vector3(0, e * 10, 0);
                        wall.Add(WallP, WallRotate[i]); 
                    }
                }
            }
        }

        return wall;
    }
    public static HashSet<Vector3> Pillar(Dictionary<Vector3, Quaternion> wall)
    {
        HashSet<Vector3> pillar = new HashSet<Vector3>();
        var list = wall.ToList();

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Value == Quaternion.Euler(0, 90, 0))
            {
                var pillarL = list[i].Key + new Vector3(0, 0, 5);
                pillar.Add(pillarL);
                var pillarR = list[i].Key + new Vector3(0, 0, -5);
                pillar.Add(pillarR);
            }
            else
            {
                var pillarL = list[i].Key + new Vector3(5, 0, 0);
                pillar.Add(pillarL);
                var pillarR = list[i].Key + new Vector3(-5, 0, 0);
                pillar.Add(pillarR);
            }
        }
        return pillar;
    }
    public static List<Vector3> WallPlace = new List<Vector3>()
    {
        new Vector3(5, 5, 0),
        new Vector3(-5, 5, 0),
        new Vector3(0, 5, 5),
        new Vector3(0, 5, -5),
    };
    public static List<Quaternion> WallRotate = new List<Quaternion>()
    {
        Quaternion.Euler(0, 90, 0),
        Quaternion.Euler(0, 90, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 0, 0),
    };
}

public delegate Vector3 AddRandomDireciton();

public static class Direction
{
    public delegate Vector3 Calculate(Vector3 Start, Vector3 Destinaiotn);
    public static float CalculateDireciton(Vector3 forward, Vector3 other)
    {
        
        forward = forward.normalized;
        other = other.normalized;
        var directoin = Mathf.Round(Vector3.Dot(forward, other));
        return directoin;
    }
    public static Vector3 CalculatePosition(Vector3 Start, Vector3 Destinaiotn, Calculate calculate)
    {
        var result = calculate(Start, Destinaiotn);
        return result;
    }
    public static Vector3 xPosition(Vector3 Start, Vector3 Destinaition)
    {
        Vector3 result = Vector3.zero;

        result = Start.x < Destinaition.x ? Start += new Vector3(10, 0, 0) : Start += new Vector3(-10, 0, 0);

        return result;
    }
    public static Vector3 zPosition(Vector3 Start, Vector3 Destinaition)
    {
        Vector3 result = Vector3.zero;

        result = Start.z < Destinaition.z ? Start += new Vector3(0, 0, 10) : Start += new Vector3(0, 0, -10);

        return result;
    }
    public static Vector3 AddRandomDireciton(Vector3 position, int Length)
    {
        int directoin;
        Vector3 p = position;
        directoin = Random.value > 0.5f ? directoin = -Length : directoin = Length;
        
        p.x = p.x % 10 != 0 ? p.x += directoin : p.x;
        p.z = p.z % 10 != 0 ? p.z += directoin : p.z;
        return p;
    }
    public static List<Vector3> StairList = new List<Vector3>()
    {
        new Vector3(10, 0, 0),
        new Vector3(0, 0, 10),
        new Vector3(-10, 0, 0),
        new Vector3(0, 0, -10),
    };
    public static List<Vector3> cardinalDirectionList = new List<Vector3>()
    {
        new Vector3(10, 0, 0),
        new Vector3(-10, 0, 0),
        new Vector3(0, 0, 10),
        new Vector3(0, 0, -10),
    };
    public static List<Vector3> FullcardinalDirectionList = new List<Vector3>()
    {
        new Vector3(10, 0, 0),
        new Vector3(10, 0, 10),
        new Vector3(0, 0, 10),
        new Vector3(-10, 0, 10),
        new Vector3(-10, 0, 0),
        new Vector3(-10, 0, -10),
        new Vector3(0, 0, -10),
        new Vector3(10, 0, -10),
    };

    public static Vector3 GetRandomCardinalDireciotn()
    {
        return cardinalDirectionList[Random.Range(0, cardinalDirectionList.Count)];
    }
}
