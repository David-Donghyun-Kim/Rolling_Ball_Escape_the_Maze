using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public PlayerManager player; // 플레이어 오브젝트
    public bool isChapterClear; // 챕터 클리어 확인용
    public Text chapterText; // 챕터명 text
    public Text elapsedText; // 게임 경과 시간 text
    public Text itemDestroyWallNum, itemShowMapWallNum; //아이템들의 갯수 text
    private float gameElpsedTime; // 게임 경과 시간
    void Awake()
    {
        chapterText.text = SceneManager.GetActiveScene().name; // 현재 챕터명 가져오기
        elapsedText.text = "00 : 00"; // 게임 경과 시간 00:00 으로 초기화
        Screen.SetResolution(1080, 1920, true); // 게임 해상도 1080*1920으로 고정
    }
    // Start is called before the first frame update
    void Start()
    {
        isChapterClear = false; //게임을 시작할 땐 챕터 클리어 상태가 아님
        gameElpsedTime = 0; // 게임 경과 시간 = 0임
        // 보유한 아이템들의 개수도 0개임
        itemDestroyWallNum.text = "0"; 
        itemShowMapWallNum.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        // R버튼이 눌리면 게임 초기화 =>나중에 모바일로 바꿔야하니 UI에 리셋버튼 추가 필요
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //현재 씬을 재 로드
        }
        // 플레이어가 엔드포인트에 도달할 시에 챕터 클리어 활성화
        if (player.isAtEndPoint)
        {
            isChapterClear = true;
            //챕터 클리어 UI 출력 -> 추가 필요
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); //다음 씬을 불러오기
            return;
        }
        TimeManage(); //게임 경과 시간 관리 함수
    }

    void TimeManage()
    {
        gameElpsedTime += Time.deltaTime; // 경과시간을 업데이트
        int minute = (int)gameElpsedTime / 60; // 분 계산
        int second = (int)gameElpsedTime - (minute * 60); // 초 계산

        elapsedText.text = (minute/10).ToString() + (minute%10).ToString() + " : " + 
            (second / 10).ToString() + (second % 10).ToString(); //텍스트로 변환
    }
}
