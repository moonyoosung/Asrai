using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// 사용자의 입력에 따라 앞,뒤, 좌, 우 로 이동하고 싶다.
// 중력을 적용하고 싶다.
// Space바를 누르면 점프를 시키고 싶다.
// LockOff일때 앞,뒤는v움직임을 가지고 좌, 우 움직임은 카메라 기준
public class PlayerMove : MonoBehaviour
{

    public enum PlayerState
    {
        Play,
        die
    }
    public PlayerState state;

    [Header("- Move")]
    // - 속도
    public float orzinSpeed = 7;
    public float speed = 7;
    public float dashSpeed = 20;
    public float decreaseSpeed = 10;
    public float runSpeed = 15;
    public float rotSpeed = 3;
    public float blockSpeed = 5;
    float orizinBlockSpeed;
    // - 대쉬토글
    public static bool enableDash = true;
    public static bool enableJump = true;
    public bool IsDash = false;
    public bool IsRun = false;
    public bool envasion = false;
    public float dashTime = 1;
    public float dashKeyTime = 0.5f;

    [Header("- Gravity&Jump")]
    // - 중력
    public float gravity = -10;
    // - 떨어지는 속도
    float yVelocity;
    // - 점프파워
    public float jumpPower = 5;
    // - 바닥 레이어마스크
    LayerMask layermask;
    // - 점프상태
    public bool isJump;
    public bool isFalling;
    public int jumpCount;
    public float jumpSpeed = 10;

    [Header("- Equipment")]
    public GameObject katana;


    CameraMove cam;
    CameraLock camLock;
    [HideInInspector]
    public CameraCollision camcol;
    AudioSource playerAudio;

