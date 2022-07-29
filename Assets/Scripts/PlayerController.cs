using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

using System.Linq;
using System;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameObject LocalPlayerInstance;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float dashSpeed;

    [SerializeField] private SpriteRenderer body;
    [SerializeField] private SpriteRenderer projectileIndicator;
    [SerializeField] private Color hasProjectileColor;
    [SerializeField] private Color hasNoProjectileColor;
    [SerializeField] private Image cooldownIndicator;

    [SerializeField] private Rigidbody2D rbPlayer;

    private Vector2 moveVal;

    [SerializeField] private float force;
    [SerializeField] private float cooldown;
    [SerializeField] private float dashCooldown;
    [SerializeField] private float shootCooldown;

    private bool isDashing;

    public bool IsDead;

    [SerializeField] private ProjectileController projectile;

    [SerializeField] PlayerInput playerInput;

    [SerializeField] private VisualEffect deathVisualEffect;

    private Camera cam;
    private ScreenShake screenShake;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        screenShake = cam.GetComponent<ScreenShake>();
        projectile = GameManager.Instance.Projectile;
    }

    private void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

        if (!IsDead)
            Cooldown();
    }

    private void Cooldown()
    {
        if (cooldown > 0f)
        {
            cooldownIndicator.fillAmount = cooldown / .5f;
            cooldown -= Time.deltaTime;
        }
        else
        {
            cooldown = 0f;
            cooldownIndicator.fillAmount = 0f;
        }

    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

        if (!projectile)
            projectile = GameObject.FindObjectOfType<ProjectileController>();

        if (!IsDead)
            Move();
    }

    #region Input

    private void OnMove(InputValue inputValue)
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

        moveVal = inputValue.Get<Vector2>();
    }

    private void OnDash(InputValue inputValue)
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

        StartCoroutine(Dash());
    }

    private void OnShoot(InputValue inputValue)
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

        Shoot();
        photonView.RPC("SyncShoot", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    private void OnMenu(InputValue inputValue)
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;
    }

    #endregion

    private void Move()
    {
        if (isDashing)
            return;
        if (moveVal.magnitude > 0.5f)
        {
            rbPlayer.drag = 1f;
            rbPlayer.AddForce(moveVal * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rbPlayer.drag = 5f;
        }


        Vector3 diff = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
    }


    IEnumerator Dash()
    {
        if (cooldown > 0f)
            yield break;

        isDashing = true;
        rbPlayer.drag = 1f;
        rbPlayer.AddForce(transform.up * dashSpeed, ForceMode2D.Impulse);
        cooldown = dashCooldown;
        yield return new WaitForSeconds(.3f);
        rbPlayer.drag = 5f;
        isDashing = false;
    }

    private void Shoot()
    {
        if (cooldown > 0f)
            return;

        projectileIndicator.color = hasNoProjectileColor;
        playerInput.SwitchCurrentActionMap("Gameplay no Weapon");
        Vector2 vec = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
        projectile.Shoot(vec, force);
    }

    [PunRPC]
    private void SyncShoot()
    {
        projectile.StartFlying();
    }
    [PunRPC]
    private void SyncPickup(int _viewID)
    {
        projectile.PickUp(_viewID);
    }
    [PunRPC]
    private void PCB_Death(int _viewID)
    {
        GameManager.Instance.PlayerDied(_viewID);
    }
    public void OnDie()
    {
        IsDead = true;
        deathVisualEffect.Play();
        body.enabled = false;
        projectileIndicator.enabled = false;
        rbPlayer.simulated = false;
        screenShake.StartShake(11, 0.7f, 80);
    }
    private void Call_Dead()
    {
        photonView.RPC("PCB_Death", RpcTarget.All, this.photonView.ViewID);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 10)
        {
            if (projectile.Owner != this)
            {
                if (isDashing)
                {
                    PickUpProjectile();

                }
                else if (projectile.IsFlying)
                {
                    projectile.StopFlying();
                    PhotonNetwork.SendAllOutgoingCommands();
                    Call_Dead();
                }
                else
                {
                    PickUpProjectile();
                }
            }
        }
    }

    private void PickUpProjectile()
    {
        cooldown = shootCooldown;
        projectile.PickUp(photonView.ViewID);
        photonView.RPC("SyncPickup", RpcTarget.All, photonView.ViewID);
        PhotonNetwork.SendAllOutgoingCommands();
        projectileIndicator.color = hasProjectileColor;
        playerInput.SwitchCurrentActionMap("Gameplay Weapon");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float relVelSqrMag = collision.relativeVelocity.sqrMagnitude;
        if (relVelSqrMag > 5f)
        {
            screenShake.StartShake(6, relVelSqrMag * 0.001f, 80);
        }
    }

    private void Initialize()
    {
        GameManager.Instance.RegisterPlayer(this.photonView.ViewID, this);
        ChangePlayerColor();

        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this.gameObject;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }

    private void ChangePlayerColor()
    {
        switch (photonView.ViewID)
        {
            case 1001:
                body.color = GameManager.Instance.PlayerColors[0];
                break;
            case 2001:
                body.color = GameManager.Instance.PlayerColors[1];
                break;
            case 3001:
                body.color = GameManager.Instance.PlayerColors[2];
                break;
            case 4001:
                body.color = GameManager.Instance.PlayerColors[3];
                break;
            default:
                break;
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
    #region Player Leaves Edgecases
    private void OnDestroy()
    {
        GameManager.Instance.UnRegisterPlayer(photonView.ViewID);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        GameManager.Instance.UnRegisterPlayer(photonView.ViewID);
        base.OnDisconnected(cause);
    }
    public override void OnLeftRoom()
    {
        GameManager.Instance.UnRegisterPlayer(photonView.ViewID);
        base.OnLeftRoom();
    }
    public override void OnLeftLobby()
    {
        GameManager.Instance.UnRegisterPlayer(photonView.ViewID);
        base.OnLeftLobby();
    }
    public override void OnDisable()
    {
        GameManager.Instance.UnRegisterPlayer(photonView.ViewID);
        base.OnDisable();
    }
    #endregion
}