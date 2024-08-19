using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CountEffects : EditorWindow
{
    [MenuItem("Tools/Count Passive Effects")]
    public static void ShowWindow()
    {
        GetWindow<CountEffects>("Count Passive Effects");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Count Passive Effects"))
        {
            CountPassiveEffects();
        }
    }

    private void CountPassiveEffects()
    {
        // "t:EffectInf" を検索し、そのファイル名に "New Passive Effect" が含まれるもののみをカウント
        string[] guids = AssetDatabase.FindAssets("t:EffectInf");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EffectInf effect = AssetDatabase.LoadAssetAtPath<EffectInf>(path);
            if (effect != null && path.Contains("New Draw Card"))
            {
                count++;
            }
        }
        Debug.Log("Number of 'New BuffAttack FieldCards' effects: " + count);
    }
}
