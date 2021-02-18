using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//플레이어의 HP를 나타내는 UI를 만들고 싶다.
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    [Space(10)]
    [Header("- Player")]
    //플레이어 현재체력
    [SerializeField]
    public static float playerhp = 100;
    //플레이어 목숨
    public int life = 3;
    //체간
    public float blockGuage = 0;

    [Header("- Enemy")]
    public float enemyHp = 100;
    public int enemyLife = 2;
    public float enemyBlock = 0;
    [Space(10)]
    [Header("- PlayerUI")]
    //Player slide UI
    public Slider hpUI;
    public Text lifeText;
    //player Block UI
    public Slider blockUILeft;
    public Slider blockUIRight;
    [Space(10)]
    [Header("- EnemyUI")]
    //Enemy slide UI
    public Slider enemyHPUI;
    public Text enemyLifeText;
    //Enemy Block UI
    public Slider enemyBloLeft;
    public Slider enemyBloRight;
    [Space(10)]
    [Header("- DieUI")]
    //Die UI
    public GameObject dieUI;
    public TextMeshProUGUI text1;
    public Text text2;
    public float a = 1;
    public bool blockDec = true;

    //줄어드는 속도
    public float decreaseBlock = 0.1f;
    PlayerControl pc;
    PlayerMove pm;
    #region"EnemyUI"
    public float ENEMYHP
    {
        get { return enemyHp; }
        set
        {
            enemyHp = value;
            enemyHPUI.value = enemyHp / 100;
        }
    }
    public float ENEMYBLOCKGUAGE
    {
        get { return enemyBlock; }
        set
        {
            enemyBlock = value;
            enemyBloLeft.value = enemyBlock / 50;
            enemyBloRight.value = enemyBlock / 50;
        }
    }

    public int ENEMYLIFE
    {
        get { return enemyLife; }
        set
        {
            enemyLife = value;
            enemyLifeText.text = "목숨 : " + enemyLife;
        }
    }
    #endregion

    #region"PlayerUI"
    public float PLAYERHP
    {
        get { return playerhp; }
        set
        {
            playerhp = value;
            hpUI.value = playerhp / 100;
        }
    }
    public float BlOCKGUAGE
    {
        get { return blockGuage; }
        set
        {
            blockGuage = value;
            blockUILeft.value = blockGuage / 50;
            blockUIRight.value = blockGuage / 50;

        }
    }
    public int LIFE
    {
        get { return life; }
        set
        {
            life = value;
            lifeText.text = "남은 목숨 : " + life;
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        pm = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>();
        hpUI.value = 1;
        enemyHPUI.value = 1;
        enemyBloLeft.value = 0;
        enemyBloRight.value = 0;
        ENEMYLIFE = enemyLife;
        blockUIRight.value = 0;
        blockUILeft.value = 0;
        LIFE = life;
    }

    // Update is called once per frame
    void Update()
    {

        // 락온이 되면 UI가 적방향으로 날라가고
        // 알파값이 점점 커진다.
        //만약 블락게이지가 0보다크거나 같다면
        if (blockGuage > 0)
        {
            //매프래임만큼 줄여준다.
            BlOCKGUAGE -= Time.deltaTime * decreaseBlock;
        }
        if(enemyBlock >0 && blockDec)
        {
            ENEMYBLOCKGUAGE -= Time.deltaTime * decreaseBlock;
        }
        //만약 플레이어가 죽었다면
        //if(pm.state == PlayerMove.PlayerState.die)
        //{
        //    //Die UI를 활성화시키고
        //    dieUI.SetActive(true);
        //    //Text의 알파값을 1로 서서히 올려준다.
        //    text1.faceColor += new Color(0, 0, 0, 1);
        //    text2.color += new Color(0, 0, 0, 1);
        //}
        ////만약 플레이어가 살았다면
        //if(pm.state == PlayerMove.PlayerState.Play)
        //{
        //    text1.color -= new Color(0, 0, 0, 1);
        //    text2.color -= new Color(0, 0, 0, 1);
        //    dieUI.SetActive(false);
        //}
        //만약 T키가 눌린다면
        if (Input.GetKeyDown(KeyCode.T))
        {
            //플레이어hp를 -10씩깍아준다.
            PLAYERHP -= 10;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            blockGuage += 30;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ENEMYBLOCKGUAGE += 30;
            
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            
            BlOCKGUAGE += 30;
        }
    }
}
