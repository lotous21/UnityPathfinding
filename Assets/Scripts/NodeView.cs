using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeView : MonoBehaviour
{
    public GameObject Tile;
    public GameObject Arrow;
    Node m_node;

    [Range(0,0.5f)]
    public float borderSize = 0.15f;

    public void Init(Node node)
    {
        if (Tile != null)
        {
            gameObject.name = "Node (" + node.xIndex + "," + node.yIndex + ")";
            gameObject.transform.position = node.position;
            Tile.transform.localScale = new Vector3(1f - borderSize, 1f, 1f - borderSize);
            m_node = node;
            EnableObject(Arrow, false);
        }
    }

    public void ColorNode(Color color, GameObject go)
    {
        if (go != null)
        {
            Renderer goRenderer = go.GetComponent<Renderer>();
            if (goRenderer != null)
            {
                goRenderer.material.color = color;
            }
        }
    }

    public void ColorNode(Color color)
    {
        ColorNode(color, Tile);
    }

    void EnableObject(GameObject go, bool state)
    {
        if (go != null)
        {
            go.SetActive(state);
        }
    }

    public void ShowArrows(Color color)
    {
        if (m_node != null && Arrow != null && m_node.previousNode != null)
        {
            EnableObject(Arrow, true);

            Vector3 dirToPrevious = (m_node.previousNode.position - m_node.position).normalized;
            Arrow.transform.rotation = Quaternion.LookRotation(dirToPrevious);

            Renderer renderer = Arrow.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }
    }
}
