using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class Constants {
    public const bool debugSight = true; // km per sec.
}

/*
 * Game Coordinate 기준 좌표계
 * 오직 함수만 사용 가능
 */
public class Location {
    private float x;
    private float y;
    private float z;

    public Location()
    {
        x = 0;
        y = 0;
        z = 0;
    }
    public Location(Vector3 vt)
    {
        x = vt.x;
        y = vt.y;
        z = vt.z;
    }
    public Location(float inputX, float inputY, float inputZ)
    {
        x = inputX;
        y = inputY;
        z = inputZ;
    }

    #region set Coordination
    public void setXbyMapCoordinate(int inputX)
    {
        x = (inputX - 1) * 4f - 28;
    }

    public void setYbyMapCoordinate(int inputY)
    {
        y = inputY;
    }

    public void setZbyMapCoordinate(int inputZ)
    {
        z = -(inputZ - 1) * 4f + 28;
    }

    public void setVectorbyMapCoordinate(Vector3 inputVt)
    {
        setXbyMapCoordinate((int)inputVt.x);
        setYbyMapCoordinate((int)inputVt.y);
        setZbyMapCoordinate((int)inputVt.z);
    }

    public void setXbyGameCoordinate(float inputX)
    {
        x = inputX;
    }

    public void setYbyGameCoordinate(float inputY)
    {
        y = inputY;
    }

    public void setZbyGameCoordinate(float inputZ)
    {
        z = inputZ;
    }

    public void setVectorbyGameCoordinate(Vector3 inputVt)
    {
        setXbyGameCoordinate(inputVt.x);
        setYbyGameCoordinate(inputVt.y);
        setZbyGameCoordinate(inputVt.z);
    }
    #endregion
    #region get Coordination
    public int getXbyMapCoordinate()
    {
        return (int)((28 + x) * 0.25f + 1);
    }

    public int getYbyMapCoordinate()
    {
        return (int)y;
    }

    public int getZbyMapCoordinate()
    {
        return (int)((-z + 28) * 0.25f + 1);
    }

    public Vector3 getVectorbyMapCoordinate()
    {
        return new Vector3(getXbyMapCoordinate(), getYbyMapCoordinate(), getZbyMapCoordinate());
    }

    public float getXbyGameCoordinate()
    {
        return x;
    }

    public float getYbyGameCoordinate()
    {
        return y;
    }

    public float getZbyGameCoordinate()
    {
        return z;
    }

    public Vector3 getVectorbyGameCoordinate()
    {
        return new Vector3(x, y, z);
    }
    #endregion
    public Location clone()
    {
        Location rvLoc = new Location();
        rvLoc.x = x;
        rvLoc.y = y;
        rvLoc.z = z;
        return rvLoc;
    }
}

public class EnemyManager : MonoBehaviour
{
    MapGenerator mg;
    public int[,] map;
    public int mapRow;
    public int mapColumn;
    public Transform enemyTransform;

    private int blankpointX, blankpointY;

    [SerializeField] [Tooltip("This option only applies at start up.")]
    private bool isSentry;

    private Unit me;

    // Start is called before the first frame update
    void Start()
    {
        enemyTransform = GetComponent<Transform>();
        // 맵 데이터 불러오기
        mg = GameObject.Find("MapGenerator").GetComponent<MapGenerator>();
        int mapRow = mg.mazeMap.GetLength(0);
        int mapColumn = mg.mazeMap.GetLength(1);
        map = new int[mapRow, mapColumn];
        map = mg.mazeMap;
        Debug.Log(mapRow + "*" + mapColumn);

        // 유닛 타입 생성
        if (isSentry)
            me = new SentryUnit(5f, 10f, 10, enemyTransform, new Point(), 10); // 임시
        else
            me = new PatrolUnit(5f, 10f, 10, enemyTransform);

        // 유닛에 맵 데이터 전달
        me.setMapSetting(map, mapRow, mapColumn);

        StartCoroutine("runUnit");
    }

    IEnumerator runUnit()
    {
        while (true)
        {
            me.run();
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //me.run(); // 유저 탐색 및 경로 검색(코루틴에서 0.1초마다 확인)
        me.tracingUser(); // 유저 시야 확인
        transform.position = me.moveNextFrame(Time.deltaTime).getVectorbyGameCoordinate(); // 매 프레임마다 호출해서 위치를 이동시킴
    }
}

/*
 * Unit 클래스
 * 게임 내 적군 객체 1개에 대한 구성
 */
public abstract class Unit {
    protected Transform me;
    
    protected Vector3 velocity; // 현재 속도
    protected Vector3 accelerateDirection; // 가속 방향
    protected float maximumSpeed; // 최고 속도
    protected float acceleration; //가속도

    protected StateType state; // 현재 상태
    protected enum StateType {
        Active,
        Detect,
        Trace,
        Suspect
    }

    protected Stack<Point> st; // 목적지 경로
    protected Location loc; // 현재 좌표

