using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Photon.Pun;
using Photon.Realtime;

public class ProjectileController : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    public PlayerController PlayerOwner;
    public bool IsFlying;

    [SerializeField] private Vector3 poolingPosition;
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private CircleCollider2D circleTrigger;
    [SerializeField] private float flyingThreshold = 10;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private VisualEffect collisionVisualEffect;
    [SerializeField] private AudioClip collisionSFX;
    [SerializeField] private AudioClip m_PickupSFX;
    private Vector3 preCollisionVelocity;

    private void Update()
    {
        if (IsFlying)
        {
            preCollisionVelocity = rb.velocity;
            if (PlayerOwner == null)
                if (rb.velocity.magnitude < flyingThreshold)
                {
                    StopFlying();
                }
        }
    }

    public void StopFlying()
    {
        IsFlying = false;
        trailRenderer.emitting = false;
        trailRenderer.Clear();
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void PickUp(int _viewID)
    {
        PlayerOwner = GameManager.Instance.Players[_viewID];
        transform.position = poolingPosition; 
        StopFlying();
        AudioManager.Instance.PlaySFX(m_PickupSFX);
        //Debug.Log($"Owner is {PlayerOwner}");
    }
    public void PickupOwnership()
    {
        this.photonView.RequestOwnership();
    }

    public void Shoot(Vector2 _direction, float _force)
    {
        //Debug.Log($"Owner is {PlayerOwner}");
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(_direction.normalized * _force, ForceMode2D.Impulse);
        transform.position = PlayerOwner.transform.position;
        IsFlying = true;
        trailRenderer.Clear();
        trailRenderer.emitting = true;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerOwner = null;
        collisionVisualEffect.SetVector3("Velocity", preCollisionVelocity);
        collisionVisualEffect.Play();
        AudioManager.Instance.PlaySFX(collisionSFX);
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        Debug.Log($"{requestingPlayer} requests an ownership transfer");

        targetView.TransferOwnership(requestingPlayer);
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        Debug.Log($"Ownership transfered from {previousOwner} to {this.photonView.Owner}");
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        Debug.Log($"{senderOfFailedRequest} failed to send request");
    }
}
