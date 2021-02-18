using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextControl : MonoBehaviour
{
    public Text text;
    public GameObject canvas;
    // Start is called before the first frame update
    void Start()
    {
        text.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnText1()
    {
        text.enabled = true;
        text.text = "불사의 계약을 나와 맺으시게";
    }
    public void OnText2()
    {
        text.enabled = true;
        text.text = "아니되오, 겐이치로 공";
    }
    public void OnText3()
    {
        text.enabled = true;
        text.text = "설령 패배하더라도";
    }
    public void OnText4()
    {
        text.enabled = true;
        text.text = "목숨을 걸고 주군을 되찾아오는 것";
    }
    public void OnText5()
    {
        text.enabled = true;
        text.text = "그것이 내 닌자이니";
    }
    public void OnText6()
    {
        text.enabled = true;
        text.text = "모시러 왔습니다";
    }
    public void OnText7()
    {
        text.enabled = true;
        text.text = "지금, 잠시만";
    }
    public void OnText8()
    {
        text.enabled = true;
        text.text = "기다려주십시오";
    }
    public void OffText()
    {
        text.enabled = false;
    }
}
