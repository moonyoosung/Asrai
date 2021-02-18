using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    public float minDistance = 1;
    public float maxDistance = 4;
    public float lockOnMaxDistance = 2;
    public float orizinMaxDistance;
    public float smooth = 10;
    Vector3 dollyDir;
    public Vector3 dollyDiradjusted;
    public float distance;
    public int layer;
    CameraMove cm;
    PlayerControl pc;
    // Start is called before the first frame update
    void Start()
    {
        orizinMaxDistance = maxDistance;
        dollyDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
        cm = GetComponentInParent<CameraMove>();
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
    }

    // Update is called once per frame
    void Update()
    {
        ////만약 락온이 된다면 maxdistance값을 LockonMaxdistance값으로 변경하고
        //if(cm.state == CameraMove.State.LockOn)
        //{
        //    maxDistance = lockOnMaxDistance;
        //}
        //if(cm.state == CameraMove.State.LockOff)
        //{
        //    maxDistance = orizinMaxDistance;
            
        //}
        if(cm.state == CameraMove.State.PlayerDead)
        {
            maxDistance = 9;
        }
        if(pc.state == PlayerControl.AttackState.Execution)
        {
            //카메라콜리전의 max값을 1.5로 변경
            maxDistance = Mathf.Lerp(maxDistance, 1.5f, Time.deltaTime);
        }

        Vector3 desiredCameraPos = transform.parent.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;
        layer = 1 << LayerMask.NameToLayer("Player") | 1<<LayerMask.NameToLayer("Enemy") | 1<<LayerMask.NameToLayer("Sowrd");
        
        //레이캐스트를 던져서 맞는 부분이 플레이어 레이어를 제외한 다른 레이어 라면
        if (Physics.Linecast(transform.parent.position, desiredCameraPos, out hit, ~layer))
        {
            distance = Mathf.Clamp((hit.distance * 0.87f), minDistance, maxDistance);
        }
        else
        {
            distance = maxDistance;
        }
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, dollyDir * distance, Time.deltaTime * smooth);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.parent.position, distance);
    }
}
