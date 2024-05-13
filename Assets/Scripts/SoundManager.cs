using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip[] jump;
    public AudioClip[] shoot;
    public AudioClip[] punch;
    public AudioClip[] hit;
    public AudioClip[] die;
    public AudioClip[] firework;

    public AudioClip three;
    public AudioClip two;
    public AudioClip one;
    public AudioClip go;
    public AudioClip win;
    public AudioClip lastWin;

    public AudioClip[] music;

    public AudioSource sfxAudio;
    public AudioSource musicAudio;

    private bool isPlayingMusic;

    void Start()
    {
        PlayRandomMusic();
    }

    public void PlayRandomMusic()
    {
        if (!isPlayingMusic)
        {
            int randomMusic = Random.Range(0, music.Length);
            musicAudio.clip = music[randomMusic];
            musicAudio.Play();
            isPlayingMusic = true;
            StartCoroutine(WaitForMusicEnd(music[randomMusic].length, randomMusic));
        }
    }

    IEnumerator WaitForMusicEnd(float clipLength, int currentIndex)
    {
        yield return new WaitForSeconds(clipLength);
        isPlayingMusic = false;
        PlayRandomMusic();
    }

    public void PlayJump()
    {
        sfxAudio.PlayOneShot(jump[Random.Range(0, jump.Length)]);
    }

    public void PlayShoot()
    {
        sfxAudio.PlayOneShot(shoot[Random.Range(0, shoot.Length)]);
    }

    public void PlayPunch()
    {
        sfxAudio.PlayOneShot(punch[Random.Range(0, punch.Length)]);
    }

    public void PlayHit()
    {
        sfxAudio.PlayOneShot(hit[Random.Range(0, hit.Length)]);
    }

    public void PlayDie()
    {
        sfxAudio.PlayOneShot(die[Random.Range(0, die.Length)]);
    }

    public void PlayFirework()
    {
        sfxAudio.PlayOneShot(firework[Random.Range(0, firework.Length)]);
    }

    public void PlayThree()
    {
        sfxAudio.PlayOneShot(three);
    }

    public void PlayTwo()
    {
        sfxAudio.PlayOneShot(two);
    }

    public void PlayOne()
    {
        sfxAudio.PlayOneShot(one);
    }

    public void PlayGo()
    {
        sfxAudio.PlayOneShot(go);
    }

    public void PlayWin()
    {
        sfxAudio.PlayOneShot(win);
    }
}
