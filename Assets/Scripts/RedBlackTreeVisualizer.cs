using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;
using TMPro;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class RedBlackTreeVisualizer : MonoBehaviour
{
    private class Line
    {
        public LineRenderer lineRenderer;
        public GameObject startPoint;
        public GameObject endPoint;
    }
    
    public Button insertButton;
    public TMPro.TMP_InputField inputField;
    public Transform nodeContainer;
    
    public GameObject nodePrefab;
    public LineRenderer edgePrefab;

    private List<Line> edges = new List<Line>();
    
    // for making tree
    public Node root;
    private Node TNULL; // NIL 노드 (조건 3 관련)
    private int maxLevel = 0;
    private List<Node> nodes = new List<Node>();

    public float width = 10.0f;
    public float height = 1f;
    
    void Start()
    {
        insertButton.onClick.AddListener(ClickInsertButton);
    }

    private void ClickInsertButton()
    {
        if (int.TryParse(inputField.text, out int data))
        {
            StartCoroutine(InsertAnimation(data));
            inputField.text = "";
        }
    }

    IEnumerator UpdateVisualizer(Node root)
    {
        SetLine();

        float time = 0.0f;
        float lerptime = 0.3f;

        foreach (Node n in nodes)
        {
            n.prevPosition = n.newPosition;
        }

        StartCoroutine(ColorUpdate());
        CalculatePosition(root);
        
        while (time < lerptime)
        {
            foreach (Node n in nodes)
            {
                Vector3 prevPosition = n.prevPosition;
                Vector3 newPosition = n.newPosition;
                n.obj.transform.localPosition = Vector3.Lerp(prevPosition, newPosition, time/lerptime);
            }
            UpdateLine();
            time += Time.deltaTime;
            yield return null;
        }
        foreach (Node n in nodes)
        {
            n.obj.transform.localPosition = n.newPosition;
            UpdateLine();
        }
        
    }

    private void SetLine()
    {
        for (int i = 0; i < edges.Count; i++)
        {
            Destroy(edges[i].lineRenderer.gameObject);
        }
        edges.Clear();

        foreach (var node in nodes)
        {
            if (node.parent != null)
            {
                Line line = new Line();
                line.lineRenderer = Instantiate(edgePrefab, nodeContainer).GetComponent<LineRenderer>();
                line.startPoint = node.obj;
                line.endPoint = node.parent.obj;
                line.lineRenderer.positionCount = 2;
                edges.Add(line);
            }
        }
    }
    
    private void UpdateLine()
    {
        foreach (var edge in edges)
        {
            edge.lineRenderer.SetPosition(0, edge.startPoint.transform.localPosition);
            edge.lineRenderer.SetPosition(1, edge.endPoint.transform.localPosition);
        }
    }

    private void CalculatePosition(Node n, int depth = 0)
    // calculate position in preorder sequence
    {
        if (n == null) return;
        
        if (n.parent == null)
        {
            n.newPosition = new Vector3(0, 0, 0);
        }
        else
        {
            Vector3 parentPostion = n.parent.newPosition;

            if (n == n.parent.left)
            {
                n.newPosition =  new Vector3(parentPostion.x - width/(float)(Math.Pow(2, depth)), parentPostion.y - height, parentPostion.z);
            }
            else
            {
                n.newPosition = new Vector3(parentPostion.x + width/(float)(Math.Pow(2, depth)), parentPostion.y - height, parentPostion.z);
            }
        }
        CalculatePosition(n.left, depth + 1);
        CalculatePosition(n.right, depth + 1);
    }

    IEnumerator ColorUpdate()
    {        
        List<Color> prevColors = new List<Color>();
        foreach (Node n in nodes)
        {
            prevColors.Add(n.obj.GetComponent<MeshRenderer>().material.color);
        }
        
        float time = 0.0f;
        float lerptime = 0.2f;

        while (time < lerptime)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node n = nodes[i];
                if (n.color == NodeColor.Black) n.obj.GetComponent<Renderer>().material.color = 
                    Color.Lerp(prevColors[i], Color.black, time/lerptime);
                else n.obj.GetComponent<Renderer>().material.color = 
                    Color.Lerp(prevColors[i], Color.red, time/lerptime);
            }
            time += Time.deltaTime;
            yield return null;
        }

        foreach (Node n in nodes)
        {
            if (n.color == NodeColor.Black) n.obj.GetComponent<Renderer>().material.color = Color.black;
            else n.obj.GetComponent<Renderer>().material.color = Color.red;
        }
    }
    
    
    IEnumerator InsertAnimation(int key)
    {
        GameObject nodeObj = Instantiate(nodePrefab, nodeContainer);
        nodeObj.GetComponentInChildren<TextMeshPro>().text = key.ToString();
        Node node = new Node(key, nodeObj);
        nodes.Add(node);
        node.left = TNULL;
        node.right = TNULL;

        Node y = null;
        Node x = root;

        // 삽입 위치 찾기
        while (x != TNULL)
        {
            y = x;
            x = node.data < x.data ? x.left : x.right;
        }

        node.parent = y;

        if (y == null)
            root = node; // 조건 2: 루트는 항상 Black이 되도록 InsertFixup에서 처리
        else if (node.data < y.data)
            y.left = node;
        else
            y.right = node;
        
        yield return StartCoroutine(UpdateVisualizer(root));
        yield return StartCoroutine(InsertFixup(node)); // 레드-블랙 트리 속성 복구
    }


    // 삽입 시 트리 재조정을 위한 좌회전
    private void LeftRotate(Node x)
    {
        Node y = x.right;
        x.right = y.left;

        if (y.left != TNULL)
            y.left.parent = x;

        y.parent = x.parent;

        if (x.parent == null)
            root = y;
        else if (x == x.parent.left)
            x.parent.left = y;
        else
            x.parent.right = y;

        y.left = x;
        x.parent = y;
    }

    // 삽입 시 트리 재조정을 위한 우회전
    private void RightRotate(Node x)
    {
        Node y = x.left;
        x.left = y.right;

        if (y.right != TNULL)
            y.right.parent = x;

        y.parent = x.parent;

        if (x.parent == null)
            root = y;
        else if (x == x.parent.right)
            x.parent.right = y;
        else
            x.parent.left = y;

        y.right = x;
        x.parent = y;
    }

    // 삽입 후 레드-블랙 트리 속성 복구
    IEnumerator InsertFixup(Node k)
    {
        Node u;
        // k의 부모가 Red일 때
        while (k.parent != null && k.parent.color == NodeColor.Red)
        {
            // k의 부모가 k의 조부모의 오른쪽 자손
            if (k.parent == k.parent.parent.right)
            {
                // 삼촌노드
                u = k.parent.parent.left;
                // Case 1: 삼촌 노드가 Red인 경우 => Recoloring
                if (u != null && u.color == NodeColor.Red)
                {
                    // 색상 변경으로 해결
                    u.color = NodeColor.Black;
                    k.parent.color = NodeColor.Black;
                    k.parent.parent.color = NodeColor.Red;
                    k = k.parent.parent;
                }
                // Case 2 & 3: 삼촌 노드가 Black인 경우 => Restructuring
                else
                {
                    // k는 왼쪽자손
                    if (k == k.parent.left)
                    {
                        k = k.parent;
                        RightRotate(k);
                    }

                    // 색상 변경 및 회전으로 해결
                    k.parent.color = NodeColor.Black;
                    k.parent.parent.color = NodeColor.Red;
                    LeftRotate(k.parent.parent);
                }
            }
            // k의 부모가 k의 조부모의 왼쪽 자손
            else
            {
                // 삼촌노드
                u = k.parent.parent.right;
                // 삼촌노드가 red
                if (u != null && u.color == NodeColor.Red)
                {
                    u.color = NodeColor.Black;
                    k.parent.color = NodeColor.Black;
                    k.parent.parent.color = NodeColor.Red;
                    k = k.parent.parent;
                }
                // 삼촌노드가 black
                else
                {
                    if (k == k.parent.right)
                    {
                        k = k.parent;
                        LeftRotate(k);
                    }

                    k.parent.color = NodeColor.Black;
                    k.parent.parent.color = NodeColor.Red;
                    RightRotate(k.parent.parent);
                }
            }
            
            yield return StartCoroutine(UpdateVisualizer(root));

            if (k == root)
                break;
        }

        // 조건 2: 루트는 항상 Black
        root.color = NodeColor.Black;
        StartCoroutine(ColorUpdate());
    }

    void Update()
    {
        // Preorder(root);
        // Debug.Log("------------------");
    }

    public void Preorder(Node n)
    {
        if (n == null) return;
        
        Debug.Log(n.data);
        Preorder(n.left);
        Preorder(n.right);
    }
}
