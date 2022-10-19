using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    PathfindGrid grid;
    public Transform StartPosition;
    public Transform TargetPosition;

    private void Awake()
    {
        grid = GetComponent<PathfindGrid>();
        
    }

    private void Update()
    {
        //Debug.Log(StartPosition.position.x + ", " + StartPosition.position.z + "|" + TargetPosition.position.x + ", " + TargetPosition.position.z);
        FindPath(StartPosition.position, TargetPosition.position);
    }

    void FindPath(Vector3 a_StartPos, Vector3 a_TargetPos)
    {
        //Debug.Log(a_StartPos.x + ", " + a_StartPos.z);
        
        PathfindNode StartNode = grid.NodeFromWorldPosition(a_StartPos);
        PathfindNode TargetNode = grid.NodeFromWorldPosition(a_TargetPos);

        //Debug.Log(StartNode.position.x + ", " + StartNode.position.z);

        List<PathfindNode> OpenList = new List<PathfindNode>();
        HashSet<PathfindNode> ClosedList = new HashSet<PathfindNode>();

        OpenList.Add(StartNode);

        while(OpenList.Count > 0)
        {
            PathfindNode CurrentNode = OpenList[0];
            for (int i = 1; i < OpenList.Count; i++)
            {
                if (OpenList[i].FCost < CurrentNode.FCost || OpenList[i].FCost == CurrentNode.FCost && OpenList[i].hCost < CurrentNode.hCost)
                {
                    CurrentNode = OpenList[i];
                }
            }
            OpenList.Remove(CurrentNode);
            ClosedList.Add(CurrentNode);

            if (CurrentNode == TargetNode)
            {
                GetFinalPath(StartNode, TargetNode);
            }

            foreach (PathfindNode NeighborNode in grid.GetNeighboringNodes(CurrentNode))
            {
                if (!NeighborNode.IsWall || ClosedList.Contains(NeighborNode))
                {
                    continue;
                }
                int MoveCost = CurrentNode.gCost + GetManhattenDistance(CurrentNode, NeighborNode);

                if (MoveCost < NeighborNode.gCost || !OpenList.Contains(NeighborNode))
                {
                    NeighborNode.gCost = MoveCost;
                    NeighborNode.hCost = GetManhattenDistance(NeighborNode, TargetNode);
                    NeighborNode.Parent = CurrentNode;

                    if (!OpenList.Contains(NeighborNode))
                    {
                        OpenList.Add(NeighborNode);
                    }
                }
            }
        }
    }

    void GetFinalPath(PathfindNode a_StartingNode, PathfindNode a_EndNode)
    {
        List<PathfindNode> FinalPath = new List<PathfindNode>();
        PathfindNode CurrentNode = a_EndNode;

        while(CurrentNode != a_StartingNode)
        {
            FinalPath.Add(CurrentNode);
            CurrentNode = CurrentNode.Parent;
        }

        FinalPath.Reverse();

        grid.FinalPath = FinalPath;
    }

    int GetManhattenDistance(PathfindNode a_nodeA, PathfindNode a_nodeB)
    {
        int ix = Mathf.Abs(a_nodeA.gridX - a_nodeB.gridX);
        int iz = Mathf.Abs(a_nodeA.gridY - a_nodeB.gridY);

        return ix + iz;
    }
}