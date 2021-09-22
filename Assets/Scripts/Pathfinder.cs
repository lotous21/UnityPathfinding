using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class Pathfinder : MonoBehaviour
{
    Node m_startNode;
    Node m_goalNode;
    Graph m_graph;
    GraphView m_graphView;

    PriorityQueue<Node> m_frontierNodes;
    List<Node> m_exploredNode;
    List<Node> m_pathNodes;

    public Color startColor = Color.green;
    public Color goalColor = Color.red;
    public Color frontierColor = Color.magenta;
    public Color exploredColor = Color.gray;
    public Color PathColor = Color.cyan;
    public Color ArrowColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    public Color HighlightColor = new Color(1f,1f,0.5f,1f);

    public bool isComplete = false;
    int m_iterations = 0;

    public bool exitOnGoal;
    public bool showIterations;
    public bool showArrows;
    public bool showColors;

    public enum Mode
    {
        BreadthFirstSearch = 0,
        Djikstra = 1,
        GreedyBestFirst =2,
        AStar = 3
    }

    public Mode mode = Mode.BreadthFirstSearch;

    public void Init(Graph graph, GraphView graphView, Node start, Node goal)
    {
        if (graph == null || graphView == null || start == null || goal == null)
        {
            Debug.LogWarning("Missing Components");
            return;
        }

        if (start.nodeType == NodeType.Blocked || goal.nodeType == NodeType.Blocked)
        {
            Debug.LogWarning("Goal/Start are blocked type");
        }

        m_graph = graph;
        m_graphView = graphView;
        m_startNode = start;
        m_goalNode = goal;

        m_frontierNodes = new PriorityQueue<Node>();
        m_frontierNodes.Enqueue(start);
        m_exploredNode = new List<Node>();
        m_pathNodes = new List<Node>();

        for (int x = 0; x < m_graph.Width; x++)
        {
            for (int y =0; y < m_graph.Height; y++)
            {
                m_graph.nodes[x, y].ResetNode();
            }
        }

        isComplete = false;
        m_iterations = 0;
        m_startNode.distanceTraveled = 0;

    }
    void ShowColors(GraphView graphView, Node start, Node goal, bool lerpColor = false, float lerpValue = 0.5f)
    {
        if (graphView == null || start == null || goal == null)
        {
            Debug.LogWarning("Missing Components");
            return;
        }

        if (m_frontierNodes != null)
        {
            graphView.ColorNodes(m_frontierNodes.ToList(), frontierColor, lerpColor, lerpValue);
        }

        if (m_exploredNode != null)
        {
            graphView.ColorNodes(m_exploredNode, exploredColor, lerpColor, lerpValue);
        }

        if (m_pathNodes != null && m_pathNodes.Count > 0)
        {
            graphView.ColorNodes(m_pathNodes, PathColor, lerpColor, lerpValue * 2);
        }

        NodeView startNodeView = graphView.nodeViews[start.xIndex, start.yIndex];
        if (startNodeView != null)
        {
            startNodeView.ColorNode(startColor);
        }

        NodeView goalNodeView = graphView.nodeViews[goal.xIndex, goal.yIndex];
        if (goalNodeView != null)
        {
            goalNodeView.ColorNode(goalColor);
        }
    }
    void ShowColors(bool lerpColor = false, float lerpValue = 0.5f)
    {
        ShowColors(m_graphView, m_startNode, m_goalNode, lerpColor, lerpValue);
    }
    public IEnumerator SearchRoutine(float timeStep = 0.1f)
    {
        float timeStart = Time.time;
        yield return null;

        while (!isComplete)
        {
            if (m_frontierNodes.Count > 0)
            {
                Node currentNode = m_frontierNodes.Dequeue();
                m_iterations++;

                if (!m_exploredNode.Contains(currentNode))
                {
                    m_exploredNode.Add(currentNode);
                }

                if (mode == Mode.BreadthFirstSearch)
                {
                    ExpandFrontierBreadthFirst(currentNode);
                }
                else if (mode == Mode.Djikstra)
                {
                    ExpandFrontierDjikstra(currentNode);
                }
                else if (mode == Mode.GreedyBestFirst)
                {
                    ExpandFrontierGreedyBreadthFirst(currentNode);
                }
                else if (mode == Mode.AStar)
                {
                    ExpandFrontierAStar(currentNode);
                }

                if (m_frontierNodes.Contains(m_goalNode))
                {
                    m_pathNodes = GetPathNodes(m_goalNode);
                    if (exitOnGoal)
                    {
                        isComplete = true;
                        Debug.Log("Pathfinfer Mode: " + mode.ToString() + "    path length = " + m_goalNode.distanceTraveled.ToString());
                    }
                }

                if (showIterations)
                {
                    ShowDiagnostics(true, 0.5f);
                    yield return new WaitForSeconds(timeStep);
                }                                     
            }
            else 
            {
                isComplete = true;

            }            
        }
        ShowDiagnostics(true, 0.5f);
        Debug.Log("Search Path Time: " + (Time.time - timeStart).ToString() + " Seconds");
    }
    void ExpandFrontierBreadthFirst (Node node)
    {
        if (node != null)
        {
            for (int i = 0; i< node.neighbors.Count; i++)
            {
                if (!m_exploredNode.Contains(node.neighbors[i]) && !m_frontierNodes.Contains(node.neighbors[i]))
                {
                    float distanceToNeighbor = m_graph.GetNodeDistance(node, node.neighbors[i]);
                    float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int)node.nodeType;
                    node.neighbors[i].distanceTraveled = newDistanceTraveled;
                    node.neighbors[i].previousNode = node;
                    node.neighbors[i].priority = m_exploredNode.Count;
                    m_frontierNodes.Enqueue(node.neighbors[i]);
                }
            }
        }
    }
    void ExpandFrontierDjikstra(Node node)
    {
        if (node != null)
        {
            for (int i = 0; i < node.neighbors.Count; i++)
            {
                if (!m_exploredNode.Contains(node.neighbors[i]))
                {
                    float distanceToNeighbor = m_graph.GetNodeDistance(node, node.neighbors[i]);
                    float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int)node.nodeType;

                    if (float.IsPositiveInfinity(node.neighbors[i].distanceTraveled) || newDistanceTraveled < node.neighbors[i].distanceTraveled)
                    {
                        node.neighbors[i].previousNode = node;
                        node.neighbors[i].distanceTraveled = newDistanceTraveled;
                    }

                    if (!m_frontierNodes.Contains(node.neighbors[i]))
                    {
                        node.neighbors[i].priority = (int)node.neighbors[i].distanceTraveled;
                        m_frontierNodes.Enqueue(node.neighbors[i]);
                    }

                }
            }
        }
    }
    void ExpandFrontierGreedyBreadthFirst(Node node)
    {
        if (node != null)
        {
            for (int i = 0; i < node.neighbors.Count; i++)
            {
                if (!m_exploredNode.Contains(node.neighbors[i]) && !m_frontierNodes.Contains(node.neighbors[i]))
                {
                    float distanceToNeighbor = m_graph.GetNodeDistance(node, node.neighbors[i]);
                    float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int)node.nodeType;
                    node.neighbors[i].distanceTraveled = newDistanceTraveled;
                    node.neighbors[i].previousNode = node;
                    if (m_graph != null)
                    {
                        node.neighbors[i].priority = (int)m_graph.GetNodeDistance(node.neighbors[i], m_goalNode);
                    }
                    m_frontierNodes.Enqueue(node.neighbors[i]);
                }
            }
        }
    }
    void ExpandFrontierAStar(Node node)
    {
        if (node != null)
        {
            for (int i = 0; i < node.neighbors.Count; i++)
            {
                if (!m_exploredNode.Contains(node.neighbors[i]))
                {
                    float distanceToNeighbor = m_graph.GetNodeDistance(node, node.neighbors[i]);
                    float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int)node.nodeType;

                    if (float.IsPositiveInfinity(node.neighbors[i].distanceTraveled) || newDistanceTraveled < node.neighbors[i].distanceTraveled)
                    {
                        node.neighbors[i].previousNode = node;
                        node.neighbors[i].distanceTraveled = newDistanceTraveled;
                    }

                    if (!m_frontierNodes.Contains(node.neighbors[i]) && m_graph != null)
                    {
                        int distanceToGoal = (int)m_graph.GetNodeDistance(node.neighbors[i], m_goalNode);

                        node.neighbors[i].priority = (int)node.neighbors[i].distanceTraveled + distanceToGoal;
                        m_frontierNodes.Enqueue(node.neighbors[i]);
                    }

                }
            }
        }
    }
    List<Node> GetPathNodes(Node endNode)
    {
        List<Node> path = new List<Node>();
        if (endNode == null)
        {
            return path;
        }
        path.Add(endNode);

        Node currentNode = endNode.previousNode;

        while (currentNode != null) 
        {
            path.Insert(0, currentNode);
            currentNode = currentNode.previousNode;
        }

        return path;
    }
    void ShowDiagnostics(bool lerpColor, float lerpValue)
    {
        if (showColors)
        {
            ShowColors(lerpColor, lerpValue);
        }

        if (m_graphView != null && showArrows)
        {
            m_graphView.ShowNodeArrow(m_frontierNodes.ToList(), ArrowColor);

            if (m_frontierNodes.Contains(m_goalNode))
            {
                m_graphView.ShowNodeArrow(m_pathNodes, HighlightColor);
            }
        }
    }
    public void SetMode(TMP_Dropdown dropdown)
    {
        if (dropdown.value == 0)
        {
            mode = Mode.BreadthFirstSearch;
        }
        else if (dropdown.value == 1)
        {
            mode = Mode.GreedyBestFirst;
        }
        else if (dropdown.value == 2)
        {
            mode = Mode.Djikstra;
        }
        else if (dropdown.value == 3)
        {
            mode = Mode.AStar;
        }
    }
}
