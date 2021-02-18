using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Target을 기준으로 사용자의 마우스 X,Y 입력에 따라 카메라를 회전시키고 싶다.
public class CameraMove : MonoBehaviour
{
    CameraLock camLock;
    public GameObject cameraFollowObj;  // - Target
    public bool Locking = false;
    public float camYOffset = 2;
    //public float followDistance = 0.5f;
    //public float followSpeed = 10;
    [Header("-LockOffSetting")]
    public float cameraMoveSpeed = 120.0f;  // - 이동속도    
    public float cameraOrzinMoveSpeed;
    public float camRotateSpeed = 150;  // - 회전속도
    public float clampLookUp = -45; // - 올려다 보는 제한값
    public float clampLookDown = 70;  // - 내려다 보는 제한값    
    private float rotY;  // - 부모 y축 회전 저장   
    private float rotX;  // - 부모 x축 회전 저장
    [Header("-LockOnSetting")]
    //public float horizontalDivision = 0.475f; // - 가로 화면 분활점
    //public float verticalDivision = 0.3f;  // - 세로 화면 분활점
    public float camRockSpeed = 2; // 추적하는 스피드
    public GameObject player;
    CameraCollision cc;
    PlayerControl pc;

    [Header("- ShakeCamera")]
    // - 카메라 흔드는 시간
    public float time = 0.2f;
    // - 카메라 흔들리는 강도
    public float size = 1f;
    public float LandingSize = 1f;


    // - 카메라 상태에 대한 변수
    public enum State
    {
        LockOn,
        LockOff,
        PlayerDead,
    }
    public State state;

    // Start is called before the first frame update
    void Start()
    {
        cameraOrzinMoveSpeed = cameraMoveSpeed;
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        //부모의 회전값으로 초기값을 정함
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
        state = State.LockOff;
        //LockOnUI.SetActive(false);
        camLock = GetComponentInChildren<CameraLock>();
        cc = GetComponentInChildren<CameraCollision>();

    }

    // Update is called once per frame
    void Update()
    {
        ControlCameraMode();
        switch (state)
        {
            case State.LockOn:
                CameraLockOn();
                break;
            case State.LockOff:
                CameraLockOff();
                break;
            case State.PlayerDead:
                CameraDeath();
                break;
        }

    }

    private void ControlCameraMode()
    {
        // 만약 마우스 휠 버튼을 누르면
        if (Input.GetMouseButtonDown(2))
        {
            // 만약 Locking이 true이면
            if (Locking)
            {
                cc.maxDistance = cc.orizinMaxDistance;
                // 락을 꺼주고
                state = State.LockOff;
                // 락상태의 카메라값을 전달시켜준다
                rotX = transform.eulerAngles.x;
                rotY = transform.eulerAngles.y;
            }
            else
            {
                cc.maxDistance = cc.lockOnMaxDistance;
                // 상태를 락온으로 바꾼다.
                state = State.LockOn;
            }
        }
    }

    private void CameraDeath()
    {
        //카메라가 자유롭게 회전한다.
        CamLockOff();
        //락온UI를 꺼준다.
        camLock.LockOnUI.SetActive(false);
        // print("락오프!");
        Locking = false;
    }

    private void CameraLockOff()
    {
        //카메라가 자유롭게 회전한다.
        CamLockOff();
        //락온UI를 꺼준다.
        camLock.LockOnUI.SetActive(false);
        // print("락오프!");
        Locking = false;
    }

