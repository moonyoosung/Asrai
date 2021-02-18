using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//이미지와 텍스트 의 알파값을 줄였다 늘리고 싶다.
public class StartUIControl : MonoBehaviour
{
    public Text text;
    public Image image;
    // 알파값 변화 속도
    public float alphaSpeed = 1;
    public float changeTime = 2;
    float currentTime;
    int a = 0;
    Color orizinImage;
    // Start is called before the first frame update
    void Start()
    {
        orizinImage = image.color - new Color(0,0,0,1);
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        //시간이 흐르고
        currentTime += Time.deltaTime;
        if (currentTime > changeTime)
        {
            //만약 알파값이 0 이라면
            if (a == 0)
            {
                // 1로 바꿔주고
                a = 1;
            }
            else//0이 아니라면
            {
                // 0으로 바꿔준다.
                a = 0;
            }
            currentTime = 0;
        }
        // 변화할 색상
        Color textColor = new Color(1, 1, 1, a);

        text.color = Color.Lerp(text.color, textColor, alphaSpeed * Time.deltaTime);
        image.color = Color.Lerp(image.color, orizinImage + new Color(0, 0, 0, a), alphaSpeed * Time.deltaTime);
    }
}
