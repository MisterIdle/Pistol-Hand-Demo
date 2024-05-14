using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Transition")]
    public Image transition;

    [Header("Components")]
    public CinemachineVirtualCamera virtualCamera;
    public Camera mainCamera;
    public PlayersController player;
    public PlayersManager playersManager;
    public HUDManager hudManager;
    public SkinManager skinManager;
    public Firework firework;
    public SoundManager soundManager;

    [Header("Hit")]
    public float slowfactor = 0.05f;
    public float slowDuration = 0.02f;
    public float shakeTime;

    [Header("Game")]
    public int playersInGame = 0;
    public int playersDeath = 0;
    public int randomArena = 0;
    public int round = 0;
    public int needToWin = 3;

    public GameObject[] spawnPoints;
    public GameObject[] treePoints;

    [Header("Status")]
    public bool inGame = false;
    public bool inLobby = true;
    public bool draw = false;
    public bool map = true;
    public bool playersReady = false;

    void Start()
    {
        virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(virtualCamera.gameObject);
        DontDestroyOnLoad(Camera.main.gameObject);

        hudManager = FindObjectOfType<HUDManager>();
        mainCamera = Camera.main;

        hudManager.backother.gameObject.SetActive(false);

        StartCoroutine(Rotate());

        Screen.SetResolution(1920, 1080, true);

        for (int i = 0; i < 4; i++)
        {
            hudManager.crown[i].enabled = false;
        }

        FadeOut(0.5f);
    }

    private void FixedUpdate()
    {
        InGame();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0)
            {
                CinemachineBasicMultiChannelPerlin noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                noise.m_AmplitudeGain = 0;
            }
        }
    }

    public void InGame()
    {
        player = FindObjectOfType<PlayersController>();
        skinManager = FindObjectOfType<SkinManager>();
        soundManager = FindObjectOfType<SoundManager>();

        soundManager.musicAudio.volume = 0.4f;

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
        playersManager = FindObjectOfType<PlayersManager>();
        soundManager.musicAudio.volume = 0.2f;

        if ((playersDeath == playersInGame - 1 && playersInGame != 1 && !inGame) || (playersInGame > 1 && playersDeath == playersInGame))
        {
            if (inLobby)
            {
                Debug.Log("Loading Game");
                inLobby = false;

                foreach (PlayersController player in FindObjectsOfType<PlayersController>())
                {
                    player.crown.enabled = false;
                }

                FadeIn(1f);
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
        yield return new WaitForSeconds(1f);

        MoveCameraTransition(true, 0.5f);
        map = false;

        List<int> randomMapTaken = new List<int>();

        yield return new WaitForSeconds(1f);

        if (!map)
        {
            randomArena = Random.Range(1, SceneManager.sceneCountInBuildSettings);

            while (randomMapTaken.Contains(randomArena))
            {
                randomArena = Random.Range(1, SceneManager.sceneCountInBuildSettings);
            }

            randomMapTaken.Add(randomArena);
            LoadScene(randomArena);

            map = true;
        }

        yield return new WaitForSeconds(0.5f);

        virtualCamera.m_Lens.OrthographicSize = 14;

        spawnPoints = GameObject.FindGameObjectsWithTag("Spawn");

        List<int> randomPosTaken = new List<int>();

        foreach (PlayersController player in FindObjectsOfType<PlayersController>())
        {
            player.IsDead(true);

            yield return new WaitForSeconds(0.1f);
            player.canMove = false;

            player.IsDead(false);
            player.lifes = 3;

            int randomPos = Random.Range(0, spawnPoints.Length);
            while (randomPosTaken.Contains(randomPos))
            {
                randomPos = Random.Range(0, spawnPoints.Length);
            }

            randomPosTaken.Add(randomPos);

            player.transform.position = spawnPoints[randomPos].transform.position;
        }

        yield return new WaitForSeconds(0.2f);

        hudManager.backother.gameObject.SetActive(false);

        hudManager.count.color = Color.white;
        hudManager.count.text = "";

        hudManager.whoWins.color = Color.white;
        hudManager.whoWins.text = "";

        hudManager.round.text = "Round: " + round;

        yield return new WaitForSeconds(0.2f);
        StartCoroutine(StartNewRound());
    }

    IEnumerator StartNewRound()
    {
        yield return new WaitForSeconds(0.5f);

        MoveCameraTransition(false, 0.5f);
        FadeOut(0.5f);

        yield return new WaitForSeconds(0.5f);

        hudManager.timer.text = "0.00";

        round++;
        playersDeath = 0;

        hudManager.round.text = "Round: " + round;
        hudManager.count.text = "3";
        soundManager.PlayThree();

        yield return new WaitForSeconds(0.5f);
        hudManager.count.text = "2";
        soundManager.PlayTwo();

        yield return new WaitForSeconds(0.5f);
        hudManager.count.text = "1";
        soundManager.PlayOne();

        yield return new WaitForSeconds(0.5f);
        hudManager.count.text = "";
        soundManager.PlayGo();

        inGame = true;

        hudManager.StartTimer();

        foreach (PlayersController player in FindObjectsOfType<PlayersController>())
        {
            player.canMove = true;
        }
    }

    IEnumerator RoundEndStop()
    {
        hudManager.StopTimer();

        foreach (PlayersController player in FindObjectsOfType<PlayersController>())
        {
            player.crown.enabled = false;

            if (!player.isDead)
            {
                yield return new WaitForSeconds(0.1f);

                hudManager.backother.gameObject.SetActive(true);

                if (playersDeath == playersInGame)
                    draw = true;

                if (!draw)
                {
                    switch (player.playerID)
                    {
                        case 0:
                            hudManager.count.text = "Blue";
                            hudManager.whoWins.text = "Wins";
                            hudManager.count.color = Color.blue;
                            hudManager.whoWins.color = Color.white;
                            player.crown.enabled = true;
                            break;
                        case 1:
                            hudManager.count.text = "Red";
                            hudManager.whoWins.text = "Wins";
                            hudManager.count.color = Color.red;
                            hudManager.whoWins.color = Color.white;
                            player.crown.enabled = true;
                            break;
                        case 2:
                            hudManager.count.text = "Green";
                            hudManager.whoWins.text = "Wins";
                            hudManager.count.color = Color.green;
                            hudManager.whoWins.color = Color.white;
                            player.crown.enabled = true;
                            break;
                        case 3:
                            hudManager.count.text = "Yellow";
                            hudManager.whoWins.text = "Wins";
                            hudManager.count.color = Color.yellow;
                            hudManager.whoWins.color = Color.white;
                            player.crown.enabled = true;
                            break;
                    }

                    player.wins++;

                }
                else
                {
                    hudManager.count.text = "Draw";
                    hudManager.whoWins.text = "Oof";
                    hudManager.count.color = Color.white;
                    hudManager.whoWins.color = Color.white;
                }

                soundManager.PlayWin();

                draw = false;

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
            hudManager.count.fontSize = 70;
            hudManager.whoWins.fontSize = 100;

            StartCoroutine(SpawnFirework());

            hudManager.backother.gameObject.SetActive(true);

            if (player.wins == needToWin)
            {
                switch (player.playerID)
                {
                    case 0:
                        hudManager.count.text = "Winner";
                        hudManager.whoWins.text = "Blue";
                        hudManager.whoWins.color = Color.blue;
                        hudManager.count.color = Color.white;
                        break;
                    case 1:
                        hudManager.count.text = "Winner";
                        hudManager.whoWins.text = "Red";
                        hudManager.whoWins.color = Color.red;
                        hudManager.count.color = Color.white;
                        break;
                    case 2:
                        hudManager.count.text = "Winner";
                        hudManager.whoWins.text = "Green";
                        hudManager.whoWins.color = Color.green;
                        hudManager.count.color = Color.white;
                        break;
                    case 3:
                        hudManager.count.text = "Winner";
                        hudManager.whoWins.text = "Yellow";
                        hudManager.whoWins.color = Color.yellow;
                        hudManager.count.color = Color.white;
                        break;
                }
            }
        }

        soundManager.PlayWin();

        playersDeath = 0;
        round = 0;

        yield return new WaitForSeconds(5f);
        hudManager.count.text = "";
        hudManager.whoWins.text = "";

        hudManager.backother.gameObject.SetActive(false);

        hudManager.count.fontSize = 100;
        hudManager.whoWins.fontSize = 50;

        yield return new WaitForSeconds(1f);
        StartCoroutine(ReturnToLobby());
    }

    IEnumerator SpawnFirework()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 position = new Vector3(Random.Range(-20, 20), Random.Range(-5, 5), 0);
            Instantiate(firework, position, Quaternion.identity);
            soundManager.PlayFirework();
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ReturnToLobby()
    {
        FadeIn(1f);

        yield return new WaitForSeconds(1f);

        playersReady = false;

        if (!playersReady)
        {
            foreach (PlayersController playersController in FindObjectsOfType<PlayersController>())
            {
                skinManager.userSkin = -1;
                skinManager.hudManager.skins[playersController.playerID].sprite = skinManager.noPlayer;
                skinManager.hudManager.face[playersController.playerID].sprite = skinManager.faceSkins[8];
                skinManager.hudManager.lives[playersController.playerID].color = Color.white;

                playersController.playerID = 0;

                Destroy(playersController.gameObject);
            }

            playersReady = true;
        }

        yield return new WaitForSeconds(0.5f);

        LoadScene(0);

        virtualCamera.m_Lens.OrthographicSize = 12;

        inLobby = true;

        playersDeath = 0;
        round = 0;
        hudManager.round.text = "Lobby";
        hudManager.timer.text = "0.00";

        for (int i = 0; i < 4; i++)
        {
            hudManager.crown[i].enabled = false;
        }

        StartCoroutine(Rotate());

        yield return new WaitForSeconds(0.5f);

        FadeOut(1.5f);
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

    public void MoveCameraTransition(bool moveUp, float time)
    {
        int direction = moveUp ? 1 : -1;
        Vector3 targetPosition = virtualCamera.transform.position + new Vector3(0, direction * 50, 0);
        StartCoroutine(LerpCameraPosition(targetPosition, time));
    }

    IEnumerator LerpCameraPosition(Vector3 targetPosition, float time)
    {
        float elapsedTime = 0f;
        
        Vector3 originalPos = virtualCamera.transform.position;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / time);
            virtualCamera.transform.position = Vector3.Lerp(originalPos, targetPosition, t);

            yield return null;
        }
    }

    IEnumerator Rotate()
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
