using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSoundRight : MonoBehaviour
{
    AudioSource player;
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            SoundManager.Instance.OnRightFootSound(player);
        }
    }
}
