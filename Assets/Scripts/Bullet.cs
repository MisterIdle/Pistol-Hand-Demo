using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 40f;
    public int damage = 1;
    public Rigidbody2D rb;

    public PlayersController shooter;
    void Start()
    {
        Destroy(gameObject, 5f);

        rb.velocity = transform.right * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            PlayersController players = collision.gameObject.GetComponent<PlayersController>();

            if (players != shooter)
            {
                players.HitPlayer(damage, 20, gameObject, players);
                Destroy(gameObject);
            } 
        }

        if (collision.gameObject.layer == 6)
        {
            Destroy(gameObject);
        }
    }
}
