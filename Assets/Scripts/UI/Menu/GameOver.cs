using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public Text roundsText;
    [StringInList(typeof(PropertyDrawersHelper), "AllSceneNames")] public string mainMenu;

    void OnEnable()
    {
        roundsText.text = PlayerStats.Instance.rounds.ToString();
    }

    public void Retry ()
    {
        SceneLoader.Instance.LoadWorldScene(SceneLoader.Instance.currentScene, true);
    }

    public void Menu ()
    {
        SceneLoader.Instance.Load("MainMenu", true);
    }
}

