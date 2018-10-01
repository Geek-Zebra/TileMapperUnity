﻿using UnityEditor;
using UnityEngine;

public class TileMapper : EditorWindow
{
    int tileSize = 5;
    int mapSize = 50;
    int scrollBarSize = 30;
    int buttonSize = 35;
    int margin = 150;
    int selectedButton = 0;
    MapData tileMapper;
    Vector2 scrollPosSelection;
    Vector2 scrollPosTileEditor;
    private bool isFocused;
    private bool isCameraReplaced;
    private bool wasCameraOrthographic;
    private bool isCameraMemorized;
    private Vector3 lastCameraPosition;
    private Vector3 lastCameraRoation;
    GameObject[] tilesModels;
    string[] guids1;
    Texture2D[] previews;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/My Window")]
    static void Init()
    {
        TileMapper window = (TileMapper)EditorWindow.GetWindow(typeof(TileMapper));
        window.Show();

    }

    void OnFocus()
    {
        tileMapper = FindObjectOfType<MapData>();
        tileMapper.SetTiles(mapSize * mapSize);
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        isFocused = true;
    }

    void OnLostFocus()
    {
        isFocused = false;
        isCameraMemorized = false;
        isCameraReplaced = false;
    }

    void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (isFocused && !isCameraMemorized)
        {
            isCameraMemorized = true;
            wasCameraOrthographic = SceneView.lastActiveSceneView.orthographic;
            lastCameraRoation = sceneView.rotation.eulerAngles;
        }

        if (isFocused)
        {
            //SceneView.lastActiveSceneView.orthographic = false;
            //sceneView.LookAt(Vector3.zero, Quaternion.Euler(90, 0, 0));
            //SceneView.lastActiveSceneView.Repaint();
            //sceneView.size = mapSize * tileSize;
        }

        if (!isFocused && !isCameraReplaced)
        {
            isCameraReplaced = true;
            //SceneView.lastActiveSceneView.orthographic = wasCameraOrthographic;
            //sceneView.rotation = Quaternion.Euler(lastCameraRoation);
        }
    }

    void setTiles()
    {
        guids1 = AssetDatabase.FindAssets("l:terrain");
        tilesModels = new GameObject[guids1.Length];
        previews = new Texture2D[guids1.Length];
        for (var i = 0; i < guids1.Length; i++)
        {
            GameObject gameObject = (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids1[i]), typeof(GameObject));
            tilesModels[i] = gameObject;
        }
    }

    void OnGUI()
    {
        if (guids1 == null)
            setTiles();

        //setTiles();
        GUILayout.BeginHorizontal();

        scrollPosSelection = EditorGUILayout.BeginScrollView(scrollPosSelection, GUILayout.MaxWidth(margin));
        var deleteButtonSize = 30;
        var imageButtonSize = 75;
        var labelSise = 15;
        var marginSize = 10;
        var numberOfVisibleElements = 12;
        var blockSize = labelSise + imageButtonSize + marginSize;
        if(selectedButton == -1)
            GUI.color = Color.red;

        if (GUILayout.Button("Delete", GUILayout.MaxWidth(margin - scrollBarSize), GUILayout.Height(deleteButtonSize)))
        {
            selectedButton = -1;
        }
        GUI.color = Color.white;
        for (var i = 0; i < guids1.Length; i++)
        {
            if (i == selectedButton)
                GUI.color = Color.green;

            if (scrollPosSelection.y < (i * blockSize) + deleteButtonSize && scrollPosSelection.y > (i * blockSize) - blockSize * numberOfVisibleElements)
            {
                if (previews[i] == null && !AssetPreview.IsLoadingAssetPreviews())
                {
                    previews[i] = AssetPreview.GetAssetPreview(tilesModels[i]);
                }
            } else {
                previews[i] = null;
            }
            GUILayout.Label(tilesModels[i].name, GUILayout.MaxWidth(margin - scrollBarSize), GUILayout.Height(labelSise));
            if (GUILayout.Button(previews[i], GUILayout.Height(imageButtonSize)))
            {
                selectedButton = i;
            }
            GUI.color = Color.white;
        }

        GUILayout.EndScrollView();
        scrollPosTileEditor = EditorGUILayout.BeginScrollView(scrollPosTileEditor, GUILayout.Width(position.width - margin), GUILayout.Height(position.height));
        var decimals = 0;
        GUILayout.Label("", GUILayout.Width(mapSize * buttonSize), GUILayout.Height(mapSize * buttonSize));
        for (var i = 0; i < mapSize; i++)
        {
            for (var j = 0; j < mapSize; j++)
            {
                if (tileMapper.GetTiles()[i + (j * mapSize)] != null)
                    GUI.color = Color.green;

                if (GUI.Button(new Rect(i * buttonSize, j * buttonSize, buttonSize, buttonSize), (i + (j * mapSize)).ToString()))
                {
                    if (selectedButton != -1)
                    {

                        GameObject clone = PrefabUtility.InstantiatePrefab(tilesModels[selectedButton] as GameObject) as GameObject;
                        var gameObjectInstance = tileMapper.AddTile(i + (j * mapSize), clone, new Vector3(i * tileSize, 0, -j * tileSize));

                        if (clone.gameObject.GetComponent<TileData>().TileSize.x > 1)
                        {
                            tileMapper.AddExtraSpace((i + 1) + (j * mapSize), gameObjectInstance, new Vector3(i * tileSize, 0, -j * tileSize));
                        }
                        if (clone.gameObject.GetComponent<TileData>().TileSize.y > 1)
                            tileMapper.AddExtraSpace(i + ((j + 1) * mapSize), gameObjectInstance, new Vector3(i * tileSize, 0, -j * tileSize));
                    }
                    else
                        tileMapper.RemoveTile(i + (j * mapSize));
                }
                GUI.color = Color.white;
            }
            decimals++;
        }

        GUILayout.EndScrollView();
        GUILayout.EndHorizontal();
    }
}