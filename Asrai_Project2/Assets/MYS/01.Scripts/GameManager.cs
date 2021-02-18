using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    string nextScene;
    // 로딩 바
    public Slider loadingProgress;
    public Canvas startSceneUI;
    public GameObject uioff;
    //변화 시킬 이미지
    public Image fadeInOut;
    float currentTime;
    public float fadeTime = 1f;
    PlayableDirector pd;
    public AudioSource startAudio;
    public AudioClip buttonSound;
    bool coroutineOnce = true;
    bool coroutineOnce2 = true;
    EnemyMove em;
    public Text endText;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            startAudio.clip = buttonSound;
            startAudio.playOnAwake = false;
            startAudio.Play();
            LoadScene("CutScene");
            ButtonFade();
        }
        if (SceneManager.GetActiveScene().name == "CutScene")
        {
            CheckPD();
        }

        if(SceneManager.GetActiveScene().name == "PlayScene")
        {
            CheckEnemyDie();
        }

    }

    private void CheckEnemyDie()
    {
        if (em == null)
        {
            em = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyMove>();
        }
        if (em.isDie)
        {
            if (coroutineOnce2)
            {
                StopAllCoroutines();
                StartCoroutine(OnFadeIn());
                coroutineOnce2 = false;
            }
        }
    }

    private void CheckPD()
    {
        print(coroutineOnce);
        if(pd == null)
        {
            //PD받아옴
            //print("PD받아옴");
            pd = GameObject.FindGameObjectWithTag("PD").GetComponent<PlayableDirector>();
            //print(pd.duration);
        }
        //만약 PD의 플레이타임이 완료가 되면
        if (pd.time >= pd.duration)
        {
            if (coroutineOnce)
            {
                LoadScene("PlayScene");
                ButtonFade();
                coroutineOnce = false;
            }

        }
        //print(pd.time);
    }

    //처음 화면을 로드해오는 함수
    public void LoadScene(string sceneName)
    {
        nextScene = sceneName;
    }

    public void ButtonFade()
    {
        StopAllCoroutines();
        StartCoroutine(OnFadeInOut());
    }
    IEnumerator OnFadeIn()
    {
        startSceneUI.gameObject.SetActive(true);
        //활성화 시키고
        fadeInOut.gameObject.SetActive(true);
        currentTime = 0;
        Color alpha = fadeInOut.color;
        //알파값이 1보다 작으면 화면을 검은색으로 Fade In한다.
        while (alpha.a < 1f)
        {
            currentTime += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(0, 1, currentTime);
            fadeInOut.color = alpha;
            SoundManager.Instance.enviroPlayer.volume -= 0.1f;
            SoundManager.Instance.bgmPlayer.volume -= 0.1f;
            yield return null;
        }
        //끝 텍스트 보여주기
        endText.gameObject.SetActive(true);
    }

    // Fade In-Out기능에 씬을 로드하는 기능 추가

    IEnumerator OnFadeInOut()
    {
        startSceneUI.gameObject.SetActive(true);
        //활성화 시키고
        fadeInOut.gameObject.SetActive(true);
        currentTime = 0;
        Color alpha = fadeInOut.color;
        //알파값이 1보다 작으면 화면을 검은색으로 Fade In한다.
        while (alpha.a < 1f)
        {
            currentTime += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(0, 1, currentTime);
            fadeInOut.color = alpha;
            yield return null;
        }
        currentTime = 0;
        yield return new WaitForSeconds(1f);
        uioff.SetActive(false);
        // 화면이 완전히 검게 됐으면 로딩 UI를 작동시키고 씬을 로드한다.
        loadingProgress.gameObject.SetActive(true);
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene); // 비동기식 씬 로드
        op.allowSceneActivation = false;// 씬이 90퍼 센트만 로드 되었을 때 넘어가지 않게 설정

        float timer = 0f;
        while (!op.isDone)
        {
            yield return null;
            if (op.progress < 0.9f)
            {
                loadingProgress.value = op.progress;
            }
            else
            {
                if (SceneManager.GetActiveScene().name == "StartScene")
                {
                    startAudio.enabled = false;
                }
                timer += Time.unscaledDeltaTime;
                loadingProgress.value = Mathf.Lerp(0.9f, 1f, timer);
                if (loadingProgress.value >= 1f)
                {
                    //씬활성화
                    op.allowSceneActivation = true;
                    //로딩바 비활성화
                    loadingProgress.gameObject.SetActive(false);

                    //활성화과 되고 나면 다시 Fade-Out
                    while (alpha.a > 0f)
                    {
                        currentTime += Time.deltaTime / fadeTime / 0.5f;
                        alpha.a = Mathf.Lerp(1, 0, currentTime);
                        fadeInOut.color = alpha;
                        yield return null;
                    }
                    //Destroy(startSceneUI.gameObject);
                    //fadeInOut.gameObject.SetActive(false);
                    startSceneUI.gameObject.SetActive(false);
                    yield return null;
                }
            }
        }


        ////활성화과 되고 나면 다시 Fade-Out
        //while (alpha.a > 0f)
        //{
        //    currentTime += Time.deltaTime / fadeTime / 2;
        //    alpha.a = Mathf.Lerp(1, 0, currentTime);
        //    fadeInOut.color = alpha;
        //    yield return null;
        //}        
        //fadeInOut.gameObject.SetActive(false);

        //yield return null;
    }
}
