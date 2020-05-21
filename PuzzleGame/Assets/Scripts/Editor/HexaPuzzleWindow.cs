using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityScript;

using Hexa;

public class HexaPuzzleWindow : EditorWindow
{
    private List<string> dotPaths = new List<string>();
    private string backGroundTilePath;
    private string nodePath;

    int minHeight = 0;
    int totalWidth = 0;
    int maxHeight = 0;

    ObjectInfo dotInfo = new ObjectInfo();
    ObjectInfo tileInfo = new ObjectInfo();
    List<ObjectInfo> currentDotList = new List<ObjectInfo>();

    bool isOpenStep1 = true;
    bool isOpenStep2 = true;
    bool isOpenStep3 = true;
    bool isOpenStep4 = true;

    Vector2 scrollPos;

    [MenuItem("Window/Puzzle Maker/Hexa")]

    static void Init()
    {
        HexaPuzzleWindow myWindow = (HexaPuzzleWindow)EditorWindow.GetWindow(typeof(HexaPuzzleWindow));
        myWindow.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("! Make a Hexa Puzzle !", EditorStyles.boldLabel);
        GUILayout.Space(6.0f);

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        // 보드판 만들기
        isOpenStep1 = EditorGUILayout.Foldout(isOpenStep1, "Step 1. Make a board");
        if (isOpenStep1)
        {
            MakeMenu_Board();
        }
        GUILayout.Space(20.0f);

        // 타일 만들기
        isOpenStep2 = EditorGUILayout.Foldout(isOpenStep2, "Step 2. Make Tiles");
        if (isOpenStep2)
        {
            MakeMenu_Tile();
        }
        GUILayout.Space(20.0f);

        // 폭탄 만들기
        isOpenStep3 = EditorGUILayout.Foldout(isOpenStep3, "Step 3. Make dots");
        if (isOpenStep3)
        {
            MakeMenu_Dot();
        }
        GUILayout.Space(20.0f);

        GUILayout.EndScrollView();

        if (GUILayout.Button("! Make a puzzle stage !", GUILayout.Height(30.0f)))
        {
            SetDots();
        }
        GUILayout.Space(10.0f);
    }

    public void OnInspectorUpdate()
    {
        this.Repaint();
    }

    void MakeMenu_Board()
    {
        GUILayout.Label("First, you need to set the board's size.", EditorStyles.helpBox);
        minHeight = EditorGUILayout.IntField("minimum Height", minHeight, GUILayout.Width(250.0f));
        maxHeight = EditorGUILayout.IntField("maximun Height", maxHeight, GUILayout.Width(250.0f));
        totalWidth = EditorGUILayout.IntField("total Width", totalWidth, GUILayout.Width(250.0f));

        GUILayout.Space(5.0f);
        if (GUILayout.Button("Create Board", GUILayout.Width(150.0f)))
        {
            MakeBoard(minHeight, maxHeight, totalWidth);
        }
    }

    void MakeMenu_Tile()
    {
        tileInfo.name = EditorGUILayout.TextField("Tile prefab name", tileInfo.name, GUILayout.Width(350.0f));

        tileInfo.color = EditorGUILayout.ColorField("Tile Color", tileInfo.color, GUILayout.Width(370.0f));
        tileInfo.image = (Sprite)EditorGUILayout.ObjectField(tileInfo.image, typeof(Sprite), false, GUILayout.Width(370.0f));
        if (tileInfo.image != null)
        {
            GUILayout.Box(tileInfo.image.texture, GUILayout.Width(120.0f), GUILayout.Height(120.0f));
        }

        GUILayout.Space(3.0f);
        if (GUILayout.Button("Create Tile", GUILayout.Width(150.0f)))
        {
            MakeBackGroundTile(tileInfo.image, tileInfo.color, tileInfo.name);
            
            SetTiles();
        }
    }

