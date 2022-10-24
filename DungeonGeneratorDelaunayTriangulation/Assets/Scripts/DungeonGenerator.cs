using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

// Room IDs start at 10
public enum TileType
{
    Nothing = 0,
    Hallway = 1,
    Wall = 2,
    Door = 3
}

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator instance;

    [SerializeField] public int[] RoomSizeDistribution = new int[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 7, 8, 10, 12, 14 };
    [SerializeField] private RawImage dungeonMapTexture;
    private int EndRoomID;
    private int[,] Grid;
    private List<int> ItemRoomIDs;
    [SerializeField] private GameObject player;
    [SerializeField] private Image playerDot;
    private Vector3 playerSpawnLocation;
    private RoomGenerator RoomGenerator;
    private List<Room> Rooms;
    private bool roomsGenerated = false;
    private List<int> SecondaryRoomIDs;
    private int StartRoomID;

    #region Room Stuff

    [Header("Dungeon Variables")]
    [SerializeField]
    [Range(0, 2)]
    public int ItemRoomCount = 1;

    [SerializeField]
    [Range(0, 2)]
    private float MainRoomFrequency = 1f;

    private List<int> PrimaryRoomIDs;

    [SerializeField]
    [Range(1, 500)]
    private int Radius = 50;

    [SerializeField]
    [Range(0, 1)]
    private float RoomConnectionFrequency = 0.15f;

    [SerializeField]
    [Range(10, 1000)]
    private int RoomCount = 300;

    #endregion Room Stuff

    #region Colors

    [Header("Colors")]
    public Color DisabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    public Color DoorColor = Color.magenta;
    public Color EndRoomColor = new Color(195 / 255f, 85 / 255f, 165 / 255f);
    public Color HallwayColor = Color.red;
    public Color ItemRoomColor = new Color(0 / 255f, 255 / 255f, 255 / 255f);
    public Color MainColor = new Color(200f / 255f, 150f / 255f, 65 / 255f);
    public Color SecondaryColor = new Color(0.8f, 0.8f, 0.8f);
    public Color StartRoomColor = new Color(90 / 255f, 195 / 255f, 90 / 255f);
    public Color WallColor = Color.black;

    #endregion Colors

    #region Tile Prefabs

    [Header("Tile stuff")]
    [SerializeField]
    private Transform dungeon3D;

    [SerializeField]
    private GameObject primaryRoomTile, wallTile;

    #endregion Tile Prefabs

    private void AddDoor(Vector2 p0, Vector2 p1)
    {
        //Vertical direction
        if ((int)p1.x == (int)p0.x)
        {
            for (int y = (int)p0.y; y < (int)p1.y; y++)
            {
                if ((int)p1.x < Grid.GetLength(0) - 2)
                {
                    if ((Grid[(int)p1.x, y] == (int)TileType.Wall) && (Grid[(int)p1.x + 1, y] == (int)TileType.Wall) && (Grid[(int)p1.x + 2, y] == (int)TileType.Wall))
                    {
                        Grid[(int)p1.x, y] = (int)TileType.Door;
                        Grid[(int)p1.x + 1, y] = (int)TileType.Door;
                        Grid[(int)p1.x + 2, y] = (int)TileType.Door;
                    }
                }
                else
                {
                    if ((Grid[(int)p1.x, y] == (int)TileType.Wall) && (Grid[(int)p1.x - 1, y] == (int)TileType.Wall) && (Grid[(int)p1.x - 2, y] == (int)TileType.Wall))
                    {
                        Grid[(int)p1.x, y] = (int)TileType.Door;
                        Grid[(int)p1.x - 1, y] = (int)TileType.Door;
                        Grid[(int)p1.x - 2, y] = (int)TileType.Door;
                    }
                }
            }
        }

        //Horizontal direction
        else if ((int)p1.y == (int)p0.y)
        {
            for (int x = (int)p0.x; x < (int)p1.x; x++)
            {
                if ((int)p1.y < Grid.GetLength(1) - 2)
                {
                    if ((Grid[x, (int)p1.y] == (int)TileType.Wall) && (Grid[x, (int)p1.y + 1] == (int)TileType.Wall) && (Grid[x, (int)p1.y + 2] == (int)TileType.Wall))
                    {
                        Grid[x, (int)p1.y] = (int)TileType.Door;
                        Grid[x, (int)p1.y + 1] = (int)TileType.Door;
                        Grid[x, (int)p1.y + 2] = (int)TileType.Door;
                    }
                }
                else
                {
                    if ((Grid[x, (int)p1.y] == (int)TileType.Wall) && (Grid[x, (int)p1.y - 1] == (int)TileType.Wall) && (Grid[x, (int)p1.y - 2] == (int)TileType.Wall))
                    {
                        Grid[x, (int)p1.y] = (int)TileType.Door;
                        Grid[x, (int)p1.y - 1] = (int)TileType.Door;
                        Grid[x, (int)p1.y - 2] = (int)TileType.Door;
                    }
                }
            }
        }
    }

    private void AddHallway(Vector2 p0, Vector2 p1)
    {
        //Vertical direction
        if ((int)p1.x == (int)p0.x)
        {
            for (int y = (int)p0.y; y < (int)p1.y; y++)
            {
                //if the tile is nothing then make it a hallway
                if (Grid[(int)p1.x, y] == (int)TileType.Nothing)
                {
                    Grid[(int)p1.x, y] = (int)TileType.Hallway;
                }

                //make hallways 3 units wide
                if ((int)p1.x < Grid.GetLength(0) - 2)
                {
                    if (Grid[(int)p1.x + 1, y] == (int)TileType.Nothing)
                    {
                        Grid[(int)p1.x + 1, y] = (int)TileType.Hallway;
                    }
                    if (Grid[(int)p1.x + 2, y] == (int)TileType.Nothing)
                    {
                        Grid[(int)p1.x + 2, y] = (int)TileType.Hallway;
                    }
                }
                else
                {
                    if (Grid[(int)p1.x - 1, y] == (int)TileType.Nothing)
                    {
                        Grid[(int)p1.x - 1, y] = (int)TileType.Hallway;
                    }
                    if (Grid[(int)p1.x - 2, y] == (int)TileType.Nothing)
                    {
                        Grid[(int)p1.x - 2, y] = (int)TileType.Hallway;
                    }
                }
            }
        }

        //Horizontal direction
        else if ((int)p1.y == (int)p0.y)
        {
            for (int x = (int)p0.x; x < (int)p1.x; x++)
            {
                //if the tile is nothing then make it a hallway
                if (Grid[x, (int)p1.y] == (int)TileType.Nothing)
                {
                    Grid[x, (int)p1.y] = (int)TileType.Hallway;
                }
                //make hallways 3 units wide
                if ((int)p1.y < Grid.GetLength(1) - 2)
                {
                    if (Grid[x, (int)p1.y + 1] == (int)TileType.Nothing)
                    {
                        Grid[x, (int)p1.y + 1] = (int)TileType.Hallway;
                    }
                    if (Grid[x, (int)p1.y + 2] == (int)TileType.Nothing)
                    {
                        Grid[x, (int)p1.y + 2] = (int)TileType.Hallway;
                    }
                }
                else
                {
                    if (Grid[x, (int)p1.y - 1] == (int)TileType.Nothing)
                    {
                        Grid[x, (int)p1.y - 1] = (int)TileType.Hallway;
                    }
                    if (Grid[x, (int)p1.y - 2] == (int)TileType.Nothing)
                    {
                        Grid[x, (int)p1.y - 2] = (int)TileType.Hallway;
                    }
                }
            }
        }
    }

    private void AddWalls()
    {
        //Create a copy of current grid
        int[,] gridCopy = new int[Grid.GetLength(0), Grid.GetLength(1)];
        for (int x = 0; x < Grid.GetLength(0); x++)
        {
            for (int y = 0; y < Grid.GetLength(1); y++)
            {
                gridCopy[x, y] = Grid[x, y];
            }
        }

        //Process
        for (int x = 0; x < Grid.GetLength(0); x++)
        {
            for (int y = 0; y < Grid.GetLength(1); y++)
            {
                int val = gridCopy[x, y];

                //walls for primary rooms
                if (PrimaryRoomIDs.Contains(val))
                {
                    if (x > 0 && gridCopy[x - 1, y] != val && gridCopy[x - 1, y] != (int)TileType.Wall)
                    {
                        Grid[x - 1, y] = (int)TileType.Wall;
                    }
                    else if (x < gridCopy.GetLength(0) - 1 && gridCopy[x + 1, y] != val && gridCopy[x + 1, y] != (int)TileType.Wall)
                    {
                        Grid[x + 1, y] = (int)TileType.Wall;
                    }

                    if (y > 0 && gridCopy[x, y - 1] != val && gridCopy[x, y - 1] != (int)TileType.Wall)
                    {
                        Grid[x, y - 1] = (int)TileType.Wall;
                    }
                    else if (y < Grid.GetLength(1) - 1 && gridCopy[x, y + 1] != val && gridCopy[x, y + 1] != (int)TileType.Wall)
                    {
                        Grid[x, y + 1] = (int)TileType.Wall;
                    }
                }

                //Outside borders
                if (val == 0)
                {
                    if (x > 0 && gridCopy[x - 1, y] != (int)TileType.Nothing && gridCopy[x - 1, y] != (int)TileType.Wall)
                    {
                        Grid[x, y] = (int)TileType.Wall;
                    }
                    else if (x < Grid.GetLength(0) - 1 && gridCopy[x + 1, y] != (int)TileType.Nothing && gridCopy[x + 1, y] != (int)TileType.Wall)
                    {
                        Grid[x, y] = (int)TileType.Wall;
                    }

                    if (y > 0 && gridCopy[x, y - 1] != (int)TileType.Nothing && gridCopy[x, y - 1] != (int)TileType.Wall)
                    {
                        Grid[x, y] = (int)TileType.Wall;
                    }
                    else if (y < Grid.GetLength(1) - 1 && gridCopy[x, y + 1] != (int)TileType.Nothing && gridCopy[x, y + 1] != (int)TileType.Wall)
                    {
                        Grid[x, y] = (int)TileType.Wall;
                    }
                }
            }
        }

        //update grid copy
        for (int x = 0; x < Grid.GetLength(0); x++)
        {
            for (int y = 0; y < Grid.GetLength(1); y++)
            {
                gridCopy[x, y] = Grid[x, y];
            }
        }

        //Add doors based on connecting lines
        for (int n = 0; n < Rooms.Count; n++)
        {
            if (Rooms[n].IsMainRoom)
            {
                for (int m = 0; m < Rooms[n].Connections.Count; m++)
                {
                    //Line 1
                    Vector2 p0 = Rooms[n].Connections[m].Line1.p0.Value;
                    Vector2 p1 = Rooms[n].Connections[m].Line1.p1.Value;

                    if ((int)p0.x > (int)p1.x || (int)p0.y > (int)p1.y)
                    {
                        p1 = Rooms[n].Connections[m].Line1.p0.Value;
                        p0 = Rooms[n].Connections[m].Line1.p1.Value;
                    }
                    p0 = new Vector2(p0.x - RoomGenerator.XMin, p0.y - RoomGenerator.YMin);
                    p1 = new Vector2(p1.x - RoomGenerator.XMin, p1.y - RoomGenerator.YMin);

                    AddDoor(p0, p1);

                    //Line 2
                    p0 = Rooms[n].Connections[m].Line2.p0.Value;
                    p1 = Rooms[n].Connections[m].Line2.p1.Value;

                    if ((int)p0.x > (int)p1.x || (int)p0.y > (int)p1.y)
                    {
                        p1 = Rooms[n].Connections[m].Line2.p0.Value;
                        p0 = Rooms[n].Connections[m].Line2.p1.Value;
                    }
                    p0 = new Vector2(p0.x - RoomGenerator.XMin, p0.y - RoomGenerator.YMin);
                    p1 = new Vector2(p1.x - RoomGenerator.XMin, p1.y - RoomGenerator.YMin);

                    AddDoor(p0, p1);
                }
            }
        }
    }

    private void Awake()
    {
        Init();
    }

    private void Create3dDungeon()
    {
        int width = Grid.GetLength(0);
        int height = Grid.GetLength(1);

        bool playerHasSpawnLocation = false;

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (PrimaryRoomIDs.Contains(Grid[x, y]))
                {
                    createFloor(x, y, MainColor);
                }
                else if (SecondaryRoomIDs.Contains(Grid[x, y]))
                {
                    //createFloor(x, y, SecondaryColor);
                    createFloor(x, y, HallwayColor);
                }
                else if (ItemRoomIDs.Contains(Grid[x, y]))
                {
                    createFloor(x, y, ItemRoomColor);
                }
                else if (StartRoomID == (Grid[x, y]))
                {
                    createFloor(x, y, StartRoomColor);

                    if (!playerHasSpawnLocation)
                    {
                        playerSpawnLocation = new Vector3(x, 1, y);
                        //Instantiate(playerPrefab, new Vector3(x, 0, y), Quaternion.identity);
                        playerHasSpawnLocation = true;
                    }
                }
                else if (EndRoomID == (Grid[x, y]))
                {
                    createFloor(x, y, EndRoomColor);
                }
                else if (Grid[x, y] == 1)
                {
                    createFloor(x, y, HallwayColor);
                }
                else if (Grid[x, y] == 2)
                {
                    Instantiate(wallTile, new Vector3(x, 0, y), Quaternion.identity, dungeon3D);
                }
                else if (Grid[x, y] == 3)
                {
                    createFloor(x, y, DoorColor);
                }
            }
        }
    }

    private void createFloor(int x, int y, Color color)
    {
        GameObject tile = Instantiate(primaryRoomTile, new Vector3(x, 0, y), Quaternion.identity, dungeon3D);
        Material mat = tile.GetComponent<Renderer>().material;
        mat.color = color;
    }

    private void CreateGrid()
    {
        PrimaryRoomIDs = new List<int>();
        SecondaryRoomIDs = new List<int>();
        ItemRoomIDs = new List<int>();

        //Get our boundaries and list of primary and secondary room IDs
        for (int n = 0; n < Rooms.Count; n++)
        {
            if (Rooms[n].IsVisible)
            {
                if (Rooms[n].IsItemRoom)
                {
                    ItemRoomIDs.Add(Rooms[n].ID);
                }
                else if (Rooms[n].IsStartRoom)
                {
                    StartRoomID = Rooms[n].ID;
                }
                else if (Rooms[n].IsEndRoom)
                {
                    EndRoomID = Rooms[n].ID;
                }
                else if (Rooms[n].IsMainRoom)
                {
                    PrimaryRoomIDs.Add(Rooms[n].ID);
                }
                else
                {
                    SecondaryRoomIDs.Add(Rooms[n].ID);
                }
            }
        }

        //Add padding for walls
        int width = (int)(RoomGenerator.XMax - RoomGenerator.XMin) + 2;
        int height = (int)(RoomGenerator.YMax - RoomGenerator.YMin) + 2;

        Grid = new int[width, height];

        //Create initial grid with room IDs
        for (int n = 0; n < Rooms.Count; n++)
        {
            if (Rooms[n].IsVisible)
            {
                int startx = (int)Rooms[n].TopLeft.x - (int)RoomGenerator.XMin;
                int starty = (int)Rooms[n].BottomRight.y - (int)RoomGenerator.YMin;
                int endx = startx + (int)Rooms[n].transform.localScale.x;
                int endy = starty + (int)Rooms[n].transform.localScale.y;

                for (int x = startx; x < endx; x++)
                {
                    for (int y = starty; y < endy; y++)
                    {
                        Grid[x + 1, y + 1] = Rooms[n].ID;
                    }
                }
            }
        }

        //Complete missing sections of map based on LineCasts created in Room Generator
        for (int n = 0; n < Rooms.Count; n++)
        {
            if (Rooms[n].IsMainRoom)
            {
                for (int m = 0; m < Rooms[n].Connections.Count; m++)
                {
                    Vector2 p0 = Rooms[n].Connections[m].Line1.p0.Value;
                    Vector2 p1 = Rooms[n].Connections[m].Line1.p1.Value;
                    Vector2 p2 = Rooms[n].Connections[m].Line2.p0.Value;
                    Vector2 p3 = Rooms[n].Connections[m].Line2.p1.Value;

                    //flip values if line is going in opposite direction
                    if ((int)p0.x > (int)p1.x || (int)p0.y > (int)p1.y)
                    {
                        p0 = Rooms[n].Connections[m].Line1.p1.Value;
                        p1 = Rooms[n].Connections[m].Line1.p0.Value;
                    }
                    if ((int)p2.x > (int)p3.x || (int)p2.y > (int)p3.y)
                    {
                        p3 = Rooms[n].Connections[m].Line2.p0.Value;
                        p2 = Rooms[n].Connections[m].Line2.p1.Value;
                    }

                    //Adjust lines to grid coordinates
                    p0 = new Vector2(p0.x - RoomGenerator.XMin, p0.y - RoomGenerator.YMin);
                    p1 = new Vector2(p1.x - RoomGenerator.XMin, p1.y - RoomGenerator.YMin);
                    p2 = new Vector2(p2.x - RoomGenerator.XMin, p2.y - RoomGenerator.YMin);
                    p3 = new Vector2(p3.x - RoomGenerator.XMin, p3.y - RoomGenerator.YMin);

                    //Create the hallways in our grid
                    AddHallway(p0, p1);
                    AddHallway(p2, p3);
                }
            }
        }
    }

    private Texture2D CreateMapTexture()
    {
        int width = Grid.GetLength(0);
        int height = Grid.GetLength(1);

        var texture = new Texture2D(width, height);
        var pixels = new Color[width * height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                Color color = Color.clear;

                if (PrimaryRoomIDs.Contains(Grid[x, y]))
                {
                    color = MainColor;
                }
                else if (SecondaryRoomIDs.Contains(Grid[x, y]))
                {
                    color = HallwayColor; //SecondaryColor
                }
                else if (ItemRoomIDs.Contains(Grid[x, y]))
                {
                    color = ItemRoomColor;
                }
                else if (StartRoomID == (Grid[x, y]))
                {
                    color = StartRoomColor;
                }
                else if (EndRoomID == (Grid[x, y]))
                {
                    color = EndRoomColor;
                }
                else if (Grid[x, y] == 1)
                {
                    color = HallwayColor;
                }
                else if (Grid[x, y] == 2)
                {
                    color = WallColor;
                }
                else if (Grid[x, y] == 3)
                {
                    color = DoorColor;
                }

                pixels[x + y * width] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return texture;
    }

    private void Init()
    {
        //DungeonMapTexture = transform.Find("DungeonMapTexture").gameObject;
        RoomGenerator = transform.Find("RoomGenerator").GetComponent<RoomGenerator>();
        RoomGenerator.OnRoomsGenerated += RoomGenerator_OnRoomsGenerated;
        instance = this;
        //Generates rooms and room connections
        RoomGenerator.Generate(RoomCount, Radius, MainRoomFrequency, RoomConnectionFrequency);
    }

    private void RoomGenerator_OnRoomsGenerated()
    {
        //Create a copy of all Active rooms
        Rooms = RoomGenerator.Rooms;

        //Convert rooms into a grid of integers
        CreateGrid();
        AddWalls();

        dungeonMapTexture.texture = CreateMapTexture();

        Create3dDungeon();
        roomsGenerated = true;

        player.transform.position = playerSpawnLocation;
        Camera.main.enabled = false;
        player.SetActive(true);

        RoomGenerator.ClearData();
    }

    private void Update()
    {
        if (roomsGenerated)
        {
            playerDot.rectTransform.localPosition = new Vector3((player.transform.position.x / Grid.GetLength(0) * dungeonMapTexture.rectTransform.sizeDelta.x) - dungeonMapTexture.rectTransform.sizeDelta.x / 2
            , player.transform.position.z / Grid.GetLength(1) * dungeonMapTexture.rectTransform.sizeDelta.y - dungeonMapTexture.rectTransform.sizeDelta.y / 2, 0);
        }

        //Generate a new dungeon
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene("main");
    }
}