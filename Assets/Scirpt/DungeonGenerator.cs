using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
[ExecuteInEditMode]


public class DungeonGenerator : AbstractDungeonGenerator
{
    public GameObject FloorObject;
    public GameObject PillarObject;
    public GameObject WallObject;
    public GameObject CeilingObject;
    public Transform parent;
    [SerializeField]
    protected SimpleWalkSO simpleWalkSO;
    
    // Start is called before the first frame update
    protected override void RunPCG()
    {
        CleanOBJ();
        
        HashSet<Vector3> floorP = RunRandomlyWalk(simpleWalkSO, startP);
        
        CreateFloor("floor",floorP, FloorObject, parent);

        CreateWall("wall",floorP, WallObject, parent);

    }
    protected HashSet<Vector3> RunRandomlyWalk(SimpleWalkSO parameters, Vector3 position)
    {
        var currentP = position;
        HashSet<Vector3> floorP = new HashSet<Vector3>();

        for (int i = 0; i < parameters.iterations; i++)
        {
            var path = PCGFloor.SimpleRandomWalk(currentP, parameters.walkLength);
            floorP.UnionWith(path);
            if (parameters.startRandomlyEachiterations)
            {
                currentP = floorP.ElementAt(Random.Range(0, floorP.Count));
            }
        }

        return floorP;
    }

    public void CreateFloor(string Name, HashSet<Vector3> floorP, GameObject Object, Transform parent)
    {
        int i = 0;
        foreach (var item in floorP)
        {
            i++;
            GameObject obj = Instantiate(Object, item, this.transform.rotation, parent);
            obj.name = Name + "_" + i.ToString() + item.ToString();
        }
    }
    public void CreateCeiling(string Name, HashSet<Vector3> Ceiling, GameObject Object, Transform parent)
    {
        int i = 0;
        foreach (var item in Ceiling)
        {
            i++;
            GameObject obj = Instantiate(Object, item, this.transform.rotation, parent);
            obj.name = Name + "_" + i.ToString() + item.ToString();
        }
    }

    public void CreateWall(string Name, HashSet<Vector3> floorP, GameObject Object, Transform parent)
    {
        Dictionary<Vector3, Quaternion> wall = new Dictionary<Vector3, Quaternion>();

        wall = PCGWall.Walls(floorP);
        
        foreach (var item in wall)
        {
            GameObject obj = Instantiate(Object, item.Key, item.Value, parent);
            obj.name = Name + item.Key.ToString();
        }
    }
    public void CreateWallandDoor(string Name, Dictionary<Vector3, Quaternion> walls, GameObject Object, Transform parent)
    {
        Dictionary<Vector3, Quaternion> wall = new Dictionary<Vector3, Quaternion>();

        wall =  walls;
        
        foreach (var item in wall)
        {
            GameObject obj = Instantiate(Object, item.Key, item.Value, parent);
            obj.name = Name + item.Key.ToString();
        }
    }
    public void CreatePillar(string Name, Dictionary<Vector3, Quaternion> wall, GameObject Object, Transform parent)
    {
        HashSet<Vector3> pillar = PCGWall.Pillar(wall);

        foreach (var item in pillar)
        {
            GameObject obj = Instantiate(Object, item, this.transform.rotation, parent);
            obj.name = Name + item.ToString();
        }
    }
    public void CleanOBJ()
    {
        var tempArray = new GameObject[parent.transform.childCount];

        for(int i = 0; i < tempArray.Length; i++)
        {
            tempArray[i] = parent.transform.GetChild(i).gameObject;
        }

        foreach(var child in tempArray)
        {
            DestroyImmediate(child);
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        startP = this.transform.position;
    }
}
