using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public Image[] skins;
    public Image[] face;
    public Image[] lives;
    public Image[] inputs;

    public RectTransform start;
    public RectTransform move;

    public TMPro.TextMeshProUGUI timer;
    public TMPro.TextMeshProUGUI round;
    public TMPro.TextMeshProUGUI count;

    void Start()
    {
        if (FindObjectsOfType(GetType()).Length > 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);

        StartCoroutine(Rotate());
    }

    public void Update()
    {
        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            start.gameObject.SetActive(false);
            move.gameObject.SetActive(false);
        }
        else
        {
            start.gameObject.SetActive(true);
            move.gameObject.SetActive(true);
        }
    }

    private IEnumerator Rotate()
    {
        float time = 0;
        while (true)
        {
            time += Time.deltaTime;
            for (int i = 0; i < skins.Length; i++)
            {
                skins[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(time * 2) * 2);
            }
            yield return null;
        }
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
