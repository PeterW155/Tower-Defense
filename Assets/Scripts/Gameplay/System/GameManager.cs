using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool GameIsOver;

    public string gameOverUI;
    public string gamePausedUI;

    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

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

    private void Start()
    {
        GameIsOver = false;
    }

    void Update()
    {
        if (GameIsOver)
            return;

        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            EndGame();
        }

        if (PlayerStats.Instance.lives <= 0)
        {
            EndGame();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    void EndGame()
    {
        GameIsOver = true;
        GameObject UI = Root.Instance.UIGroups.Where(obj => obj.name == gameOverUI).SingleOrDefault();
        if (UI != null)
            UI.SetActive(true);
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        GameObject UI = Root.Instance.UIGroups.Where(obj => obj.name == gamePausedUI).SingleOrDefault();
        if (UI != null)
            UI.SetActive(true);
    }
}
