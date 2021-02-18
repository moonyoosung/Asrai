using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemySword : MonoBehaviour
{
    PlayerControl pc;
    PlayerMove pm;
    EnemyMove em;

    // Start is called before the first frame update
    void Start()
    {
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        pm = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        em = GetComponentInParent<EnemyMove>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        // 만약 충돌한 객체의 태그가 Player이고 에너미상태가 공격이라면
        if (other.gameObject.tag == "Player" && em.state == EnemyMove.EnemyState.Attack)
        {
            //만약 플레이어의 상태가 Block중이라면
            if (pc.state == PlayerControl.AttackState.Block)
            {
                //만약 block이 pary라면
                if (pc.bstate == PlayerControl.BlockState.Parying)
                {
                    //패링실행
                    pc.OnParying(other.ClosestPoint(transform.position));
                    //적캐릭터 체간 상승
                }
                else if (pc.bstate == PlayerControl.BlockState.Blocking)
                {
                    pc.OnBlockHit(other.ClosestPoint(transform.position));

                }

            }
            else
            {
                if (pm.envasion == false)
                {
                    pc.OnPlayerHit(other.ClosestPoint(transform.position));
                }
            }

        }
        //else
        //{
        //    // 대쉬 중에는 안맞게 설정
        //    if (pm.envasion == false)
        //    {
        //        // 맞았다 전달!
        //        pc.OnPlayerHit(other.ClosestPoint(transform.position));

        //    }

        //}

    }

}



