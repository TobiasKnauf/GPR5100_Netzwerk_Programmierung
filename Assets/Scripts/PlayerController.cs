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

    [SerializeField] private Rigidbody2D rbProjectile;

    public bool hasProjectile;

    private Vector2 moveVal;

    [SerializeField] private float force;

    private bool isDashing;

    private bool isDead;

    [SerializeField] private Transform projectilePos;

    PlayerInput playerInput;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        //SetCamera();
        GetProjectile();
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
        rbProjectile.transform.SetParent(null);
        rbProjectile.simulated = true;
        Vector2 vec = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
        rbProjectile.AddForce(vec.normalized * force, ForceMode2D.Impulse);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 10)
        {
            if (isDashing)
            {
                playerInput.SwitchCurrentActionMap("Gameplay Weapon");
                hasProjectile = true;
                rbProjectile.transform.SetParent(projectilePos, false);
                rbProjectile.transform.position = projectilePos.position;
                rbProjectile.simulated = false;
                
            }
            else
            {
                isDead = true;
            }
        }
    }

    private void GetProjectile()
    {
        rbProjectile = GameObject.Find("Projectile").GetComponent<Rigidbody2D>();
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