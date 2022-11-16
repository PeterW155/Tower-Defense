using UnityEngine;

public class CannonBullet : MonoBehaviour
{
    private Transform target;

    public float speed = 70f;
    public GameObject impactEffect;
    public string enemyTag = "Enemy";
    public float splashDamageRange = 10f;
    public int damage = 75;
    public int splashDamage = 50;

    public void Seek(Transform _target, int damage)
    {
        target = _target;
        this.damage = damage;
    }




    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        EnemyMovement e = target.GetComponent<EnemyMovement>();

        if (e != null)
        {
            e.TakeDamage(damage);
        }

        GameObject effectInst = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effectInst, 2f);
        Destroy(gameObject.gameObject);

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(target.transform.position, enemy.transform.position);
            if(dist <= splashDamageRange && enemy != target)
            {
                GameObject effectNst = (GameObject)Instantiate(impactEffect, enemy.transform.position, enemy.transform.rotation);
                enemy.GetComponent<EnemyMovement>().TakeDamage(splashDamage);
            }
        }
    }
}
