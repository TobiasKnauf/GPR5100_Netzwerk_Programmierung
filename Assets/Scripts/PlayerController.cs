using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameObject LocalPlayerInstance;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float dashSpeed;

    [SerializeField] private Rigidbody2D rbPlayer;

    private Vector2 moveVal;

    [SerializeField] private float force;

    private bool isDashing;

    public bool IsDead;

    [SerializeField] private ProjectileController projectile;

    PlayerInput playerInput;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        //SetCamera();
        projectile = GameObject.Find("Projectile").GetComponent<ProjectileController>();
    }

    private void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }
        //if (!isDead)
        Move();
    }

    #region Input

    private void OnMove(InputValue inputValue)
    {
        if (!photonView.IsMine)
            return;

        moveVal = inputValue.Get<Vector2>();
    }

    private void OnDash(InputValue inputValue)
    {
        if (!photonView.IsMine)
            return;

        StartCoroutine(Dash());
    }

    private void OnShoot(InputValue inputValue)
    {
        if (!photonView.IsMine)
            return;

        Shoot();
    }

    private void OnMenu(InputValue inputValue)
    {
        if (!photonView.IsMine)
            return;
    }

    #endregion

    private void Move()
    {
        rbPlayer.AddForce(moveVal * moveSpeed * Time.fixedDeltaTime);
        Vector3 diff = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
    }


    IEnumerator Dash()
    {
        isDashing = true;
        rbPlayer.AddForce(transform.up * dashSpeed, ForceMode2D.Impulse);
        yield return new WaitForSeconds(.3f);
        isDashing = false;
    }

    private void Shoot()
    {
        playerInput.SwitchCurrentActionMap("Gameplay no Weapon");
        Vector2 vec = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
        projectile.Shoot(this, vec, force);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 10)
        {
            if (projectile.Owner != this)
            {
                if (isDashing)
                {
                    projectile.PickUp(this);
                    playerInput.SwitchCurrentActionMap("Gameplay Weapon");

                }
                else if (projectile.IsFlying)
                {
                    IsDead = true;
                }
                else
                {
                    projectile.PickUp(this);
                    playerInput.SwitchCurrentActionMap("Gameplay Weapon");
                }
            }
        }
    }

    private void Initialize()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this.gameObject;
            playerInput = GetComponent<PlayerInput>();
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }

    private void SetCamera()
    {
        CameraController _cameraWork = this.gameObject.GetComponent<CameraController>();

        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("Missing CameraController Component on playerPrefab.", this);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            //stream.SendNext(IsFiring);
            //stream.SendNext(Health);
        }
        else
        {
            // Network player, receive data
            //this.IsFiring = (bool)stream.ReceiveNext();
            //this.Health = (float)stream.ReceiveNext();
        }
    }
}