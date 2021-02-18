using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationFunction : MonoBehaviour
{
    Animator anim;
    PlayerMove pm;
    PlayerControl pc;
    CameraMove cm;
    EnemyMove em;
    public GameObject blooddecal;
    public GameObject bloodPour;

    private void Start()
    {
        pm = GetComponentInParent<PlayerMove>();
        anim = GetComponent<Animator>();
        pc = GetComponentInParent<PlayerControl>();
        cm = Camera.main.GetComponentInParent<CameraMove>();
        em = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyMove>();
    }

    public void ExecutionSoundFX()
    {
        //맞는 사운드 재생
        SoundManager.Instance.HitOnPlayer(em.audioPlayer);
        //피 이펙트 재생
        GameObject hitEffect = Instantiate(blooddecal, em.transform.position, Quaternion.identity);
        hitEffect.transform.parent = transform;
        Destroy(hitEffect, 0.5f);
    }
    public void FinishExecutionSoundFx()
    {
        SoundManager.Instance.ExecuteFinish(em.audioPlayer);
        //피 이펙트 재생
        GameObject hitEffect = Instantiate(bloodPour, em.transform.position, Quaternion.LookRotation(em.transform.forward));
        hitEffect.transform.parent = transform;
        Destroy(hitEffect, 3);
    }

    //애니메이션에서 호출될 함수
    public void HitExcution()
    {
        //카메라 제어
        cm.state = CameraMove.State.LockOn;
        pm.camcol.maxDistance = pm.camcol.lockOnMaxDistance;

        PlayerControl.attackEnable = true;
        pc.state = PlayerControl.AttackState.AttackIdle;
        print("PlayerHitExcution 함수 실행");
        //캐릭터 컨트롤러를 다시 켜준다.
        //pm.cc.enabled = true;
    }
    public void OnExitJump()
    {
        anim.SetTrigger("ExitJump");
    }

    public void OnEnvasion()
    {
        pm.envasion = true;
    }
    public void OffEnvasion()
    {
        pm.envasion = false;
    }

    //살아나는 애니메이션에서 호출될 함수
    public void PlayerRevive()
    {
        pm.EnablePlayer();

        print("애니메이션 함수 호출");
    }


}
