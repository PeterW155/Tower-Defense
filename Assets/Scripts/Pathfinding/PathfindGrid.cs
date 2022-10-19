using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindGrid : MonoBehaviour
{
    public Transform StartPosition;
    public LayerMask WallMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public float distance;

    PathfindNode[,] grid;
    public List<PathfindNode> FinalPath;


    float nodeDiameter;
    int gridSizeX, gridSizeY;
    int mapSize, chunkSize;
    //Vector2 gridWorldSize;

    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        GridSizeFromEditor();
        //GridSizeFromWorldObject(); // Remember to uncomment Vector2 gridWorldSize and comment public Vector2 gridWorldSize and comment GridSizeFromEditor() when using this and vice versa
        CreateGrid();
    }

    void GridSizeFromEditor()
    {
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
    }

    void GridSizeFromWorldObject()
    {
        mapSize = GameObject.Find("World").GetComponent<World>().mapSizeInChunks;
        chunkSize = GameObject.Find("World").GetComponent<World>().chunkSize;
        gridWorldSize.x = mapSize * chunkSize;
        gridWorldSize.y = mapSize * chunkSize;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
    }

    void CreateGrid()
    {
        grid = new PathfindNode[gridSizeX, gridSizeY];
        Vector3 bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        //Vector3 bottomLeft = GameObject.Find("World").transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        //Vector3 bottomLeft = transform.localPosition - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool Wall = true;

                if (Physics.CheckSphere(worldPoint, nodeRadius, WallMask))
                {
                    Wall = false;
                }

                grid[x, y] = new PathfindNode(Wall, worldPoint, x, y); // might be this
            }
        }
    }

    public PathfindNode NodeFromWorldPosition(Vector3 a_WorldPosition)
    {
        float xpoint = ((a_WorldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x);
        float ypoint = ((a_WorldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y);

        Debug.Log("xy point: " + xpoint + ", " + ypoint);

        //xpoint = Mathf.Clamp01(xpoint);
        //ypoint = Mathf.Clamp01(ypoint);

        xpoint = Mathf.Clamp(xpoint, 0.5f, 1.5f);
        ypoint = Mathf.Clamp(ypoint, 0.5f, 1.5f);

        //int x = Mathf.RoundToInt((gridSizeX - 1) * xpoint); // issue origin
        //int y = Mathf.RoundToInt((gridSizeY - 1) * ypoint);

        int x = Mathf.RoundToInt((gridSizeX) * xpoint); // issue origin
        int y = Mathf.RoundToInt((gridSizeY) * ypoint);

        x = x - Mathf.RoundToInt(gridSizeX / 2); // This partially fixed the grid shift issue
        y = y - Mathf.RoundToInt(gridSizeY / 2); 

        Debug.Log("Original: " + a_WorldPosition.x + ", " + a_WorldPosition.z);
        Debug.Log("Clamp: " + xpoint + ", " + ypoint);
        //Debug.Log(gridSizeX + ", " + gridSizeY);
        //Debug.Log((gridSizeX - 1) * xpoint);
        //Debug.Log((gridSizeY - 1) * ypoint);
        Debug.Log("Result: " + x + ", " + y);

        return grid[x, y];
    }

    public List<PathfindNode> GetNeighboringNodes(PathfindNode a_Node)
    {
        List<PathfindNode> NeighboringNodes = new List<PathfindNode>();
        int xCheck;
        int yCheck;

        //Right Side
        xCheck = a_Node.gridX + 1;
        yCheck = a_Node.gridY;
        if (xCheck >= 0 && xCheck < gridSizeX)
        {
            if (yCheck >= 0 && yCheck < gridSizeY)
            {
                NeighboringNodes.Add(grid[xCheck, yCheck]);
            }
        }

        //Left Side
        xCheck = a_Node.gridX - 1;
        yCheck = a_Node.gridY;
        if (xCheck >= 0 && xCheck < gridSizeX)
        {
            if (yCheck >= 0 && yCheck < gridSizeY)
            {
                NeighboringNodes.Add(grid[xCheck, yCheck]);
            }
        }

        //Top Side
        xCheck = a_Node.gridX;
        yCheck = a_Node.gridY + 1;
        if (xCheck >= 0 && xCheck < gridSizeX)
        {
            if (yCheck >= 0 && yCheck < gridSizeY)
            {
                NeighboringNodes.Add(grid[xCheck, yCheck]);
            }
        }

        //Bottom Side
        xCheck = a_Node.gridX;
        yCheck = a_Node.gridY - 1;
        if (xCheck >= 0 && xCheck < gridSizeX)
        {
            if (yCheck >= 0 && yCheck < gridSizeY)
            {
                NeighboringNodes.Add(grid[xCheck, yCheck]);
            }
        }
        return NeighboringNodes;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        //Debug.Log(FinalPath);
        if (grid != null) // If the grid is not empty
        {
            foreach (PathfindNode node in grid) // Loop through every node in the grid
            {
                if (node.IsWall) // If the current node is a wall node
                {
                    Gizmos.color = Color.white; // Set the color of the node
                }
                else
                {
                    Gizmos.color = Color.yellow; // Set the color of the node
                } 
                
                if (FinalPath != null) // If the final path is not empty
                {
                    if (FinalPath.Contains(node)) // If the current node is in the final path
                    {
                        //Debug.Log(node.gridX + ", " + node.gridY);
                        Gizmos.color = Color.red; // Set the color of that node
                    }
                }

                Gizmos.DrawCube(node.position, Vector3.one * (nodeDiameter - distance)); // Draw the node at the position of the node.
            }
        }
    }
}
