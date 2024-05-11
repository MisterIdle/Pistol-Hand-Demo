using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections;

public class PlayersController : MonoBehaviour
{
    [Header("Player")]
    public int playerID = 0;
    public int lives = 3;
    public bool stunned = false;
    public bool isDead = false;
    public int wins = 0;

    [Header("Movement")]
    public float speed = 10f;
    public float jumpForce = 20f;

    public bool canJump = true;
    public bool canMove = false;

    [Header("Hand")]
    public float maxHandDistance = 1.5f;
    public float handSpeed = 10f;

    [Header("Shooting")]
    public GameObject bullet;
    public GameObject muzzle;
    public GameObject explosion;

    public float shootCooldown = 0.5f;
    private float timeSinceLastShot = 0f;

    [Header("Dash/Hit")]
    public bool dash = false;
    public float dashTime = 0.5f;
    public float dashCooldown = 2f;
    private float timeSinceLastDash = 0f;

    [Header("HUD")]
    public Image skinHUD;

    [Header("Visual")]
    public GameObject hand;
    public GameObject face;

    [Header("Input")]
    private Vector2 movement;
    private Vector2 handInput;
    public bool jumped = false;
    public bool shooting = false;

    [Header("Components")]
    private Rigidbody2D rb;
    private TrailRenderer trail;
    private SkinManager skins;
    private Transform groundCheck;
    private LayerMask groundLayer;
    private GameManager gameManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        skins = GetComponent<SkinManager>();
        groundCheck = transform.Find("GroundCheck");
        groundLayer = LayerMask.GetMask("Ground");
        gameManager = FindObjectOfType<GameManager>();

        DontDestroyOnLoad(gameObject);

        name = "Player " + playerID;
    }

    void Update()
    {
        if (!stunned && canMove && !dash)
        {
            Movement();
            Jump();
            Hand();

            if (shooting)
                Shoot();
        }

        if (dash)
            Punch();

        if (!isDead)
            Dead();
    }

    void Movement()
    {
        transform.position += new Vector3(movement.x, 0) * speed * Time.deltaTime;
    }

    void Jump()
    {
        if (jumped && IsGrounded() && !stunned)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        if (movement.y > 0.95 && IsGrounded() && !stunned)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
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

        Vector3 directionToHand = hand.transform.position - transform.position;
        float angle = Mathf.Atan2(directionToHand.y, directionToHand.x) * Mathf.Rad2Deg;

        // if hand is on the right side of the player flip the player
        if (hand.transform.position.x > transform.position.x)
        {
            hand.transform.localScale = new Vector3(1, 1, 1);
            hand.transform.rotation = Quaternion.Euler(0, 0, angle - 35);
        }
        else
        {
            hand.transform.localScale = new Vector3(-1, 1, 1);
            hand.transform.rotation = Quaternion.Euler(0, 0, angle - 145);
        }

    }

    void Shoot()
    {
        if (Time.time - timeSinceLastShot > shootCooldown)
            if (skins.userHand == 1)
                Shooting();


        if (Time.time - timeSinceLastDash > dashCooldown)
            if (skins.userHand == 0)
            {
                Dash();
                dash = true;
            }
    }

    void Shooting()
    {
        GameObject newBullet = Instantiate(bullet, muzzle.transform.position, hand.transform.rotation);
        newBullet.GetComponent<Bullet>().shooter = this;
        if (hand.transform.position.x > transform.position.x)
            newBullet.transform.Rotate(0, 0, 35);

        else
            newBullet.transform.Rotate(0, 0, 145);

        timeSinceLastShot = Time.time;
    }

    void Dash()
    {
        Vector2 direction = transform.position - hand.transform.position;
        direction.Normalize();

        rb.AddForce(-direction * 30, ForceMode2D.Impulse);
        trail.emitting = true;

        StartCoroutine(StopDash());
        timeSinceLastDash = Time.time;
    }

    void Punch()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(hand.transform.position, 1f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayersController player = hit.GetComponent<PlayersController>();
                if (player.playerID != playerID)
                {
                    player.HitPlayer(1, 30, hand, player);
                }
            }
        }
    }

    public IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashTime);
        rb.velocity = Vector2.zero;
        trail.emitting = false;
        dash = false;
    }

    public void HitPlayer(int degat, int force, GameObject target, PlayersController player)
    {
        if (player.stunned)
            return;

        Vector2 direction = target.transform.position - player.transform.position;
        direction.Normalize();

        StartCoroutine(player.Stun());

        StartCoroutine(gameManager.StunAndSlowMotion());
        gameManager.ShakeCamera(5f, .1f);

        if (player.stunned)
            rb.AddForce(new Vector2(-direction.x, 1) * force, ForceMode2D.Impulse);

        player.lives -= degat;
    }

    private void Dead()
    {
        if(lives <= 0)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            IsDead(true);
        }
    }

    public void IsDead(bool state)
    {
        if(state)
        {
            skins.spriteRenderer.enabled = false;
            skins.faceRenderer.enabled = false;
            skins.handRenderers[0].enabled = false;

            skins.hudManager.face[playerID].sprite = skins.faceSkins[skins.faceSkins.Length - 1];
            skins.faceRenderer.sprite = skins.faceSkins[skins.faceSkins.Length - 1];

            GetComponent<BoxCollider2D>().enabled = false;

            gameManager.playersDeath++;

            canMove = false;
            isDead = true;
        }
        else
        {
            skins.spriteRenderer.enabled = true;
            skins.faceRenderer.enabled = true;
            skins.handRenderers[0].enabled = true;

            skins.hudManager.face[playerID].sprite = skins.faceSkins[skins.userHead];
            skins.faceRenderer.sprite = skins.faceSkins[skins.userHead];

            lives = 3;

            GetComponent<BoxCollider2D>().enabled = true;

            isDead = false;
        }
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


    string shoulderName;

    public void OnChangeHand(InputAction.CallbackContext context)
    {
        shoulderName = context.control.name;

        if (context.performed && !stunned && canMove && !dash)
        {
            Debug.Log(shoulderName);

            if (shoulderName == "leftShoulder")
                skins.userHand--;

            else if (shoulderName == "rightShoulder")
                skins.userHand++;


            if (skins.userHand >= skins.skinSets[skins.userSkin].handSkins.Length)
                skins.userHand = 0;

            else if (skins.userHand < 0)
                skins.userHand = skins.skinSets[skins.userSkin].handSkins.Length - 1;
        }
    }
}
