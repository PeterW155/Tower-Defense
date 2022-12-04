using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    [StringInList(typeof(PropertyDrawersHelper), "AllSceneNames")] public string startingScene;

    [SerializeField, ReadOnly] private List<AsyncOperation> scenesLoading;
    [Space]
    public GameObject loadScreen;
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    private Coroutine _loadingProgress;

    [ReadOnly] public string currentScene;
    [ReadOnly] public bool loading;

    private bool worldLoading = false;

    private static SceneLoader _instance;
    public static SceneLoader Instance { get { return _instance; } }

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

    // Start is called before the first frame update
    void Start()
    {
        scenesLoading = new List<AsyncOperation>();

        Load(startingScene);
    }

    public void LoadMultiple(List<string> scenes, bool unloadCurrent = false)
    {
        loadScreen.SetActive(true);

        if (unloadCurrent && SceneManager.GetSceneByName(currentScene) != null) { scenesLoading.Add(SceneManager.UnloadSceneAsync(currentScene)); }
        foreach (string scene in scenes)
        {
            scenesLoading.Add(SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive));
            currentScene = scene;
        }

        if (_loadingProgress != null) { StopCoroutine(_loadingProgress); }
        _loadingProgress = StartCoroutine(SceneLoadProgress());
    }

    public void LoadScreenless(string scene, bool unloadCurrent = false)
    {
        if (unloadCurrent && SceneManager.GetSceneByName(currentScene) != null) { scenesLoading.Add(SceneManager.UnloadSceneAsync(currentScene)); }
        scenesLoading.Add(SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive));
        currentScene = scene;

        if (_loadingProgress != null) { StopCoroutine(_loadingProgress); }
        _loadingProgress = StartCoroutine(SceneLoadProgress());
    }

    public void Load(string scene, bool unloadCurrent = false)
    {
        loadScreen.SetActive(true);

        if (unloadCurrent && SceneManager.GetSceneByName(currentScene) != null) { scenesLoading.Add(SceneManager.UnloadSceneAsync(currentScene)); }
        scenesLoading.Add(SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive));
        currentScene = scene;

        if (_loadingProgress != null) { StopCoroutine(_loadingProgress); }
        _loadingProgress = StartCoroutine(SceneLoadProgress());
    }

    public void LoadWorldScene(string scene, bool unloadCurrent = false)
    {
        loadScreen.SetActive(true);

        worldLoading = true;

        if (unloadCurrent && SceneManager.GetSceneByName(currentScene) != null) { scenesLoading.Add(SceneManager.UnloadSceneAsync(currentScene)); }
        scenesLoading.Add(SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive));
        currentScene = scene;

        if (_loadingProgress != null) { StopCoroutine(_loadingProgress); }
        _loadingProgress = StartCoroutine(SceneLoadProgress());
    }

    public void Unload(string scene)
    {
        scenesLoading.Add(SceneManager.UnloadSceneAsync(scene));
    }

    private IEnumerator SceneLoadProgress()
    {
        loading = true;
        float totalSceneProgress;

        progressBar.value = 0;

        yield return 0;

        Time.timeScale = 0;

        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (scenesLoading[i] != null && !scenesLoading[i].isDone)
            {
                totalSceneProgress = 0;

                int nullCount = 0;

                foreach(AsyncOperation operation in scenesLoading)
                {
                    if (operation != null) { totalSceneProgress += operation.progress; }
                    else { nullCount++; }
                }
                totalSceneProgress = (totalSceneProgress / (scenesLoading.Count - nullCount)) * 100f;

                progressBar.value = Mathf.RoundToInt(totalSceneProgress);
                progressText.text = Mathf.RoundToInt(totalSceneProgress) + "%";

                yield return null;
            }
        }

        progressText.text = 100 + "%";

        while (worldLoading)
        {
            if (World.Instance != null && World.Instance.IsWorldCreated)
                worldLoading = false;
            yield return null;
        }

        loadScreen.SetActive(false);

        scenesLoading.Clear();
        scenesLoading.TrimExcess();

        Time.timeScale = 1;
        loading = false;

        yield return null;
    }
}
