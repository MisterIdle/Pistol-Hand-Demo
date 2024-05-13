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

    public AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayJump()
    {
        audioSource.PlayOneShot(jump[Random.Range(0, jump.Length)]);
    }

    public void PlayShoot()
    {
        audioSource.PlayOneShot(shoot[Random.Range(0, shoot.Length)]);
    }

    public void PlayPunch()
    {
        audioSource.PlayOneShot(punch[Random.Range(0, punch.Length)]);
    }

    public void PlayHit()
    {
        audioSource.PlayOneShot(hit[Random.Range(0, hit.Length)]);
    }

    public void PlayDie()
    {
        audioSource.PlayOneShot(die[Random.Range(0, die.Length)]);
    }

    public void PlayFirework()
    {
        audioSource.PlayOneShot(firework[Random.Range(0, firework.Length)]);
    }

    public void PlayThree()
    {
        audioSource.PlayOneShot(three);
    }

    public void PlayTwo()
    {
        audioSource.PlayOneShot(two);
    }

    public void PlayOne()
    {
        audioSource.PlayOneShot(one);
    }

    public void PlayGo()
    {
        audioSource.PlayOneShot(go);
    }

    public void PlayWin()
    {
        audioSource.PlayOneShot(win);
    }
}
