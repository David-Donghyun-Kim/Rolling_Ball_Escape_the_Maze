using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEventController : MonoBehaviour
{
    public GameObject itemDestroyWallPrefab;
    public GameObject itemShowMapPrefab;
    public GameObject itemContainer;

    private GameObject item;
    private Ray ray;
    private RaycastHit hit; // 광선이 닿은 지점
    private void FixedUpdate()
    {
        #region Cheat Code for Developers
        // 마우스 왼쪽 버튼(0) 혹은 오른쪽 버튼(1)이 눌렸을때 | 2:가운데
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if(Input.GetMouseButtonDown(0)) item = Instantiate(itemDestroyWallPrefab);  // 게임오브젝트 itemDestroyWall 선언
            else item = Instantiate(itemShowMapPrefab);  // 게임오브젝트 itemShowMap 선언
            //메인 카메라로 선택된 카메라의 스크린에서 클릭한 마우스의 좌표에 광선을 쏜다
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            

            if (Physics.Raycast(ray, out hit, 100f)) //해당 광선ray가 일직선으로 나아가다 닿은 좌료를 저장
            {
                Vector3 hitPos = hit.point; //닿은 좌표 hitPos로 저장
                hitPos.y = 1f;  //생성될 아이템 좌표 높이 설정

                item.transform.parent = itemContainer.transform;    //아이템저장고를 부모로 둔다
                item.transform.position = hitPos;   //생성될위치는 광선이 닿은곳의 좌표
            }
        }

        #endregion
    }

    //아이템 제거하는 함수
    public void DestroyItem(Collider item)
    {
        if (item == null)   //오브젝트(아이템)이 존재하지 않는다면 함수를 종료한다
            return;

        Destroy(item.gameObject); //해당 게임오브젝트 제거
    }
}
