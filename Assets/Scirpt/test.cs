using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class test : MonoBehaviour
{
    public GameObject obj,boje;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void HashSetDuplicate()
    {
        Vector3 forward = obj.transform.position.normalized;
        Vector3 other = (boje.transform.position - obj.transform.position).normalized;
        Debug.Log(Mathf.Round(Vector3.Dot(forward, other)));
    }
    // Update is called once per frame
    void Update()
    {
        HashSetDuplicate();
    }
}
