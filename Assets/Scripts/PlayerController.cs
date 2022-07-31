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

public class PlayerController : MonoBehaviourPunCallbacks
{
    public static PlayerController LocalPlayerInstance;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float dashSpeed;

    [SerializeField] private SpriteRenderer body;
    [SerializeField] private SpriteRenderer projectileIndicator;
    [SerializeField] private Color hasProjectileColor;
    [SerializeField] private Color hasNoProjectileColor;
    [SerializeField] private Image cooldownIndicator;

    [SerializeField] private Rigidbody2D rbPlayer;


    [SerializeField] private AudioClip m_DashSFX;
    [SerializeField] private AudioClip m_ScreenShakeSFX;
    [SerializeField] private AudioClip m_DeathSFX;

    private Vector2 moveVal;

    [SerializeField] private float force;
    [SerializeField] private float cooldown;
    [SerializeField] private float dashCooldown;
    [SerializeField] private float shootCooldown;

    private bool isDashing;

    public bool IsDead;

    [SerializeField] private ProjectileController projectile;

    [SerializeField] public PlayerInput playerInput;

    [SerializeField] private VisualEffect deathVisualEffect;

    private Camera cam;
    private ScreenShake screenShake;

    public string NickName;
    public Color32 Color;
    public int Wins;

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
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }
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
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        if (!projectile)
            projectile = GameObject.FindObjectOfType<ProjectileController>();

        if (!IsDead)
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
        photonView.RPC("SyncShoot", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    private void OnMenu(InputValue inputValue)
    {
        if (!photonView.IsMine)
            return;

        GameManager.Instance.InGameUIManager.ShowPanel();
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
        AudioManager.Instance.PlaySFX(m_DashSFX);
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
        Shoot();
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
        AudioManager.Instance.PlaySFX(m_DeathSFX);
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
    public void OnRespawn()
    {
        IsDead = false;
        body.enabled = true;
        projectileIndicator.enabled = true;
        rbPlayer.simulated = true;
    }
    public void Reposition()
    {
        if (!photonView.IsMine) return;

        Vector2 rndPos = UnityEngine.Random.insideUnitCircle.normalized * 6;
        transform.position = rndPos;
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
            if (projectile.PlayerOwner != this)
            {
                if (isDashing)
                {                    
                    cooldown = shootCooldown;
                    projectile.PickupOwnership();
                    projectile.PickUp(photonView.ViewID);
                    photonView.RPC("SyncPickup", RpcTarget.All, photonView.ViewID);
                    PhotonNetwork.SendAllOutgoingCommands();
                    projectileIndicator.color = hasProjectileColor;
                    playerInput.SwitchCurrentActionMap("Gameplay Weapon");

                }
                else if (projectile.IsFlying)
                {
                    projectile.StopFlying();
                    projectile.PlayerOwner = null;
                    Call_Dead();
                }
                else
                {
                    cooldown = shootCooldown;
                    projectile.PickupOwnership();
                    projectile.PickUp(photonView.ViewID);
                    photonView.RPC("SyncPickup", RpcTarget.All, photonView.ViewID);
                    PhotonNetwork.SendAllOutgoingCommands();
                    projectileIndicator.color = hasProjectileColor;
                    playerInput.SwitchCurrentActionMap("Gameplay Weapon");
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float relVelSqrMag = collision.relativeVelocity.sqrMagnitude;
        if (relVelSqrMag > 5f)
        {
            screenShake.StartShake(6, relVelSqrMag * 0.001f, 80);
            AudioManager.Instance.PlaySFX(m_ScreenShakeSFX);
        }
    }

    private void Initialize()
    {
        GameManager.Instance.RegisterPlayer(this.photonView.ViewID, this);
        SetPlayerIdentities();

        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }

    private void SetPlayerIdentities()
    {
        switch (photonView.ViewID)
        {
            case 1001:
                Color = GameManager.Instance.PlayerColors[0];
                NickName = PhotonNetwork.PlayerList[0].NickName;
                body.color = Color;
                break;
            case 2001:
                Color = GameManager.Instance.PlayerColors[1];
                NickName = PhotonNetwork.PlayerList[1].NickName;
                body.color = Color;
                break;
            case 3001:
                Color = GameManager.Instance.PlayerColors[2];
                NickName = PhotonNetwork.PlayerList[2].NickName;
                body.color = Color;
                break;
            case 4001:
                Color = GameManager.Instance.PlayerColors[3];
                NickName = PhotonNetwork.PlayerList[3].NickName;
                body.color = Color;
                break;
            default:
                break;
        }
    }

    #region Player Leaves Edgecases
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnRegisterPlayer(photonView.ViewID);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnRegisterPlayer(photonView.ViewID);
        base.OnDisconnected(cause);
    }
    public override void OnLeftRoom()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnRegisterPlayer(photonView.ViewID);
        base.OnLeftRoom();
    }
    public override void OnLeftLobby()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnRegisterPlayer(photonView.ViewID);
        base.OnLeftLobby();
    }
    public override void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnRegisterPlayer(photonView.ViewID);
        base.OnDisable();
    }

    #endregion
}