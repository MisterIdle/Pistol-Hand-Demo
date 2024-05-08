using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float lifeTime = 2f;
    public bool charged = false;

    public PlayerController shooter;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        transform.Rotate(0, 0, 35);
        Destroy(gameObject, lifeTime);

        if(charged)
        {
            speed = 30f;
            spriteRenderer.color = Color.red;
        }
        else
        {
            speed = 20f;
            spriteRenderer.color = Color.yellow;
        }
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
                player.HitPlayer(10, 5, gameObject, player);
                Destroy(gameObject);
            }
        }

        // Compare layer mask
        if (collision.gameObject.layer == 6)
        {
            Destroy(gameObject);
        }
    }
}
