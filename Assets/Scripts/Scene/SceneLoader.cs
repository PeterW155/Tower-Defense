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
        _loadingProgress = StartCoroutine(sceneLoadProgress());
    }

    public void LoadScreenless(string scene, bool unloadCurrent = false)
    {
        if (unloadCurrent && SceneManager.GetSceneByName(currentScene) != null) { scenesLoading.Add(SceneManager.UnloadSceneAsync(currentScene)); }
        scenesLoading.Add(SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive));
        currentScene = scene;

        if (_loadingProgress != null) { StopCoroutine(_loadingProgress); }
        _loadingProgress = StartCoroutine(sceneLoadProgress());
    }

    public void Load(string scene, bool unloadCurrent = false)
    {
        loadScreen.SetActive(true);

        if (unloadCurrent && SceneManager.GetSceneByName(currentScene) != null) { scenesLoading.Add(SceneManager.UnloadSceneAsync(currentScene)); }
        scenesLoading.Add(SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive));
        currentScene = scene;

        if (_loadingProgress != null) { StopCoroutine(_loadingProgress); }
        _loadingProgress = StartCoroutine(sceneLoadProgress());
    }

    public void Unload(string scene)
    {
        scenesLoading.Add(SceneManager.UnloadSceneAsync(scene));
    }

    private IEnumerator sceneLoadProgress()
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
                progressText.text = progressBar.value + "%";

                yield return null;
            }
        }
        loadScreen.SetActive(false);

        scenesLoading.Clear();
        scenesLoading.TrimExcess();

        Time.timeScale = 1;
        loading = false;

        yield return null;
    }
}
