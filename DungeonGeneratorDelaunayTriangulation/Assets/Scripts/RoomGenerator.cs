using UnityEngine;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using UnityEngine.SceneManagement;

public class RoomGenerator : MonoBehaviour
{
    public DungeonGenerator dungeonGenerator;
    private Dictionary<Room, int> ConnectionCounter = new Dictionary<Room, int>();

    private List<LineSegment> DelaunayTriangulation;

    private bool Done = false;

    private GameObject LinesContainer;

    private Dictionary<Vector2, Room> MainRooms;

    private List<Vector2> Points = new List<Vector2>();

    private float RoomConnectionFrequency = 0.15f;

    private GameObject RoomsContainer;

    private List<LineSegment> SpanningTree;

    public bool IsValid
    {
        get;
        private set;
    }

    public List<Room> Rooms
    {
        get;
        private set;
    }

    public float XMax
    {
        get;
        private set;
    }

    public float XMin
    {
        get;
        private set;
    }

    public float YMax
    {
        get;
        private set;
    }

    public float YMin
    {
        get;
        private set;
    }

    public void ClearData()
    {
        for (int n = 0; n < Rooms.Count; n++)
        {
            GameObject.Destroy(Rooms[n].gameObject);
        }
        Rooms.Clear();

        GameObject.Destroy(LinesContainer);
        GameObject.Destroy(RoomsContainer);
    }

    public void Generate(int roomCount, int radius, float mainRoomFrequency, float roomConnectionFrequency)
    {
        RoomsContainer = new GameObject("Rooms");
        LinesContainer = new GameObject("Lines");

        RoomConnectionFrequency = roomConnectionFrequency;

        int totalWidth = 0, totalHeight = 0;
        Rooms = new List<Room>();

        //Initialize our rooms
        for (int n = 0; n < roomCount; n++)
        {
            Room room = (GameObject.Instantiate(Resources.Load("Room") as GameObject)).GetComponent<Room>();
            room.dungeonGenerator = dungeonGenerator;
            room.transform.parent = RoomsContainer.transform;

            int width = dungeonGenerator.RoomSizeDistribution[Random.Range(0, dungeonGenerator.RoomSizeDistribution.Length)];
            int height = dungeonGenerator.RoomSizeDistribution[Random.Range(0, dungeonGenerator.RoomSizeDistribution.Length)];
            Vector2 position = GetRandomPositionInCircle(radius);

            totalWidth += width;
            totalHeight += height;

            //Start rooms at ID 10
            room.Init(n + 10, position, width, height);
            Rooms.Add(room);
        }

        //find the main rooms
        float widthAvg = totalWidth / roomCount;
        float heightAvg = totalHeight / roomCount;

        for (int n = 0; n < Rooms.Count; n++)
        {
            if (Rooms[n].transform.localScale.x >= (2.35f - mainRoomFrequency) * widthAvg && Rooms[n].transform.localScale.y
                >= (2.35f - mainRoomFrequency) * heightAvg)
            {
                Rooms[n].SetMain();
            }
        }
    }

