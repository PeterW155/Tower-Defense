using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlockTypeButtons : MonoBehaviour
{
    private TerrainEditor terrainEditor;
    public Toggle startToggleDefault;
    public TextMeshProUGUI infoText;
    [SerializeField]
    ButtonBlockTypeDictionary serializableButtonBlockType;
    public IDictionary<Toggle, BlockType> buttonBlockType
    {
        get { return serializableButtonBlockType; }
        set { serializableButtonBlockType.CopyFrom(value); }
    }

    private void OnEnable()
    {
        terrainEditor = TerrainEditor.Instance;

        UpdateText();

        foreach (var entry in buttonBlockType)
        {
            entry.Key.onValueChanged.AddListener((b) => ChangeBlockType(entry.Value));
        }

        startToggleDefault.isOn = true;
        ChangeBlockType(buttonBlockType[startToggleDefault]);
    }

    private void OnDisable()
    {
        foreach (var entry in buttonBlockType)
        {
            entry.Key.onValueChanged.RemoveAllListeners();
        }
    }

    public void ChangeBlockType(BlockType blockType)
    {
        terrainEditor.playModeBlockType = blockType;
    }

    public void UpdateText()
    {
        infoText.text = string.Format("${0} - place\n${0} - remove", terrainEditor.cost);
    }
}