    private void CameraLockOn()
    {
        //카메라제어를 해제시킨다.
        camLock.CamLockOn();
        if (pc.state == PlayerControl.AttackState.Execution)
        {
            // - 카메라 제어 -
            ////만약 로컬 y값이 양수면
            if (transform.localEulerAngles.y >= 0)
            {
                //왼쪽으로 회전
                //처형상태가 되면 카메라의 y축을 45도만큼 회전시킨다.
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, transform.eulerAngles.y + -20, 0), Time.deltaTime);
            }
            if (transform.localEulerAngles.y < 0)
            {
                //오른으로 회전
                //처형상태가 되면 카메라의 y축을 45도만큼 회전시킨다.
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0,transform.eulerAngles.y + 20, 0), Time.deltaTime);
            }
        }
        else
        {
            CamLockOn();

        }
        //락온UI를 켜준다.
        camLock.LockOnUI.SetActive(true);
        Locking = true;
    }


    // 2. 뷰포트의 가로와 세로를 3분할로 나누어
    public void CamLockOn()
    {
        //플레이어가 적을 쳐다보는 방향
        Vector3 dir = camLock.enemy.transform.position - player.transform.position;
        dir.y = 0; // Y축회전 막기

        //회전값을 플레이어가 적을 쳐다보는 방향으로 설정하고 싶다.
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir.normalized), Time.deltaTime * camRockSpeed);
        //앵글
        #region 
        //// 2-1. 뷰포트를 나누어 교차 지점을 찾고
        //float xoffset = Camera.main.rect.width * horizontalDivision;
        ////print(xoffset);
        //float yoffset = Camera.main.rect.height * verticalDivision;
        ////print(yoffset);
        //// 2-2. 타겟의 좌표를 뷰포트로 변경
        //Vector2 PlayerPos = Camera.main.WorldToViewportPoint(cameraFollowObj.transform.position);
        //// 3. 만약 playerpos.x가 0.4 ~ 0.6 그리고 playerpos.y가 0.2 ~ 0.8 에 있으면 추적을 안하고
        //if (PlayerPos.x > xoffset && PlayerPos.x < 0.525f && PlayerPos.y > yoffset && PlayerPos.y < 0.35f)
        //{
        //    //추적하지 않는다.
        //}
        //else
        //{

        //    // 4.카메라를 타겟방향으로
        //    Vector3 dir = cameraFollowObj.transform.position - transform.position;

        //    // 5. camRockSpeed만큼 회전시키며 바라본다.
        //    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * camRockSpeed);

        //}
        #endregion
    }
    public void CamLockOff()
    {
        // 사용자의 입력을 받아
        float h = Input.GetAxis("Mouse X");
        float v = Input.GetAxis("Mouse Y");
        // 만약 마우스 X,Y축 입력이 없다면
        // 플레이어 방향으로 카메라를 회전시키고
        // 마우스 X,Y축 입력이 있다면
        // 
        // 회전변수에 넣어주고
        rotY += h * camRotateSpeed * Time.deltaTime;
        rotX += v * camRotateSpeed * Time.deltaTime;
        // 위아래 회전 제한
        rotX = Mathf.Clamp(rotX, clampLookUp, clampLookDown);
        // 회전시킨다.
        transform.rotation = Quaternion.Euler(rotX, rotY, 0.0f);

    }

    void LateUpdate()
    {
        //만약 Main카메라와 cameraFollowobj와의 거리가 cc의 distance보다 커지면

        //카메라를 이동시키고
        CameraUpdater();

    }

    // 플레이어를 따라 이동 시킨다.
    void CameraUpdater()
    {

        Transform target = cameraFollowObj.transform;
        Vector3 distance = target.position - transform.position;
        float step = cameraMoveSpeed * Time.deltaTime;
        //1. 만약 target과의 거리가 followDistance 벗어나면 
        //if (distance.magnitude > followDistance)
        //{
        //    //2. 카메라 위치가 따라간다.
        //    transform.position = Vector3.MoveTowards(transform.position, target.position, followSpeed * Time.deltaTime);
        //}
        //else
        //{
        //    //2. 카메라 위치가 따라간다.
        //}
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

    }

    // 함수가 호출되면 시간과 x,y,z 축범위를 가져와
    // 카메라를 범위만큼 이동시키는 것을 시간동안 하고 싶다.
    public IEnumerator ShakeCamera()
    {
        Vector3 localPos = transform.position;
        // - 현재시간
        float currentTime = 0f;
        while (currentTime <= time)
        {
            currentTime += Time.deltaTime;
            float x = UnityEngine.Random.Range(-1f, 1f) * size;
            float y = UnityEngine.Random.Range(-1f, 1f) * size;

            transform.position = localPos + new Vector3(x, y, 0);
            yield return null;
        }
        // 제자리로
        transform.position = localPos;
    }
    public IEnumerator LandingHandHeldCamera(float shakeSize, float time)
    {
        LandingSize = shakeSize;
        Quaternion localAngle = transform.rotation;
        //Vector3 localPos = transform.position;
        // - 현재시간
        float currentTime = 0f;
        yield return new WaitForSeconds(0.1f);
        while (currentTime <= time)
        {
            currentTime += Time.deltaTime;

            float x = UnityEngine.Random.Range(-1f, 1f) * shakeSize;
            float y = UnityEngine.Random.Range(-1f, 1f) * shakeSize;
            shakeSize -= Time.deltaTime*3;
            yield return null;
            transform.rotation = localAngle;
        }
        // 제자리로
        transform.rotation = localAngle;
        shakeSize = LandingSize;
    }

}
