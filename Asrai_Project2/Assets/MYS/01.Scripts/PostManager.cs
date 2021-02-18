using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostManager : MonoBehaviour
{
    public static PostManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    PostProcessVolume post;
    DepthOfField dof;
    // Start is called before the first frame update
    void Start()
    {
        post = GetComponent<PostProcessVolume>();
    }

    public void OnDepthOfField()
    {

    }
}
