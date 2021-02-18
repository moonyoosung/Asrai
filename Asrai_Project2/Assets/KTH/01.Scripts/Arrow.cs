using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    // 화살 속력
    public float arrowSpeed = 10;

    PlayerControl pc;
    PlayerMove pm;
    EnemyMove eac;

    void Start()
    {
        eac = GetComponentInParent<EnemyMove>();
        pm = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
    }


    void Update()
    {
        
        // 앞으로 이동하고 싶다. 
        transform.position += transform.forward * arrowSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        //만약 충돌한 객체의 태그가 Player면
        //if (other.gameObject.tag == "Player")
        if (other.gameObject.tag == "Player")
        {
            //print(other.gameObject);
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
                if (pc.bstate == PlayerControl.BlockState.Blocking)
                {
                    pc.OnBlockHit(other.ClosestPoint(transform.position));

                }
                //RaycastHit hit;
                //if (Physics.Raycast(transform.position, -transform.forward, out hit))
                //{
                //    pc.OnBlockHit(hit.point.normalized);
                //    print(hit.point.normalized);
                //}
            }
            else
            {
                // 대쉬 중에는 안맞게 설정
                if (pm.envasion == false)
                {
                    // 맞았다 전달!
                    pc.OnPlayerHit(other.ClosestPoint(transform.position));
                }

            }

        }
    }
}
