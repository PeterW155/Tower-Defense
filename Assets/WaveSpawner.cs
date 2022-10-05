using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour
{

    public Transform enemyPrefab;
    public Transform spawnPoint;
    public Text waveCountdownText;

    public float intermissionTime = 5.5f;
    private float countdown = 2f;
    private int waveIndex = 0;

    

    // Update is called once per frame
    void Update()
    {
        if (countdown <= 0)
        {
            StartCoroutine(spawnWave());
            countdown = intermissionTime;
        }

        countdown -= Time.deltaTime;

        waveCountdownText.text = Mathf.Round(countdown).ToString();
    }

    IEnumerator spawnWave()
    {
        waveIndex++;

        for(int i = 0; i <waveIndex; i++)
        {
            spawnEnemy();
            yield return new WaitForSeconds(0.5f);
        }
        waveIndex++;
    }

    void spawnEnemy()
    {
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
