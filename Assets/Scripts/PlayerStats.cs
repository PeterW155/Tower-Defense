using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [ReadOnly] public int money;
    public int startMoney = 1000;

    [ReadOnly] public int lives;
    public int startLives = 20;

    [ReadOnly] public int rounds;

    private static PlayerStats _instance;
    public static PlayerStats Instance { get { return _instance; } }

    void Awake()
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

    void Start ()
    {
        money = startMoney;
        lives = startLives;

        rounds = 0;
    }
}
