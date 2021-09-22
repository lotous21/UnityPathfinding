using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class MapData : MonoBehaviour
{
    public int width = 10;
    public int heigth = 5;
    public Camera mainCamera;
    Texture2D textureMap;
    TextAsset textAsset;

    public Color32 openColor = Color.white;
    public Color32 blockedColor = Color.black;
    public Color32 lightTerrainColor = new Color32(124, 194, 78, 255);
    public Color32 mediumTerrainColor = new Color32(252, 255, 52, 255);
    public Color32 heavyTerrainColor = new Color32(255, 129, 12, 255);

    public string Path = "MapData";

    static Dictionary<Color32, NodeType> terrainTable = new Dictionary<Color32, NodeType>();

    private void Awake()
    {
        terrainTable.Clear();
        textureMap = null;
        textAsset = null;
        SetUpTerrainTable();
    }

    public void SetDimenstion(List<string> textLines)
    {
        heigth = textLines.Count;

        foreach(string line in textLines)
        {
            if (line.Length > width)
            {
                width = line.Length;
            }
        }

        float cameraX = (float)((width) - 1f) / 2f;
        float cameraY = (float)((heigth) - 1f) / 2f;

        mainCamera.transform.position = new Vector3(cameraX, 1, cameraY);
    }

    public List<string> GetMapFromTextFile(TextAsset tAsset)
    {
        List<string> lines = new List<string>();

        if (tAsset != null)
        {
            string textData = tAsset.text;
            string[] delimiters =
            {
                "\r\n"
            };
            lines = textData.Split(delimiters, System.StringSplitOptions.None).ToList();
            lines.Reverse();
        }
        else
        {
            Debug.LogWarning("Ivalid Text Asset");
        }
        return lines;
    }
    public List<string> GetMapFromTextFile()
    {
        return GetMapFromTextFile(textAsset);
    }

    public List<string> GetMapFromTexture(Texture2D texture)
    {
        List<string> lines = new List<string>();

        for (int y = 0; y < texture.height; y++)
        {
            string newLine = "";

            for(int x = 0; x < texture.width; x++)
            {
                Color pixelColor = texture.GetPixel(x, y);

                if (terrainTable.ContainsKey(pixelColor))
                {
                    NodeType nodeType = terrainTable[pixelColor];
                    int nodeTypeNum = (int)nodeType;
                    newLine += nodeTypeNum;
                }
                else
                {
                    newLine += '0';
                }
            }

            lines.Add(newLine);
        }

        return lines;
    }

    public int[,] MakeMap()
    {
        List<string> lines = new List<string>();

        if (textureMap != null) 
        {
            lines = GetMapFromTexture(textureMap);
        }
        else
        {
            lines = GetMapFromTextFile();
        }


        SetDimenstion(lines);
        int[,] map = new int[width, heigth];
        for (int y = 0; y < heigth; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (lines[y].Length > x)
                {
                    map[x, y] = (int)char.GetNumericValue(lines[y][x]);
                }
            }
        }

        return map;
    }

    void SetUpTerrainTable()
    {
        terrainTable.Add(openColor, NodeType.Open);
        terrainTable.Add(blockedColor, NodeType.Blocked);
        terrainTable.Add(lightTerrainColor, NodeType.LightTerrain);
        terrainTable.Add(mediumTerrainColor, NodeType.MediumTerrain);
        terrainTable.Add(heavyTerrainColor, NodeType.HeavyTerrain);
    }

    public static Color GetColorFromNodeType(NodeType nodeType)
    {
        if (terrainTable.ContainsValue(nodeType))
        {
            Color colorKey = terrainTable.FirstOrDefault(x => x.Value == nodeType).Key;
            return colorKey;
        }
        return Color.white;
    }

    public void SetMap(TMP_Dropdown dropdown, Controller contoller)
    {
        if (dropdown.value == 0)
        {
            textAsset = Resources.Load<TextAsset>("MapData/Maze");
            textureMap = null;
            contoller.goalX = 15;
            contoller.goalY = 11;
        }
        else if (dropdown.value == 1)
        {
            textAsset = null;
            textureMap = Resources.Load<Texture2D>("MapData/ConcaveWalls32x18");
            contoller.goalX = 31;
            contoller.goalY = 17;
        }
        else if (dropdown.value == 2)
        {
            textAsset = null;
            textureMap = Resources.Load<Texture2D>("MapData/terrain32x18");
            contoller.goalX = 31;
            contoller.goalY = 17;
        }
        else if (dropdown.value == 3)
        {
            textAsset = null;
            textureMap = Resources.Load<Texture2D>("MapData/ThreeWalls64x36");
            contoller.goalX = 63;
            contoller.goalY = 35;
        }
    }
}
