using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorDungeonGenerator : DungeonGenerator
{
    [SerializeField]
    private int corridorLength = 14, corridorCount = 5, Width = 2;
    [SerializeField]
    [Range(0.1f, 1)]
    private float roomPercent;

    protected override void RunPCG()
    {
        corridorGeneration();
    }

    private void corridorGeneration()
    {
        HashSet<Vector3> floorP = new HashSet<Vector3>();
        HashSet<Vector3> potentialRoomPositions = new HashSet<Vector3>();
        HashSet<Vector3> CorridorsWidth = new HashSet<Vector3>();

        CleanOBJ();
        //First Create Corridors
        CreateCorridors(floorP, potentialRoomPositions);

        //Get Corridors To Create different RoomP
        HashSet<Vector3> roomPositions = creatRoom(potentialRoomPositions);

        List<Vector3> deadEnds = FindAllDeadEnds(floorP);

        CreatRoomsAtDeadEnd(deadEnds, roomPositions);

        Dictionary<Vector3, int[]> WidthCorridors = IncreasingCorridorsWidth(floorP);

        CreatCorridorsWidth(WidthCorridors, CorridorsWidth);
        floorP.UnionWith(CorridorsWidth);
        //Get UnionWith FloorP and RoomP
        floorP.UnionWith(roomPositions);

        CreateFloor("floor",floorP, FloorObject, parent);

        CreateWall("wall",floorP, WallObject, parent);
    }

    private void CreatCorridorsWidth(Dictionary<Vector3, int[]> widthCorridors, HashSet<Vector3> Corridors)
    {

        foreach (var item in widthCorridors)
        {
            for (int i = 0; i < Direction.cardinalDirectionList.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        if (item.Value[i] == 1)
                        {
                            int amount = 1;
                            while (amount <= Width)
                            {
                                Vector3 direction = item.Key + Direction.cardinalDirectionList[3] * amount;
                                Corridors.Add(direction);
                                amount++;
                            }
                        }
                    break;
                    case 1:
                        if (item.Value[i] == 1)
                        {
                            int amount = 1;
                            while (amount <= Width)
                            {
                                Vector3 direction = item.Key + Direction.cardinalDirectionList[2] * amount;
                                Corridors.Add(direction);
                                amount++;
                            }
                        }
                    break;
                    case 2:
                        if (item.Value[i] == 1)
                        {
                            int amount = 1;
                            while (amount <= Width)
                            {
                                Vector3 direction = item.Key + Direction.cardinalDirectionList[1] * amount;
                                Corridors.Add(direction);
                                amount++;
                            }
                        }
                    break;
                    case 3:
                        if (item.Value[i] == 1)
                        {
                            int amount = 1;
                            while (amount <= Width)
                            {
                                Vector3 direction = item.Key + Direction.cardinalDirectionList[0] * amount;
                                Corridors.Add(direction);
                                amount++;
                            }
                        }
                    break;
                }
            }
        }
    }
    
    private Dictionary<Vector3, int[]> IncreasingCorridorsWidth(HashSet<Vector3> floorP)
    {
        Dictionary<Vector3, int[]> Widthcorridors = new Dictionary<Vector3, int[]>();

        foreach (var corridor in floorP)
        {
            int neighboursCount = 0;
            int[] Sum = new int[4];
            for (int i = 0; i < Direction.cardinalDirectionList.Count; i++)
            {
                if(floorP.Contains(corridor + Direction.cardinalDirectionList[i]))
                {
                    neighboursCount ++;
                    if(neighboursCount == 3)
                    {
                        break;
                    }
                    Sum[i] = 1;
                }
            }

            if (neighboursCount == 2)
            {
                Widthcorridors.Add(corridor, Sum);
            }
        }

        return Widthcorridors;
    }

    private void CreatRoomsAtDeadEnd(List<Vector3> deadEnds, HashSet<Vector3> roomPositions)
    {
        foreach (var position in deadEnds)
        {
            if (!roomPositions.Contains(position))
            {
                var roomFloor = RunRandomlyWalk(simpleWalkSO, position);
                roomPositions.UnionWith(roomFloor);
            }
        }
    }

    private List<Vector3> FindAllDeadEnds(HashSet<Vector3> floorPosition)
    {
        List<Vector3> deadEnds = new List<Vector3>();
        foreach (var position in floorPosition)
        {
            int neighboursCount = 0;
            foreach (var direction in Direction.cardinalDirectionList)
            {
                if (floorPosition.Contains(position + direction))
                {
                    neighboursCount ++;
                }
            }
            if (neighboursCount == 1)
            {
                deadEnds.Add(position);
            }
        }

        return deadEnds;
    }

    private HashSet<Vector3> creatRoom(HashSet<Vector3> potentialRoomPositions)
    {
        HashSet<Vector3> roomPositions = new HashSet<Vector3>();
        int roomToCreatCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);

        List<Vector3> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreatCount).ToList();

        foreach (var roomPosition in roomsToCreate)
        {
            var roomFloor = RunRandomlyWalk(simpleWalkSO, roomPosition);
            roomPositions.UnionWith(roomFloor);
        }

        return roomPositions;
    }

    private void CreateCorridors(HashSet<Vector3> floorP, HashSet<Vector3> potentialRoomPositions)
    {
        var currentP = this.transform.position;
        potentialRoomPositions.Add(currentP);

        for (int i = 0; i < corridorCount; i++)
        {
            var corridor = PCGFloor.RandomWalkCorridor(currentP, corridorLength);
            currentP = corridor[corridor.Count - 1];
            potentialRoomPositions.Add(currentP);
            floorP.UnionWith(corridor);
        }
    }
}
