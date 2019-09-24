using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityScript;

public enum BombType
{
    Color,
    ColumnArrow,
    RowArrow,
    Adjacent
}

public enum Tile_Type
{
    Background,
    Breakable
}

[System.Serializable]
public class ObjectInfo
{
    public Sprite image;
    public string name;
    public Color color = Color.white;
}

public class PuzzleWindow : EditorWindow
{
    private List<string> dotPaths = new List<string>();
    private List<TileType> boardLayout = new List<TileType>();
    private string[] bombPaths = new string[(int)BombType.Adjacent + 1];
    private string backGroundTilePath;
    private string breakableTilePath;

    int width = 0;
    int height = 0;

    TileKind tileKind;

    ObjectInfo tileInfo = new ObjectInfo();
    Tile_Type tile_Type;
    int tile_HitPoint;

    ObjectInfo bombInfo = new ObjectInfo();
    BombType bomb_type;

    ObjectInfo dotInfo = new ObjectInfo();
    List<ObjectInfo> currentDotList = new List<ObjectInfo>();

    bool isOpenStep1 = true;
    bool isOpenStep2 = true;
    bool isOpenStep3 = true;
    bool isOpenStep4 = true;

    [MenuItem("Window/Puzzle Maker")]
    
    static void Init()
    {
        PuzzleWindow myWindow = (PuzzleWindow)EditorWindow.GetWindow(typeof(PuzzleWindow));
        myWindow.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("! Make a Puzzle !", EditorStyles.boldLabel);
        GUILayout.Space(6.0f);

        GUILayout.BeginVertical();

        // 보드판 만들기
        isOpenStep1 = EditorGUILayout.Foldout(isOpenStep1, "Step 1. Make a board");
        if(isOpenStep1)
        {
            MakeMenu_Board();
        }
        //GUILayout.Label("Step 1. Make a board", EditorStyles.boldLabel);
        GUILayout.Space(20.0f);

        // 타일 만들기
        isOpenStep2 = EditorGUILayout.Foldout(isOpenStep2, "Step 2. Make Tiles");
        if (isOpenStep2)
        {
            MakeMenu_Tile();
        }
        GUILayout.Space(20.0f);

        // 폭탄 만들기
        isOpenStep3 = EditorGUILayout.Foldout(isOpenStep3, "Step 3. Make bombs");
        if (isOpenStep3)
        {
            MakeMenu_Bomb();
        }
        GUILayout.Space(20.0f);

        // Dot 만들기
        isOpenStep4 = EditorGUILayout.Foldout(isOpenStep4, "Step 4. Make dots");
        if(isOpenStep4)
        {
            MakeMenu_Dot();
        }
        GUILayout.Space(20.0f);
        GUILayout.EndVertical();
    }

    public void OnInspectorUpdate()
    {
        this.Repaint();
    }

