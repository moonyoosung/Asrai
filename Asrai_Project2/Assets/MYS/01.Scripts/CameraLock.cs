using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLock : MonoBehaviour
{
    [Header("-LockOnSetting")]
    public float horizontalDivision = 0.4f; // - 가로 화면 분활점
    public float verticalDivision = 0.2f;  // - 세로 화면 분활점
    public GameObject enemy;  // 적들을 담을 배열 <일단한마리만>
    public float enemyDistance = 20;  // 적을 찾을 수 있는 거리
    public float camRockSpeed = 30; // 추적하는 스피드
    public GameObject LockOnUI;//락온시 표시할 UI
    public Vector3 dir;

    // 2. 뷰포트의 가로와 세로를 3분할로 나누어
    public void CamLockOn()
    {

        // 2-1. 뷰포트를 나누어 교차 지점을 찾고
        float xoffset = Camera.main.rect.width * horizontalDivision;
        //print(xoffset);
        float yoffset = Camera.main.rect.height * verticalDivision;
        //print(yoffset);
        // 2-2. 타겟의 좌표를 뷰포트로 변경
        Vector2 enemyPos = Camera.main.WorldToViewportPoint(enemy.transform.position);
        // 3. 만약 playerpos.x가 0.4 ~ 0.6 그리고 playerpos.y가 0.5 ~ 0.8 에 있으면 추적을 안하고
        if (enemyPos.x > xoffset && enemyPos.x < 1 - xoffset && enemyPos.y > yoffset && enemyPos.y < 0.9)
        {
            //추적하지 않는다.
        }
        else
        {
            // 4.카메라를 타겟방향으로
            dir = enemy.transform.position - transform.position;
            // 5. camRockSpeed만큼 회전시키며 바라본다.
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * camRockSpeed);
        }    
    }
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawLine(transform.position, enemy.transform.position);


    //}
}
