using UnityEngine;

public class Explosion : MonoBehaviour
{
    public void Start()
    {
        Destroy(gameObject, 3.0f);
    }
}
