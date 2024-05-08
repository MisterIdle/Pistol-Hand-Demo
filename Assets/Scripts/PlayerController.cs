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
    public GameObject normalMuzzle;
    public GameObject doubleMuzzle;

    [Header("Explosion")]

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private SkinManager skins;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    // TEMPORARY
    private float timeSinceLastShot = 0f;
    private bool canShoot = true;
    private bool canJump = false;
    private bool stunned = false;

    // Input system
    private float horizontal;
    private Vector2 handInput;

    private bool jumped = false;
    private bool shooting = false;
    private string shoulderName;

    private void Start()
    {
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

        transform.position += new Vector3(horizontal, 0) * speed * Time.deltaTime;
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
            foreach (var muzzle in normalMuzzle.GetComponentsInChildren<Transform>())
            {
                if (muzzle != normalMuzzle.transform)
                {
                    GameObject bulletobject = Instantiate(bullet, muzzle.position, muzzle.rotation);
                    Bullet bulletScript = bulletobject.GetComponent<Bullet>();
                    bulletScript.shooter = this;
                    bulletobject.transform.Rotate(0, 0, 0);
                    bulletobject.name = "Bullet_" + name;
                }
            }
        }

        if (skins.userHand == 2)
        {
            foreach (var muzzle in doubleMuzzle.GetComponentsInChildren<Transform>())
            {
                if (muzzle != doubleMuzzle.transform)
                {
                    if (muzzle == doubleMuzzle.transform.GetChild(0))
                    {
                        GameObject bulletobject = Instantiate(bullet, muzzle.position, muzzle.rotation);
                        bulletobject.transform.Rotate(0, 0, 50);
                    }
                    else
                    {
                        GameObject bulletobject = Instantiate(bullet, muzzle.position, muzzle.rotation);
                        bulletobject.transform.Rotate(0, 0, 45);
                    }
                }
            }
        }

        if (skins.userHand == 5)
        {
            foreach (var otherPlayer in FindObjectsOfType<PlayerController>())
            {
                // Si la distance entre les deux joueurs est inférieure à 5
                if (otherPlayer != this && Vector2.Distance(transform.position, otherPlayer.transform.position) < 5)
                {
                    otherPlayer.HitPlayer(10, 20, gameObject, otherPlayer);
                }
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
            rb.AddForce(new Vector2(-direction.x, 1) * force, ForceMode2D.Impulse);
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
            hand.transform.rotation = Quaternion.Euler(0, 0, angle - 45);
        else
            hand.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    #endregion

    public void OnMove(InputAction.CallbackContext context)
    {
            horizontal = context.ReadValue<Vector2>().x;
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
