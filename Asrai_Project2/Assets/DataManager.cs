using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    [Header("- Player")]
    public float playerAttackDamage = 1;
    public float playerBlockDamage = 1;
    public float playerParyingDamage = 1;
    public float playerAttackBlockDmg = 1;
    [Header("- Enemy")]
    public float enemyAttackDamage = 1;
    public float enemyBlockDamage = 1;
    public float enemyParyingDamage = 1; 
}
