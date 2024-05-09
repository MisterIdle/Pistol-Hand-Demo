using UnityEngine;

public class GameMananger : MonoBehaviour
{
    private static GameMananger instance;
    public static GameMananger GameInstance { get { return instance; } }

    void Awake()
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
    }
}
