using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField]
    protected Vector3 startP = Vector3.zero;

    public void GenerateDungeon()
    {
        RunPCG();
    }

    protected abstract void RunPCG();
}
