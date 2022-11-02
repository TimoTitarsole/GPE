using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Dungeon : MonoBehaviour
{
    public static bool finish = false;

    [NonSerialized]
    public static int floorNumber = 0;

    public static Dungeon instance;

    [NonSerialized]
    public int currentFloor = 0;

    [NonSerialized]
    public bool done = false;

    [NonSerialized]
    public List<DungeonGenerator> generators = new List<DungeonGenerator>();

    [SerializeField]
    [Range(0, 10)]
    private int floorAmount = 0;

    [SerializeField]
    private GameObject generatorPrefab;

    #region playerStuff

    [SerializeField] private RawImage dungeonMapTexture;
    [SerializeField] private GameObject player;
    [SerializeField] private Image playerDot;

    #endregion playerStuff

    public void FinishedFloor()
    {
        if (currentFloor + 1 < floorAmount)
        {
            currentFloor += 1;
            dungeonMapTexture.texture = generators[currentFloor].floorMapTexture;
            player.transform.position = generators[currentFloor].playerSpawnLocation;
        }
        else
        {
            Debug.Log("You Win");
        }
    }

    private void OnDungeonGenerated()
    {
        player.transform.position = generators[currentFloor].playerSpawnLocation;
        Camera.main.enabled = false;
        dungeonMapTexture.transform.parent.gameObject.SetActive(true);
        player.SetActive(true);

        done = true;
    }

    private void Start()
    {
        instance = this;
        StartCoroutine(StartFunction());
    }

    private IEnumerator StartFunction()
    {
        currentFloor = 0;
        for (int i = 0; i < floorAmount; i++)
        {
            floorNumber = i;
            GameObject obj = Instantiate(generatorPrefab, transform);
            DungeonGenerator gen = obj.GetComponent<DungeonGenerator>();
            generators.Add(gen);

            yield return new WaitUntil(() => gen.done);
        }

        dungeonMapTexture.texture = generators[currentFloor].floorMapTexture;
        OnDungeonGenerated();
        yield return null;
    }

    private void Update()
    {
        if (done)
        {
            playerDot.rectTransform.localPosition = new Vector3((player.transform.position.x / generators[currentFloor].Grid.GetLength(0) * dungeonMapTexture.rectTransform.sizeDelta.x) - dungeonMapTexture.rectTransform.sizeDelta.x / 2
            , player.transform.position.z / generators[currentFloor].Grid.GetLength(1) * dungeonMapTexture.rectTransform.sizeDelta.y - dungeonMapTexture.rectTransform.sizeDelta.y / 2, 0);
        }

        //Generate a new dungeon
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("main");
        }

        if (finish)
        {
            finish = false;
            FinishedFloor();
        }
    }
}