using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int gridX; // X position in the Node Array
    public int gridY; // Y position in the Node Array
    
    public bool IsWall; // Tells the program if this node is being obstructed.
    public Vector3 position; // The world position of the node.

    public Node Parent; // For the AStar algorithm, will store what node it previously came from so it can trace the shortest path.

    public int gCost; // The cost of moving to the next square.
    public int hCost; // The distance to the goal from this node.

    public int FCost { get { return gCost + hCost; }} // Quick get function to add G cost and H Cost, and since we'll never need to edit FCost, we don't need a set function.

    public Node(bool isWall, Vector3 a_Pos, int a_gridX, int a_gridY) // Constructor
    {
        IsWall = isWall; // Tells the program if this node is being obstructed.
        position = a_Pos; // The world position of the node.
        gridX = a_gridX; // X Position in the Node Array
        gridY = a_gridY; // Y Position in the Node Array
    }
}
