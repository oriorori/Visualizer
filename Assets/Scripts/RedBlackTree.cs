using Unity.VisualScripting;
using UnityEngine;


public enum NodeColor
{
    Red,
    Black
}

public class Node
{
    public GameObject obj;

    public Vector3 prevPosition;
    public Vector3 newPosition;
    
    public int data;
    public Node left, right, parent;
    public NodeColor color;

    public Node(int data, GameObject obj)
    {
        this.obj = obj;
        this.data = data;
        left = right = parent = null;
        color = NodeColor.Red;
        prevPosition = new Vector3(0, 0, 0);
    }
}

