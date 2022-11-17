using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{

    public int value = 75;

    public GameObject deathEffect;

    public int health = 100;


    public void TakeDamage (int amount)
    {
        health -= amount;
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
