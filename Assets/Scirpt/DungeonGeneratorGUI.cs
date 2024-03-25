using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(AbstractDungeonGenerator), true)]
public class DungeonGeneratorGUI : Editor
{
    AbstractDungeonGenerator m_Target;
    private void Awake() 
    {
        m_Target = (AbstractDungeonGenerator)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Generate"))
        {
            m_Target.GenerateDungeon();
        }
    }
    
}
