using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    private SerializedProperty blTileProp;
    private SerializedProperty bmTileProp;
    private SerializedProperty brTileProp;
    private SerializedProperty mlTileProp;
    private SerializedProperty mmTileProp;
    private SerializedProperty mrTileProp;
    private bool showTiles = true;
    private SerializedProperty tlTileProp;
    private SerializedProperty tmTileProp;
    private SerializedProperty trTileProp;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        showTiles = EditorGUILayout.Foldout(showTiles, "Tiles");
        if (showTiles)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("T", GUILayout.Width(20));
            EditorGUILayout.PropertyField(tlTileProp, new GUIContent(""));
            EditorGUILayout.PropertyField(tmTileProp, new GUIContent(""));
            EditorGUILayout.PropertyField(trTileProp, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("M", GUILayout.Width(20));
            EditorGUILayout.PropertyField(mlTileProp, new GUIContent(""));
            EditorGUILayout.PropertyField(mmTileProp, new GUIContent(""));
            EditorGUILayout.PropertyField(mrTileProp, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("B", GUILayout.Width(20));
            EditorGUILayout.PropertyField(blTileProp, new GUIContent(""));
            EditorGUILayout.PropertyField(bmTileProp, new GUIContent(""));
            EditorGUILayout.PropertyField(brTileProp, new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        DungeonGenerator myScript = (DungeonGenerator)target;
        if (GUILayout.Button("Generate Dungeon"))
        {
            myScript.GenerateDungeon();
        }
    }

    private void OnEnable()
    {
        tlTileProp = serializedObject.FindProperty("tlTile");
        tmTileProp = serializedObject.FindProperty("tmTile");
        trTileProp = serializedObject.FindProperty("trTile");
        mlTileProp = serializedObject.FindProperty("mlTile");
        mmTileProp = serializedObject.FindProperty("mmTile");
        mrTileProp = serializedObject.FindProperty("mrTile");
        blTileProp = serializedObject.FindProperty("blTile");
        bmTileProp = serializedObject.FindProperty("bmTile");
        brTileProp = serializedObject.FindProperty("brTile");
    }
}