using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyMovement : MonoBehaviour
{

    public int value = 75;

    public GameObject deathEffect;

    public int startHealth = 100;
    private float health;

    [Header("Unity Stuff")]
    public Image healthBar;


    void Start ()
    {
        health = startHealth;
    }

    public void TakeDamage (int amount)
    {
        health -= amount;
        healthBar.fillAmount = health/startHealth;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die ()
    {
        PlayerStats.Instance.money += value;

        GameObject effect = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity, WaveSpawner.Instance.effectParent);
        Destroy(effect, 5f);

        Destroy(gameObject);
    }


}
