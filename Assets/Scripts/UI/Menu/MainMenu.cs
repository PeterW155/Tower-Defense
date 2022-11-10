using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string levelToLoad = "ChrisDemo";
    public string mainMenu = "MainMenu";

    public void Play()
    {
        //world.LoadWorld();
        SceneManager.LoadScene(levelToLoad);
        //SceneManager.LoadScene(levelToLoad, LoadSceneMode.Additive);
    }

    public void Quit()
    {
        Debug.Log("Exiting");
        Application.Quit();

    }
}
