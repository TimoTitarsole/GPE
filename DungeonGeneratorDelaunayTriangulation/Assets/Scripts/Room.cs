using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    private SpriteRenderer Background;

    private Rigidbody2D RigidBody2D;

    public Vector3 BottomRight
    {
        get
        {
            return new Vector3(transform.position.x + transform.localScale.x / 2f, transform.position.y - transform.localScale.y / 2f);
        }
    }

    public Vector3 Center
    {
        get
        {
            return transform.position;
        }
    }

    public List<RoomConnection> Connections
    {
        get;
        private set;
    }

    public int ID
    {
        get;
        private set;
    }

    public bool IsEndRoom
    {
        get;
        private set;
    }

    public bool IsLocked
    {
        get;
        private set;
    }

    public bool IsMainRoom
    {
        get;
        private set;
    }

    public bool IsSleeping
    {
        get
        {
            return RigidBody2D.IsSleeping();
        }
    }

    public bool IsSpecialRoom
    {
        get;
        private set;
    }

    public bool IsStartRoom
    {
        get;
        private set;
    }

    public bool IsVisible
    {
        get;
        private set;
    }

    public Vector3 TopLeft
    {
        get
        {
            return new Vector3(transform.position.x - transform.localScale.x / 2f, transform.position.y + transform.localScale.y / 2f);
        }
    }

    public void AddRoomConnection(RoomConnection connection)
    {
        if (!Connections.Contains(connection))
        {
            Connections.Add(connection);
        }
    }

    public void Init(int id, Vector2 position, int width, int height)
    {
        ID = id;

        transform.position = position;
        transform.localScale = new Vector2(width, height);
    }

    public void SetEndRoom()
    {
        IsEndRoom = true;
    }

    public void SetLocked(bool locked)
    {
        IsLocked = locked;
    }

    public void SetMain()
    {
        IsMainRoom = true;
    }

    public void SetSpecialRoom()
    {
        IsSpecialRoom = true;
    }

    public void SetStartRoom()
    {
        IsStartRoom = true;
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;
    }

    public void Snap()
    {
        int x = Mathf.CeilToInt(transform.position.x);
        int y = Mathf.FloorToInt(transform.position.y);

        transform.position = new Vector2(x, y);
    }

    private void Awake()
    {
        Background = GetComponent<SpriteRenderer>();
        RigidBody2D = GetComponent<Rigidbody2D>();

        Connections = new List<RoomConnection>();

        IsVisible = true;
    }

    private void FixedUpdate()
    {
        if (!IsLocked)
        {
            Snap();
        }
    }

    private void Update()
    {
        if (IsVisible)
        {
            if (IsMainRoom)
            {
                if (IsStartRoom)
                    Background.color = DungeonGenerator.instance.StartRoomColor;
                else if (IsEndRoom)
                    Background.color = DungeonGenerator.instance.EndRoomColor;
                else if (IsSpecialRoom)
                    Background.color = DungeonGenerator.instance.SpecialRoomColor;
                else
                    Background.color = DungeonGenerator.instance.MainColor;
            }
            else
                Background.color = DungeonGenerator.instance.SecondaryColor;
        }
        else
        {
            Background.color = DungeonGenerator.instance.DisabledColor;
        }
    }
}