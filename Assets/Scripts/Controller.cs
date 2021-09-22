using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class Controller : MonoBehaviour
{
    public MapData mapData;
    public Graph graph;
    public Pathfinder pathfinder;
    public int startX = 0;
    public int startY = 0;
    public int goalX = 15;
    public int goalY = 1;

    public GameObject UIObj;
    public TMP_Dropdown MapDropdown;
    public TMP_Dropdown AlgorithmsDropdown;
    public TMP_InputField SpeedInput;
    public Button StartButton;

    public float timeStep = 0.1f;

    private void Start()
    {
        UIObj.transform.Find("MainPanel").gameObject.SetActive(true);
        UIObj.transform.Find("InGame").gameObject.SetActive(false);
        StartButton.onClick.AddListener(() => startGame());
    }

    private void Update()
    {
        if (SpeedInput.text.Length == 0)
        {
            SpeedInput.text = "0";
        }

        if (UIObj.transform.Find("InGame").gameObject.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene("Pathfinding");
            }
        }
    }

    void startGame()
    {
        pathfinder.GetComponent<Pathfinder>().SetMode(AlgorithmsDropdown);
        mapData.GetComponent<MapData>().SetMap(MapDropdown, this);
        SetSpeed(SpeedInput);

        UIObj.transform.Find("MainPanel").gameObject.SetActive(false);
        UIObj.transform.Find("InGame").gameObject.SetActive(true);

        GraphView graphView = graph.GetComponent<GraphView>();

        if (mapData != null && graph != null)
        {
            int[,] mapInstance = mapData.MakeMap();
            graph.Init(mapInstance);

            if (graphView != null)
            {
                graphView.Init(graph);
            }
        }

        if (graph.IsWithinBounds(startX, startY) && graph.IsWithinBounds(goalX, goalY) && pathfinder != null)
        {
            Node startNode = graph.nodes[startX, startY];
            Node goalNode = graph.nodes[goalX, goalY];
            pathfinder.Init(graph, graphView, startNode, goalNode);
            StartCoroutine(pathfinder.SearchRoutine(timeStep));
        }
    }

    void SetSpeed(TMP_InputField input)
    {
        if (float.TryParse(input.text, out timeStep))
        {
            timeStep = float.Parse(input.text);
        }
        else
        {
            timeStep = 0;
        }
    }
}