    //In order for our dungeon to look interesting, we will add more connections to our minimum spanning tree
    private void AddExtraConnections(bool drawLines)
    {
        List<int> range = new List<int>();
        for (int n = 0; n < DelaunayTriangulation.Count; n++)
        {
            range.Add(n);
        }

        for (int n = 0; n < (int)(DelaunayTriangulation.Count * RoomConnectionFrequency); n++)
        {
            int idx = Random.Range(0, range.Count);
            int value = range[idx];
            range.RemoveAt(idx);

            if (drawLines)
            {
                GameObject line = GameObject.Instantiate(Resources.Load("Line") as GameObject);
                line.GetComponent<LineRenderer>().SetPosition(0, DelaunayTriangulation[value].p0.Value);
                line.GetComponent<LineRenderer>().SetPosition(1, DelaunayTriangulation[value].p1.Value);
                line.GetComponent<LineRenderer>().sortingOrder = 100;
                line.GetComponent<LineRenderer>().startColor = Color.blue;
                line.GetComponent<LineRenderer>().endColor = Color.blue;

                line.transform.parent = LinesContainer.transform;
            }

            //Create an index so we can keep track of the actual connections count of each main room
            if (!ConnectionCounter.ContainsKey(MainRooms[DelaunayTriangulation[value].p0.Value]))
                ConnectionCounter.Add(MainRooms[DelaunayTriangulation[value].p0.Value], 0);
            if (!ConnectionCounter.ContainsKey(MainRooms[DelaunayTriangulation[value].p1.Value]))
                ConnectionCounter.Add(MainRooms[DelaunayTriangulation[value].p1.Value], 0);

            //increment the counter
            ConnectionCounter[MainRooms[DelaunayTriangulation[value].p0.Value]]++;
            ConnectionCounter[MainRooms[DelaunayTriangulation[value].p1.Value]]++;

            MainRooms[DelaunayTriangulation[value].p0.Value].AddRoomConnection(CreateRoomConnection(DelaunayTriangulation[value].p0.Value, DelaunayTriangulation[value].p1.Value));
        }
    }

    private void Calculate()
    {
        List<uint> colors = new List<uint>();
        MainRooms = new Dictionary<Vector2, Room>();

        //Get a point list of all our main rooms
        for (int n = 0; n < Rooms.Count; n++)
        {
            if (Rooms[n].IsMainRoom)
            {
                Points.Add(Rooms[n].Center);
                colors.Add(0);

                if (!MainRooms.ContainsKey(Rooms[n].Center))
                    MainRooms.Add(Rooms[n].Center, Rooms[n]);
            }
        }

        //Calculate min spanning tree
        Voronoi v = new Voronoi(Points, colors, new Rect(0, 0, 50, 50));
        SpanningTree = v.SpanningTree(KruskalType.MINIMUM);
        DelaunayTriangulation = v.DelaunayTriangulation();

        //Add room connections
        GenerateSpanningTree(true);
        ProcessRoomConnections(false);

        SetSpecialRooms();

        //Calculate boundaries
        XMin = float.MaxValue;
        YMin = float.MaxValue;
        XMax = float.MinValue;
        YMax = float.MinValue;

        for (int n = 0; n < Rooms.Count; n++)
        {
            if (Rooms[n].IsVisible)
            {
                XMin = Mathf.Min(XMin, Rooms[n].TopLeft.x);
                XMax = Mathf.Max(XMax, Rooms[n].BottomRight.x);
                YMin = Mathf.Min(YMin, Rooms[n].BottomRight.y);
                YMax = Mathf.Max(YMax, Rooms[n].TopLeft.y);
            }
        }

        dungeonGenerator.onRoomsGenerated = true;
    }

    private RoomConnection CreateRoomConnection(Vector2 p0, Vector2 p1)
    {
        //Create the room connection
        Room room = MainRooms[p1];

        //Determine the direction of the connection
        ConnectionType direction = ConnectionType.Up;

        float xdiff = Mathf.Abs(p0.x - p1.x);
        float ydiff = Mathf.Abs(p0.y - p1.y);

        if (xdiff > ydiff)
        {
            if (p0.x > p1.x)
            {
                direction = ConnectionType.Left;
            }
            else
            {
                direction = ConnectionType.Right;
            }
        }
        else
        {
            if (p0.y > p1.y)
            {
                direction = ConnectionType.Down;
            }
            else
            {
                direction = ConnectionType.Up;
            }
        }

        return new RoomConnection(room, direction);
    }

