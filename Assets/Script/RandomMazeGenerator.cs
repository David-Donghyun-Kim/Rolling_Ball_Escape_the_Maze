using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct space {
    public int x, y;
    public int value;
}

public class RandomMazeGenerator : MonoBehaviour
{
    // Public Variables
    public int mapRow, mapColumn; //맵의 가로세로 크기
    public float size; // Cell 한칸의 사이즈
    public GameObject wall, floor, outerWall; //Prefabs
    private const int maxRandom = 1000;

    // Private Variables
    private int currentPosX, currentPosZ;
    private MazeCell[,] map;

    private int[,] mazeCnt;
    private bool[,] chk, maze;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start : " + Time.realtimeSinceStartup);
        InitializeMap();
        GenerateMaze();
    }
    void InitializeMap()
    {
        #region Map Initialization
        GameObject outerWalls = new GameObject("Outer Walls");
        GameObject floors = new GameObject("Floors");
        map = new MazeCell[mapRow, mapColumn];
        for (int i = 0; i < mapRow; i++)
        {
            for (int j = 0; j < mapColumn; j++)
            {
                // 바닥 초기화
                map[i, j] = new MazeCell();
                map[i, j].rowIndex = i;
                map[i, j].columnIndex = j;
                map[i, j].floor = Instantiate(floor);
                map[i, j].floor.transform.position = new Vector3(i * size, -(size / 2f), j * size);
                map[i, j].floor.name = "Floor(" + i + "," + j + ")";
                map[i, j].floor.transform.parent = floors.transform;
            }
        }
        Debug.Log("InitFloor : " + Time.realtimeSinceStartup);

        // 외벽 초기화
        GameObject outerWallTop =  Instantiate(outerWall);
        GameObject outerWallBot = Instantiate(outerWall);
        GameObject outerWallLeft = Instantiate(outerWall);
        GameObject outerWallRight = Instantiate(outerWall);
        // Top outer wall
        outerWallTop.transform.position = new Vector3((mapRow * size) / 2 - (size / 2), 1, (mapColumn * size));
        outerWallTop.transform.localScale = new Vector3((mapRow * size) + (2 * size), size, size);
        outerWallTop.name = "Outer Wall Top";
        outerWallTop.transform.parent = outerWalls.transform;
        // Bottom outer wall
        outerWallBot.transform.position = new Vector3((mapRow * size) / 2 - (size / 2), 1, -1 * size);
        outerWallBot.transform.localScale = new Vector3((mapRow * size) + (2 * size), size, size);
        outerWallBot.transform.parent = outerWalls.transform;
        outerWallBot.name = "Outer Wall Bot";
        // Left outer wall
        outerWallLeft.transform.position = new Vector3((-1 * size), 1, (mapColumn * size) / 2 - (size / 2));
        outerWallLeft.transform.localScale = new Vector3(size, size, (mapColumn * size) + (2 * size));
        outerWallLeft.transform.parent = outerWalls.transform;
        outerWallLeft.name = "Outer Wall Right";
        // Right outer wall
        outerWallRight.transform.position = new Vector3((mapRow * size), 1, (mapColumn * size) / 2 - (size / 2));
        outerWallRight.transform.localScale = new Vector3(size, size, (mapColumn * size) + (2 * size));
        outerWallRight.transform.parent = outerWalls.transform;
        outerWallRight.name = "Outer Wall Left";
        #endregion
        Debug.Log("Wall : " + Time.realtimeSinceStartup);
    }

    void GenerateMaze()
    {
        Debug.Log("Log1 : " + Time.realtimeSinceStartup);
        setStartPoint(mapRow, mapColumn, out currentPosX, out currentPosZ);
        Debug.Log("Log2 : " + Time.realtimeSinceStartup);
        initGenerator();
        Debug.Log("Log3 : " + Time.realtimeSinceStartup);
        runGenerator();
        Debug.Log("Log4 : " + Time.realtimeSinceStartup);
        int[,] printOutArray = new int[mapRow,mapColumn]; // 2차원 배열로 디버그 창에 출력

        GameObject walls = new GameObject("Inner Walls");
        for(int i = 0; i < mapRow; i++)
        {
            for(int l = 0; l < mapColumn; l++)
            {
                if (!maze[i + 1, l + 1])
                {
                    map[i, l].wall = Instantiate(wall);
                    map[i, l].wall.transform.position = new Vector3(i * size, 0, l * size);
                    map[i, l].wall.name = "Wall(" + i + "," + l + ")";
                    map[i, l].wall.transform.parent = walls.transform;
                    printOutArray[i, l] = 1;
                }
            }
        }
        Debug.Log("Log5 : " + Time.realtimeSinceStartup);

        // 디버그 창에 출력
        string arrayLine = "";
        arrayLine += "{";
        for (int j = mapColumn - 1; j >= 0; j--)
        {
            arrayLine += "{";
            for (int i = 0; i < mapRow; i++)
            {
                arrayLine += printOutArray[i, j].ToString();
                if (i < mapRow - 1)
                    arrayLine += ",";
            }
            arrayLine += "}";
            if (j != 0) arrayLine += ",\n";
        }
        arrayLine += "}";
        Debug.Log(arrayLine);
    }

    // 시작점은 맵의 테두리에서만 발생한다고 가정
    void setStartPoint(int mapRow, int mapColumn, out int currentPosX, out int currentPosZ)
    {
        #region Random Starting Point Generator
        int random = Random.Range(0, 4);
        int[] posX = new int[]{
            Random.Range(0,mapRow),
            0,
            mapRow - 1,
            Random.Range(0, mapRow)
        };
        int[] posZ = new int[]{
            0,
            Random.Range(0, mapColumn),
            Random.Range(0, mapColumn),
            mapColumn - 1
        };
        currentPosX = posX[random];
        currentPosZ = posZ[random];
        #endregion
    }

    void initGenerator()
    {
        mazeCnt = new int[mapRow + 2, mapColumn + 2]; ;
        maze = new bool[mapRow + 2, mapColumn + 2];
        chk = new bool[mapRow + 2, mapColumn + 2];
        for (int i = 0; i <= mapRow + 1; i++)
            for (int l = 0; l <= mapColumn + 1; l++)
            {
                maze[i, l] = chk[i, l] = false;
                mazeCnt[i, l] = -1;
            }
        for (int i = 0; i <= mapRow + 1; i++)
            chk[i, 0] = chk[i, mapColumn + 1] = true;
        for (int l = 0; l <= mapColumn + 1; l++)
            chk[0, l] = chk[mapRow + 1, l] = true;
    }

    void runGenerator()
    {
        space first;
        first.x = currentPosX + 1;
        first.y = currentPosZ + 1;
        first.value = 0;

        heap<space> hp = new heap<space>();
        hp.push(first, first.value);

        while (!hp.empty())
        {
            space s = hp.top();
            hp.pop();
            int x = s.x;
            int y = s.y;
            if (chk[y, x] && mazeCnt[y, x] != 0) continue;
            else
            {
                maze[y, x] = chk[y, x] = true;
                int[] px = new int[4]{ 1, 0, -1, 0 };
                int[] py = new int[4]{ 0, 1, 0, -1 };
                for (int k = 0; k < 4; k++)
                {
                    int nextx = x + px[k];
                    int nexty = y + py[k];
                    mazeCnt[nexty, nextx]++;
                    if (!chk[nexty, nextx] && mazeCnt[nexty, nextx] == 0)
                    {
                        chk[nexty, nextx] = true;
                        space tmp;
                        tmp.x = nextx;
                        tmp.y = nexty;
                        tmp.value = Random.Range(0, maxRandom);
                        hp.push(tmp, tmp.value);
                    }
                }
            }
        }
    }

}
// 참고로 아직 힙 구조는 아님
public class heap<T> {
    #region heap class
    ArrayList list;
    int listCnt;
    int size;

