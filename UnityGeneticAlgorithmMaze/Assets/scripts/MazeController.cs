using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MazeController : MonoBehaviour
{
    [NonSerialized] public Vector2 endPosition;
    public GameObject exitPrefab;
    public GeneticAlgorithm geneticAlgorithm;
    public int[,] map;
    public GameObject pathPrefab;
    [NonSerialized] public List<GameObject> pathTiles;
    [NonSerialized] public Vector2 startPosition;
    public GameObject startPrefab;
    public TextMeshProUGUI text;
    public GameObject wallPrefab;

    [Header("Game Settings")]
    [SerializeField] private float loopDelay = 0f;

    [SerializeField] private bool quickFind = true;
    [SerializeField] private bool workingMaze = true;

    public void ClearPathTiles()
    {
        foreach (GameObject pathTile in pathTiles)
        {
            Destroy(pathTile);
        }
        pathTiles.Clear();
    }

    public Vector2 Move(Vector2 position, int direction)
    {
        switch (direction)
        {
            case 0: // North
                if (position.y - 1 < 0 || map[(int)(position.y - 1), (int)position.x] == 1)
                {
                    break;
                }
                else
                {
                    position.y -= 1;
                }
                break;

            case 1: // South
                if (position.y + 1 >= map.GetLength(0) || map[(int)(position.y + 1), (int)position.x] == 1)
                {
                    break;
                }
                else
                {
                    position.y += 1;
                }
                break;

            case 2: // East
                if (position.x + 1 >= map.GetLength(1) || map[(int)position.y, (int)(position.x + 1)] == 1)
                {
                    break;
                }
                else
                {
                    position.x += 1;
                }
                break;

            case 3: // West
                if (position.x - 1 < 0 || map[(int)position.y, (int)(position.x - 1)] == 1)
                {
                    break;
                }
                else
                {
                    position.x -= 1;
                }
                break;
        }
        return position;
    }

    public void Populate()
    {
        Debug.Log("Map size is: (" + map.GetLength(1) + map.GetLength(0) + ")");

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                GameObject prefab = PrefabByTile(map[y, x]);
                if (prefab != null)
                {
                    GameObject wall = Instantiate(prefab);
                    wall.transform.position = new Vector3(x, 0, -y);
                }
            }
        }
    }

    public GameObject PrefabByTile(int tile)
    {
        if (tile == 1) return wallPrefab;
        if (tile == 2) return startPrefab;
        if (tile == 3) return exitPrefab;
        return null;
    }

    public void RenderFittestChromosomePath()
    {
        ClearPathTiles();
        Genome fittestGenome = geneticAlgorithm.genomes[geneticAlgorithm.fittestGenome];
        List<int> fittestDirections = geneticAlgorithm.Decode(fittestGenome.bits);
        Vector2 position = startPosition;

        foreach (int direction in fittestDirections)
        {
            position = Move(position, direction);
            GameObject pathTile = Instantiate(pathPrefab);
            pathTile.transform.position = new Vector3(position.x, 0, -position.y);
            pathTiles.Add(pathTile);
        }
    }

    public double TestRoute(List<int> directions)
    {
        Vector2 position = startPosition;

        int pathLength = 2;

        for (int directionIndex = 0; directionIndex < directions.Count; directionIndex++)
        {
            int nextDirection = directions[directionIndex];
            pathLength++;
            position = Move(position, nextDirection);
        }

        Vector2 deltaPosition = new Vector2(
            Math.Abs(position.x - endPosition.x),
            Math.Abs(position.y - endPosition.y));
        double result = 1 / ((double)((deltaPosition.x + deltaPosition.y) * ((deltaPosition.x + deltaPosition.y) / pathLength) + 1));
        if (result == 1)
            Debug.Log("TestRoute result=" + result + ",(" + position.x + "," + position.y + ")");
        return result;
    }

    // Use this for initialization
    private void Start()
    {
        if (workingMaze)
        {
            map = new int[,] {
      {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
      {1,0,1,0,0,0,0,0,1,1,1,0,0,0,1},
      {1,0,0,0,0,0,0,0,1,1,1,0,0,0,2},
      {1,0,0,0,1,1,1,0,0,1,0,0,0,1,1},
      {3,0,0,0,1,1,1,0,0,0,0,0,1,1,1},
      {1,1,0,0,0,0,0,0,0,0,0,0,1,1,1},
      {1,0,0,1,1,1,1,1,0,1,1,1,1,1,1},
      {1,1,0,1,1,1,1,1,1,1,0,0,0,0,1},
      {1,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
      };
        }
        else
        {
            map = new int[,] {
      {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
      {1,0,1,0,0,0,0,0,1,1,1,0,0,0,1},
      {1,0,0,0,0,0,0,0,1,1,1,0,0,0,2},
      {1,0,0,0,1,1,1,0,0,1,0,0,0,1,1},
      {1,0,0,0,1,1,1,0,0,0,0,0,1,1,1},
      {1,1,0,0,0,0,0,0,0,0,0,0,1,1,1},
      {1,0,0,1,1,1,1,1,0,1,1,1,1,1,1},
      {1,1,0,1,1,1,1,1,1,1,0,0,0,0,1},
      {1,1,0,0,0,0,0,0,3,0,0,0,0,0,1},
      {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
      };
        }

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                if (map[y, x] == 2)
                {
                    startPosition = new Vector2(x, y);
                    Debug.Log("Start Pos:" + startPosition);
                }
                else if (map[y, x] == 3)
                {
                    endPosition = new Vector2(x, y);
                    Debug.Log("End Pos:" + endPosition);
                }
            }
        }

        Populate();
        pathTiles = new List<GameObject>();

        geneticAlgorithm = new GeneticAlgorithm();
        geneticAlgorithm.mazeController = this;
        geneticAlgorithm.Run();

        if (!quickFind)
        {
            StartCoroutine(UpdatePath(loopDelay));
        }
    }

    private void Update()
    {
        if (quickFind)
        {
            if (geneticAlgorithm.busy)
            {
                geneticAlgorithm.Epoch();
            }
            RenderFittestChromosomePath();
            text.text = "Generation: " + geneticAlgorithm.generation;
        }
    }

    private IEnumerator UpdatePath(float waitTime)
    {
        while (true)
        {
            if (geneticAlgorithm.busy)
            {
                geneticAlgorithm.Epoch();
            }
            RenderFittestChromosomePath();
            text.text = "Generation: " + geneticAlgorithm.generation;

            yield return new WaitForSeconds(waitTime);
        }
    }
}