    //Create the smallest path possible between all our main rooms
    private void GenerateSpanningTree(bool drawLines)
    {
        for (int n = 0; n < SpanningTree.Count; n++)
        {
            if (drawLines)
            {
                GameObject line = GameObject.Instantiate(Resources.Load("Line") as GameObject);
                line.GetComponent<LineRenderer>().SetPosition(0, SpanningTree[n].p0.Value);
                line.GetComponent<LineRenderer>().SetPosition(1, SpanningTree[n].p1.Value);
                line.GetComponent<LineRenderer>().sortingOrder = 100;
                line.GetComponent<LineRenderer>().startColor = Color.red;
                line.GetComponent<LineRenderer>().endColor = Color.red;
                line.transform.parent = LinesContainer.transform;
            }

            //Create an index so we can keep track of the actual connections count of each main room
            if (!ConnectionCounter.ContainsKey(MainRooms[SpanningTree[n].p0.Value]))
                ConnectionCounter.Add(MainRooms[SpanningTree[n].p0.Value], 0);
            if (!ConnectionCounter.ContainsKey(MainRooms[SpanningTree[n].p1.Value]))
                ConnectionCounter.Add(MainRooms[SpanningTree[n].p1.Value], 0);

            //increment the counter
            ConnectionCounter[MainRooms[SpanningTree[n].p0.Value]]++;
            ConnectionCounter[MainRooms[SpanningTree[n].p1.Value]]++;

            //Add the room connection to the Room object
            MainRooms[SpanningTree[n].p0.Value].AddRoomConnection(CreateRoomConnection(SpanningTree[n].p0.Value, SpanningTree[n].p1.Value));
        }

        AddExtraConnections(drawLines);
    }

    private Vector2 GetRandomPositionInCircle(float radius)
    {
        float angle = Random.Range(0f, 1f) * Mathf.PI * 2f;

        float rad = Mathf.Sqrt(Random.Range(0f, 1f)) * radius;
        float x = this.transform.localPosition.x + rad * Mathf.Cos(angle);
        float y = this.transform.localPosition.y + rad * Mathf.Sin(angle);

        return new Vector2((int)x, (int)y);
    }

    //Create the lines between main rooms and find secondary rooms
    private void ProcessRoomConnections(bool drawLines)
    {
        for (int n = 0; n < Rooms.Count; n++)
        {
            Rooms[n].SetVisible(false);
        }

        for (int n = 0; n < Rooms.Count; n++)
        {
            if (Rooms[n].IsMainRoom)
            {
                Rooms[n].SetVisible(true);

                for (int i = 0; i < Rooms[n].Connections.Count; i++)
                {
                    Room connectingRoom = Rooms[n].Connections[i].Room;
                    ConnectionType direction = Rooms[n].Connections[i].Direction;

                    //Get line points
                    Vector2 p0 = Rooms[n].Center;
                    Vector2 p1 = connectingRoom.Center;
                    Vector2 p2 = Vector2.zero;
                    Vector2 p3 = Vector2.zero;

                    if (direction == ConnectionType.Up)
                    {
                        p2 = new Vector2(p0.x, p1.y);
                        p3 = p2;
                        //Hallways are off by 3 pixels in this direction only.  Not sure why.
                        //Adjust by 3 units
                        if (p0.x > p1.x)
                        {
                            p3 = new Vector2(p0.x, p1.y + 3);
                        }
                    }
                    else if (direction == ConnectionType.Right)
                    {
                        p2 = new Vector2(p1.x, p0.y);
                        p3 = p2;
                    }
                    else if (direction == ConnectionType.Down)
                    {
                        p2 = new Vector2(p0.x, p1.y);
                        p3 = p2;
                    }
                    else if (direction == ConnectionType.Left)
                    {
                        p2 = new Vector2(p1.x, p0.y);
                        p3 = p2;
                    }

                    //LineCast for collisions.  Hit objects will become secondary rooms.
                    RaycastHit2D[] hit = Physics2D.LinecastAll(p0, p2);
                    for (int x = 0; x < hit.Length; x++)
                    {
                        hit[x].collider.GetComponent<Room>().SetVisible(true);
                    }

                    hit = Physics2D.LinecastAll(p2, p1);
                    for (int x = 0; x < hit.Length; x++)
                    {
                        hit[x].collider.GetComponent<Room>().SetVisible(true);
                    }

                    //Store lines
                    Rooms[n].Connections[i].Line1 = new LineSegment(p0, p3);
                    Rooms[n].Connections[i].Line2 = new LineSegment(p2, p1);
                }
            }
        }
    }

