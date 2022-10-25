using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockTypeButtons : MonoBehaviour
{
    public TerrainEditor terrainEditor;
    public Toggle startToggleDefault;
    [SerializeField]
    ButtonBlockTypeDictionary serializableButtonBlockType;
    public IDictionary<Toggle, BlockType> buttonBlockType
    {
        get { return serializableButtonBlockType; }
        set { serializableButtonBlockType.CopyFrom(value); }
    }

    private void Awake()
    {
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
}
