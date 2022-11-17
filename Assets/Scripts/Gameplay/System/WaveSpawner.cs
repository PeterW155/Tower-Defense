using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public class WaveSpawner : MonoBehaviour
{

    public Transform enemyPrefab;
    public Transform fastEnemyPrefab;
    public Transform slowEnemyPrefab;
    public Transform spawnPoint;
    public Transform parent;
    public Transform effectParent;

    public UnityEvent startWave;
    public UnityEvent endWave;

    //public Text waveCountdownText;

    //public float intermissionTime = 5.5f;
    //private float countdown = 2f;
    private int waveIndex = 0;
    private static int waveNum = 1;

    private static WaveSpawner _instance;
    public static WaveSpawner Instance { get { return _instance; } }

    private void Awake()
    {
        // If an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            // We destroy this instance
            Destroy(this.gameObject);
        }
        else
        {
            // Make this the instance
            _instance = this;
        }
    }

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
            startWave.Invoke();
        }
        else
        {
            endWave.Invoke();
        }
    }

    public void DisableUI(string UIName)
    {
        GameObject UI = Root.Instance.UIGroups.Where(obj => obj.name == UIName).SingleOrDefault();
        if (UI != null)
            UI.SetActive(false);
    }
    public void EnableUI(string UIName)
    {
        GameObject UI = Root.Instance.UIGroups.Where(obj => obj.name == UIName).SingleOrDefault();
        if (UI != null)
            UI.SetActive(true);
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
        if (waveNum > 5)
        {
            StartCoroutine(spawnWaveSlow());
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

    IEnumerator spawnWaveSlow()
    {

        for (int i = 0; i < waveIndex / 3; i++)
        {
            spawnEnemy(slowEnemyPrefab);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void spawnEnemy(Transform prefab)
    {
        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, parent);
    }
}