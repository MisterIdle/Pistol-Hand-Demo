using System.Collections.Generic;
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    PlayersController playersController;
    int PlayerID = 0;

    public void OnPlayerJoin()
    {
        playersController = FindObjectOfType<PlayersController>();
        playersController.playerID = PlayerID;

        PlayerID++;
    }
}
