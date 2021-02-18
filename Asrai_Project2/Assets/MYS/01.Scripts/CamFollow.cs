using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//target과의 distance를 벗어나면 target과의 거리좌표로 이동시키고 싶다.
public class CamFollow : MonoBehaviour
{
    //- 이동속도
    public float speed = 10;
    //- 타겟
    public GameObject target;
    //- 카메라offset거리
    public float offsetDis = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 dis = target.transform.position - transform.position;
        //1. target과의 offsetDis를 벗어나면 
        if (dis.magnitude> offsetDis)
        {
            //4. 이동시키고 싶다.
            transform.position = Vector3.Lerp(transform.position, target.transform.position, speed * Time.deltaTime);
        }
    }
}
