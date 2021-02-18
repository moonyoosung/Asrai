using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EnemyMove : MonoBehaviour
{
    PlayerMove pm;



    #region "에너미 상태"
    // 에너미 상태는
    public enum EnemyState
    {
        // 태어남
        Birth,
        // 기본상태 
        Idle,
        // 좌로 이동 
        MoveLeft,
        // 우로 이동 
        MoveRight,
        // 앞으로 이동
        MoveForward,
        // 궁수모드
        ArcherMode,
        // 뒤로 이동
        MoveBackward,
        // 공격
        Attack,
        // 점프 공격
        JumpAttack,
        // 맞기,
        GetHit,
        // 방어
        Block,
        // 튕겨내기
        Parrying,
        // 스턴
        Stun,
        // 처형
        Execution
    }

    // 에너미상태를 state라 한다. 
    public EnemyState state;
    #endregion

    #region >>> 에너미 공통속성
    [Header(">>> Common")]

    // CharacterController는 cc
    CharacterController cc;
    // Animator는 anim
    Animator anim;

    // 현재시간
    float currentTime;
    // 타겟방향
    Vector3 dir;
    // 에너미 회전속력
    public float rotSpeed = 100;
    // 에너미 칼 가져오기
    public GameObject sword;
    // 에너미 손에있는 활 가져오기
    public GameObject bowHold;
    // 에너미 등에있는 활 가져오기
    public GameObject bowBack;
    // 칼 잔상
    public GameObject trail;
    #endregion

    #region >>> Birth 속성
    [Header(">>> Birth")]
    // 태어남 끝나는 시간
    public float birthFinishTime = 3;
    #endregion

    #region >>> Idle 속성
    [Header(">>> Idle")]
    // 어떤이동을 할지 결정하는 넘버
    int idleAfterNum;
    // Idle 애니메이션 끝나는 시간
    float idleFinishTime;
    // Idle 애니메이션 끝나는 최소시간
    public float idleFinishMinTime = 0;
    // Idle 애니메이션 끝나는 최대시간
    public float idleFinishMaxTime = 3;
    #endregion

    #region >>> MoveRL 필요속성
    [Header(">>> MoveRL")]
    // moveRL하는 속도 
    public float moveRLSpeed = 3;
    // moveRL이 끝나는 시간 
    float moveRLFinishTime;
    // moveRL이 끝나는 최소시간 
    public float moveRLFinishMinTime = 1.5f;
    // moveRL이 끝나는 최대시간 
    public float moveRLFinishMaxTIme = 2.5f;
    #endregion

    #region >>> ArcherMode 속성
    [Header(">>> ArcherMode")]
    // 화살발사시간
    public float arrowFireTime = 0.7f;
    // 화살공장
    public GameObject arrowFactory;
    // 격발지점 
    public Transform firePosition;
    // 쏘는사람
    public GameObject Enemy;
    // 화살 쏜 수
    int arrowCount = 0;
    // 화살 최대 개수
    public int arrowMaxCount = 4;
    #endregion

    #region >>> MoveForward 필요속성
    [Header(">>> MoveForward")]
    // 타겟을 지정한다.
    public GameObject target;
    // 타겟(플레이어) 기준 MoveForward 애니메이션이 멈추는 거리 
    public float stopRange = 3;
    // Attack 애니메이션 넘버 
    int attackNum;
    // 에너미 속력
    public float moveForwardSpeed = 10;
    #endregion

    #region >>> Attack 필요속성
    [Header(">>> Attack")]
    // 공격시간
    public float attackTime = 5;
    // 공격할 때 이동속도
    public float attackMoveSpeed = 1;
    #endregion

    #region >>> MoveBackward 필요속성
    [Header(">>> MoveBackward")]
    // 뒤로 움직이는 속도
    public float moveBackwardSpeed = 5;
    // 뒤로 움직이는게 끝나는 시간 
    public float moveBackwardFinishTime = 1;
    #endregion

    #region >>> Block, 충돌시 필요속성
    [Header(">>> Block")]
    // 플레이어가 때린 횟수
    int hitCount = 0;
    // 방어시 충돌 후 행동 랜덤 넘버
    int blockAfterNum;
    // 방어시 충돌에 의한 이동속도
    public float onBlockHitMoveSpeed = 1;
    // 방어가 끝나는 시간 
    public float blockFinishTime = 1;
    // 막는데 걸리는 시간
    public float OnBlockTime = 0.5f;
    #endregion

    public Image executeImage;
    Color orizinImage;

    AnimatorClipInfo[] currentClip;

    public Animator hh;
    public bool isDie = false;
    void Start()
    {
        pm = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();

        audioPlayer = GetComponent<AudioSource>();
        orizinImage = executeImage.color;
        // 애니메이터를 anim으로 가져온다.
        anim = GetComponent<Animator>();

        // 이 게임오브젝트에서, CharacterController 컴포넌트를 cc로 가져온다.
        cc = gameObject.GetComponent<CharacterController>();

        // 게임오브젝트 Player를 찾아서, target이라 한다. 
        target = GameObject.Find("Player");

        // 에너미상태를 태어남으로 한다.
        state = EnemyState.Birth;

        //
        hh = GetComponent<Animator>();

    }

    public bool isStun = false;
    void Update()
    {

        // 체력 체크
        CheckEnemyHp();

        // 블락게이지 체크
        CheckBlockGuage();
        // 공격에 따른 상태 확인
        CheckPlayerAttack();
        // 방어기억시간이 끝나면
        if (blockMemoryTime > blockMemoryFinTime)
        {
            // 맞은횟수 초기화
            hitCount = 0;
            // 방어기억시간 초기화
            blockMemoryTime = 0;
        }
        #region "계속 플레이어 방향으로 회전하는 코드"
        if (isDie == false)
        {
            // 계속해서 타겟방향을 추적한다.
            dir = target.transform.position - transform.position;
            // y축 방향은 고정한다. 
            dir.y = 0;
            // 타겟방향으로 회전하고싶다. 
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir.normalized), rotSpeed * Time.deltaTime);
        }
        #endregion

        //print(idleFinishTime);
        switch (state)
        {
            case EnemyState.Birth:
                Birth();
                break;
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.MoveLeft:
                MoveLeft();
                break;
            case EnemyState.MoveRight:
                MoveRight();
                break;
            case EnemyState.MoveForward:
                MoveForward();
                break;
            case EnemyState.ArcherMode:
                ArcherMode();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.JumpAttack:
                JumpAttack();
                break;

            case EnemyState.GetHit:
                GetHit();
                break;
            case EnemyState.Block:
                Block();
                break;
            case EnemyState.Parrying:
                Parrying();
                break;

            case EnemyState.MoveBackward:
                MoveBackward();
                break;
            case EnemyState.Stun:
                Stun();
                break;
            case EnemyState.Execution:
                break;
        }
    }

    public void EnemyParyingDirection(Vector3 hitpos)
    {
        // 위치에 따라 애니메이션 틀어지게 
        if (hitpos.x < 0)
        {
            anim.SetTrigger("ParryingLeft");
        }
        else if (hitpos.x > 0)
        {
            anim.SetTrigger("ParryingRight");
        }
        // 맞은 이펙트를 실행시켜주고
        GameObject parryingEffect = Instantiate(parryingFx, hitpos, Quaternion.identity);
        parryingEffect.transform.parent = transform;
        Destroy(parryingEffect, 0.5f);
        // 사운드 재생
        SoundManager.Instance.OnParyingSound(audio1, audio2, audio3);
        UIManager.Instance.BlOCKGUAGE += DataManager.Instance.enemyParyingDamage;
    }

    private void CheckPlayerAttack()
    {
        //플레이어가 공격상태이고 내가 스턴상태가 아닐때
        if (target.GetComponent<PlayerControl>().state == PlayerControl.AttackState.Attack && isStun == false)
        {
            // 선택랜덤값
            chooseHBP = UnityEngine.Random.Range(0, 100);
            chooseBP = UnityEngine.Random.Range(0, 100);

            if (chooseHBP < 30)
            {
                //그냥 맞을거다

            }
            else if (chooseHBP >= 60 && chooseHBP < 90)
            {
                // 상태를 막기로
                state = EnemyState.Block;
                currentTime = 0;

            }
            else if (chooseHBP >= 90)
            {
                // 상태를 튕겨내기로
                state = EnemyState.Parrying;
                currentTime = 0;
           
            }
            else if(chooseBP >= 30 && chooseBP < 60)
            {
                state = EnemyState.MoveForward;
                currentTime = 0;
            }

            bowHold.SetActive(false);
            bowBack.SetActive(true);
            sword.SetActive(true);
            //// 1대째 맞을 때
            //if (hitCount == 1)
            //{
            //    // 랜덤값이 60 미만 
            //    if (chooseHBP < 60)
            //    {
            //        // 상태를 맞기로
            //        state = EnemyState.GetHit;

            //        // 맞은 이펙트를 실행시켜주고
            //        GameObject hitEffect = Instantiate(hitFx, hitpos, Quaternion.identity);
            //        hitEffect.transform.parent = transform;
            //        Destroy(hitEffect, 0.5f);
            //        // 사운드 재생
            //        SoundManager.Instance.HitOnPlayer(audioPlayer);

            //        // 위치에 따라 애니메이션 틀어지게 
            //        if (hitpos.x < 0 && hitpos.z > 0)
            //        {
            //            anim.SetTrigger("GetHitLeft");
            //        }
            //        else if (hitpos.x > 0 && hitpos.z > 0)
            //        {
            //            anim.SetTrigger("GetHitRight");
            //        }
            //        else if (hitpos.z < 0)
            //        {
            //            anim.SetTrigger("GetHitBack");
            //        }
            //        else if (hitpos.z > 0)
            //        {
            //            anim.SetTrigger("GetHitFront");
            //        }

            //        // 맞은횟수 초기화 
            //        hitCount = 0;
            //    }

            //    // 랜덤값이 60 이상, 90 미만 
            //    else if (chooseHBP >= 60 && chooseHBP < 90)
            //    {
            //        // 상태를 막기로
            //        state = EnemyState.Block;

            //        // 맞은 이펙트를 실행시켜주고
            //        GameObject blockEffect = Instantiate(blockFx, hitpos, Quaternion.identity);
            //        blockEffect.transform.parent = transform;
            //        Destroy(blockEffect, 0.5f);
            //        // 사운드 재생
            //        // SoundManager.Instance.OnBlockSoundPlayer(audio1, audio2, audio3);

            //        // 위치에 따라 애니메이션 틀어지게 
            //        if (hitpos.x < 0 && hitpos.z > 0)
            //        {
            //            anim.SetTrigger("BlockLeft");
            //        }
            //        else if (hitpos.x > 0 && hitpos.z > 0)
            //        {
            //            anim.SetTrigger("BlockRight");
            //        }
            //        else if (hitpos.z >= 0 || hitpos.z < 0)
            //        {
            //            anim.SetTrigger("BlockFront");
            //        }
            //    }

            //    // 랜덤값이 90 이상 
            //    else if (chooseHBP >= 90)
            //    {
            //        // 상태를 튕겨내기로
            //        state = EnemyState.Parrying;

            //        // 맞은 이펙트를 실행시켜주고
            //        GameObject parryingEffect = Instantiate(parryingFx, hitpos, Quaternion.identity);
            //        parryingEffect.transform.parent = transform;
            //        Destroy(parryingEffect, 0.5f);
            //        // 사운드 재생
            //        // SoundManager.Instance.OnParyingSound(audio1, audio2, audio3);
            //        SoundManager.Instance.OnBreakBlockSound(audioPlayer);

            //        // 위치에 따라 애니메이션 틀어지게 
            //        if (hitpos.x < 0)
            //        {
            //            anim.SetTrigger("ParryingLeft");
            //        }
            //        else if (hitpos.x > 0)
            //        {
            //            anim.SetTrigger("ParryingRight");
            //        }
            //        // 맞은횟수 초기화 
            //        hitCount = 0;
            //    }
            //}

            //// 2대 이상 맞을 때 
            //if (hitCount >= 2)
            //{
            //    // 랜덤값이 70 미만 
            //    if (chooseBP < 70)
            //    {
            //        // 상태를 막기로
            //        state = EnemyState.Block;

            //        // 맞은 이펙트를 실행시켜주고
            //        GameObject blockEffect = Instantiate(blockFx, hitpos, Quaternion.identity);
            //        blockEffect.transform.parent = transform;
            //        Destroy(blockEffect, 0.5f);
            //        // 사운드 재생
            //        // SoundManager.Instance.OnBlockSoundPlayer(audio1, audio2, audio3);

            //        // 위치에 따라 애니메이션 틀어지게 
            //        if (hitpos.x < 0 && hitpos.z > 0)
            //        {
            //            anim.SetTrigger("BlockLeft");
            //        }
            //        else if (hitpos.x > 0 && hitpos.z > 0)
            //        {
            //            anim.SetTrigger("BlockRight");
            //        }
            //        else if (hitpos.z >= 0 || hitpos.z < 0)
            //        {
            //            anim.SetTrigger("BlockFront");
            //        }
            //    }

            //    // 랜덤값이 70 이상
            //    else if (chooseBP >= 70)
            //    {
            //        // 상태를 튕겨내기로
            //        state = EnemyState.Parrying;

            //        // 맞은 이펙트를 실행시켜주고
            //        GameObject parryingEffect = Instantiate(parryingFx, hitpos, Quaternion.identity);
            //        parryingEffect.transform.parent = transform;
            //        Destroy(parryingEffect, 0.5f);
            //        // 사운드 재생
            //        // SoundManager.Instance.OnParyingSound(audio1, audio2, audio3);
            //        SoundManager.Instance.OnBreakBlockSound(audioPlayer);

            //        // 위치에 따라 애니메이션 틀어지게 
            //        if (hitpos.x < 0)
            //        {
            //            anim.SetTrigger("ParryingLeft");
            //        }
            //        else if (hitpos.x > 0)
            //        {
            //            anim.SetTrigger("ParryingRight");
            //        }
            //        // 맞은횟수 초기화 
            //        hitCount = 0;
            //    }
            //}
        }
    }
    public void StateStun()
    {
        // 모든 행동을 중단하고 스턴상태가 된다.
        state = EnemyState.Stun;

        // 스턴 상태 애니메이션 실행
        anim.SetTrigger("IsStun");

    }
    private void CheckBlockGuage()
    {
        //만약 체간 게이지가 꽉찬다면
        if (UIManager.Instance.ENEMYBLOCKGUAGE >= 50)
        {
            print("적 스턴");
            isStun = true;
            UIManager.Instance.blockDec = false;

            // 시간초기화
            currentTime = 0;

        }
    }

    private void CheckEnemyHp()
    {
        if (UIManager.Instance.ENEMYHP <= 0)
        {
            UIManager.Instance.ENEMYLIFE--;
            UIManager.Instance.ENEMYHP = 100;
            currentTime = 0;
        }
    }

    private void Birth()
    {
        // Birth 애니메이션을 틀고
        anim.SetTrigger("Birth");

        // 시간이 흐르다가
        currentTime += Time.deltaTime;
        // 시간이 Birth가 끝나는 시간이 되면
        if (currentTime > birthFinishTime)
        {
            // 상태를 Idle로 전환하고
            state = EnemyState.Idle;
            // Idle 애니메이션을 틀고 
            anim.SetTrigger("Idle");
            // 시간을 초기화한다. 
            currentTime = 0;
        }
    }



    private void Idle()
    {

        // 시간이 흐르다가 
        currentTime += Time.deltaTime;
        // moveNum을 하나 정하고 
        idleAfterNum = UnityEngine.Random.Range(0, 100);

        // 시간이 idle이 끝나는 시간이 되면 
        if (currentTime > idleFinishTime)
        {
            // moveNum이 20보다 작으면
            if (idleAfterNum < 20)
            {
                // moveRLFinishTime을 정하고
                moveRLFinishTime = UnityEngine.Random.Range(moveRLFinishMinTime, moveRLFinishMaxTIme);
                // 상태를 LeftForward로 전환하고
                state = EnemyState.MoveLeft;
                // MoveLeft 애니메이션을 튼다.
                anim.SetTrigger("MoveLeft");
                // 시간을 초기화한다. 
                currentTime = 0;
            }
            // moveNum이 20보다 크고 40보다 작으면
            else if (idleAfterNum >= 20 && idleAfterNum < 40)
            {
                // moveRLFinishTime을 정하고
                moveRLFinishTime = UnityEngine.Random.Range(moveRLFinishMinTime, moveRLFinishMaxTIme);
                // 상태를 LeftForward로 전환하고
                state = EnemyState.MoveRight;
                // MoveRight 애니메이션을 튼다.
                anim.SetTrigger("MoveRight");
                // 시간을 초기화한다. 
                currentTime = 0;
            }
            // moveNum이 40보다 크고 60보다 작으면
            else if (idleAfterNum >= 40 && idleAfterNum < 60)
            {
                // 칼을 끄고
                sword.SetActive(false);
                // 상태를 궁수모드로 전환 
                state = EnemyState.ArcherMode;
                // 애니메이션을 EquipBow 시키고
                anim.SetTrigger("EquipBow");
                // 등에 있는 활을 끄고, 팔에 있는 활을 킨다.
                bowBack.SetActive(false);
                bowHold.SetActive(true);
                // 시간을 초기화한다. 
                currentTime = 0;
            }
            else if (idleAfterNum >= 60)
            {
                // 상태를 MoveForward로 바꿔주고
                state = EnemyState.MoveForward;
                // MoveForward 애니메이션을 틀고
                anim.SetTrigger("MoveForward");
                // 시간을 초기화한다. 
                currentTime = 0;
            }
        }
    }



    private void MoveLeft()
    {
        // 시간이 흐르다가
        currentTime += Time.deltaTime;
        // 에너미 기준, 왼쪽으로 움직이고
        cc.Move(-transform.right * moveRLSpeed * Time.deltaTime);
        // 시간이 moveRL이 끝나는 시간이 되면
        if (currentTime > moveRLFinishTime)
        {
            // idleFinishTime을 정해주고
            idleFinishTime = UnityEngine.Random.Range(0, 1);
            // 상태를 Idle로 바꿔주고
            state = EnemyState.Idle;
            // 애니메이션을 MoveLeftStop 시키고 
            anim.SetTrigger("MoveLeftStop");
            // 시간을 초기화한다. 
            currentTime = 0;
        }
    }
    private void MoveRight()
    {
        // 시간이 흐르다가
        currentTime += Time.deltaTime;
        // 에너미 기준, 오른쪽으로 움직이고 
        cc.Move(transform.right * moveRLSpeed * Time.deltaTime);
        // 시간이 moveRL이 끝나는 시간이 되면
        if (currentTime > moveRLFinishTime)
        {
            // idleFinishTime을 정해주고
            idleFinishTime = UnityEngine.Random.Range(0, 1);
            // 상태를 Idle로 바꿔주고
            state = EnemyState.Idle;
            // 애니메이션을 MoveRightStop 시키고 
            anim.SetTrigger("MoveRightStop");
            // 시간을 초기화한다. 
            currentTime = 0;
        }
    }

    private void MoveForward()
    {
        // 타겟방향으로 이동한다. 
        cc.Move(dir.normalized * moveForwardSpeed * Time.deltaTime);
        Vector3 targetPos;
        targetPos = target.transform.position - new Vector3(0, target.transform.position.y, 0);

        // 타겟과의 거리를 구하고
        float distance = Vector3.Distance(targetPos, transform.position);


        // 거리가 멈추는 거리 이내로 들어오면
        if (distance < stopRange)
        {
            // attackNum를 0 ~ 2 중에 정하고 
            attackNum = UnityEngine.Random.Range(0, 3);
            // 상태를 Attack으로 바꿔주고
            state = EnemyState.Attack;
            // Attack 애니메이션을 튼다.
            anim.SetTrigger("Attack" + attackNum);
        }
    }

    //public void MoveToTarget()
    //{
    //    // Player의 현재 위치를 받아오는 Object
    //    target = GameObject.Find("Player").transform;
    //    // Player의 위치와 이 객체의 위치를 빼고 단위 벡터화 한다.
    //    direction = (target.position - transform.position).normalized;
    //    // 가속도 지정 (추후 힘과 질량, 거리 등 계산해서 수정할 것)
    //    accelaration = 0.1f;
    //    // 초가 아닌 한 프레임으로 가속도 계산하여 속도 증가
    //    velocity = (velocity + accelaration * Time.deltaTime);
    //    // Player와 객체 간의 거리 계산
    //    float distance = Vector3.Distance(target.position, transform.position);
    //    // 일정거리 안에 있을 시, 해당 방향으로 무빙
    //    if (distance <= 10.0f)
    //    {
    //        this.transform.position = new Vector3(transform.position.x + (direction.x * velocity),
    //                                               transform.position.y + (direction.y * velocity),
    //                                                 transform.position.z);
    //    }
    //    // 일정거리 밖에 있을 시, 속도 초기화 
    //    else
    //    {
    //        velocity = 0.0f;
    //    }

    private void ArcherMode()
    {
        // 시간이 흐르다가 
        currentTime += Time.deltaTime;
        // 만약 화살 쏜 횟수가 화살 최대 개수라면


        // 2초 흐른 후, 현재시간이 격발시간이 되면
        if (currentTime > arrowFireTime)
        {
            if (arrowCount == arrowMaxCount)
            {
                // idleFinishTime을 랜덤하게 하나 정하고 
                idleFinishTime = UnityEngine.Random.Range(idleFinishMinTime, idleFinishMaxTime);

                // 화살 쏜 수를 초기화시킨다. 
                arrowCount = 0;
                // 시간을 초기화한다. 
                currentTime = 0;

                // 팔에있는 활 끄고, 등에있는 활 킨다.
                bowHold.SetActive(false);
                bowBack.SetActive(true);

                // 칼을 키고
                sword.SetActive(true);
                // 상태를 Idle로 전환한다. 
                state = EnemyState.Idle;
                // 애니메이션을 AttackStop 시킨다. 
                anim.SetTrigger("AttackStop");
            }
            else
            {
                anim.SetTrigger("ArrowFire");
                // 화살 하나를 화살공장에서 가져오고 
                GameObject arrow = Instantiate(arrowFactory);
                // 화살의 위치를 격발지점에 가져다 놓고
                arrow.transform.position = firePosition.position;
                // 화살의 방향을 설정한다.

                arrow.transform.forward = (target.transform.position - firePosition.position).normalized;
                //arrow.transform.forward = dir.normalized;
                Destroy(arrow, 1f);
                // 화살 쏜 수를 하나 증가시켜주고  
                arrowCount++;
                // 시간을 2초로 돌아간다.(다시 격발하게끔)
                currentTime = 0;
            }
        }
    }

    public bool isMove = false;

    public void EnemyOnMove()
    {
        isMove = true;
    }
    public void EnemyMoveStop()
    {
        isMove = false;
    }
    private void Attack()
    {
        // 시간이 흐르다가
        currentTime += Time.deltaTime;
        //공격대기
        if (isMove)
        {
            // 타겟방향으로 이동한다. 
            cc.Move(dir.normalized * attackMoveSpeed * Time.deltaTime);
        }


        if (currentTime > attackTime)
        {
            // idleFinishTime을 정해주고
            idleFinishTime = UnityEngine.Random.Range(idleFinishMinTime, idleFinishMaxTime);
            // 상태를 Idle로 전환한다.
            state = EnemyState.Idle;
            // 애니메이션을 AttackStop 시키고
            anim.SetTrigger("AttackStop");
            // 시간을 초기화한다. 
            currentTime = 0;
            // 칼 잔상 끄기
            trail.SetActive(false);
        }
        // 시간이 공격이 끝나는 시간이 되면
    }

    private void JumpAttack()
    {

    }


    public void OnBlockHit(Vector3 hitpos)
    {
        currentTime = 0;
        // 맞은 이펙트를 실행시켜주고
        GameObject blockEffect = Instantiate(blockFx, hitpos, Quaternion.identity);
        blockEffect.transform.parent = transform;
        Destroy(blockEffect, 0.5f);
        // 사운드 재생
        SoundManager.Instance.OnBlockSoundPlayer(audio1, audio2, audio3);
        // 체간게이지 상승
        UIManager.Instance.ENEMYBLOCKGUAGE += DataManager.Instance.enemyBlockDamage;


        //blockAfterNum = UnityEngine.Random.Range(0, 100);
        //// 만약 blockAfterNum이 30 이하면 
        //if (blockAfterNum < 10)
        //{
        //    // 상태를 뒤로 움직이는 걸로 전환하고
        //    state = EnemyState.MoveBackward;
        //    // 애니메이션을 MoveBackward 시킨다. 
        //    anim.SetTrigger("MoveBackward");
        //    // 시간을 초기화한다. 
        //    currentTime = 0;
        //}

        if (UIManager.Instance.ENEMYBLOCKGUAGE < 50)
        {
            //위치에 따라 애니메이션 틀어지게
            if (hitpos.x < 0 && hitpos.z > 0)
            {
                anim.SetTrigger("BlockLeft");
            }
            else if (hitpos.x > 0 && hitpos.z > 0)
            {
                anim.SetTrigger("BlockRight");
            }
            else if (hitpos.z >= 0 || hitpos.z < 0)
            {
                anim.SetTrigger("BlockFront");
            }
        }

        // 블락게이지 체크
        CheckEnemyHp();

        CheckBlockGuage();
    }


    float a;

    // 방어횟수
    //int blockCount = 0;

    // HBP선택랜덤값
    float chooseHBP;
    // BP선택랜덤값
    float chooseBP;

    // Pos랜덤값
    float getHitPos;
    float blockPos;
    float parryingPos;

    public AudioSource audio1;
    public AudioSource audio2;
    public AudioSource audio3;
    public AudioSource audioPlayer;
    public GameObject hitFx;
    public GameObject blockFx;
    public GameObject parryingFx;

    public void OnHit(Vector3 hitpos)
    {

        // 시간이 흐른다면 초기화하고
        currentTime = 0;
        // 검기가 있다면 끄고
        trail.SetActive(false);
        // 활이 손에 있다면 끄고 칼을 켜야지
        bowHold.SetActive(false); bowBack.SetActive(true); sword.SetActive(true);

        // 맞은횟수 1대 추가
        hitCount++;

        // 적 체력 깎음, 체간게이지 상승
        UIManager.Instance.ENEMYHP -= DataManager.Instance.playerAttackDamage;
        UIManager.Instance.ENEMYBLOCKGUAGE += DataManager.Instance.playerAttackBlockDmg;


        // 상태를 맞기로
        state = EnemyState.GetHit;

        // 맞은 이펙트를 실행시켜주고
        GameObject hitEffect = Instantiate(hitFx, hitpos, Quaternion.identity);
        hitEffect.transform.parent = transform;
        Destroy(hitEffect, 0.5f);
        // 사운드 재생
        SoundManager.Instance.HitOnPlayer(audioPlayer);

        if (UIManager.Instance.ENEMYBLOCKGUAGE < 50)
        {
            // 위치에 따라 애니메이션 틀어지게 
            if (hitpos.x < 0 && hitpos.z > 0)
            {
                anim.SetTrigger("GetHitLeft");
                hitDir = transform.right;
            }
            else if (hitpos.x > 0 && hitpos.z > 0)
            {
                anim.SetTrigger("GetHitRight");
                hitDir = -transform.right;
            }
            else if (hitpos.z < 0)
            {
                anim.SetTrigger("GetHitBack");
                hitDir = transform.forward;
            }
            else if (hitpos.z > 0)
            {
                anim.SetTrigger("GetHitFront");
                hitDir = -transform.forward;
            }
        }
        else
        {
            StateStun();
        }




    }
    Vector3 hitDir;

    public float getHitTime = 0.8f;
    public float getHitSpeed = 1;
    private void GetHit()
    {
        // 시간흘러
        currentTime += Time.deltaTime;
        // 뒤로 밀려나
        cc.Move(-transform.forward * getHitSpeed * Time.deltaTime);

        // 맞는시간 지나면
        if (currentTime > getHitTime)
        {
            anim.SetTrigger("Idle");
            state = EnemyState.Idle;
            idleFinishTime = UnityEngine.Random.Range(0, 2);

            currentTime = 0;
        }
    }

    // 방어기억시간
    float blockMemoryTime;
    // 방어기억시간 끝
    public float blockMemoryFinTime;
    public float blockHitSpeed = 1;

    private void Block()
    {

        // 시간 흘러 
        currentTime += Time.deltaTime;
        // 방어 기억시간 흘러
        blockMemoryTime += Time.deltaTime;

        // 충격에 의해 뒤로 밀리고
        //cc.Move(-transform.forward * blockHitSpeed * Time.deltaTime);

        // 현재시간이 방어시간이 끝나는 시간이 되면
        if (currentTime > blockFinishTime)
        {
            // 상태를 기본상태로 전환하고 
            state = EnemyState.Idle;
            // 애니메이션을 Idle 시킨다. 
            anim.SetTrigger("Idle");
            // 시간을 초기화한다. 
            currentTime = 0;
        }


    }
    public float paryingTime = 1f;
    private void Parrying()
    {

        currentTime += Time.deltaTime;
        if (currentTime > paryingTime)
        {
            state = EnemyState.Idle;
            currentTime = 0;
        }

    }

    private void MoveBackward()
    {
        // 시간이 흐르다가
        currentTime += Time.deltaTime;
        // 에너미 기준, 뒤쪽으로 움직이고
        cc.Move(-transform.forward * moveBackwardSpeed * Time.deltaTime);

        // 시간이 뒤로 움직이는게 끝나는 시간이 되면
        if (currentTime > moveBackwardFinishTime)
        {
            // idleFinishTime을 정해주고
            idleFinishTime = UnityEngine.Random.Range(idleFinishMinTime, idleFinishMaxTime);
            // 상태를 Idle로 전환하고
            state = EnemyState.Idle;
            // 애니메이션을 MoveBackwardStop 시키고
            anim.SetTrigger("MoveBackwardStop");
            // 시간을 초기화한다. 
            currentTime = 0;
        }
    }

    public bool executionState = false;
    public bool OnexecutinFlag = false;
    float exeTime;
    private void Stun()
    {
        exeTime += Time.deltaTime;
        currentTime += Time.deltaTime;
        // 처형 표시 UI를 표시해주고
        if (executionState == false)
        {
            executeImage.color = Color.Lerp(executeImage.color, Color.red, Time.deltaTime * 3);
        }
        if (exeTime > 1)
        {
            executionState = true;
        }
        // 스턴시간이 넘어버리면 다시 IDle상태로 전환된다.
        if (currentTime > 3)
        {
            currentTime = 0;
            state = EnemyState.Idle;
            anim.SetTrigger("Idle");
            executeImage.color = orizinImage;
            if (OnexecutinFlag)
            {
                UIManager.Instance.ENEMYBLOCKGUAGE = 0;
            }
            else
            {
                UIManager.Instance.ENEMYBLOCKGUAGE = 30;
            }
            UIManager.Instance.ENEMYHP = 100;
            UIManager.Instance.blockDec = true;
            executionState = false;
            OnexecutinFlag = false;
            exeTime = 0;
            isStun = false;
        }
    }
    //애니메이션이 끝나면 실행될 함수
    public void DefaultState()
    {
        //만약 적의 목숨이 1개 이상이면
        if (UIManager.Instance.ENEMYLIFE >= 1)
        {
            state = EnemyState.Stun;
            executeImage.color = orizinImage;
        }
        else//아니면 엔딩시네마
        {
            anim.SetTrigger("EnemyDie");
            cc.enabled = false;
            isDie = true;
            state = EnemyState.Execution;
        }
    }
    public void ShotExecution()
    {
        state = EnemyState.Execution;
        anim.SetTrigger("HitExecution");
        UIManager.Instance.ENEMYLIFE--;
        UIManager.Instance.ENEMYBLOCKGUAGE = 0;
        UIManager.Instance.ENEMYHP = 100;
        executionState = false;
        OnexecutinFlag = true;
    }

    public void TrailTrue()
    {
        trail.SetActive(true);
    }

    public void TrailFalse()
    {
        trail.SetActive(false);
    }

    public AudioSource swordSound1;
    public AudioSource swordSound2;
    public AudioSource swordSound3;
    public AudioSource swordSound4;
    public AudioSource jumpSound;

    public AudioSource arrowSound;

    public AudioSource moveLeft;
    public AudioSource moveRight;
    public AudioSource moveForward;

    public void chooseSwordSound()
    {
        int a = UnityEngine.Random.Range(0, 2);
        if (a == 0)
        {
            SwordSound1();
        }
        else if (a == 1)
        {
            SwordSound2();
        }
        else if (a == 2)
        {
            SwordSound3();
        }
    }
    public void SwordSound1()
    {
        swordSound1.Play();
    }
    public void SwordSound2()
    {
        swordSound2.Play();
    }
    public void SwordSound3()
    {
        swordSound3.Play();
    }
    public void SwordSound4()
    {
        swordSound4.Play();
    }
    public void JumpSound()
    {
        jumpSound.Play();
    }

    public void ArrowSound()
    {
        arrowSound.Play();
    }

    public void MoveLeftSound()
    {
        moveLeft.Play();
    }
    public void MoveRightSound()
    {
        moveRight.Play();
    }
    public void MoveForwardSound()
    {
        moveForward.Play();
    }





}









