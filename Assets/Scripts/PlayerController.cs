using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerController : MonoBehaviour, IOnEventCallback
{
    #region Components
    private Rigidbody2D rb;
    #endregion

    #region Movement
    private Vector2 moveVal;
    [SerializeField]
    private float moveSpeed;
    #endregion

    [SerializeField]
    private PhotonView view;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (!view.IsMine) return;

        view.Owner.NickName = PhotonNetwork.NickName;

        PhotonNetwork.AddCallbackTarget(this);
    }

    private void Update()
    {
        if (!view.IsMine) return;
    }

    private void FixedUpdate()
    {
        if (!view.IsMine) return;

        rb.AddForce(moveVal * moveSpeed * Time.fixedDeltaTime);
    }

    #region Input
    private void OnMove(InputValue inputValue)
    {
        moveVal = inputValue.Get<Vector2>();
    }

    private void OnDash(InputValue inputValue)
    {

    }

    private void OnShoot(InputValue inputValue)
    {

    }

    private void OnMenu(InputValue inputValue)
    {

    }

    public void OnEvent(EventData photonEvent)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