    // 보드 만들기 메뉴 함수
    void MakeMenu_Board()
    {
        GUILayout.Label("First, you need to set the board's width and height.", EditorStyles.helpBox);
        width = EditorGUILayout.IntField("Width", width, GUILayout.Width(250.0f));
        height = EditorGUILayout.IntField("Height", height, GUILayout.Width(250.0f));
        GUILayout.Space(5.0f);
        if (GUILayout.Button("Create Board", GUILayout.Width(150.0f)))
        {
            MakeBoard(height, width);
        }

        GUILayout.Space(15.0f);
        GUILayout.Label("Choose a tile type, " +
                        "and Click the tile position button what you want to change a tile type.", EditorStyles.helpBox);
        //GUILayout.Label("You don't need to choose 'normal'.", EditorStyles.helpBox);
        tileKind = (TileKind)EditorGUILayout.EnumPopup("Select Tile type", tileKind, GUILayout.Width(350.0f));
        for (int i = 0; i < height; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for(int j = 0; j < width; j++)
            {
                if(GetTileKindWithPosition(i, j) == TileKind.Normal)
                {
                    if (GUILayout.Button(i + ", " + j, GUILayout.Width(40.0f)))
                    {
                        SetTileKindWithPosition(i, j, tileKind);
                    }
                }
                else if(GetTileKindWithPosition(i, j) == TileKind.Breakable)
                {
                    var style = new GUIStyle(GUI.skin.button);
                    style.normal.textColor = Color.blue;

                    if (GUILayout.Button(i + ", " + j, GUILayout.Width(40.0f)))
                    {
                        SetTileKindWithPosition(i, j, tileKind);
                    }
                }
                else if(GetTileKindWithPosition(i, j) == TileKind.Blank)
                {
                    var style = new GUIStyle(GUI.skin.button);
                    style.normal.textColor = Color.red;

                    if (GUILayout.Button(i + ", " + j, GUILayout.Width(40.0f)))
                    {
                        SetTileKindWithPosition(i, j, tileKind);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
        }
    }

    // 타일 만들기 메뉴 함수
    void MakeMenu_Tile()
    {
        tileInfo.name = EditorGUILayout.TextField("Tile prefab name", tileInfo.name, GUILayout.Width(350.0f));
        tile_Type = (Tile_Type)EditorGUILayout.EnumPopup("Select Tile Type", tile_Type, GUILayout.Width(350.0f));
        if (tile_Type == Tile_Type.Breakable)
        {
            tile_HitPoint = EditorGUILayout.IntField("Number of times to break", tile_HitPoint, GUILayout.Width(350.0f));
        }
        tileInfo.color = EditorGUILayout.ColorField("Tile Color", tileInfo.color, GUILayout.Width(370.0f));
        tileInfo.image = (Sprite)EditorGUILayout.ObjectField(tileInfo.image, typeof(Sprite), false, GUILayout.Width(370.0f));
        if (tileInfo.image != null)
        {
            GUILayout.Box(tileInfo.image.texture, GUILayout.Width(120.0f), GUILayout.Height(120.0f));
        }
        GUILayout.Space(3.0f);
        if (GUILayout.Button("Create Tile", GUILayout.Width(150.0f)))
        {
            if (tile_Type == Tile_Type.Background)
            {
                MakeBackGroundTile(tileInfo.image, tileInfo.color, tileInfo.name);
            }
            else if (tile_Type == Tile_Type.Breakable)
            {
                MakeBreakableTile(tileInfo.image, tileInfo.color, tileInfo.name, tile_HitPoint);
            }
            SetTiles();
        }
    }

    // 폭탄 만들기 메뉴 함수
    void MakeMenu_Bomb()
    {
        bomb_type = (BombType)EditorGUILayout.EnumPopup("Select Bomb Type", bomb_type, GUILayout.Width(350.0f));

        if (bomb_type == (BombType.Color))
        {
            if (bombPaths[(int)BombType.Color] != null)
            {
                bombInfo = LoadObjectInfo(bombPaths[(int)BombType.Color]);
            }
        }
        else if (bomb_type == (BombType.ColumnArrow))
        {
            if (bombPaths[(int)BombType.ColumnArrow] != null)
            {
                bombInfo = LoadObjectInfo(bombPaths[(int)BombType.ColumnArrow]);
            }
        }
        else if (bomb_type == (BombType.RowArrow))
        {
            if (bombPaths[(int)BombType.RowArrow] != null)
            {
                bombInfo = LoadObjectInfo(bombPaths[(int)BombType.RowArrow]);
            }
        }
        else if (bomb_type == (BombType.Adjacent))
        {
            if (bombPaths[(int)BombType.Adjacent] != null)
            {
                bombInfo = LoadObjectInfo(bombPaths[(int)BombType.Adjacent]);
            }
        }
        bombInfo.name = EditorGUILayout.TextField("Bomb prefab name", bombInfo.name, GUILayout.Width(350.0f));
        bombInfo.color = EditorGUILayout.ColorField("Bomb Color", bombInfo.color, GUILayout.Width(370.0f));
        bombInfo.image = (Sprite)EditorGUILayout.ObjectField(bombInfo.image, typeof(Sprite), false, GUILayout.Width(370.0f));
        if (bombInfo.image != null)
        {
            GUILayout.Box(bombInfo.image.texture, GUILayout.Width(120.0f), GUILayout.Height(120.0f));
        }
        if (GUILayout.Button("Create Bomb", GUILayout.Width(150.0f)))
        {
            MakeBombObject(bombInfo.image, bombInfo.color, bombInfo.name, bomb_type);
        }
    }

    // 도트 만들기 메뉴 함수
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

    // 보드 만들기
    void MakeBoard(int height, int width)
    {
        Board board = FindObjectOfType<Board>();

        if(board != null)
        {
            DestroyImmediate(board.gameObject);
        }

        GameObject tmpBoard = new GameObject("Board");
        tmpBoard.AddComponent<Board>();
        board = tmpBoard.GetComponent<Board>();

        board.height = height;
        board.width = width;

        MakeFindMatches();
        AddCameraScalar();
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

    // 배경 타일 만들기
    void MakeBackGroundTile(Sprite sprite, Color color, string name)
    {
        if (name == null)
        {
            return;
        }

        GameObject backGroundTile = new GameObject(name);
        backGroundTile.AddComponent<SpriteRenderer>();
        backGroundTile.AddComponent<BackgroundTile>();

        backGroundTile.GetComponent<SpriteRenderer>().sprite = sprite;
        backGroundTile.GetComponent<SpriteRenderer>().color = color;
        backGroundTile.GetComponent<BackgroundTile>().hitPoints = 1;

        backGroundTilePath = MakePrefab(backGroundTile);
        DestroyImmediate(backGroundTile, true);
    }

    // 부셔지는 타일 만들기
    void MakeBreakableTile(Sprite sprite, Color color, string name, int HitPoint)
    {
        if (name == null)
        {
            return;
        }

        GameObject breakableTile = new GameObject(name);
        breakableTile.AddComponent<SpriteRenderer>();
        breakableTile.AddComponent<BackgroundTile>();

        breakableTile.GetComponent<SpriteRenderer>().sprite = sprite;
        breakableTile.GetComponent<SpriteRenderer>().color = color;
        breakableTile.GetComponent<BackgroundTile>().hitPoints = HitPoint;

        breakableTilePath = MakePrefab(breakableTile);
        DestroyImmediate(breakableTile, true);
    }

    // 보드에 사용될 타일을 할당
    void SetTiles()
    {
        Board board = FindObjectOfType<Board>();

        if(board != null)
        {
            Debug.Log(backGroundTilePath);
            //board.tilePrefab = Resources.Load(backGroundTilePath) as GameObject;
            board.tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(backGroundTilePath);
            board.breakableTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(breakableTilePath);
        }
    }

    // 폭탄 세팅, 프리펩 등록
    void MakeBombObject(Sprite sprite, Color color, string name, BombType bombType)
    {
        if(name == null)
        {
            return;
        }

        GameObject bomb = new GameObject(name);

        bomb.AddComponent<SpriteRenderer>();
        bomb.GetComponent<SpriteRenderer>().sprite = sprite;
        bomb.GetComponent<SpriteRenderer>().color = color;

        bombPaths[(int)bombType] = MakePrefab(bomb);
        DestroyImmediate(bomb, true);
    }

    // 도트 세팅, 프리펩 등록
    void MakeDotObject(Sprite sprite, Color color, string name, string tag)
    {
        if(name == null)
        {
            return;
        }

        GameObject dot = new GameObject(name);
        AddTag(tag);
        dot.tag = tag;
        
        dot.AddComponent<SpriteRenderer>();
        dot.AddComponent<CircleCollider2D>();
        dot.AddComponent<Dot>();

        dot.GetComponent<SpriteRenderer>().sprite = sprite;
        dot.GetComponent<SpriteRenderer>().color = color;
        //SetBombs();

        string dotPath = MakePrefab(dot);

        if(!dotPaths.Contains(dotPath))
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
        if(dotName == null)
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

    // 세팅된 폭탄들을 도트들에 등록
    void SetBombs()
    {
        for(int i = 0; i < dotPaths.Count; i++)
        {
            GameObject dot = AssetDatabase.LoadAssetAtPath<GameObject>(dotPaths[i]);
            GameObject dotInstance = UnityEditor.PrefabUtility.InstantiatePrefab(dot) as GameObject;

            dotInstance.GetComponent<Dot>().columnArrow = AssetDatabase.LoadAssetAtPath<GameObject>(bombPaths[(int)BombType.ColumnArrow]);
            dotInstance.GetComponent<Dot>().rowArrow = AssetDatabase.LoadAssetAtPath<GameObject>(bombPaths[(int)BombType.RowArrow]);
            dotInstance.GetComponent<Dot>().colorBomb = AssetDatabase.LoadAssetAtPath<GameObject>(bombPaths[(int)BombType.Color]);
            dotInstance.GetComponent<Dot>().adjacentMarker = AssetDatabase.LoadAssetAtPath<GameObject>(bombPaths[(int)BombType.Adjacent]);

            UnityEditor.PrefabUtility.ReplacePrefab(dotInstance, dot);
            AssetDatabase.SaveAssets();
            DestroyImmediate(dotInstance, true);
        }
    }

    // 타일의 종류와 위치 설정
    void SetTileKindWithPosition(int x, int y, TileKind tileKind)
    {
        TileType tile = new TileType();
        tile.x = (int)x;
        tile.y = (int)y;
        tile.tileKind = tileKind;

        Debug.Log(tile.x + ", " + tile.y + " sucessful");
        Board board = GameObject.Find("Board").GetComponent<Board>();
        boardLayout.Add(tile);

        SetBoardLayouts();
    }

    TileKind GetTileKindWithPosition(int x, int y)
    {
        TileKind tileKind = new TileKind();

        for (int i = 0; i < boardLayout.Count; i++)
        {
            if(boardLayout[i].x == x && boardLayout[i].y == y)
            {
                tileKind = boardLayout[i].tileKind;
            }
        }

        return tileKind;
    }

    // 보드에 사용되는 도트 및 타일들 세팅
    void SetDots()
    {
        Board board = FindObjectOfType<Board>();

        if(board != null)
        {
            board.dotPrefabs = new GameObject[dotPaths.Count];

            for (int i = 0; i < dotPaths.Count; i++)
            {
                board.dotPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(dotPaths[i]);
            }
        }
    }

    void SetBoardLayouts()
    {
        Board board = FindObjectOfType<Board>();

        if (board != null)
        {
            board.boardLayout = new TileType[boardLayout.Count];

            for (int i = 0; i < boardLayout.Count; i++)
            {
                board.boardLayout[i] = boardLayout[i];
            }
        }
    }
    
    ObjectInfo LoadObjectInfo(string objPath)
    {
        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);
        ObjectInfo objectInfo = new ObjectInfo();

        if(obj != null)
        {
            objectInfo.name = obj.name;
            objectInfo.image = obj.GetComponent<SpriteRenderer>().sprite;
            objectInfo.color = obj.GetComponent<SpriteRenderer>().color;
        }

        return objectInfo;
    }

    string MakePrefab(GameObject obj)
    {
        string path = "Assets/Prefabs/" + obj.name + ".prefab";
        AssetDatabase.DeleteAsset(path);
        //PrefabUtility.SavePrefabAsset(obj);
        PrefabUtility.CreatePrefab(path, obj);

        return path;
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
        for(int i = 0; i < paths.Count; i++)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);

            if(obj == null)
            {
                paths.RemoveAt(i);
            }
        }
    }
}
