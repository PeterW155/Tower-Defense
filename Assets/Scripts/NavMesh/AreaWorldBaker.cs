using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;
using Unity.AI.Navigation;

public class AreaWorldBaker : MonoBehaviour
{
    [SerializeField]
    private NavMeshSurface Surface;
    //[SerializeField]
    private GameObject Player;
    [SerializeField]
    private float UpdateRate = 0.5f;
    [SerializeField]
    private float MovementThreshold = 3;
    [SerializeField]
    private Vector3 NavMeshSize = new Vector3(20, 20, 20);

    private Vector3 WorldAnchor;
    private NavMeshData NavMeshData;
    private NavMeshDataInstance NavMeshDataInstance;
    private List<NavMeshBuildSource> Sources = new List<NavMeshBuildSource>();

    private void Start()
    {
        NavMeshData = new NavMeshData();
        Player = this.gameObject;
        NavMeshDataInstance = NavMesh.AddNavMeshData(NavMeshData);
        BuildNavMesh(false);
        StartCoroutine(CheckGameObjectMovement());
    }

    private IEnumerator CheckGameObjectMovement()
    {
        WaitForSeconds Wait = new WaitForSeconds(UpdateRate);

        for (; ;)
        {
            if (GameObject.Find(Player.name))
            {
                BuildNavMesh(true);
            }
            
            else if (Vector3.Distance(WorldAnchor, Player.transform.position) > MovementThreshold)
            {
                BuildNavMesh(true);
                WorldAnchor = Player.transform.position;
            }

            yield return Wait;
        }
    }

    private void BuildNavMesh(bool Async)
    {
        Bounds navMeshBounds = new Bounds(Player.transform.position, NavMeshSize);
        List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();

        List<NavMeshModifier> modifiers;
        if (Surface.collectObjects == CollectObjects.Children)
        {
            modifiers = new List<NavMeshModifier>(Surface.GetComponentsInChildren<NavMeshModifier>());
        }
        else
        {
            modifiers = NavMeshModifier.activeModifiers;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (((Surface.layerMask & (1 << modifiers[i].gameObject.layer)) == 1) && modifiers[i].AffectsAgentType(Surface.agentTypeID))
            {
                markups.Add(new NavMeshBuildMarkup()
                {
                    root = modifiers[i].transform,
                    overrideArea = modifiers[i].overrideArea,
                    area = modifiers[i].area,
                    ignoreFromBuild = modifiers[i].ignoreFromBuild
                });
            }
        }

        if (Surface.collectObjects == CollectObjects.Children)
        {
            NavMeshBuilder.CollectSources(Surface.transform, Surface.layerMask, Surface.useGeometry, Surface.defaultArea, markups, Sources);
        }
        else
        {
            NavMeshBuilder.CollectSources(navMeshBounds, Surface.layerMask, Surface.useGeometry, Surface.defaultArea, markups, Sources);
        }

        Sources.RemoveAll(source => source.component != null && source.component.gameObject.GetComponent<NavMeshAgent>() != null);

        if (Async)
        {
            NavMeshBuilder.UpdateNavMeshDataAsync(NavMeshData, Surface.GetBuildSettings(), Sources, new Bounds(Player.transform.position, NavMeshSize));
        }
    }

    private void OnDestroy() 
    {
        NavMesh.RemoveNavMeshData(NavMeshDataInstance);
    }
}
