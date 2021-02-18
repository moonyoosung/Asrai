using System;
using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public GameObject cameraBase;
    public UIManager player_ui;
    public GameObject enemy;
    public bool hitState = false;
    public enum AttackState
    {
        AttackIdle,
        Attack,
        AttackDelay,
        Block,
        Stun,
        Execution
    }
    public AttackState state;

    public enum BlockState
    {
        Parying,
        Blocking
    }
    public BlockState bstate;
    public float paryingTime = 1;
    Animator anim;
    float curretTime;
    float blockCurrentTime;
    public int attackCount = 0;
    public float attackTime = 0.3f;
    public float attackSpeed = 3;
    public float StunTime = 2;
    public static bool blockEnable = true;
    public static bool attackEnable = true;
    bool activeBlock = false;
    // 검기 이펙트
    public GameObject swordEff;
    // 막기 이펙트
    public GameObject blockEff;
    public GameObject paryEffect;
    public GameObject bloodFX;
    public Transform swordEffPos;
    CameraMove cm;
    PlayerMove pm;
    public GameObject coll1;
    public GameObject coll2;
    public GameObject coll3;


    AudioSource Audio1;
    AudioSource Audio2;
    AudioSource Audio3;
    AudioSource playerAudio;



    //이동 제어
    //Coroutine co_Move;
    Transform target;

    [HideInInspector]
    public Vector3 hitdir;
    public bool stapAttack = false;
    public bool backAttack = false;


    void Start()
    {
        Audio1 = coll1.GetComponent<AudioSource>();
        Audio2 = coll2.GetComponent<AudioSource>();
        Audio3 = coll3.GetComponent<AudioSource>();

        playerAudio = GetComponent<AudioSource>();
        target = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Transform>();
        state = AttackState.AttackIdle;
        anim = GetComponentInChildren<Animator>();
        cm = Camera.main.GetComponentInParent<CameraMove>();
        pm = GetComponent<PlayerMove>();
        player_ui = GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>();
    }

    void Update()
    {
        curretTime += Time.deltaTime;
        //스턴상태로 넘어가기 위한 블락게이지 검사
        ChangeStunState();


        //공격 초기화 만약 3초가 지나면 count를 0로 바꾼다
        CheckAttackCount();

        // 방어컨트롤
        On_Block_Fire2();
        if (pm.state == PlayerMove.PlayerState.Play)
        {
            //print("state 는 : " + state);
            switch (state)
            {
                case AttackState.AttackIdle:
                    OnAttackIdle();
                    break;
                case AttackState.Attack:
                    Attack();
                    break;
                case AttackState.AttackDelay:
                    OnAttackDelay();
                    break;
                case AttackState.Block:
                    OnBlock();
                    break;
                case AttackState.Stun:
                    OnStun();
                    break;
                case AttackState.Execution:
                    OnExecutionState();
                    break;
            }


        }
    }

    private void OnAttackDelay()
    {
        curretTime += Time.deltaTime;
        //어택 딜레이 시간이 넘어가면
        if (curretTime > 1f)
        {
            //다시 Idle상태로 돌아감
            state = AttackState.AttackIdle;
            attackEnable = true;
            curretTime = 0;
        }

    }
    public void OnAttackDelayState(Vector3 hitpos)
    {
        state = AttackState.AttackDelay;
        curretTime = 0;
        attackEnable = false;
        // 플레이어 공격 도중 튕겨나가는 모션을 플레이한다.
        // 만약 부딪힌 지점이 플레이어보다 왼쪽이라면
        if (hitpos.x < 0)
        {
            anim.SetTrigger("AttackBlockLeft");

        }else if(hitpos.x > 0)
        {
            anim.SetTrigger("AttackBlockRight");
        }
    }

    private void On_Block_Fire2()
    {
        if (blockEnable)
        {
            //만약 Fire2버튼이 눌리면
            if (Input.GetButtonDown("Fire2"))
            {
                activeBlock = true;
                PlayerMove.enableDash = false;
                PlayerMove.enableJump = false;
                state = AttackState.Block;
                bstate = BlockState.Parying;
                curretTime = 0;
            }

            //만약 Fire2버튼이 떼지면
            if (Input.GetButtonUp("Fire2"))
            {
                activeBlock = false;
                PlayerMove.enableDash = true;
                PlayerMove.enableJump = true;
                anim.SetBool("IsBlock", false);
            }
        }
    }

    private void OnAttackFire()
    {
        //만약 Fire1버튼이 눌리면 공격모션을 취하고 0.9초내에 버튼이 한번더 눌린다면
        if (Input.GetButtonDown("Fire1") && attackEnable == true)
        {
            //만약 시간이 흐르다가 curretime이 attackTime 보다 크다면 //첫 공격 시간
            if (curretTime > attackTime)
            {
                //누를때마다 count를 1씩 중가시킨다.
                attackCount++;
                //만약 attackcount가 4보다 작다면
                if (attackCount > 3)
                {
                    attackCount = 1;
                }
                //공격 모션을 취한다.
                SoundManager.Instance.OnSwordSlash(Audio1, Audio2);
                state = AttackState.Attack;
                anim.SetTrigger("IsAttack");
                anim.SetInteger("AttackCount", attackCount);
                PlayerMove.IsMove = false;
                PlayerMove.enableDash = false;
                PlayerMove.enableJump = false;
                // AttackTime 후에 시간을 초기화 해준다.
                curretTime = 0;
            }
        }
    }

    private void CheckAttackCount()
    {
        if (curretTime > attackTime * 2)
        {
            attackCount = 0;
            swordEff.SetActive(false);
        }
    }

    private void OnExecutionState()
    {
        //적과의 거리가 1이 될때까지 앞으로 간다.
        if ((enemy.transform.position - transform.position).magnitude > 1)
        {
            transform.position += transform.forward * 1 * Time.deltaTime;
        }

    }

    //블락게이지가 꽉차면 불린다.
    private void OnStun()
    {
        pm.envasion = true;
        //시간이 흐르고
        blockCurrentTime += Time.deltaTime;
        // 만약 현재시간이 스턴시간보다 커진다면
        if (blockCurrentTime > StunTime)
        {
            pm.envasion = false;
            //idle상태로 돌아간다.
            state = AttackState.AttackIdle;
            PlayerMove.IsMove = true;
            PlayerMove.enableDash = true;
            PlayerMove.enableJump = true;
            blockEnable = true;
            attackEnable = true;
            anim.SetTrigger("RecoveryState");
            anim.SetLayerWeight(1, 1);
            blockCurrentTime = 0;            
        }
    }

    private void OnBlock()
    {
        //블락키가 눌리고 0.1초동안은 패링상태가 되었다가
        if (curretTime > paryingTime)
        {
            bstate = BlockState.Blocking;
        }
        switch (bstate)
        {
            case BlockState.Parying:
                break;
            case BlockState.Blocking:
                break;
        }
        // 만약 activeBlock이 true이면
        if (activeBlock)
        {
            //막기 모션을 취한다.
            anim.SetBool("IsBlock", true);
            //이동속도가 느려진다.
            pm.speed = 1.5f;
        }
        else
        {
            //막기 모션을 푼다.
            anim.SetBool("IsBlock", false);
            state = AttackState.AttackIdle;
            //이동속도가 빨라진다.
            pm.speed = pm.orzinSpeed;
        }
    }

    public bool isStun = false;
    private void ChangeStunState()
    {
        //만약 블락게이지가 꽉찬다면
        if (player_ui.BlOCKGUAGE >= 50)
        {
            state = PlayerControl.AttackState.Stun;
            
            bstate = BlockState.Parying;
            anim.SetBool("IsBlock", false);
            anim.SetLayerWeight(1, 0);
            anim.SetTrigger("IsBreakblock");
            StopAllCoroutines();
            StartCoroutine(cm.LandingHandHeldCamera(2f, 0.2f));
            //플레이어가 이동, 점프, 공격, 방어를 할 수 없는 상태가 된다.
            PlayerMove.IsMove = false;
            PlayerMove.enableDash = false;
            PlayerMove.enableJump = false;
            //blockEnable = false;
            attackEnable = false;
            //음악 재생
            SoundManager.Instance.OnBreakBlockSound(playerAudio);
            player_ui.BlOCKGUAGE = 0;
        }
    }


    // 1번만 실행되는 것들
    public void OnExecution()
    {
        attackEnable = false;
        //상태를 처형으로 전환하고
        state = AttackState.Execution;
        //처형 공격 애니메이션을 실행
        anim.SetTrigger("IsExecute");
        //슬로우모션
        TimeManager.Instance.DoSlowMotion();
        StartCoroutine(SoundManager.Instance.DoSlowSound(playerAudio));
        //캐릭터 컨트롤러를 꺼준다.
    }



    private void OnAttackIdle()
    {
        if (cm.state == CameraMove.State.LockOn)
        {
            // IsLock준비자세 애니메이션 실행
            anim.SetBool("IsLock", true);
        }

        //Idle중에 버튼이 눌리면 공격해라
        OnAttackFire();

        //if (co_Move != null)
        //{
        //    StopCoroutine(co_Move);
        //    //print("그만움직여!");

        //    pm.IsMove = true;

        //    pm.enableDash = true;
        //    pm.enableJump = true;
        //}
        //StopAttack();
        //print("그만움직여!");
    }
    private void EnableSwordEffct()
    {
        swordEff.SetActive(false);
    }

    // 클릭할 때 마다 전 단계가 실행중이라면
    private void Attack()
    {

        //검기 이펙트 켜주기
        swordEff.SetActive(true);

        switch (attackCount)
        {
            case 1:
                //플레이어를 앞으로 1만큼 이동시키고 싶다.
                //co_Move = StartCoroutine(AttackMoving(attackSpeed));
                AttackMoving(attackSpeed);

                break;
            case 2:
                //swordEff.SetActive(true);
                ////두번 째 공격 모션을 취한다.
                //anim.SetTrigger("IsAttack");
                //anim.SetInteger("AttackCount", attackCount);
                //플레이어를 앞으로 1만큼 이동시키고 싶다.
                //co_Move = StartCoroutine(AttackMoving(attackSpeed));
                AttackMoving(attackSpeed);
                break;
            case 3:
                //swordEff.SetActive(true);
                ////세번 째 공격 모션을 취한다.
                //anim.SetTrigger("IsAttack");
                //anim.SetInteger("AttackCount", attackCount);
                //플레이어를 앞으로 2만큼 이동시키고 싶다.
                //co_Move = StartCoroutine(AttackMoving(attackSpeed + 1));
                AttackMoving(attackSpeed + 1);
                break;
        }
    }
    //플레이어를 일정거리만큼 이동시키고 싶다.
    #region"코루틴Attack"
    //IEnumerator AttackMoving(float attackMoveSize)
    //{
    //    pm.IsMove = false;
    //    pm.enableDash = false;
    //    pm.enableJump = false;
    //    //플레이어의 방향입력값에 값을 준다.
    //    //이동은 PlayerMove스크립트에서 cc를 이용해서 하기 때문에
    //    //pm.cc.Move(transform.forward * attackMoveSize * Time.deltaTime);
    //    //적과의 거리가 1보다 작아지면 이동을 멈추고 싶다.
    //    if ((target.position - transform.position).magnitude > 1)
    //    {
    //        transform.position += transform.forward * attackMoveSize * Time.deltaTime;

    //    }

    //    anim.SetInteger("AttackCount", attackCount);

    //    //print("공격! 이동");
    //    // 만약 버튼이 눌리고 현재시간이 공격시간보다 커지면 
    //    if (curretTime > attackTime)
    //    {
    //        state = AttackState.AttackIdle;
    //        pm.enableDash = false;
    //        pm.enableJump = false;
    //        Invoke("EnableSwordEffct", 0.5f);
    //    }

    //    yield return null;

    //}
    #endregion
    public void AttackMoving(float attackMoveSize)
    {

        //적과의 거리가 1.5보다 작아지면 이동을 멈추고 싶다.
        if ((target.position - transform.position).magnitude > 1.5)
        {
            pm.dir = transform.forward;
            pm.speed = attackMoveSize;
        }
        // 만약 버튼이 눌리고 현재시간이 공격시간보다 커지면 
        if (curretTime > attackTime)
        {
            state = AttackState.AttackIdle;
            PlayerMove.enableDash = true;
            PlayerMove.enableJump = true;
            PlayerMove.IsMove = true;
            Invoke("EnableSwordEffct", 0.5f);
        }
    }

    //공격에 맞았을 때 처리되는 함수
    public void OnPlayerHit(Vector3 hitpos)
    {
        if (pm.state == PlayerMove.PlayerState.Play)
        {

            hitState = true;
            //pm의 시간을 초기화하고
            pm.currentTime = 0;
            //만약 hitpos가 플레이어 기준 왼쪽에 있으면

            if (hitpos.x < 0 && hitpos.z > 0)
            {
                anim.SetTrigger("OnHitLeft");
                hitdir = transform.right;
            }
            else if (hitpos.x > 0 && hitpos.z > 0)
            {
                anim.SetTrigger("OnHitRight");
                hitdir = -transform.right;
            }
            else if (hitpos.z < 0 && stapAttack == true)
            {
                anim.SetTrigger("OnHitBack");
                hitdir = transform.forward;
            }
            else if (hitpos.z > 0 && backAttack == true)
            {
                anim.SetTrigger("OnHitFront");
                hitdir = -transform.forward;
            }
            //플레이어의 피를 깍는다.
            player_ui.PLAYERHP -= DataManager.Instance.enemyAttackDamage;
            //print("적이 플레이어를 떄림");

            //이펙트를 실행시켜주고
            GameObject bloodEff = Instantiate(bloodFX, hitpos, Quaternion.LookRotation(transform.forward));
            bloodEff.transform.parent = gameObject.transform;
            Destroy(bloodEff, 1f);

            //카메라를 흔들어준다.
            StopAllCoroutines();
            StartCoroutine(cm.ShakeCamera());
            //사운드 실행시켜주고
            SoundManager.Instance.HitOnPlayer(playerAudio);
        }
    }
    public void OnParying(Vector3 hitPos)
    {
        //음악 재생
        SoundManager.Instance.OnParyingSound(Audio1, Audio2, Audio3);
        //맞은 위치가 플레이어 기준 왼쪽이면
        if (hitPos.x < 0)
        {
            //Left
            anim.SetTrigger("ParyingLeft");
        }
        //맞은 위치가 플레이어 기준 오른쪽이면
        if (hitPos.x > 0)
        {
            //Right
            anim.SetTrigger("ParyingRight");
        }
        //이펙트를 실행시켜주고
        GameObject paryEff = Instantiate(paryEffect, hitPos, Quaternion.identity);
        paryEff.transform.parent = swordEffPos;
        Destroy(paryEff, 0.5f);
        //때렸을 때 판정은 적칼에서
    }

    public void OnBlockHit(Vector3 hitPos)
    {
        SoundManager.Instance.OnBlockSoundPlayer(Audio1, Audio2, Audio3);
        //뒷걸음
        anim.SetTrigger("BlockHit");
        //체간 게이지 증가
        player_ui.BlOCKGUAGE += DataManager.Instance.enemyBlockDamage;
        //print(player_ui.BlOCKGUAGE);
        //카메라흔들기
        StartCoroutine(cm.ShakeCamera());
        //print("막았따!");
        //적 공격을 막았을 때 뒤로 밀려나고 싶다.
        //이동 상태 끄고
        PlayerMove.IsMove = false;
        pm.IsBlock = true;
        //맞은 이펙트를 실행시켜주고
        GameObject block = Instantiate(blockEff, hitPos, Quaternion.identity);
        block.transform.parent = swordEffPos;
        Destroy(block, 0.5f);
    }

    public void StopAttack()
    {
        state = AttackState.AttackIdle;
        PlayerMove.enableDash = true;
        PlayerMove.enableJump = true;
        PlayerMove.IsMove = true;
        Invoke("EnableSwordEffct", 0.5f);
    }
}
