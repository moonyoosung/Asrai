using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//사용자의 입력에 따라 좌,우, 상,하로 회전시키고 싶다.
public class CamRotate : MonoBehaviour
{
    // - 회전속도
    public float speed = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //1. 사용자의 마우스 입력에 따라
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        //2. 방향을 생성하고
        Vector3 dir = new Vector3(-y, x, 0);
        dir.Normalize();
        //3. 회전시킨다.
        transform.eulerAngles += dir * speed ;
    }
}
