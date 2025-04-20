using System;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static BoidSummoner;
[CustomEditor(typeof(LevelBox))]
public class LevelBoxEditor : Editor
{
    SerializedProperty position;
    SerializedProperty centerAtHeightZero;

    SerializedProperty height;
    SerializedProperty xWidth;
    SerializedProperty zWidth;


    SerializedProperty wallParent;
    SerializedProperty wallThickness;

    SerializedProperty wallPrefab;
    SerializedProperty floorPrefab;


    SerializedProperty outlineColor;
    SerializedProperty showType;
    SerializedProperty drawEachWall;

    bool isShow = true;

    bool showWalls = true;

    void OnEnable()
    {
        // Setup the SerializedProperties.
        position = serializedObject.FindProperty("position");
        centerAtHeightZero = serializedObject.FindProperty("centerAtHeightZero");

        height = serializedObject.FindProperty("height");
        xWidth = serializedObject.FindProperty("xWidth");
        zWidth = serializedObject.FindProperty("zWidth");

        wallParent = serializedObject.FindProperty("wallParent");
        wallThickness = serializedObject.FindProperty("wallThickness");
        wallPrefab = serializedObject.FindProperty("wallPrefab");
        floorPrefab = serializedObject.FindProperty("floorPrefab");

        outlineColor = serializedObject.FindProperty("outlineColor");
        showType = serializedObject.FindProperty("showType"); 
        drawEachWall = serializedObject.FindProperty("drawEachWall");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(position, new GUIContent("Level Center"));
        EditorGUILayout.PropertyField(centerAtHeightZero, new GUIContent("Have Level Centered from Bottom of level?"));
        EditorGUILayout.Space();
        isShow = EditorGUILayout.BeginFoldoutHeaderGroup(isShow, new GUIContent("Dimensions"), null, null);
        if (isShow)
        {
            EditorGUILayout.PropertyField(height, new GUIContent("Level Height"));
            EditorGUILayout.PropertyField(xWidth, new GUIContent("Level X Width"));
            EditorGUILayout.PropertyField(zWidth, new GUIContent("Level Z Width"));

        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(new GUIContent("Wall Stuff"));

        EditorGUILayout.PropertyField(wallParent, new GUIContent("Wall Parent Object"));

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(wallThickness, new GUIContent("Wall Thickness"));

        EditorGUILayout.PropertyField(wallPrefab, new GUIContent("Wall Prefab"));
        EditorGUILayout.PropertyField(floorPrefab, new GUIContent("Wall Prefab"));

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(new GUIContent("Editor Options"));

        EditorGUILayout.PropertyField(outlineColor, new GUIContent("Outline Color"));
        EditorGUILayout.PropertyField(showType, new GUIContent("Outline Draw Type"));
        EditorGUILayout.PropertyField(drawEachWall, new GUIContent("Draw Each Wall?"));

        serializedObject.ApplyModifiedProperties();
        
    }
    private void OnSceneGUI()
    {
    }

}
