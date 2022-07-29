using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Photon.Pun;
using Photon.Realtime;

public class ProjectileController : MonoBehaviourPunCallbacks
{
    public PlayerController Owner;
    public bool IsFlying;

    [SerializeField] private Vector3 poolingPosition;
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private CircleCollider2D circleTrigger;
    [SerializeField] private float flyingThreshold = 10;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private VisualEffect collisionVisualEffect;
    [SerializeField] private AudioClip collisionSFX;
    private Vector3 preCollisionVelocity;

    private void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

        if (IsFlying)
        {
            preCollisionVelocity = rb.velocity;
            if (Owner == null)
                if (rb.velocity.magnitude < flyingThreshold)
                {
                    StopFlying();
                }
        }
    }


    public void StartFlying()
    {
        IsFlying = true;
        transform.position = Owner.transform.position;
        trailRenderer.Clear();
        trailRenderer.emitting = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public void StopFlying()
    {
        IsFlying = false;
        trailRenderer.emitting = false;
        trailRenderer.Clear();
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void PickUp(int _player)
    {
        StopFlying();
        Owner = GameManager.Instance.Players[_player];
        transform.position = poolingPosition;
    }

    public void Shoot(Vector2 _direction, float _force)
    {
        StartFlying();
        rb.AddForce(_direction.normalized * _force, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collisionVisualEffect.SetVector3("Velocity", preCollisionVelocity);
        collisionVisualEffect.Play();
        AudioManager.Instance.PlaySFX(collisionSFX);
        Owner = null;
    }
}
