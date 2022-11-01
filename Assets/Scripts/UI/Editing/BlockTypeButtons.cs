using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlockTypeButtons : MonoBehaviour
{
    public TerrainEditor terrainEditor;
    public Toggle startToggleDefault;
    public TextMeshProUGUI infoText;
    [SerializeField]
    ButtonBlockTypeDictionary serializableButtonBlockType;
    public IDictionary<Toggle, BlockType> buttonBlockType
    {
        get { return serializableButtonBlockType; }
        set { serializableButtonBlockType.CopyFrom(value); }
    }

    private void Awake()
    {
        UpdateText();

        foreach(var entry in buttonBlockType)
        {
            entry.Key.onValueChanged.AddListener((b) => ChangeBlockType(entry.Value));
        }
    }
    private void Start()
    {
        startToggleDefault.isOn = true;
        ChangeBlockType(buttonBlockType[startToggleDefault]);
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