    [HideInInspector]
    public Vector3 lookDir;
    [HideInInspector]
    public Vector3 dir;
    [HideInInspector]
    public Vector3 enemyDir;
    // - 애니메이터
    Animator anim;
    [HideInInspector]
    // - 캐릭터 컨트롤러
    public CharacterController cc;
    public static bool IsMove;
    public bool IsBlock;
    public float blockingTimde = 0.3f;
    float blockCurrentTime;
    [HideInInspector]
    public float currentTime;
    float h;
    float v;
    // Start is called before the first frame update
    void Start()
    {
        playerAudio = GetComponent<AudioSource>();
        orizinBlockSpeed = blockSpeed;
        cc = GetComponent<CharacterController>();
        cam = Camera.main.GetComponentInParent<CameraMove>();
        camLock = Camera.main.GetComponent<CameraLock>();
        camcol = Camera.main.GetComponent<CameraCollision>();
        anim = GetComponentInChildren<Animator>();
        IsMove = true;
        orzinSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case PlayerState.Play:
                OnPlaying();
                break;
            case PlayerState.die:
                OnDie();
                break;
        }

    }

    private void OnDie()
    {
        currentTime += Time.deltaTime;
        //print(currentTime);
        //카메라 죽음으로 돌리기
        Camera.main.gameObject.GetComponentInParent<CameraMove>().state = CameraMove.State.PlayerDead;
        //Die UI를 활성화시키고
        UIManager.Instance.dieUI.SetActive(true);
        UIManager.Instance.text1.color += new Color(0, 0, 0, UIManager.Instance.a * Time.deltaTime);
        UIManager.Instance.text2.color += new Color(0, 0, 0, UIManager.Instance.a * Time.deltaTime);
        if (currentTime > 2f)
        {
            //만약 플레이어의 부활 횟수가 남아있다면
            if (UIManager.Instance.life >= 1)
            {
                //부활을 선택하면
                if (Input.GetButtonUp("Fire1"))
                {
                    //카메라 처음으로 돌리기
                    Camera.main.gameObject.GetComponentInParent<CameraMove>().state = CameraMove.State.LockOff;
                    UIManager.Instance.text1.color -= new Color(0, 0, 0, 1);
                    UIManager.Instance.text2.color -= new Color(0, 0, 0, 1);
                    state = PlayerState.Play;
                    EnablePlayer();

                    UIManager.Instance.dieUI.SetActive(false);
                    UIManager.Instance.PLAYERHP = 80;
                    UIManager.Instance.LIFE--;
                    anim.SetTrigger("IsAlive");

                    if (cam.state == CameraMove.State.LockOn)
                    {
                        camcol.maxDistance = camcol.lockOnMaxDistance;
                    }
                    if (cam.state == CameraMove.State.LockOff)
                    {
                        camcol.maxDistance = camcol.orizinMaxDistance;

                    }
                }
                //죽는다를 선택하면
                if (Input.GetButtonDown("Fire2"))
                {
                    //카메라 처음으로 돌리기
                    Camera.main.gameObject.GetComponentInParent<CameraMove>().state = CameraMove.State.LockOff;
                    UIManager.Instance.text1.color -= new Color(0, 0, 0, 1);
                    UIManager.Instance.text2.color -= new Color(0, 0, 0, 1);
                    state = PlayerState.Play;

                    UIManager.Instance.dieUI.SetActive(false);
                    UIManager.Instance.PLAYERHP = 100;
                    UIManager.Instance.LIFE = 3;

                    if (cam.state == CameraMove.State.LockOn)
                    {
                        camcol.maxDistance = camcol.lockOnMaxDistance;
                    }
                    if (cam.state == CameraMove.State.LockOff)
                    {
                        camcol.maxDistance = camcol.orizinMaxDistance;

                    }
                    //처음 화면으로(수정필요함, 게임매니저 연결시 FadeIn-out)
                    SceneManager.LoadScene("StartScene");
                }
            }
        }
    }


    public void EnablePlayer()
    {
        IsMove = true;
        enableDash = true;
        enableJump = true;
        PlayerControl.blockEnable = true;
        PlayerControl.attackEnable = true;
        cc.enabled = true;
    }
    public void DisablePlayer()
    {
        IsMove = false;
        enableDash = false;
        enableJump = false;
        PlayerControl.blockEnable = false;
        PlayerControl.attackEnable = false;
    }

    private void OnPlaying()
    {
        if (IsDie())
        {
            return;
        }
        //적방향값을 가져온다.
        GetEnemyDirection();
        //사용자의 입력을 받아온다.
        GetPlayerInput();
        if (IsMove)
        {
            //2. 방향을 생성하여
            dir = new Vector3(h, 0, v);
            dir.Normalize();
        }
        currentTime += Time.deltaTime;
        // 캐릭터 회전과 Walk관련"
        ControlWhenLockOn();
        ControlWhenLockOff();
        //Shift키를 눌렀을 때 대쉬를 실행
        ChangeToDashState();
        //Dash가 실행되기 위한 조건
        ControlDash();
        //Shift키가 눌리고있으면 달리기 상태로 변환
        ChangeToRunState();
        //달리기를 실행하기 위한 조건
        ControlRunState();
        //Shift키를 떼면 달리기를 끝낸다.
        CheckRunFinish();
        //다시 원상태로 돌아가기 위한 조건
        ControlNormalState();
        // 점프를 실행
        Jump();
        // 점프 후에 처리할 부분
        ChangeToFallingState();
        // 떨어지는 상태로 변화
        Landing();
        //막았을 때 처리해야할 조건
        CheckBlock();
        //맞았을 때 처리해야할 조건
        CheckHit();
        dir.y = yVelocity;
        yVelocity += gravity * Time.deltaTime;
        // 플레이어를 이동시킨다. 
        cc.Move(dir * speed * Time.deltaTime);
    }

    private void CheckHit()
    {
        //만약 맞았다면
        if (GetComponent<PlayerControl>().hitState)
        {
            IsMove = false;
            // 현재시간이 hitDelayTime 커지면
            if (currentTime > 0.2f)
            {
                IsMove = true;
                GetComponent<PlayerControl>().hitdir = Vector3.zero;
                GetComponent<PlayerControl>().hitState = false;
                speed = orzinSpeed;
            }
            speed = Mathf.Lerp(speed, 0, decreaseSpeed * Time.deltaTime);
            //플레이어를 맞은방향 반대쪽으로 밀어준다.
            dir = GetComponent<PlayerControl>().hitdir;
        }

    }

    private void ChangeToFallingState()
    {
        if (isJump)
        {

            //점프후 입력받은 속도만큼 이동시키고
            //cc.Move(dir * speed * Time.deltaTime);
            //만약 떨어지다가
            if (yVelocity < 0)
            {
                isFalling = true;
            }

            ////앞으로 ray2를 쏴서
            //// Wall 과 플레이어크기 + 0.5f에 던져서 맞았다면 
            //LayerMask wallLayer = 1 << LayerMask.NameToLayer("Wall");
            //Ray ray2 = new Ray(transform.position, transform.forward);
            //RaycastHit hitinfo2 = new RaycastHit();
            //if (Physics.Raycast(ray2, out hitinfo2, cc.bounds.extents.z + 0.1f, wallLayer))
            //{

            //    print("앞벽 검출 실행");
            //    //만약 점프카운트가 2라면
            //    if (jumpCount == 2)
            //    {
            //        yVelocity = jumpPower;
            //        jumpCount++;
            //    }

            //    isJump = false;
            //    isFalling = true;
            //    Debug.DrawLine(transform.position, hitinfo2.point, Color.yellow, 2);
            //}
            ////아래로 ray3를 쏴서 적이있다면
            //Ray ray3 = new Ray(transform.position, Vector3.down);
            //RaycastHit hitinfo3 = new RaycastHit();
            //LayerMask enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
            //if (Physics.Raycast(ray3, out hitinfo3, cc.bounds.extents.y + 0.1f, enemyLayer))
            //{

            //    print("아래벽 검출 실행");
            //    //만약 점프카운트가 2라면
            //    if (jumpCount == 2)
            //    {
            //        yVelocity = jumpPower;
            //        jumpCount++;
            //    }

            //    isJump = false;
            //    isFalling = true;
            //    Debug.DrawLine(transform.position, hitinfo3.point, Color.yellow, 2);
            //}
        }
    }

    private void Landing()
    {
        if (isFalling)
        {
            anim.SetTrigger("IsFalling");
            //아래 방향으로 ray를 쏴서
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit hitInfo = new RaycastHit();
            layermask = 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Enemy");
            // Floor 와의 플레이어크기 + 0.5f에 던져서 맞았다면 
            if (Physics.Raycast(ray, out hitInfo, cc.bounds.extents.y + 0.1f, ~layermask))
            {
                print("Floor 검출 실행");
                speed = orzinSpeed;
                // 바닥에 닿는 애니메이션을 실행
                anim.SetTrigger("IsGround");
                // 바닥에 쿵 떨어지는 느낌을 주고 싶다.
                if (cam.state == CameraMove.State.LockOn)
                {
                    camcol.maxDistance = camcol.lockOnMaxDistance;
                }
                if (cam.state == CameraMove.State.LockOff)
                {
                    camcol.maxDistance = camcol.orizinMaxDistance;

                }
                isJump = false;
                isFalling = false;
                yVelocity = 0;
                jumpCount = 0;
                enableDash = true;
                //Debug.DrawLine(transform.position, hitInfo.point, Color.red, 2);
                //카메라 속도 원위치
                cam.cameraMoveSpeed = cam.cameraOrzinMoveSpeed;
                //랜딩 카메라 효과
                StopAllCoroutines();
                StartCoroutine(cam.LandingHandHeldCamera(2f, 0.1f));
                SoundManager.Instance.OnLandingSound(playerAudio);
            }

        }
    }

    private void Jump()
    {
        // Space바를 누르면 점프를 시키고 싶다.
        if (Input.GetButtonDown("Jump") && enableJump == true)
        {
            enableDash = false;
            SoundManager.Instance.OnJumpSound(playerAudio);

            //카메라 이동속도 감소
            cam.cameraMoveSpeed = 8;
            //만약 점프카운트가 2보다 작거나 같다면
            if (jumpCount < 2)
            {
                //바닥에서만 점프
                if (jumpCount < 1)
                {
                    yVelocity = jumpPower;
                }
                jumpCount++;
                isJump = true;

                anim.SetTrigger("IsJumping");
                //만약 방향키 입력이 있고 점프가 눌렸다면
                if (h != 0 || v != 0)
                {
                    // 가속도 값을 증가시켜주고
                    speed = jumpSpeed;
                }
                else
                {
                    //입력없이 점프만 눌렸다면 속도값을 1로
                    speed = orzinSpeed;
                }
            }

        }
    }

    private void CheckBlock()
    {
        if (IsBlock)
        {
            //시간이 흐르다가
            blockCurrentTime += Time.deltaTime;
            // 현재시간이 blockTime보다 커지면
            if (blockCurrentTime > blockingTimde)
            {

                IsBlock = false;
                IsMove = true;
                blockCurrentTime = 0;
                blockSpeed = orizinBlockSpeed;
                anim.SetTrigger("BlockHitExit");
            }
            blockSpeed -= Time.deltaTime;
            //뒤 방향이 필요
            dir = -transform.forward;
        }
    }

    private void ControlNormalState()
    {
        if (IsDash == false && IsRun == false)
        {
            // 대쉬 후 IsRun 상태가 아니라면 orizinSpeed까지 속도를 줄인다.
            if (speed > orzinSpeed)
            {
                speed = orzinSpeed;
            }
        }
    }

    private void ControlRunState()
    {
        if (IsRun)//Shift가 눌려있는 상태
        {
            enableJump = false;
            ////만약 락온상태라면
            //if (cam.state == CameraMove.State.LockOn)
            //{
            //    anim.SetBool("IsRun", true);
            //}

            ////만약 락오프상태라면
            //if (cam.state == CameraMove.State.LockOff)
            //{
            //    //대시 후 달리기 실행
            //    anim.SetBool("IsRun", true);
            //}

            //// 대쉬 후 runSpeed만큼만 속도를 줄여주고
            //if (speed > runSpeed)
            //{
            //    speed -= Time.deltaTime * dashSpeed;
            //}

            // print(speed);
        }
    }

    private void ControlDash()
    {
        if (IsDash)
        {
            //현재시간이 대쉬시간보다 크다면
            if (currentTime > dashTime)
            {
                dir = Vector3.zero;
                IsDash = false;
                speed = orzinSpeed;
                //anim.speed = 1;
                enableJump = true;
            }
            else
            {
                speed = Mathf.Lerp(speed, 0, decreaseSpeed * Time.deltaTime);
                //print("speed : " + speed);

            }
            //만약 락온상태라면
            if (cam.state == CameraMove.State.LockOn)
            {
                //좌우전후
                if (h > 0) { dir = transform.right; }
                else if (h < 0) { dir = -transform.right; }
                else if (v < 0) { dir = -transform.forward; }
                else { dir = transform.forward; }

            }
            ////만약 락오프상태라면
            if (cam.state == CameraMove.State.LockOff)
            {
                //앞으로 전진하기 위한 방향 설정
                dir = transform.forward;

            }
            dir.Normalize();
            //cc.Move(dir.normalized * speed * Time.deltaTime);
        }
    }


    private void CheckRunFinish()
    {
        if (Input.GetKeyUp(KeyCode.LeftShift) && IsRun)
        {
            IsRun = false;
            speed = orzinSpeed;
            anim.SetBool("IsRun", false);
            enableJump = true;
        }
    }

    private void ChangeToRunState()
    {
        if (enableDash && IsDash == false)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                //dir = Vector3.zero;
                //IsDash = false;
                speed = runSpeed;
                anim.SetBool("IsRun", true);
                //print("달리기");
                //anim.speed = 1;
                IsRun = true;
                //currentTime = 0;
                
            }

        }


    }

    private void ChangeToDashState()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && enableDash)
        {
            //만약 현재시간이 dashKeyTime보다 커진다면
            if (currentTime > dashKeyTime)
            {
                //Speed를 Maxspeed로 바꿔준다
                speed = dashSpeed;

                IsDash = true;
                //anim.speed = 1.5f;
                anim.SetTrigger("IsDash");
                SoundManager.Instance.OnDashSound(playerAudio);

                yVelocity = 0.3f;
                enableJump = false;
                currentTime = 0;
            }
        }
    }

    private void ControlWhenLockOff()
    {
        // 만약 카메라가 락오프라면
        if (cam.state == CameraMove.State.LockOff)
        {
            if (IsMove)
            {
                // 카메라가 바라보는 방향이 앞방향이 된다.
                dir = Camera.main.transform.TransformDirection(dir);
            }
            // IsLock준비자세 애니메이션 실행
            anim.SetBool("IsLock", false);
            //만약 dir벡터가 0,0,0이 아니라면
            if (dir != Vector3.zero)
            {
                //Move블랜더로 실행되고
                anim.SetBool("IsWalk", true);
                anim.SetFloat("DirX", h);
                anim.SetFloat("DirZ", v);
            }
            //플레이어 캐릭터의 회전
            if (h != 0 || v != 0)
            {
                LookRotation(h, v);
            }
            else
            {
                anim.SetBool("IsWalk", false);
            }
        }
    }

    private void ControlWhenLockOn()
    {
        // 만약 카메라 락온이 된다면
        if (cam.state == CameraMove.State.LockOn)
        {
            // 적을 바라보는 방향이 앞방향이 된다.
            if (IsMove)
            {
                dir = transform.TransformDirection(dir);
            }           
            katana.SetActive(true);
            //만약 방향키 입력이 있다면
            if (h != 0 || v != 0)
            {
                //LockOnMove블랜더로 실행
                anim.SetBool("IsLockWalk", true);
                anim.SetFloat("DirX", h);
                anim.SetFloat("DirZ", v);
            }
            else
            {
                anim.SetBool("IsLockWalk", false);
            }
            enemyDir.y = 0;//플레이어가 위아래로 회전되는 걸 막음            
            // 회전한다.
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(enemyDir), Time.deltaTime * 10);
        }
    }

    private void GetPlayerInput()
    {
        //1. 사용자의 입력에 따라
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
    }

    private void GetEnemyDirection()
    {
        //플레이어는 락온된 적 방향으로
        enemyDir = camLock.enemy.transform.position - transform.position;
        enemyDir.Normalize();
    }

    private bool IsDie()
    {
        //만약 플레이어 체력이 0보다 작거나 같아진다면
        if (UIManager.playerhp <= 0)
        {
            state = PlayerMove.PlayerState.die;
            SoundManager.Instance.OnDieSound(playerAudio);
            cc.enabled = false;
            anim.SetTrigger("IsDead");
            print(state);
            currentTime = 0;
            return true;
        }
        return false;
    }

    public void OffDash()
    {
        IsRun = false;
        IsDash = false;
        speed = orzinSpeed;
    }
    // 플레이어를 이동하는 방향으로 회전시키고 싶다.
    public void LookRotation(float h, float v)
    {
        // 회전 방향
        lookDir = new Vector3(h, 0, v);

        // 카메라가 바라보는 방향이 앞방향이 된다.
        lookDir = Camera.main.transform.TransformDirection(lookDir);
        lookDir.y = 0;//플레이어가 위아래로 회전되는 걸 막음
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * rotSpeed);

    }


}
