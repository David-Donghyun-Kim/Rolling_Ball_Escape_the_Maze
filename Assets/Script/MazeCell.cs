using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCell
{
    public bool isVisited = false;
    public GameObject leftWall, rightWall, UpWall, DownWall, floor, wall;
    public int rowIndex, columnIndex;
}
