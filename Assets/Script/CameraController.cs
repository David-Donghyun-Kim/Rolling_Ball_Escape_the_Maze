using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject targetCamera;
    public GameObject targetObject;

    private Vector3 offset; //카메라와 타겟사이의 거리
    private float cameraHeight = 15f; // 카메라와 타겟사이의 높이 차이

    private float itemUsingTime = 5f;
    public bool isItemUsed = false;
    public float camMoveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        //카메라의 초기 위치를 플레이어의 위로 지정
        targetCamera.transform.position = new Vector3(targetObject.transform.position.x, 
            targetObject.transform.position.y + cameraHeight, targetObject.transform.position.z);
        //거리 = 타겟카메라의 위치 - 타겟오브젝트의 위치
        offset = targetCamera.transform.position - targetObject.transform.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!isItemUsed) //아이템 사용중이 아닐땐 플레이어를 추적
            // 타겟 카메라 위치 설정 ( 타겟오브젝트로부터 거리만큼 띄워서 포지션 설정)
            targetCamera.transform.position = offset + targetObject.transform.position;
        else
        {
            //맵보여주기 아이템시 사용됬을 시 실행
            isItemUsed = true;
            int mapSize = GameObject.Find("MapGenerator").GetComponent<MapGenerator>().mazeMap.GetLength(0); //현재 맵의 사이즈를 가져옴
            targetCamera.transform.position = new Vector3(0, mapSize * 5f, 0); //카메라의 포지션을 맵의 위로 변경

            itemUsingTime -= Time.deltaTime; //아이템 사용시간 소모
            if (itemUsingTime <= 0) //아이템 사용시간이 모두 소모되었을 때
            {
                isItemUsed = false;
                itemUsingTime = 5f; //아이템 사용시간을 원래대로 돌려둠
            }
        }
    }
}

