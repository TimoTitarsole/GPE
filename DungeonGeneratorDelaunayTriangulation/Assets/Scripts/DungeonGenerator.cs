using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;
using System;

//using VoronoiNS;
using System.Linq;

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
    [NonSerialized]
    public int floor = 0;

    [NonSerialized]
    public Texture floorMapTexture;

    [NonSerialized]
    public int[,] Grid;

    [NonSerialized]
    public bool onRoomsGenerated = false, done = false;

    [NonSerialized]
    public Vector3 playerSpawnLocation;

    private int EndRoomID;

    private FloorScriptableObject floorData;
    private List<int> ItemRoomIDs;
    private List<int> PrimaryRoomIDs;
    private RoomGenerator RoomGenerator;
    private List<Room> Rooms;
    private List<int> SecondaryRoomIDs;
    private int StartRoomID;

    #region Tile Prefabs

    private Transform dungeon3D;

    [Header("Tile stuff")]
    [SerializeField]
    private GameObject endTrigger;

    [SerializeField]
    private GameObject primaryRoomTile;

    [SerializeField]
    private GameObject wallTile;

    #endregion Tile Prefabs

    #region Floor Data

    [NonSerialized]
    public int[] RoomSizeDistribution;

    #region Room Stuff Data

    [NonSerialized]
    public int ItemRoomCount;

    [NonSerialized]
    public float MainRoomFrequency;

    [NonSerialized]
    public int Radius;

    [NonSerialized]
    public float RoomConnectionFrequency;

    [NonSerialized]
    public int RoomCount;

    #endregion Room Stuff Data

    #region Colors Data

    [Header("Colors")]
    [NonSerialized]
    public Color DisabledColor, DoorColor, EndRoomColor, HallwayColor, ItemRoomColor, MainColor, SecondaryColor, StartRoomColor, WallColor, CeilingColor;

    #endregion Colors Data

    #endregion Floor Data

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
                        if (x > 0 && gridCopy[x - 1, y] != val && gridCopy[x - 1, y] != (int)TileType.Hallway)
                        {
                            Grid[x - 1, y] = (int)TileType.Wall;
                        }
                    }
                    else if (x < gridCopy.GetLength(0) - 1 && gridCopy[x + 1, y] != val && gridCopy[x + 1, y] != (int)TileType.Wall)
                    {
                        if (x < gridCopy.GetLength(0) - 1 && gridCopy[x + 1, y] != val && gridCopy[x + 1, y] != (int)TileType.Hallway)
                        {
                            Grid[x + 1, y] = (int)TileType.Wall;
                        }
                    }

                    if (y > 0 && gridCopy[x, y - 1] != val && gridCopy[x, y - 1] != (int)TileType.Wall)
                    {
                        if (y > 0 && gridCopy[x, y - 1] != val && gridCopy[x, y - 1] != (int)TileType.Hallway)
                        {
                            Grid[x, y - 1] = (int)TileType.Wall;
                        }
                    }
                    else if (y < Grid.GetLength(1) - 1 && gridCopy[x, y + 1] != val && gridCopy[x, y + 1] != (int)TileType.Wall)
                    {
                        if (y < Grid.GetLength(1) - 1 && gridCopy[x, y + 1] != val && gridCopy[x, y + 1] != (int)TileType.Hallway)
                        {
                            Grid[x, y + 1] = (int)TileType.Wall;
                        }
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

    private void AddWallsBad()
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
                        playerSpawnLocation = new Vector3(x, 1 + floor * 10, y);
                        playerHasSpawnLocation = true;
                    }
                }
                else if (EndRoomID == (Grid[x, y]))
                {
                    createFloor(x, y, EndRoomColor);

                    Instantiate(endTrigger, new Vector3(x, dungeon3D.transform.position.y, y), Quaternion.identity, dungeon3D);
                }
                else if (Grid[x, y] == 1)
                {
                    createFloor(x, y, HallwayColor);
                }
                else if (Grid[x, y] == 3)
                {
                    createFloor(x, y, DoorColor);
                }
                else if (Grid[x, y] == 2)
                {
                    Instantiate(wallTile, new Vector3(x, dungeon3D.transform.position.y, y), Quaternion.identity, dungeon3D);
                }
            }
        }
        CreateCeiling();
    }

    private void CreateCeiling()
    {
        float width = Grid.GetLength(0);

        float length = Grid.GetLength(1);

        float height = 3.5f + 10 * floor;

        GameObject ceiling = new GameObject("Ceiling");
        ceiling.transform.parent = dungeon3D;

        MeshRenderer meshRenderer = ceiling.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = Resources.Load("Ceiling") as Material; //new Material(Shader.Find("Unlit/Color"));
        meshRenderer.sharedMaterial.color = CeilingColor;

        MeshFilter meshFilter = ceiling.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, height, 0),
            new Vector3(width, height, 0),
            new Vector3(0, height, length),
            new Vector3(width, height, length)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6]
        {
            // lower left triangle
            0, 1, 2,
            // upper right triangle
            2, 1, 3
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        meshFilter.mesh = mesh;
    }

    private void createFloor(int x, int y, Color color)
    {
        GameObject tile = Instantiate(primaryRoomTile, new Vector3(x, dungeon3D.transform.position.y, y), Quaternion.identity, dungeon3D);
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
        LoadFloorData();
        GameObject dungeonList = new GameObject("dungeonList");
        dungeonList.transform.parent = transform;
        dungeon3D = dungeonList.transform;
        Vector3 newPos = new Vector3(0, 10 * floor, 0);
        dungeon3D.transform.position = newPos;
        RoomGenerator = transform.Find("RoomGenerator").GetComponent<RoomGenerator>();
        RoomGenerator.dungeonGenerator = this;
        //Generates rooms and room connections
        RoomGenerator.Generate(RoomCount, Radius, MainRoomFrequency, RoomConnectionFrequency);
    }

    private void LoadFloorData()
    {
        floor = Dungeon.floorNumber;
        floorData = Dungeon.instance.floorScriptableObjects[floor];
        RoomSizeDistribution = floorData.RoomSizeDistribution;
        ItemRoomCount = floorData.ItemRoomCount;
        MainRoomFrequency = floorData.MainRoomFrequency;
        Radius = floorData.Radius;
        RoomConnectionFrequency = floorData.RoomConnectionFrequency;
        RoomCount = floorData.RoomCount;

        DisabledColor = floorData.DisabledColor;
        DoorColor = floorData.DoorColor;
        EndRoomColor = floorData.EndRoomColor;
        HallwayColor = floorData.HallwayColor;
        ItemRoomColor = floorData.ItemRoomColor;
        MainColor = floorData.MainColor;
        SecondaryColor = floorData.SecondaryColor;
        StartRoomColor = floorData.StartRoomColor;
        WallColor = floorData.WallColor;
        CeilingColor = floorData.CeilingColor;
    }

    private void OnRoomsGenerated()
    {
        //Create a copy of all Active rooms
        Rooms = RoomGenerator.Rooms;

        //Convert rooms into a grid of integers
        CreateGrid();
        AddWalls();
        //AddWallsBad();

        floorMapTexture = CreateMapTexture();

        Create3dDungeon();

        RoomGenerator.ClearData();
        onRoomsGenerated = false;
        done = true;
    }

    private void Update()
    {
        if (onRoomsGenerated)
        {
            OnRoomsGenerated();
        }
    }
}