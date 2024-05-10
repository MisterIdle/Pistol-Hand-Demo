using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager GameInstance { get { return instance; } }

    public Image transition;

    public PlayersController player;
    public HUDManager hudManager;
    public SkinManager skinManager;

    public SceneAsset[] scenes;

    public bool gameCanStart = false;
    public bool inGame = false;

    public int playersInGame = 0;

    public int round;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        FadeOut();
    }

    private void Update()
    {
        Ready();
    }

    // Lobby logic
    public void Ready()
    {
        player = FindObjectOfType<PlayersController>();
        hudManager = FindObjectOfType<HUDManager>();
        skinManager = FindObjectOfType<SkinManager>();

        if (player != null)
        {
            if (playersInGame >= 2)
            {
                gameCanStart = true;
            }

            if (gameCanStart && playersInGame == 1 && SceneManager.GetActiveScene().name == "Lobby" && !inGame)
            {
                StartCoroutine(LoadArena(1));
                inGame = true;
            }
        }
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(scenes[index].name);
    }

    IEnumerator LoadArena(int index)
    {
        FadeIn();
        yield return new WaitForSeconds(1);
        LoadScene(index);
        foreach (PlayersController player in FindObjectsOfType<PlayersController>())
        {
            player.LiveState(false);
            player.detected = false;
            Debug.Log("Unloading players");
            player.canMove = false;
        }

        round = 1;
        hudManager.round.text = "ROUND : " + round;

        yield return new WaitForSeconds(0.1f);

        foreach (PlayersController player in FindObjectsOfType<PlayersController>())
        {
            player.LiveState(true);
            player.lives = 3;
            Debug.Log("Reloading players");
        }

        FadeOut();

        yield return new WaitForSeconds(1.5f);

        hudManager.count.text = "3";

        yield return new WaitForSeconds(0.5f);

        hudManager.count.text = "2";

        yield return new WaitForSeconds(0.5f);

        hudManager.count.text = "1";

        yield return new WaitForSeconds(0.5f);

        hudManager.count.text = "GO!";
        hudManager.StartTimer();

        foreach (PlayersController player in FindObjectsOfType<PlayersController>())
        {
            player.canMove = true;
        }

        yield return new WaitForSeconds(1);

        hudManager.count.text = "";

    }

    public void FadeIn()
    {
        transition.CrossFadeAlpha(1, 1, false);
        Debug.Log("Fade In");
    }

    public void FadeOut()
    {
        transition.CrossFadeAlpha(0, 1, false);
        Debug.Log("Fade Out");
    }
}