    public heap()
    {
        list = new ArrayList();
        listCnt = 1; size = 0;
        list.Add(0);
    }

    public void push(T t, int value)
    {
        node<T> nd = new node<T>();
        nd.initNode(t, value);
        list.Insert(listCnt, nd);
        int cur = listCnt;
        listCnt++;
        size++;
        while (cur != 1)
        {
            node<T> curNode = (node<T>)list[cur];
            node<T> parentNode = (node<T>)list[cur / 2];
            if (curNode.value < parentNode.value || !parentNode.use)
            {
                object tmp = list[cur];
                list[cur] = list[cur / 2];
                list[cur / 2] = tmp;
                cur /= 2;
            }
            else
                break;
        }
    }
    
    public void pop()
    {
        if (empty()) return;
        if (((node<T>)list[1]).use == false)
            UnityEngine.Debug.Log("error " + size);
        ((node<T>)list[1]).use = false;
        //for (int i = 1; i < listCnt; i++)
        //{
        //    int idx = i;
        //    for(int l = i + 1; l < listCnt; l++)
        //    {
        //        if (!((node<T>)list[l]).use) continue;
        //        if ( ((node<T>)list[idx]).value < ((node<T>)list[l]).value
        //            || !((node<T>)list[idx]).use && ((node<T>)list[l]).use)
        //        {
        //            idx = l;
        //        }
        //    }
        //    object tmp = list[i];
        //    list[i] = list[idx];
        //    list[idx] = tmp;
        //}
        

        int cur = 1;
        while (cur < listCnt)
        {
            bool hasLeft = true;
            bool hasRight = true;
            if (listCnt <= cur * 2     || !((node<T>)list[cur * 2    ]).use ) hasLeft = false;
            if (listCnt <= cur * 2 + 1 || !((node<T>)list[cur * 2 + 1]).use ) hasRight = false;

            int next;
            if (hasLeft && hasRight)
            {
                if( ((node<T>)list[cur*2]).value < ((node<T>)list[cur*2 + 1]).value)
                    next = cur * 2;
                else
                    next = cur * 2 + 1;
            }
            else if (!hasLeft && !hasRight)
                break;
            else if (hasLeft)
                next = cur * 2;
            else
                next = cur * 2 + 1;
            swapNode(cur, next);
            //if (!((node<T>)list[cur]).use ||
            //    ((node<T>)list[cur]).value > ((node<T>)list[next]).value)
            //    swapNode(cur, next);
            //else
            //    break;
            cur = next;
        }
        size--;
    }

    public T top()
    {
        return ((node<T>)list[1]).data;
    }

    public bool empty()
    {
        if (size == 0) return true;
        else return false;
    }

    public void swapNode(int n1, int n2)
    {
        node<T>tmp = (node<T>)list[n1];
        list[n1] = (node<T>)list[n2];
        list[n2] = tmp;
    }

    class node<T>
    {
        public bool use;
        public T data;
        public int value;

        public void initNode(T target, int value)
        {
            this.value = value;
            data = target;
            use = true;
        }
    }

    #endregion
}

