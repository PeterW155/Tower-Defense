using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GameObjectIntDictionary))]
[CustomPropertyDrawer(typeof(ButtonBlockTypeDictionary))]
[CustomPropertyDrawer(typeof(ButtonGameObjectDictionary))]
public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}
