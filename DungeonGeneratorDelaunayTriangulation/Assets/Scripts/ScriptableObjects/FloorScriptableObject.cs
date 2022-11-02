using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/FloorScriptableObject", order = 1)]
public class FloorScriptableObject : ScriptableObject
{
    [SerializeField] public int[] RoomSizeDistribution = new int[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 7, 8, 10, 12, 14 };

    #region Room Stuff

    [Header("Dungeon Variables")]
    [SerializeField]
    [Range(0, 2)]
    public int ItemRoomCount = 2;

    [SerializeField]
    [Range(0, 2)]
    public float MainRoomFrequency = 1.5f;

    [SerializeField]
    [Range(1, 500)]
    public int Radius = 60;

    [SerializeField]
    [Range(0, 1)]
    public float RoomConnectionFrequency = 0.15f;

    [SerializeField]
    [Range(10, 1000)]
    public int RoomCount = 100;

    #endregion Room Stuff

    #region Colors

    [Header("Colors")]
    public Color CeilingColor = new Color(0.5f, 0.5f, 0.5f);

    public Color DisabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    public Color DoorColor = new Color(0.735849f, 0.43402f, 0f);
    public Color EndRoomColor = new Color(1f, 0.05638304f, 0f);
    public Color HallwayColor = new Color(0.7641f, 0.7641f, 0.7641f);
    public Color ItemRoomColor = new Color(0f, 1f, 1f);
    public Color MainColor = new Color(0.7843137f, 0.5882353f, 0.254902f);
    public Color SecondaryColor = new Color(0.7641f, 0.7641f, 0.7641f);
    public Color StartRoomColor = new Color(0.2457366f, 0.8113208f, 0f);
    public Color WallColor = Color.black;

    #endregion Colors
}