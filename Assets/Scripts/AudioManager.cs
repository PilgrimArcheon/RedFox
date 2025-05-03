using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioClip menuBGAudio, gameBGAudio;
    public AudioSource audioSource;

    void Awake()
    {
        if(Instance) Destroy(gameObject);
        else Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        MenuBGAudio();
    }
    public void MenuBGAudio()
    {
        audioSource.clip = menuBGAudio;
        audioSource.volume = 0.25f;
        audioSource.Play();
    }

    public void GameBGAudio()
    {
        audioSource.clip = gameBGAudio;
        audioSource.volume = 0.2f;
        audioSource.Play();
    }
}
