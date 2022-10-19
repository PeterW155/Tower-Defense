using UnityEngine;

public class Node : MonoBehaviour
{
    private GameObject turret;

    public Color hoverColor;
    private Renderer rend;
    private Color startColor;
    public Vector3 posOff;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        startColor = rend.material.color;
    }

    void OnMouseEnter()
    {
        rend.material.color = hoverColor;
    }

    private void OnMouseExit()
    {
        rend.material.color = startColor;
    }

    private void OnMouseDown()
    {
        if(turret != null)
        {
            Debug.Log("Cannot Build There");
            return;
        }

        GameObject turretToBuild = BuildManager.instance.GetTurretToBuild();
        turret = (GameObject)Instantiate(turretToBuild, transform.position + posOff, transform.rotation);
    }
}
