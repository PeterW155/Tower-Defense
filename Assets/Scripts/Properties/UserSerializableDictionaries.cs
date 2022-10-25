using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GameObjectIntDictionary : SerializableDictionary<GameObject, int> {}
[Serializable]
public class ButtonBlockTypeDictionary : SerializableDictionary<Toggle, BlockType> { }
[Serializable]
public class ButtonGameObjectDictionary : SerializableDictionary<Toggle, GameObject> { }