    protected float fieldOfView = 120.0f; // 시야각
    protected float sightDistance = 10.0f; // 시야 범위
    protected LayerMask layerMask = 1 << LayerMask.NameToLayer("Player");

    protected bool hasMap; // 맵 보유 여부
    protected int[,] map; // 맵 데이터
    protected int row, column; // 맵 행열 길이

    // 초기화
    public Unit(float inputMaximumSpeed, float inputAccel, int unitSight, Transform transform)
    {
        loc = new Location(transform.position);
        state = StateType.Active;
        me = transform;
        acceleration = inputAccel;
        maximumSpeed = inputMaximumSpeed;
        sightDistance = unitSight;
        velocity = new Vector3(0, 0, 0);
        accelerateDirection = new Vector3(0, 0, 0);
        hasMap = false;
        st = new Stack<Point>();
    }

    // 유닛의 종류에 따라 개별 처리
    public abstract void active();

    public void run()
    {
        if (state == StateType.Active)
            active();
        if(state == StateType.Suspect)
        {
            state = StateType.Active;
            st.Clear();
        }
    }

    // 유저를 추적
    public bool tracingUser()
    {
        if (Constants.debugSight)
            Debug.DrawRay(me.position, me.forward * sightDistance, Color.green);
        // layerMask에 저장된 Sphere를 일정 거리 내에서 찾는다.
        Collider[] cols = Physics.OverlapSphere(me.position, sightDistance, layerMask);
        if (cols.Length > 0) // 한 개라도 있을 경우
        {
            Transform player = cols[0].transform; // 1번째 'Player'
            // 'Player'를 향한 방향
            Vector3 directionToPlayer = (player.position - me.position).normalized;
            if (Constants.debugSight)
                Debug.DrawRay(me.position, directionToPlayer * sightDistance, Color.blue);
            // 'Player'와 '추적 유닛의 정면'과 각도 차이
            float angleBetweenUnitAndPlayer = Vector3.Angle(directionToPlayer, me.forward);
            if (angleBetweenUnitAndPlayer < fieldOfView * 0.5f) // 양방향 합친 것이므로 반으로 나눈다.
            {
                if (Physics.Raycast(
                    me.position,
                    directionToPlayer,
                    out RaycastHit hit,
                    sightDistance)) // 레이저를 쏘보고 충돌 여부가 참
                {
                    if (hit.transform.name == "Player") // 쏜 레이저와 충돌한 것이 'Player' 일 때
                    {
                        if (Constants.debugSight)
                            Debug.DrawRay(me.position, directionToPlayer * sightDistance, Color.red);

                        // '추적 유닛'의 정면을 'Player' 방향으로 바꾼다.
                        Vector3 t = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
                        Vector3 nDir = Vector3.RotateTowards(me.forward, t, 10f, 10f);
                        me.rotation = Quaternion.LookRotation(nDir);

                        // 'Player' 방향으로 가속
                        accelerateDirection.x = directionToPlayer.x;
                        accelerateDirection.y = 0;
                        accelerateDirection.z = directionToPlayer.z;
                        accelerateDirection = accelerateDirection.normalized * acceleration;
                        state = StateType.Trace;
                        return true;
                    }
                }
            }
        }
        if(state == StateType.Trace)
            state = StateType.Suspect;
        return false;
    }

    // 다음 deltaTime 만큼 이동 및 방향 전환.
    public Location moveNextFrame(float deltaTime)
    {
        loc.setXbyGameCoordinate(me.position.x + velocity.x * deltaTime);
        loc.setYbyGameCoordinate(me.position.y);
        loc.setZbyGameCoordinate(me.position.z + velocity.z * deltaTime);
        velocity += accelerateDirection * deltaTime;
        velocity.y = 0;
        float currentSpeedExp = velocity.x * velocity.x + velocity.z * velocity.z;
        if(maximumSpeed*maximumSpeed < currentSpeedExp)
        {
            velocity = velocity.normalized * maximumSpeed;
        }
        return loc;
    }

    // 맵 데이터 저장
    public void setMapSetting(int[,] mapData, int mapRow, int mapColumn)
    {
        hasMap = true;
        row = mapRow;
        column = mapColumn;
        map = new int[mapRow + 2, mapColumn + 2];
        for(int i = 1; i <= mapRow; i++)
        {
            for(int l = 1; l <= mapColumn; l++)
            {
                map[i,l] = mapData[i - 1, l - 1] == 1? -1 : 0;
            }
        }
        for (int i = 0; i <= mapRow + 1; i++)
            map[i, 0] = map[i, mapColumn + 1] = -1;
        for (int l = 0; l <= mapColumn + 1; l++)
            map[0, l] = map[mapRow + 1, l] = -1;
    }

    // 맵 파괴 시 해당 좌표 업데이트
    // 주의) 빈 공간 0, 벽 -1
    public void mapUpdate(int x, int z, int state)
    {
        if(x > 0 && column >= x && z > 0 && row >= z)
            map[z, x] = state;
    }
    
