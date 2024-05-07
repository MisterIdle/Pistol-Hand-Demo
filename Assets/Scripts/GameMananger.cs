using UnityEngine;

public class GameMananger : MonoBehaviour
{
    private static GameMananger instance;
    public static GameMananger GameInstance { get { return instance; } }

    public int usedSkin;

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
