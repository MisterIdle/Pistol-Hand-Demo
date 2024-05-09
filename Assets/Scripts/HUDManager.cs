using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public Image[] skins;
    public Image[] face;
    public Image[] lives;
    public Image[] inputs;

    public TMPro.TextMeshProUGUI timer;

    // Start the timer

    void Start()
    {
        StartTimer();
    }

    public void StartTimer()
    {
        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        float time = 0;
        while (true)
        {
            time += Time.deltaTime;
            timer.text = time.ToString("F2");
            yield return null;
        }
    }

}