    /* input  : Point(curLocation: 현재 위치)
     *   ''   : Point(destLocation: 목적지)
     * BFS 사용하며, 여기서 st에 경로를 저장
     */ 
    public void findPath(Point curLocation, Point destLocation)
    {
        Debug.Log("x : " + curLocation.x + "z: " + curLocation.z);
        st.Clear();

        #region BFS Search
        int[] px = new int[] { 0, 1, 0, -1 };
        int[] pz = new int[] { 1, 0, -1, 0 };

        Queue<Point> quTmp = new Queue<Point>();
        quTmp.Enqueue(curLocation);
        map[curLocation.z, curLocation.x] = 1;
        bool isFindPath = false;
        while(quTmp.Count > 0 && !isFindPath)
        {
            Point s = quTmp.Dequeue();
            for(int k = 0; k < 4; k++)
            {
                int nx = px[k] + s.x;
                int nz = pz[k] + s.z;
                if (map[nz, nx] == 0)
                {
                    map[nz, nx] = k + 1;
                    Point inq = new Point();
                    inq.x = nx;
                    inq.z = nz;
                    quTmp.Enqueue(inq);
                    if(inq.x == destLocation.x && inq.z == destLocation.z)
                    {
                        isFindPath = true;
                        break;
                    }
                }
            }
        }
        #endregion

        #region 경로 저장
        if (isFindPath)
        {
            Point cur = new Point();
            cur.x = destLocation.x;
            cur.z = destLocation.z;
            while (cur.x != curLocation.x || cur.z != curLocation.z)
            {
                Point tmpPt = new Point();
                tmpPt.x = cur.x;
                tmpPt.z = cur.z;
                st.Push(tmpPt);
                int loc = map[cur.z, cur.x] - 1;
                cur.x = cur.x - px[loc];
                cur.z = cur.z - pz[loc];
            }
        }
        #endregion
        cleanMap();
    }

    // 경로 탐색 후 생긴 찌꺼기 청소
    private void cleanMap()
    {
        for (int i = 1; i <= row; i++)
        {
            for (int l = 1; l <= column; l++)
            {
                if(map[i, l] > 0) map[i, l] = 0;
            }
        }
    }
}

/* Parent Class : Unit
 * 현 위치에서 무작위 빈 공간으로 순찰 이후 반복하는 적 유닛 
*/
public class PatrolUnit : Unit {
    Point destination; // 목적지

    // 초기화
    public PatrolUnit(float inputMaximumSpeed, float inputAccel, int unitSight, Transform transform)
        : base(inputMaximumSpeed, inputAccel, unitSight, transform)
    {
        destination = new Point();
    }

    /* [Override]
     * 활동 상태
     */
    public override void active()
    {
        if (st.Count > 0)
        {

            Vector3 t = new Vector3(0f, 0f, 0f);
            t = (velocity - t);
            t.y = 0f;
            Vector3 nDir = Vector3.RotateTowards(me.forward, t, 10f, 0f);
            me.rotation = Quaternion.LookRotation(nDir);


            // 경로가 남아 있는 경우
            if (loc.getXbyMapCoordinate() == st.Peek().x && loc.getZbyMapCoordinate() == st.Peek().z)
            {
                // 경로에 있는 좌표가 enemy 좌표와 동일 시 삭제
                st.Pop();
            }
            else
            {
                // 경로에 있는 좌표가 enemy 좌표와 다를 시, 경로에 있는 좌표로 이동
                Location lt = new Location();
                lt.setXbyMapCoordinate(st.Peek().x);
                lt.setZbyMapCoordinate(st.Peek().z);

                Vector3 vt = new Vector3(lt.getXbyGameCoordinate() + 2, 0, lt.getZbyGameCoordinate() - 2);
                Vector3 dir = (vt - loc.getVectorbyGameCoordinate()).normalized;
                accelerateDirection.x = dir.x;
                accelerateDirection.y = 0;
                accelerateDirection.z = dir.z;
                accelerateDirection = accelerateDirection.normalized * acceleration;
            }
        }
        else
        {
            // 경로가 없는 경우(처음이거나 탐색 완료 됬거나 추적을 종료했을 때)

            do
            { // 임의의 좌표를 뽑아 빈 공간이 아닐 시 반복
                destination.z = Random.Range(0, row) + 1;
                destination.x = Random.Range(0, column) + 1;
            } while (map[destination.z, destination.x] != 0);
            Point pt = new Point();
            pt.x = loc.getXbyMapCoordinate();
            pt.z = loc.getZbyMapCoordinate();
            findPath(pt, destination);
        }
    }
}

/* Parent Class : Unit
 * 구역을 정해놓고 그 안에서만 방황하는 유닛. 
*/
public class SentryUnit : Unit {
    Point center;
    int dist;

    Queue<Point> qu;
    public SentryUnit(float inputMaximumSpeed, float inputAccel, int unitSight, Transform transform, Point pt, int distant)
        : base(inputMaximumSpeed, inputAccel, unitSight, transform)
    {
        center = pt;
        dist = distant;
    }
    public override void active()
    {

    }
}


public class Point {
    public int x, z;
}