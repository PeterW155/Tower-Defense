using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerTypeButtons : MonoBehaviour
{
    public TowerEditor towerEditor;
    public Toggle startToggleDefault;
    [SerializeField]
    ButtonGameObjectDictionary serializableButtonGameObject;
    public IDictionary<Toggle, GameObject> buttonGameObject
    {
        get { return serializableButtonGameObject; }
        set { serializableButtonGameObject.CopyFrom(value); }
    }

    private void Awake()
    {
        foreach (var entry in buttonGameObject)
        {
            entry.Key.onValueChanged.AddListener((b) => ChangeTowerType(entry.Value));
        }
    }
    private void Start()
    {
        startToggleDefault.isOn = true;
        ChangeTowerType(buttonGameObject[startToggleDefault]);
    }

    public void ChangeTowerType(GameObject tower)
    {
        towerEditor.NewSelectedTower(tower);
    }
}
