using System.Collections.Generic;
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    PlayersController playersController;
    GameManager gameManager;
    SkinManager skinManager;
    int PlayerID = 0;

    public void Update()
    {
        ForceResetPlayers();
    }

    public void OnPlayerJoin()
    {
        gameManager = FindObjectOfType<GameManager>();
        playersController = FindObjectOfType<PlayersController>();
        skinManager = FindObjectOfType<SkinManager>();
        playersController.playerID = PlayerID;

        Debug.Log("Player " + PlayerID + " joined the game!");

        PlayerID++;
    }

    public void ForceResetPlayers()
    {
        if (Input.GetKeyDown(KeyCode.R) && gameManager.inLobby)
        {
            foreach (PlayersController playersController in FindObjectsOfType<PlayersController>())
            {
                skinManager.userSkin = -1;
                skinManager.hudManager.skins[playersController.playerID].sprite = skinManager.noPlayer;
                skinManager.hudManager.face[playersController.playerID].sprite = skinManager.faceSkins[8];
                skinManager.hudManager.lives[playersController.playerID].color = Color.white;

                PlayerID = 0;

                Destroy(playersController.gameObject);

                Debug.Log(playersController.gameObject.name + " Reset");
            }
        }
    }
}
