using Cinemachine;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Transition")]
    public Image transition;

    [Header("Components")]
    private CinemachineVirtualCamera virtualCamera;
    public PlayersController player;
    public HUDManager hudManager;
    public SkinManager skinManager;

    [Header("Hit")]
    public float slowfactor = 0.05f;
    public float slowDuration = 0.02f;
    public float shakeTime;

    [Header("Game")]
    public int playersInGame = 0;
    public int playersDeath = 0;
    public int randomArena = 0;
    public int round = 0;
    public int needToWin = 2;

    [Header("Status")]
    public bool inGame = false;
    public bool inLobby = true;
    public bool draw = false;

    void Start()
    {
        virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(virtualCamera);
        DontDestroyOnLoad(Camera.main);

        hudManager = FindObjectOfType<HUDManager>();

        virtualCamera.name = "Logic Camera";
        Camera.main.name = "Effect Camera";

        StartCoroutine(Rotate());

        FadeOut(0.5f);
    }

    private void Update()
    {
        InGame();

        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0)
            {
                CinemachineBasicMultiChannelPerlin noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                noise.m_AmplitudeGain = 0;
            }
        }


        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.name == "Main Camera")
                Destroy(cam.gameObject);
        }
    }

    public void InGame()
    {
        player = FindObjectOfType<PlayersController>();
        skinManager = FindObjectOfType<SkinManager>();

        if (player != null)
        {
            playersInGame = FindObjectsOfType<PlayersController>().Length;

            if (inLobby)
                InLobby();
            else
                InParty();
        }
    }

    public void InLobby()
    {
        if(playersDeath == playersInGame - 1 && !inGame && playersInGame != 1)
        {
            if (inLobby)
            {
                Debug.Log("Loading Game");
                inLobby = false;
                StartCoroutine(LoadRandomArena());
            }
        }
    }

    public void InParty()
    {
        if (playersDeath == playersInGame - 1 && inGame && round != 0)
        {
            if (inGame)
            {
                Debug.Log("Reload Round");
                inGame = false;
                StartCoroutine(RoundEndStop());
            }
        }

    }

    IEnumerator LoadRandomArena()
    {
        FadeIn(1f);

        yield return new WaitForSeconds(1f);

        RandomArena();

        playersDeath = 0;

        foreach (PlayersController player in FindObjectsOfType<PlayersController>())
        {
            player.IsDead(true);

            yield return new WaitForSeconds(0.1f);

            player.IsDead(false);
            player.transform.position = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), 0);
        }

        yield return new WaitForSeconds(1f);

        hudManager.round.text = "Loading";
        hudManager.count.text = "";

        StartCoroutine(StartNewRound());
    }

    public IEnumerator StartNewRound()
    {
        FadeOut(1f);

        hudManager.timer.text = "0.00";

        round++;
        playersDeath = 0;

        inGame = true;

        yield return new WaitForSeconds(0.5f);
        hudManager.round.text = "Round: " + round;
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

        yield return new WaitForSeconds(0.5f);
        hudManager.count.text = "";
    }

    IEnumerator RoundEndStop()
    {
        hudManager.StopTimer();

        foreach (PlayersController player in FindObjectsOfType<PlayersController>())
        {
            if (playersDeath == playersInGame)
            {
                hudManager.count.text = "Draw!";
                draw = true;
            }

            yield return new WaitForSeconds(0.2f);

            if (!player.isDead && !draw)
            {
                if (player.playerID == 0)
                    hudManager.count.text = "Player Blue wins!";
                else if (player.playerID == 1)
                    hudManager.count.text = "Player Red wins!";
                else if (player.playerID == 2)
                    hudManager.count.text = "Player Green wins!";
                else if (player.playerID == 3)
                    hudManager.count.text = "Player Yellow wins!";

                yield return new WaitForSeconds(1f);
                draw = false;

                player.wins++;

                if (player.wins == needToWin)
                    StartCoroutine(StartFinishGame());
                else
                    StartCoroutine(LoadRandomArena());
            }
        }
    }

    IEnumerator StartFinishGame()
    {
        inGame = false;

        yield return new WaitForSeconds(3f);

        foreach (PlayersController player in FindObjectsOfType<PlayersController>())
        {
            if (player.wins == needToWin)
            {
                if (player.playerID == 0)
                    hudManager.count.text = "Player Blue is the winner!";
                else if (player.playerID == 1)
                    hudManager.count.text = "Player Red is the winner!";
                else if (player.playerID == 2)
                    hudManager.count.text = "Player Green is the winner!";
                else if (player.playerID == 3)
                    hudManager.count.text = "Player Yellow is the winner!";
            }
        }

        playersDeath = 0;
        round = 0;

        yield return new WaitForSeconds(5f);
        hudManager.count.text = "";

        yield return new WaitForSeconds(1f);
        StartCoroutine(ReturnToLobby());
    }

    IEnumerator ReturnToLobby()
    {
        FadeIn(1f);

        yield return new WaitForSeconds(1f);

        LoadScene(0);

        foreach (PlayersController player in FindObjectsOfType<PlayersController>())
        {
            player.IsDead(true);

            yield return new WaitForSeconds(0.1f);

            player.IsDead(false);
            player.transform.position = new Vector3(0, 0);

            player.wins = 0;
            player.lives = 1;
        }

        playersDeath = 0;
        round = 0;
        hudManager.round.text = "Lobby";
        hudManager.timer.text = "0.00";

        yield return new WaitForSeconds(1f);

        foreach (PlayersController player in FindObjectsOfType<PlayersController>())
        {
            player.canMove = true;
        }

        inLobby = true;

        FadeOut(1f);
    }

    private void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void FadeIn(float time)
    {
        transition.CrossFadeAlpha(1, time, false);
        Debug.Log("Fade In");
    }

    public void FadeOut(float time)
    {
        transition.CrossFadeAlpha(0, time, false);
        Debug.Log("Fade Out");
    }

    private IEnumerator Rotate()
    {
        float time = 0;
        while (true)
        {
            time += Time.deltaTime;
            for (int i = 0; i < hudManager.skins.Length; i++)
            {
                hudManager.skins[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(time * 2) * 2);
            }
            yield return null;
        }
    }

    public void RandomArena()
    {
        int random = Random.Range(1, SceneManager.sceneCountInBuildSettings);

        if (random == randomArena)
            random = Random.Range(1, SceneManager.sceneCountInBuildSettings);

        randomArena = random;

        LoadScene(randomArena);
        Debug.Log("Arena: " + randomArena);
    }

    public void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = intensity;
        shakeTime = time;
    }

    public IEnumerator StunAndSlowMotion()
    {
        Time.timeScale = slowfactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        yield return new WaitForSeconds(slowDuration);
        Time.timeScale = 1f;
    }
}
