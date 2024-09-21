using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public List<AudioClip> bgmClips;
    public List<AudioClip> battleClips;
    public List<AudioClip> buttonClips;
    public List<AudioClip> cardClips;
    public AudioSource audioSource;
    public AudioSource SEaudioSource;
    public AudioSource voiceSource;
    private string currentSceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlaySound();
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    private void Update()
    {
        string newSceneName = SceneManager.GetActiveScene().name;
        if (newSceneName != currentSceneName)
        {
            currentSceneName = newSceneName;
            PlaySound();
            // シーン切り替え後の処理
        }
    }

    public void PlaySound()
    {
        if (SceneManager.GetActiveScene().name == "Title")
        {
            audioSource.clip = bgmClips[0];
        }
        else if (SceneManager.GetActiveScene().name == "ChoiceDeckNumber")
        {
            audioSource.clip = bgmClips[1];
        }
        else if (SceneManager.GetActiveScene().name == "playGame")
        {
            audioSource.clip = bgmClips[2];
        }
        else if (SceneManager.GetActiveScene().name == "makeDeck")
        {
            audioSource.clip = bgmClips[3];
        }
        audioSource.Play();
        // サウンド再生処理
    }

    public void PlayBattleSound()
    {
        SEaudioSource.clip = battleClips[0];
        SEaudioSource.Play();
    }

    public void PlayDrawSound()
    {
        SEaudioSource.clip = cardClips[0];
        SEaudioSource.Play();
    }

    public void PlayPlayCardSound()
    {
        SEaudioSource.clip = cardClips[1];
        SEaudioSource.Play();
    }

    public void ButtonSound()
    {
        SEaudioSource.clip = buttonClips[1];
        SEaudioSource.Play();
    }

    public void EffectSound(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            SEaudioSource.clip = audioClip;
            SEaudioSource.Play();
        }

    }

    public void voiceSound(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            voiceSource.clip = audioClip;
            voiceSource.Play();
        }

    }

    public void TurnEndButtonSound()
    {
        SEaudioSource.clip = buttonClips[0];
        SEaudioSource.Play();
    }
}
