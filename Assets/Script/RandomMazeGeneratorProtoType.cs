using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMazeGeneratorProtoType : MonoBehaviour
{
    // Public Variables
    public int mapRow, mapColumn; //맵의 가로세로 크기
    public float size;
    public GameObject wall, floor;

    // Private Variables
    private int currentPosX, currentPosZ;
    private bool isGenerationComplete = false;
    private MazeCell[,] map;
    private List<MazeCell> visitedCells = new List<MazeCell>();
    // Start is called before the first frame update
    void Start()
    {
        InitializeMap();
        GenerateMaze();
    }

    void InitializeMap()
    {
        #region Map Initialization
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

                // 왼쪽 벽 초기화
                if (j == 0)
                {
                    map[i, j].leftWall = Instantiate(wall);
                    map[i, j].leftWall.transform.position = new Vector3(i * size, 0, j * size - (size / 2f));
                    map[i, j].leftWall.name = "LeftWall(" + i + "," + j + ")";
                }

                // 오른쪽 벽 초기화
                map[i, j].rightWall = Instantiate(wall);
                map[i, j].rightWall.transform.position = new Vector3(i * size, 0, j * size + (size / 2f));
                map[i, j].rightWall.name = "RightWall(" + i + "," + j + ")";

                // 위쪽 벽 초기화
                if (i == 0)
                {
                    map[i, j].UpWall = Instantiate(wall);
                    map[i, j].UpWall.transform.rotation = Quaternion.Euler(0, 90, 0);
                    map[i, j].UpWall.transform.position = new Vector3(i * size - (size / 2f), 0, j * size);
                    map[i, j].UpWall.name = "UpWall(" + i + "," + j + ")";
                }
                // 아래쪽 벽 초기화
                map[i, j].DownWall = Instantiate(wall);
                map[i, j].DownWall.transform.rotation = Quaternion.Euler(0, 90, 0);
                map[i, j].DownWall.transform.position = new Vector3(i * size + (size / 2f), 0, j * size);
                map[i, j].DownWall.name = "DownWall(" + i + "," + j + ")";
            }
        }
        #endregion
    }

    void GenerateMaze()
    {
        //프림의 알고리즘을 이용할 것

        // 첫번째로, 시작 셀을 정한다
        // 시작점은 맵의 테두리에서만 발생한다고 가정

        #region Random Starting Point Generator
        switch (Random.Range(0, 4))
        {
            case 0: //시작점이 맵의 위쪽에 존재 -> [,0]고정
                    //정확한 시작점 정하기
                currentPosX = Random.Range(0, mapRow);
                currentPosZ = 0;
                break;
            case 1: //시작점이 맵의 왼쪽 존재 -> [0,]고정
                currentPosX = 0;
                currentPosZ = Random.Range(0, mapColumn);
                break;
            case 2: //시작점이 맵의 오른쪽에 존재 -> [row-1,]고정
                currentPosX = mapRow - 1;
                currentPosZ = Random.Range(0, mapColumn);
                break;
            case 3: //시작점이 맵의 아래에 존재 -> [,column-1] 고정
                currentPosX = Random.Range(0, mapRow);
                currentPosZ = mapColumn - 1;
                break;
            default:
                Debug.Log("Error : Generating random starting point has problem");
                break;
        }
        #endregion
        
        //시작 지점을 방문처리
        map[currentPosX, currentPosZ].isVisited = true;
        visitedCells.Add(map[currentPosX, currentPosZ]);
        while (!isGenerationComplete)
        {
            Search();
            if (visitedCells.Count == mapRow * mapColumn)
                isGenerationComplete = true;
        }
        
    }

    void Search()
    {
        //방문한 셀중에 하나를 무작위로 뽑음
        int index = Random.Range(0, visitedCells.Count);
        // 뽑힌 셀이 이동 가능한 상태인지 확인
        // 이동 불가능하면 새로 뽑음
        while(!isItPossibleToMove(visitedCells[index]))
        {
            index = Random.Range(0, visitedCells.Count);
        }

        // 길을 뚫어서 만듬!
        makeRoute(visitedCells[index]);
    }
    bool isItPossibleToMove(MazeCell cell)
    {
        int possible = 0;
        if (cell.rowIndex > 0 && !map[cell.rowIndex - 1, cell.columnIndex].isVisited)
            possible++;
        if (cell.rowIndex < mapRow - 1 && !map[cell.rowIndex + 1, cell.columnIndex].isVisited)
            possible++;
        if (cell.columnIndex > 0 && !map[cell.rowIndex, cell.columnIndex - 1].isVisited)
            possible++;
        if (cell.columnIndex < mapColumn - 1 && !map[cell.rowIndex, cell.columnIndex + 1].isVisited)
            possible++;

        return possible > 0;
    }
    void makeRoute(MazeCell cell)
    {
        bool flag = true;
        while (flag)
        {
            switch (Random.Range(0, 4))
            {
                case 0: //진행방향 위쪽
                    // 위쪽의 셀이 Visited 인지 확인
                    if (cell.rowIndex==0 || map[cell.rowIndex - 1, cell.columnIndex].isVisited)
                        continue; // Visited면 스위치문 다시 실행
                    else
                    {
                        if (map[cell.rowIndex, cell.columnIndex].UpWall != null)
                            Destroy(map[cell.rowIndex, cell.columnIndex].UpWall);
                        else
                            Destroy(map[cell.rowIndex - 1, cell.columnIndex].DownWall);
                        // 방문 리스트에 추가
                        map[cell.rowIndex - 1, cell.columnIndex].isVisited = true;
                        visitedCells.Add(map[cell.rowIndex - 1, cell.columnIndex]);
                    }
                    break;
                case 1: // 진행방향 아래쪽
                        // 아래쪽의 셀이 Visited 인지 확인
                    if (cell.rowIndex == mapRow - 1 || map[cell.rowIndex + 1, cell.columnIndex].isVisited)
                        continue; // Visited면 스위치문 다시 실행
                    else
                    {
                        if (map[cell.rowIndex, cell.columnIndex].DownWall != null)
                            Destroy(map[cell.rowIndex, cell.columnIndex].DownWall);
                        else
                            Destroy(map[cell.rowIndex + 1, cell.columnIndex].UpWall);
                        // 방문 리스트에 추가
                        map[cell.rowIndex + 1, cell.columnIndex].isVisited = true;
                        visitedCells.Add(map[cell.rowIndex + 1, cell.columnIndex]);
                    }
                    break;
                case 2: // 진행방향 왼쪽
                    // 왼쪽의 셀이 Visited 인지 확인
                    if (cell.columnIndex == 0 || map[cell.rowIndex, cell.columnIndex - 1].isVisited)
                        continue; // Visited면 스위치문 다시 실행
                    else
                    {
                        if (map[cell.rowIndex, cell.columnIndex].leftWall != null)
                            Destroy(map[cell.rowIndex, cell.columnIndex].leftWall);
                        else
                            Destroy(map[cell.rowIndex, cell.columnIndex - 1].rightWall);
                        // 방문 리스트에 추가
                        map[cell.rowIndex, cell.columnIndex - 1].isVisited = true;
                        visitedCells.Add(map[cell.rowIndex, cell.columnIndex - 1]);
                    }
                    break;
                case 3: // 진행방향 오른쪽
                    // 오른쪽의 셀이 Visited 인지 확인
                    if (cell.columnIndex == mapColumn - 1 || map[cell.rowIndex, cell.columnIndex + 1].isVisited)
                        continue; // Visited면 스위치문 다시 실행
                    else
                    {
                        if (map[cell.rowIndex, cell.columnIndex].rightWall != null)
                            Destroy(map[cell.rowIndex, cell.columnIndex].rightWall);
                        else
                            Destroy(map[cell.rowIndex, cell.columnIndex + 1].leftWall);
                        // 방문 리스트에 추가
                        map[cell.rowIndex, cell.columnIndex + 1].isVisited = true;
                        visitedCells.Add(map[cell.rowIndex, cell.columnIndex + 1]);
                    }
                    break;
                default:
                    break;
            }
            flag = false;
        }
    }

}
