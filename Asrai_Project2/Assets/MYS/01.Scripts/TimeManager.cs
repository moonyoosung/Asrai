﻿using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    public float slowdownFactor = 0.05f;
    public float slowdownLength = 2f;
    private void Update()
    {
        Time.timeScale += (1f / slowdownLength) * Time.unscaledDeltaTime;
        Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
    }
    public void DoSlowMotion()
    {
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;        
    }

}
