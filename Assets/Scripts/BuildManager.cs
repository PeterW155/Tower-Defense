using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;
    private GameObject turretToBuild;
    public GameObject turretPrefab;

    private void Awake()
    {
        if(instance != null)
        {
            Debug.Log("MORE THAN 1 BM IN SCENE");
            return;
        }
        instance = this;
    }
    public GameObject GetTurretToBuild()
    {
        return turretToBuild;
    }

    private void Start()
    {
        turretToBuild = turretPrefab;
    }


}
