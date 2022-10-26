using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour
{

    public Transform enemyPrefab;
    public Transform fastEnemyPrefab;
    public Transform spawnPoint;
    public Text waveCountdownText;

    public float intermissionTime = 5.5f;
    private float countdown = 2f;
    private int waveIndex = 0;
    private int waveNum = 1;

    

    // Update is called once per frame
    void Update()
    {
        if (countdown <= 0)
        {
            StartCoroutine(spawnWave());
            if(waveNum > 5)
            {
                StartCoroutine(spawnWaveFast());
            }
            countdown = intermissionTime;
            waveNum++;
        }

        countdown -= Time.deltaTime;

        countdown = Mathf.Clamp(countdown, 0f, Mathf.Infinity);

        waveCountdownText.text = string.Format("{0:00.00}", countdown); ;
    }

    IEnumerator spawnWave()
    {
        waveIndex++;

        for(int i = 0; i <waveIndex; i++)
        {
            spawnEnemy(enemyPrefab);
            yield return new WaitForSeconds(0.5f);
        }
        waveIndex++;
    }

    IEnumerator spawnWaveFast()
    {

        for (int i = 0; i < waveIndex / 2; i++)
        {
            spawnEnemy(fastEnemyPrefab);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void spawnEnemy(Transform prefab)
    {
        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}
