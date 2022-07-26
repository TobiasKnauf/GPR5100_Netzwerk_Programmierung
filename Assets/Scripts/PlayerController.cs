using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public static PlayerController LocalPlayerInstance;

    [SerializeField] private float moveSpeed;

    [SerializeField] private Rigidbody2D rb;

    private Vector2 moveVal;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        //SetCamera();
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
    private void OnTestDeath(InputValue inputValue)
    {
        Debug.LogWarning("TEST DEATH CALLED");
        if (!photonView.IsMine) return;

        GameManager.Instance.PlayerDied(photonView.ViewID);
    }
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
    }

    private void OnShoot(InputValue inputValue)
    {
        if (!photonView.IsMine)
            return;
    }

    private void OnMenu(InputValue inputValue)
    {
        if (!photonView.IsMine)
            return;
    }

    private void Move()
    {
        rb.AddForce(moveVal * moveSpeed * Time.fixedDeltaTime);
    }

    private void Initialize()
    {
        GameManager.Instance.RegisterPlayer(photonView.ViewID);
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

    public void Died()
    {
        /*
         * Die stuff...
         */
    }
}