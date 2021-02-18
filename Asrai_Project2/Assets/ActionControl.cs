using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionControl : MonoBehaviour
{
    public Animator enemy;
    public Animator dog;
    public Animator player;
    public GameObject sword;
    // 적 애니메이션
    public void OnEnemyWalk()
    {
        enemy.SetTrigger("Walk");
    }
    public void StopEnemyWalk()
    {
        enemy.SetTrigger("Idle");
    }
    public void GiveSword()
    {
        enemy.SetTrigger("Give");
    }
    public void EndGiveEnemy()
    {
        enemy.SetTrigger("EndGive");
    }
    // 강아지 애니메이션
    public void OnDogWalk()
    {
        dog.SetTrigger("Walk");
    }
    public void Dogreject()
    {
        dog.SetTrigger("Reject");
    }
    public void OnDogIdle()
    {
        dog.SetTrigger("Idle");
    }

    //플레이어 애니메이션
    public void OnPlayerLanding()
    {
        player.SetTrigger("Landing");
    }
    public void PlayerGetUp()
    {
        player.SetTrigger("Getup");
    }
    public void OnPlayerSword()
    {
        player.SetTrigger("OnSword");
        sword.SetActive(true);

    }
}
