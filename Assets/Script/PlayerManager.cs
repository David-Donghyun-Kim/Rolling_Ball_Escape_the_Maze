using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    private Rigidbody playerRigidbody;
    private MeshRenderer playerMeshRenderer;
    public float ballSpeed = 10f; // Ball의 최대 속도
    public float jumpPower = 20f;  // 볼이 점프할수 있는 힘
    public GameObject viewCamera;
    float inputX, inputZ;
    
    private bool isJumping;
    public bool isAtEndPoint = false;


    private int itemDestroyWallNum; // 벽을 부수는 아이템 갯수
    private int itemShowMapNum; // 맵을 보여주는 아니템 갯수
    public Text itemDestroyWallNumText;
    public Text itemShowMapNumText;

    public Material ballUsedItem;  // 벽 파괴 아이템 사용시 사용할 볼 마테리얼
    public Material normalStateBall; // 공의 원래 마테리얼
    public bool canBreakWall; // 벽 파괴가능 유무 확인

    public GameObject _blockChunk; // 벽 파괴시 생기는 작은 청크의 오브젝트

    // Start is called before the first frame update
    void Start()
    {
        //playerRigidbody에 ball의 rigidbody 컴포넌트를 저장
        playerRigidbody = GetComponent<Rigidbody>();
        playerMeshRenderer = GetComponent<MeshRenderer>();
        isJumping = false; // 바닥에 닿은 상태로 만들어준다.
        canBreakWall = false;
    }

    // Update is called once per frame
    void Update()
    {
        inputX = Input.GetAxis("Horizontal"); //유저 입력 - 좌,우 (-1 ~ 1)
        inputZ = Input.GetAxis("Vertical"); // 유저 입력 - 앞,뒤 (-1 ~ 1)
        

        float gravity = playerRigidbody.velocity.y; // ball의 중력 저장

        Vector3 velocity = new Vector3(inputX, 0, inputZ); // 새로운 속도 저장
        velocity *= ballSpeed; //새로운 속도에 ball의 최대 속도 적용
        velocity.y = gravity; // 중력 재 적용(위에서 중력이 0으로 초기화 됬으므로)

        playerRigidbody.velocity = velocity; //새로운 ball의 속도 적용

        Jump(); //점프 실행
        UseDestroyWallItem(); //벽부수기 아이템 사용
        UseShowMapItem(); // 맵보여주기 아이템 사용
    }

    void Jump()
    {
        // 스페이스바 입력시 점프
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isJumping) //바닥에 있을때 점프
            {
                isJumping = true;   //점프중으로 바꿔준다
                playerRigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            }
            else  //점프중인상태일 경우
            {
                print("점프불가능");
                return;  //점프불가능 그대로 반환
            }
        }
    }
    void UseDestroyWallItem()
    {
        if (Input.GetKeyDown(KeyCode.G))    //G키 누를시 벽부수기 상태 on
        {
            if (itemDestroyWallNum > 0)   //이때 남은 아이템이 1개 이상이어야 동작함
            {
                if (canBreakWall == false)  // 그리고 이미 아이템 사용한상태이면 중복해서 사용이 되지 않음
                {
                    playerMeshRenderer.material.color = ballUsedItem.color; // 아이템을 사용했다는것을 공의색을 바꿔 시각적으로 표현 
                    canBreakWall = true;    // 벽을 부술수있는 상태로 만듬
                    itemDestroyWallNumText.text = "" + --itemDestroyWallNum;    // 아이템 갯수 -1개 해준다.

                }
            }
        }
    }

    void UseShowMapItem()
    {
        if(Input.GetKeyDown(KeyCode.M)) //M키 누를시 맵보여주기 상태 on
        {
            if (itemShowMapNum > 0) //남은 아이템이 1개 이상일 때 동작
            {
                // 카메라 컨트롤러에 아이템이 사용되었음을 알림
                GameObject.Find("UserCameraController").GetComponent<CameraController>().isItemUsed = true;
                itemShowMapNumText.text = "" + --itemShowMapNum; //아이템 갯수 -1개 해줌
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // 컬라이더의 태그가 "Item"인 경우 Item 삭제
        if(other.gameObject.CompareTag("Item_DW") || other.gameObject.CompareTag("Item_SM"))
        {
            if(other.gameObject.CompareTag("Item_DW"))
                itemDestroyWallNumText.text = "" + ++itemDestroyWallNum;
            else
                itemShowMapNumText.text = "" + ++itemShowMapNum;
            ItemEventController itemEventController = GameObject.FindObjectOfType<ItemEventController>();
            itemEventController.DestroyItem(other);
        }
        // 컬라이더의 태그가 "End Point"인 경우, 플레이어가 엔드포인트에 도달했음을 알리고
        // 해당 End Point는 파괴함
        if(other.gameObject.CompareTag("End Point"))
        {
            Debug.Log("엔드포인트 도달!");
            isAtEndPoint = true;
            Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("WallBarrier"))
        {
            if (canBreakWall == true)   //벽을 부술수있는 상태라면
            {
                canBreakWall = false;   //부술수있는 상태를 비활성화 시키고
                Vector3 position = other.gameObject.transform.position; //기존 벽의 좌표를 저장
                Destroy(other.gameObject);  // 닿은 벽을 삭제한다.
                Destroy(other.transform.parent.gameObject);
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            GameObject blockChunk = Instantiate(_blockChunk);
                            //blockChunk.transform.parent = blockChunksParents.transform;
                            blockChunk.transform.position = new Vector3(position.x - 1.5f + (1f * k),
                                position.y - 1.5f + (1f * j),
                                position.z - 1.5f + (1f * i));
                            Destroy(blockChunk, 5.0f);
                            if (j == 0)
                            {
                                blockChunk.GetComponent<Rigidbody>().AddExplosionForce(200f, blockChunk.transform.position, 50f, 10f);
                            }
                        }
                    }
                }
                playerMeshRenderer.material.color = normalStateBall.color;  // 그리고 공을 원래 색으로 돌려놓는다
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 바닥및 큐브에 닿는다면
        if (collision.gameObject.CompareTag("Floor"))
            {
            // 점프가 가능한 상태
            isJumping = false;
        }
        if (collision.gameObject.CompareTag("Cube"))
        {
            // 점프가 가능한 상태
            isJumping = false;
            
        }
    }
}
