using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour
{

    public Transform enemyPrefab;
    public Transform fastEnemyPrefab;
    public Transform spawnPoint;
    public GameObject text;
    public GameObject uiButtons1;
    public GameObject uiButtons2;
    public GameObject uiButtons3;

    //public Text waveCountdownText;

    //public float intermissionTime = 5.5f;
    //private float countdown = 2f;
    private int waveIndex = 0;
    private static int waveNum = 1;

    // Update is called once per frame
    /*void Update()
    {

        if (countdown <= 0)
        {
            //Check for any MarketBuildings
            MarketBuilding[] markets = FindObjectsOfType(typeof(MarketBuilding)) as MarketBuilding[];
            foreach (MarketBuilding item in markets)
            {
                item.PayPlayer(item.buildingLevel);
            }

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
    */

    private void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length >= 1)
        {
            gameObject.GetComponent<Button>().enabled = false;
            gameObject.GetComponent<Image>().enabled = false;
            text.SetActive(false);
            uiButtons1.SetActive(false);
            uiButtons2.SetActive(false);
            uiButtons3.SetActive(false);
        }
        else
        {
            gameObject.GetComponent<Button>().enabled = true;
            gameObject.GetComponent<Image>().enabled = true;
            text.SetActive(true);
            uiButtons1.SetActive(true);
            uiButtons2.SetActive(true);
            uiButtons3.SetActive(true);
        }
    }

    public static string getWaveNum()
    {
        return string.Format("{0}", waveNum - 1);
    }

    public void SpawnNextWave()
    {
        //Check for any MarketBuildings
        MarketBuilding[] markets = FindObjectsOfType(typeof(MarketBuilding)) as MarketBuilding[];
        foreach (MarketBuilding item in markets)
        {
            item.PayPlayer(item.buildingLevel);
        }

        StartCoroutine(spawnWave());
        if (waveNum > 3)
        {
            StartCoroutine(spawnWaveFast());
        }

        waveNum++;
    }

    IEnumerator spawnWave()
    {
        waveIndex++;
        PlayerStats.Instance.rounds++;

        for (int i = 0; i < waveIndex; i++)
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