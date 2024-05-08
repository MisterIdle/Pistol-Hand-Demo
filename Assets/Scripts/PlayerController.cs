using Cinemachine;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float jumpForce = 20f;

    [Header("Hand")]
    public GameObject hand;
    public float maxHandDistance = 2f;
    public float handSpeed = 15f;

    [Header("Shooting")]
    public GameObject bullet;
    public GameObject muzzle;

    [Header("Explosion")]

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private SkinManager skins;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private CinemachineTargetGroup targetGroup;

    // TEMPORARY
    private float timeSinceLastShot = 0f;
    private bool canShoot = true;
    private bool canJump = false;
    private bool stunned = false;

    // Input system
    private Vector2 movement;
    private Vector2 handInput;

    private bool jumped = false;
    private bool shooting = false;
    private bool floorHit = false;
    private bool falling = false;
    private string shoulderName;

    private float fallTime = 0.5f;

    private void Start()
    {
        targetGroup = FindObjectOfType<CinemachineTargetGroup>();

        targetGroup.AddMember(transform, 1, 7);
        name = "Player_" + skins.userSkin;
    }

    void Update()
    {
        Movement();

        HandMove();
        HandRotation();

        if (shooting && canShoot)
        {
            Shoot();
            canShoot = false;
            timeSinceLastShot = 0f;
        }

        timeSinceLastShot += Time.deltaTime;
        if (timeSinceLastShot >= 0.1f)
        {
            canShoot = true;
        }
    }

    private void FixedUpdate()
    {
        Jump();
    }

    #region Movement

    private void Movement()
    {
        if (stunned)
            return;

        transform.position += new Vector3(movement.x, 0) * speed * Time.deltaTime;
    }

    private void Jump()
    {
        if (jumped && IsGrounded() && !stunned)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false;
        }
        else if (jumped && canJump && !stunned)
        {
            rb.AddForce(Vector2.up * jumpForce * 2, ForceMode2D.Impulse);
            canJump = false;
        }

        if (movement.y > 0.9 && IsGrounded() && !stunned)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false;
        }
        else if (movement.y > 0.9 && canJump && !stunned)
        {
            rb.AddForce(Vector2.up * jumpForce * 2, ForceMode2D.Impulse);
            canJump = false;
        }

        if (floorHit && !stunned && !falling && !IsGrounded())
        {
            rb.AddForce(Vector2.down * 25, ForceMode2D.Impulse);
            falling = true;

            StartCoroutine(FallTime());
        }

        if (falling && IsGrounded())
        {
            falling = false;
            Bombe();
            // A fix, bn chuis éclaté !
            StopCoroutine(FallTime());
        }
    }

    // Cancel la chute si le joueur touche le sol
    public IEnumerator FallTime()
    {
        // Si le joueur tombe durant plus de 2 secondes, c'est cancel
        yield return new WaitForSeconds(fallTime);
        falling = false;
        rb.velocity = Vector2.zero;
        canJump = true;
        skins.spriteRenderer.color = Color.green;
    }


    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
    #endregion

    private void Shoot()
    {
        if (skins.userHand == 3)
        {
            GameObject bulletobject = Instantiate(bullet, muzzle.transform.position, hand.transform.rotation);
            Bullet bulletScript = bulletobject.GetComponent<Bullet>();
            bulletScript.shooter = this;
            bulletScript.charged = false;
        }

        if (skins.userHand == 2)
        {
            GameObject bulletobject = Instantiate(bullet, muzzle.transform.position, hand.transform.rotation);
            Bullet bulletScript = bulletobject.GetComponent<Bullet>();
            bulletScript.shooter = this;
            bulletobject.transform.Rotate(0, 0, 55);
            bulletScript.charged = true;
        }

        if (skins.userHand == 5)
        {
            Bombe();
        }
    }

    private void Bombe()
    {
        foreach (var otherPlayer in FindObjectsOfType<PlayerController>())
        {
            if (otherPlayer != this && Vector2.Distance(transform.position, otherPlayer.transform.position) < 5)
            {
                otherPlayer.HitPlayer(10, 5, gameObject, otherPlayer);
            }
        }
    }


    #region Hit
    public void HitPlayer(int degat, int force, GameObject target, PlayerController player)
    {
        if(player.stunned)
            return;

        Vector2 direction = target.transform.position - player.transform.position;
        direction.Normalize();

        StartCoroutine(Stun());

        if(player.stunned)
            rb.AddForce(new Vector2(-direction.x, 5) * force, ForceMode2D.Impulse);
    }
    #endregion

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

    #region Hand
    private void HandMove()
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

    private void HandRotation()
    {

        Vector3 lookDirection = hand.transform.position - transform.position;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

        if (skins.userHand == 5)
            hand.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (skins.userHand == 3)
            hand.transform.rotation = Quaternion.Euler(0, 0, angle - 35);
        else
            hand.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }
    #endregion

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

    public void OnFloorHit(InputAction.CallbackContext context)
    {
        floorHit = context.action.triggered;
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        shooting = context.action.triggered;
    }

    public void OnChangeHand(InputAction.CallbackContext context)
    {
        shoulderName = context.control.name;

        if (context.performed)
        {
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