    private void SetSpecialRooms()
    {
        List<Room> roomsWithOneConnection = new List<Room>();
        List<Room> roomsWithTwoConnection = new List<Room>();

        //check connection counters
        foreach (KeyValuePair<Room, int> kvp in ConnectionCounter)
        {
            if (kvp.Value == 1)
            {
                roomsWithOneConnection.Add(kvp.Key);
            }
            if (kvp.Value == 2)
            {
                roomsWithTwoConnection.Add(kvp.Key);
            }
        }

        Room start = null;
        Room end = null;
        List<Room> items = new List<Room>();
        float distance = 0;
        int endID = 0;
        bool endHasOneConnection = false;

        //attempt to grab start room
        if (roomsWithOneConnection.Count >= 1)
        {
            start = roomsWithOneConnection[0];
            roomsWithOneConnection.RemoveAt(0);
        }
        else if (roomsWithTwoConnection.Count > 1)
        {
            start = roomsWithTwoConnection[0];
            roomsWithTwoConnection.RemoveAt(0);
        }

        //attempt to grab end room
        if (start != null)
        {
            for (int n = 0; n < roomsWithOneConnection.Count; n++)
            {
                float d = (roomsWithOneConnection[n].Center - start.Center).magnitude;
                if (d > distance)
                {
                    distance = d;
                    end = roomsWithOneConnection[n];
                    endID = n;
                    endHasOneConnection = true;
                }
            }
            for (int n = 0; n < roomsWithTwoConnection.Count; n++)
            {
                float d = (roomsWithTwoConnection[n].Center - start.Center).magnitude;
                if (d > distance)
                {
                    distance = d;
                    end = roomsWithTwoConnection[n];
                    endID = n;
                    endHasOneConnection = false;
                }
            }
            if (end != null)
            {
                if (endHasOneConnection)
                {
                    roomsWithOneConnection.RemoveAt(endID);
                }
                else
                {
                    roomsWithTwoConnection.RemoveAt(endID);
                }
            }
        }

        if (start != null && end != null)
        {
            for (int i = 0; i < dungeonGenerator.ItemRoomCount; i++)
            {
                if (roomsWithOneConnection.Count >= 1)
                {
                    int n = Random.Range(0, roomsWithOneConnection.Count);
                    items.Add(roomsWithOneConnection[n]);
                    roomsWithOneConnection.RemoveAt(n);
                }
                else if (roomsWithTwoConnection.Count > 1)
                {
                    int n = Random.Range(0, roomsWithTwoConnection.Count);
                    items.Add(roomsWithTwoConnection[n]);
                    roomsWithTwoConnection.RemoveAt(0);
                }
            }
        }

        if (start != null && end != null && items.Count == dungeonGenerator.ItemRoomCount)
        {
            start.SetStartRoom();
            end.SetEndRoom();
            foreach (Room room in items)
            {
                room.SetItemRoom();
            }
            IsValid = true;
        }
        else
        {
            IsValid = false;
            SceneManager.LoadScene("main");
        }
    }

    private void Update()
    {
        bool allSleeping = true;
        for (int n = 0; n < Rooms.Count; n++)
        {
            if (!Rooms[n].IsSleeping)
            {
                allSleeping = false;
                Rooms[n].SetLocked(false);
            }
            else
            {
                Rooms[n].SetLocked(true);
            }
        }

        //Check if all physics objects are done settling
        if (allSleeping && !Done)
        {
            Done = true;
            Calculate();
        }
    }
}