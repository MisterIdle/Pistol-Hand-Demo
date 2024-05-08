using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;
    public PlayerController shooter;

    void Start()
    {
        transform.Rotate(0, 0, 35);
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != shooter)
            {
                player.HitPlayer(10, 20, gameObject, player);
                Destroy(gameObject);
            }
        }
    }
}
