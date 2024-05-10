using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections;
using Cinemachine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

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
    public float handSpeed = 10f;

    [Header("Shooting")]
    public GameObject bullet;
    public GameObject muzzle;
    public GameObject explosion;
    public bool stunned = false;
    public int lives = 3;
    public float shootCooldown = 0.5f;
    public float timeSinceLastShot = 0f;

    [Header("Components")]
    private Rigidbody2D rb;
    private TrailRenderer trail;
    private SkinManager skins;
    private Transform groundCheck;
    private LayerMask groundLayer;
    private GameManager gameManager;

    [Header("HUD")]
    public Image skinHUD;
    public float ShakeTime;

    [Header("Other")]
    public GameObject face;
    public GameObject[] spawnPoint;
    public bool alive = true;
    public bool detected = false;
    public bool canMove = true;
    public bool canPunch = false;

    [Header("Input")]
    private Vector2 movement;
    private Vector2 handInput;
    private bool jumped = false;
    private bool shooting = false;

    private float slowfactor = 0.05f;
    private float slowDuration = 0.02f;

    private CinemachineVirtualCamera virtualCamera;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        skins = GetComponent<SkinManager>();
        groundCheck = transform.Find("GroundCheck");
        groundLayer = LayerMask.GetMask("Ground");

        virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
        gameManager = FindObjectOfType<GameManager>();

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(virtualCamera);
        DontDestroyOnLoad(Camera.main.gameObject);

        canMove = true;
        canPunch = false;

        name = "Player " + playerID;
    }

    void Update()
    {
        NeverOutside();
        Punch();

        if (alive && canMove)
        {
            Movement();
            Jump();
            Hand();

            if (shooting)
            {
                Shoot();
            }

            if (ShakeTime > 0)
            {
                ShakeTime -= Time.deltaTime;
                if (ShakeTime <= 0)
                {
                    CinemachineBasicMultiChannelPerlin noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                    noise.m_AmplitudeGain = 0;
                }
            }

            LiveState(true);
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

        if (movement.y > 0.95 && IsGrounded() && !stunned)
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

        if (hand.transform.position.x < transform.position.x)
        {
            hand.transform.localScale = new Vector3(-1, 1, 1);
            if (skins.userHand == 0)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                hand.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            }
            else
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                hand.transform.rotation = Quaternion.Euler(0, 0, angle - 145);
            }
        }
        else
        {
            hand.transform.localScale = new Vector3(1, 1, 1);

            if (skins.userHand == 0)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                hand.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            }
            else
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                hand.transform.rotation = Quaternion.Euler(0, 0, angle - 45);
            }
        }
    }

    void Shoot()
    {
        if (Time.time - timeSinceLastShot > shootCooldown)
        {
            if(skins.userHand == 0)
            {
                Dash();
            }

            if (skins.userHand == 1)
            {
                Shooting();
            }
        }
    }

    void Shooting()
    {
        GameObject newBullet = Instantiate(bullet, muzzle.transform.position, hand.transform.rotation);
        newBullet.GetComponent<Bullet>().shooter = this;

        if(hand.transform.position.x < transform.position.x)
        {
            newBullet.transform.Rotate(0, 0, 145);
        }
        else
        {
            newBullet.transform.Rotate(0, 0, 35);
        }

        timeSinceLastShot = Time.time;
    }

    void Dash()
    {
        Vector2 direction = transform.position - hand.transform.position;
        direction.Normalize();

        rb.AddForce(-direction * 30, ForceMode2D.Impulse);
        trail.emitting = true;
        canMove = false;
        canPunch = true;

        StartCoroutine(StopDash());
        timeSinceLastShot = Time.time;
    }

    void Punch()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(hand.transform.position, 1f);
        if (canPunch)
        {
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
    }

    public IEnumerator StopDash()
    {
        yield return new WaitForSeconds(0.5f);
        rb.velocity = Vector2.zero;
        trail.emitting = false;
        canMove = true;
        canPunch = false;
    }

    public void HitPlayer(int degat, int force, GameObject target, PlayersController player)
    {
        if (player.stunned)
            return;

        Vector2 direction = target.transform.position - player.transform.position;
        direction.Normalize();

        StartCoroutine(player.Stun());
        StartCoroutine(StunAndSlowMotion());

        if (player.stunned)
            rb.AddForce(new Vector2(-direction.x, 1) * force, ForceMode2D.Impulse);

        player.lives -= degat;
    }

    private void Dead()
    {
        if(lives <= 0)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            LiveState(false);
        }
    }

    private void NeverOutside()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        if (screenPos.x > Screen.width || screenPos.x < 0 || screenPos.y > Screen.height || screenPos.y < 0)
        {
            if (screenPos.x > Screen.width)
                transform.position = new Vector2(-transform.position.x, transform.position.y);

            if (screenPos.x < 0)
                transform.position = new Vector2(-transform.position.x, transform.position.y);

            trail.emitting = false;
        }
    }

    public void LiveState(bool isAlive)
    {
        if(!isAlive)
        {
            gameManager.playersInGame--;
            skins.spriteRenderer.enabled = false;
            skins.faceRenderer.enabled = false;
            skins.handRenderers[0].enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
            alive = false;
        }
        else if(!detected)
        {
            gameManager.playersInGame++;
            skins.spriteRenderer.enabled = true;
            skins.faceRenderer.enabled = true;
            skins.handRenderers[0].enabled = true;

            int randomFace = Random.Range(0, skins.faceSkins.Length - 1);

            skins.hudManager.face[playerID].sprite = skins.faceSkins[randomFace];
            skins.hudManager.face[playerID].color = new Color(1, 1, 1, 1);
            skins.faceRenderer.sprite = skins.faceSkins[randomFace];

            skins.hudManager.lives[playerID].color = Color.green;

            GetComponent<BoxCollider2D>().enabled = true;

            transform.position = new Vector2(0, 0);

            alive = true;
            detected = true;
        }
    }

    IEnumerator StunAndSlowMotion()
    {
        Time.timeScale = slowfactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        yield return new WaitForSeconds(slowDuration);

        ShakeCamera(5f, .1f);
        Time.timeScale = 1f;

        yield return new WaitForSeconds(0.1f);
        Dead();
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

    void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = intensity;
        ShakeTime = time;
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

        if (context.performed)
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
