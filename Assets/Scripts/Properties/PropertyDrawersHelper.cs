using System.Collections.Generic;
using UnityEditor;
using UnityEngine.InputSystem;

public static class PropertyDrawersHelper
{
#if UNITY_EDITOR
    public static string[] AllActionMaps()
    {
        var temp = new List<string>();
        InputActionAsset playerInputs = (InputActionAsset)AssetDatabase.LoadAssetAtPath("Assets/Input/PlayerInput.inputactions", typeof(InputActionAsset));
        foreach (var actionMap in playerInputs.actionMaps)
        {
             temp.Add(actionMap.name);
        }
        return temp.ToArray();
    }
    public static string[] AllPlayerInputs()
    {
        var temp = new List<string>();
        InputActionAsset playerInputs = (InputActionAsset)AssetDatabase.LoadAssetAtPath("Assets/Input/PlayerInput.inputactions", typeof(InputActionAsset));
        foreach (var actionMap in playerInputs.actionMaps)
        {
            foreach (var binding in actionMap.bindings)
            {
                temp.Add(actionMap.name + "/" + binding.action);
            } 
        }
        return temp.ToArray();
    }
    public static string[] AllSceneNames()
    {
        var temp = new List<string>();
        foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
        {
            if (S.enabled)
            {
                string name = S.path.Substring(S.path.LastIndexOf('/') + 1);
                name = name.Substring(0, name.Length - 6);
                temp.Add(name);
            }
        }
        return temp.ToArray();
    }

#endif
}