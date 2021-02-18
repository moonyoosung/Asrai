using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    PlayerControl pc;
    EnemyMove em;

    void Start()
    {
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        em = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyMove>();
    }

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 객체태그가 Enemy고, 플레이어 상태가 공격이면
        if (other.gameObject.tag == "Enemy" && pc.state == PlayerControl.AttackState.Attack)
        {

            // 맞았다면 플레이어가 제자리에 멈춘다.
            pc.StopAttack();

            // 만약 때렸을 때 적이 막았다면
            if (em.state == EnemyMove.EnemyState.Block )
            {
                em.OnBlockHit(other.ClosestPoint(transform.position));
            }
            // 만약 때렸을 때 적이 튕겨냈다면
            else if ( em.state == EnemyMove.EnemyState.Parrying)
            {
                //플레이어 공격 도중 튕겨나가는 모션을 플레이한다.
                pc.OnAttackDelayState(other.ClosestPoint(transform.position));
                //적도 튕겨내는 모션을 플레이한다.
                em.EnemyParyingDirection(other.ClosestPoint(transform.position));
            }
            else if (em.state == EnemyMove.EnemyState.Stun && em.executionState)
            {
                print("처형");
                // 처형 애니메이션이 실행되고
                //처형을 시작한다.
                pc.OnExecution();
                em.ShotExecution();
            }
            else
            {
                // 적에게 때렷다고 전달해준다.
                em.OnHit(other.ClosestPoint(transform.position));
            }
        }
    }
}
