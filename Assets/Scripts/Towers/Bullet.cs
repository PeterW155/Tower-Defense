using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;

    public float speed = 70f;
    public GameObject impactEffect;
    public int damage = 50;



    public void Seek(Transform _target, int damage)
    {
        this.damage = damage;
        target = _target;


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

        if(dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        EnemyMovement e = target.GetComponent<EnemyMovement>();
        MovePlayer p = target.GetComponent<MovePlayer>();

        if(e != null)
        {
            e.TakeDamage(damage);
        }
        
        if(p != null)
        {
            p.TakeDamage(damage);
        }

        GameObject effectInst = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation, WaveSpawner.Instance.effectParent);
        Destroy(effectInst, 2f);
        Destroy(gameObject.gameObject);
        //Destroy(target.gameObject);
    }
}
