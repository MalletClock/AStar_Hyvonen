using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    /* Demo purposes to visualize NodeFromWorldPoint */
    //public Transform player;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];

        // With this we can calculate the world positions in any grid size by placing the starting point at the bottom left corner
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Map out the grid moving node diameter + its radius in x and y directions, check if there are obstacles in that space and mark it unwalkable is colliding with an object on the unwalkable layer
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint,x,y);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        // Do a 3x3 check
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) // Skip since we are in the center node
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                // Map out the neighbours of the node into a list
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    // We use this to get a Node from a worldPosition, for example with this we can see which Node the player occupies at the moment
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        //float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        //float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;

        // Optimization for the code from comments of people, optimized calculations
        float percentX = (worldPosition.x / gridWorldSize.x + 0.5f);
        float percentY = (worldPosition.z / gridWorldSize.y + 0.5f);

        // Clamp to avoid errors
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        //int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        //int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        // Optimization for the code from comments of people, this way the accuracy is better (we get the very center the player object)
        int x = Mathf.FloorToInt(Mathf.Min(gridSizeX * percentX, gridSizeX - 1));
        int y = Mathf.FloorToInt(Mathf.Min(gridSizeY * percentY, gridSizeY - 1));
        return grid[x, y];
    }

    public List<Node> path;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            /* Demo purposes to visualize NodeFromWorldPoint */
            //Node playerNode = NodeFromWorldPoint(player.position);

            // Draw a different color cube if the Node was marked unwalkable
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                if (path != null)
                    if (path.Contains(n))
                        Gizmos.color = Color.black;
                /* Demo purposes to visualize NodeFromWorldPoint */
                //if (playerNode == n)
                //{
                //    Gizmos.color = Color.cyan;
                //}

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f)); // Space the cubes out a bit for readability 
            }
        }
    }
}
