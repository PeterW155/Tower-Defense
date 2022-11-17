using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveButton : MonoBehaviour
{
    public void SpawnWaveButton()
    {
        WaveSpawner.Instance.SpawnNextWave();
    }
}