    void MakeMenu_Dot()
    {
        GUILayout.BeginHorizontal();
        for (int i = 0; i < currentDotList.Count; i++)
        {
            if (GUILayout.Button(currentDotList[i].image.texture, GUILayout.Width(50.0f), GUILayout.Height(50.0f)))
            {
                dotInfo = currentDotList[i];
            }
            //(Sprite)EditorGUILayout.ObjectField(currentDotList[i].image, typeof(Sprite), false, GUILayout.Width(370.0f));
        }
        GUILayout.EndHorizontal();

        dotInfo.name = EditorGUILayout.TextField("Dot prefab name", dotInfo.name, GUILayout.Width(350.0f));
        dotInfo.color = EditorGUILayout.ColorField("Dot Color", dotInfo.color, GUILayout.Width(370.0f));
        dotInfo.image = (Sprite)EditorGUILayout.ObjectField(dotInfo.image, typeof(Sprite), false, GUILayout.Width(370.0f));
        if (dotInfo.image != null)
        {
            GUILayout.Box(dotInfo.image.texture, GUILayout.Width(80.0f), GUILayout.Height(80.0f));
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create", GUILayout.Width(150.0f)))
        {
            MakeDotObject(dotInfo.image, dotInfo.color, dotInfo.name, dotInfo.name);
        }
        if (GUILayout.Button("Remove", GUILayout.Width(150.0f)))
        {
            RemoveDot(dotInfo.name);
        }
        GUILayout.EndHorizontal();
    }

    void MakeBoard(int minHeight, int maxHeight, int totalWidth)
    {
        Board board = FindObjectOfType<Board>();
        Normal.Board normalBoard = FindObjectOfType<Normal.Board>();

        if (board != null)
        {
            DestroyImmediate(board.gameObject, true);
        }
        else if (normalBoard != null)
        {
            DestroyImmediate(normalBoard.gameObject, true);
        }

        GameObject tmpBoard = new GameObject("Board");
        tmpBoard.AddComponent<Board>();
        board = tmpBoard.GetComponent<Board>();

        GameObject node = new GameObject("Node");
        node.AddComponent<Node>();
        node.AddComponent<CircleCollider2D>();
        node.GetComponent<CircleCollider2D>().radius = 0.4f;
        node.layer = LayerMask.NameToLayer("Node");
        nodePath = MakePrefab(node);
        DestroyImmediate(node, true);

        board.maxHeight = maxHeight;
        board.minHeight = minHeight;
        board.totalWidth = totalWidth;

        board.nodePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(nodePath);

        AddCameraScalar();
        MakeFindMatches();
    }

    // 카메라 세팅 붙이기
    void AddCameraScalar()
    {
        GameObject mainCamera = GameObject.Find("Main Camera");
        mainCamera.AddComponent<CameraScalar>();
    }

    // 매치를 판단해주는 오브젝트 생성
    void MakeFindMatches()
    {
        FindMatches findMatches = FindObjectOfType<FindMatches>();

        if (findMatches != null)
        {
            return;
        }
        else
        {
            GameObject tmpBoard = new GameObject("FindMatches");
            tmpBoard.AddComponent<FindMatches>();
        }

    }

    void MakeBackGroundTile(Sprite sprite, Color color, string name)
    {
        if (name == null)
        {
            return;
        }

        GameObject backGroundTile = new GameObject(name);
        backGroundTile.AddComponent<SpriteRenderer>();

        backGroundTile.GetComponent<SpriteRenderer>().sprite = sprite;
        backGroundTile.GetComponent<SpriteRenderer>().color = color;

        backGroundTilePath = MakePrefab(backGroundTile);
        DestroyImmediate(backGroundTile, true);
    }

    string MakePrefab(GameObject obj)
    {
        string path = "Assets/Prefabs/" + obj.name + ".prefab";
        AssetDatabase.DeleteAsset(path);
        //PrefabUtility.SavePrefabAsset(obj);
        PrefabUtility.CreatePrefab(path, obj);

        return path;
    }

    // 보드에 사용될 타일을 할당
    void SetTiles()
    {
        Board board = FindObjectOfType<Board>();

        if (board != null)
        {
            Debug.Log(backGroundTilePath);
            board.tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(backGroundTilePath);
        }
    }

    // 도트 세팅, 프리펩 등록
    void MakeDotObject(Sprite sprite, Color color, string name, string tag)
    {
        if (name == null)
        {
            return;
        }

        GameObject dot = new GameObject(name);
        AddTag(tag);
        dot.tag = tag;

        dot.AddComponent<SpriteRenderer>();
        dot.AddComponent<Dot>();

        dot.GetComponent<SpriteRenderer>().sprite = sprite;
        dot.GetComponent<SpriteRenderer>().color = color;

        string dotPath = MakePrefab(dot);

        if (!dotPaths.Contains(dotPath))
        {
            dotPaths.Add(dotPath);
        }

        DestroyImmediate(dot, true);

        UpdatePaths(dotPaths);
        CurrentDotListUpdate();
        Debug.Log(currentDotList.Count);
    }

    void RemoveDot(string dotName)
    {
        if (dotName == null)
        {
            return;
        }

        string dotPath = "Assets/Prefabs/" + dotName + ".prefab";
        if (dotPaths.Contains(dotPath))
        {
            dotPaths.Remove(dotPath);
        }
        UpdatePaths(dotPaths);
        CurrentDotListUpdate();

        AssetDatabase.DeleteAsset(dotPath);
    }

    void CurrentDotListUpdate()
    {
        currentDotList.Clear();

        for (int i = 0; i < dotPaths.Count; i++)
        {
            if (dotPaths[i] != null)
            {
                currentDotList.Add(LoadObjectInfo(dotPaths[i]));
            }
        }
    }

    // 보드에 사용되는 도트 및 타일들 세팅
    void SetDots()
    {
        Board board = FindObjectOfType<Board>();

        if (board != null)
        {
            board.dotPrefabs = new GameObject[dotPaths.Count];

            for (int i = 0; i < dotPaths.Count; i++)
            {
                board.dotPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(dotPaths[i]);
            }
        }
    }

    ObjectInfo LoadObjectInfo(string objPath)
    {
        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);
        ObjectInfo objectInfo = new ObjectInfo();

        if (obj != null)
        {
            objectInfo.name = obj.name;
            objectInfo.image = obj.GetComponent<SpriteRenderer>().sprite;
            objectInfo.color = obj.GetComponent<SpriteRenderer>().color;
        }

        return objectInfo;
    }

    void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        SerializedProperty layersProp = tagManager.FindProperty("layers");

        string s = tag;

        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(s))
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = s;
        }

        tagManager.ApplyModifiedProperties();
    }

    void UpdatePaths(List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);

            if (obj == null)
            {
                paths.RemoveAt(i);
            }
        }
    }
}
