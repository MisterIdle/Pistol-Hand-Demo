using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayersController : MonoBehaviour
{
    [Header("Player")]
    public int playerID = 0;

    [Header("Movement")]
    public float speed = 10f;
    public float jumpForce = 20f;
    public bool canJump = true;

    [Header("Hand")]
    public GameObject hand;
    public float maxHandDistance = 1.5f;
    public float handSpeed = 15f;

    [Header("Shooting")]
    public GameObject bullet;
    public GameObject muzzle;
    public bool stunned = false;

    [Header("Components")]
    private Rigidbody2D rb;
    private TrailRenderer trail;
    private SkinManager skins;
    private Transform groundCheck;
    private LayerMask groundLayer;

    [Header("HUD")]
    public Canvas HUD;
    public Image skinHUD;

    [Header("Other")]
    public GameObject face;

    [Header("Input")]
    private Vector2 movement;
    private Vector2 handInput;
    private bool jumped = false;
    private bool shooting = false;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        skins = GetComponent<SkinManager>();
        groundCheck = transform.Find("GroundCheck");
        groundLayer = LayerMask.GetMask("Ground");

        name = "Player " + playerID;
    }

    void Update()
    {
        Movement();
        Jump();
        Hand();

        if (shooting)
        {
            Shoot();
            Punch();
        }

    }

    void Movement()
    {
        transform.position += new Vector3(movement.x, 0) * speed * Time.deltaTime;
    }

    void Jump()
    {
        if (jumped && IsGrounded() && !stunned)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        if(movement.y > 0.9 && IsGrounded() && !stunned)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    void Hand()
    {
        Vector3 newPosition = hand.transform.position + new Vector3(handInput.x, handInput.y) * handSpeed * Time.deltaTime;
        Vector2 direction = newPosition - transform.position;

        if (direction.magnitude > maxHandDistance)
        {
            direction = direction.normalized * maxHandDistance;
            newPosition = transform.position + (Vector3)direction;
        }

        hand.transform.position = newPosition;
    }

    void Shoot()
    {
    }

    void Punch()
    {
        // Dans un distance de 3 unités, tout le monde est hit
        if (Vector2.Distance(hand.transform.position, transform.position) < 3)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(hand.transform.position, 0.5f);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    PlayersController player = hit.GetComponent<PlayersController>();
                    if (player.playerID != playerID)
                    {
                        player.HitPlayer(1, 20, hand, player);
                    }
                }
            }
        }
    }

    public void HitPlayer(int degat, int force, GameObject target, PlayersController player)
    {
        if (player.stunned)
            return;

        Vector2 direction = target.transform.position - player.transform.position;
        direction.Normalize();

        StartCoroutine(Stun());

        if (player.stunned)
            rb.AddForce(new Vector2(-direction.x, 1.5f) * force, ForceMode2D.Impulse);
    }

    public IEnumerator Stun()
    {
        stunned = true;
        skins.spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.8f);
        stunned = false;
        skins.spriteRenderer.color = Color.white;
        rb.velocity = Vector2.zero;
        canJump = true;
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    public void OnHand(InputAction.CallbackContext context)
    {
        handInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumped = context.action.triggered;
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        shooting = context.action.triggered;
    }
}
