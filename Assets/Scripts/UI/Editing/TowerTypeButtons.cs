using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerTypeButtons : MonoBehaviour
{
    private TowerEditor towerEditor;
    public Toggle startToggleDefault;
    [SerializeField]
    ButtonGameObjectDictionary serializableButtonGameObject;
    public IDictionary<Toggle, GameObject> buttonGameObject
    {
        get { return serializableButtonGameObject; }
        set { serializableButtonGameObject.CopyFrom(value); }
    }

    private void OnEnable()
    {
        towerEditor = TowerEditor.Instance;

        UpdateText();

        foreach (var entry in buttonGameObject)
        {
            entry.Key.onValueChanged.AddListener((b) => ChangeTowerType(entry.Value));
        }

        startToggleDefault.isOn = true;
        ChangeTowerType(buttonGameObject[startToggleDefault]);
    }

    private void OnDisable()
    {
        foreach (var entry in buttonGameObject)
        {
            entry.Key.onValueChanged.RemoveAllListeners();
        }
    }

    public void ChangeTowerType(GameObject tower)
    {
        towerEditor.NewSelectedTower(tower);
    }

    public void UpdateText()
    {
        foreach (var entry in buttonGameObject)
        {
            TowerData td = entry.Value.GetComponent<TowerData>();
            ButtonData bd = entry.Key.GetComponent<ButtonData>();
            bd.subText.text = string.Format("${0}", td.cost);
        }
    }
}
