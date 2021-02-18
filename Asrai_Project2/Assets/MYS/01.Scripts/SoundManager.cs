using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//오디오 소스 컴포넌트 부착
[RequireComponent(typeof(AudioSource))]
//play씬에서 배경음을 재생하고 싶다.
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    public AudioClip bgmClip;
    public AudioSource bgmPlayer;
    public AudioSource enviroPlayer;
    public float maxVolume = 0.03f;
    public AudioClip[] blockClip;
    public AudioClip blockBreak;
    public AudioClip[] parying;
    public AudioClip die;
    public AudioClip warning;
    public AudioClip detect;
    public AudioClip[] swordSlash;
    public AudioClip enviroment;
    public AudioClip HitSound;
    public AudioClip jump;
    public AudioClip landing;
    public AudioClip leftFoot;
    public AudioClip rightFoot;
    public AudioClip dash;
    public AudioClip executefinish;
    PlayerControl pc;

    // Start is called before the first frame update
    void Start()
    {
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        bgmPlayer = GetComponent<AudioSource>();
        SetUpBgm();
        bgmPlayer.Play();
        SetUpEnviromentSound();
    }
    public void ExecuteFinish(AudioSource player)
    {
        player.clip = executefinish;
        player.volume = 0.5f;
        player.Play();
    }
    public void OnDashSound(AudioSource player)
    {
        player.clip = dash;
        player.volume = 0.2f;
        player.Play();
    }
    public void OnLeftFootSound(AudioSource player)
    {
        player.clip = leftFoot;
        player.volume = 0.1f;
        player.Play();
    }
    public void OnRightFootSound(AudioSource player)
    {
        player.clip = rightFoot;
        player.volume = 0.1f;
        player.Play();
    }
    public void OnLandingSound(AudioSource player)
    {
        player.clip = landing;
        player.volume = 0.25f;
        player.Play();
    }
    public void OnJumpSound(AudioSource player)
    {
        player.clip = jump;
        player.volume = 0.2f;
        player.Play ();
    }
    public void HitOnPlayer(AudioSource player)
    {
        player.clip = HitSound;
        player.volume = 0.5f;
        player.Play();
    }
    public void SetUpEnviromentSound()
    {
        enviroPlayer.clip = enviroment;
        enviroPlayer.loop = true;
        enviroPlayer.volume = 0.01f;
        enviroPlayer.Play();
    }

    public void SetUpBgm()
    {
        bgmPlayer.clip = bgmClip;
        bgmPlayer.loop = true;
        bgmPlayer.volume = maxVolume;
    }

    public void OnSwordSlash(AudioSource player1, AudioSource player2)
    {
        int index;
        if (pc.attackCount == 1)
        {
            index = 0;
        }
        else if(pc.attackCount == 2)
        {
            index = 1;
        }
        else
        {
            index = 2;
        }

        if (player1.isPlaying)
        {
            player2.clip = swordSlash[index];
            player2.volume = 0.2f;
            player2.Play();
        }
        else
        {
            player1.clip = swordSlash[index];
            player1.volume = 0.2f;
            player1.Play();
        }
    }
    //막을 때 발생하는 사운드를 재생
    public void OnBlockSoundPlayer(AudioSource player1, AudioSource player2, AudioSource player3)
    {
        //        
        //함수가 불릴때마다 랜덤하게 숫자를 뽑아
        int index = Random.Range(0, blockClip.Length);
        //만약 player1이 재생중이면
        if (player1.isPlaying)
        {
            //player2에 재생
            player2.clip = blockClip[index];
            player2.volume = 1;
            player2.Play();

        }
        else
        {
            player1.clip = blockClip[index];
            player1.volume = 1;
            player1.Play();
        }
        //만약 player1,2 둘다 재생중이면
        if (player1.isPlaying && player2.isPlaying)
        {
            //player3에 재생
            player3.clip = blockClip[index];
            player3.volume = 1;
            player3.Play();
        }
    }

    public void OnBreakBlockSound(AudioSource player)
    {
        player.clip = blockBreak;
        player.volume = 1;
        player.Play();
    }

    public void OnParyingSound(AudioSource player1, AudioSource player2, AudioSource player3)
    {
        //        
        //함수가 불릴때마다 랜덤하게 숫자를 뽑아
        int index = Random.Range(0, parying.Length);
        //만약 player1이 재생중이면
        if (player1.isPlaying)
        {
            //player2에 재생
            player2.clip = parying[index];
            player2.volume = 1;
            player2.Play();

        }
        else
        {
            player1.clip = parying[index];
            player1.volume = 1;
            player1.Play();
        }
        //만약 player1,2 둘다 재생중이면
        if (player1.isPlaying && player2.isPlaying)
        {
            //player3에 재생
            player3.clip = parying[index];
            player3.volume = 1;
            player3.Play();
        }
    }
    public void OnDieSound(AudioSource player)
    {
        player.clip = die;
        player.volume = 1;
        player.Play();
    }
    // 못 막는 공격 올때 나는 소리
    public void OnWarningSound(AudioSource player)
    {
        player.clip = warning;
        player.volume = 1;
        player.Play();
    }

    public void OnEnemyDetectSound(AudioSource player)
    {
        player.clip = detect;
        player.volume = 1;
        player.Play();
    }
    public IEnumerator DoSlowSound(AudioSource player)
    {
        float timer = 0;
        while (timer < 2)
        {
            timer += Time.deltaTime;
            bgmPlayer.pitch -= Time.deltaTime * 0.2f;
            player.pitch -= Time.deltaTime * 0.2f;
            yield return null;
        }
        while (bgmPlayer.pitch <= 1)
        {
            bgmPlayer.pitch += Time.deltaTime * 0.5f;
            player.pitch += Time.deltaTime * 0.5f;
            yield return null;
        }
    }
}
