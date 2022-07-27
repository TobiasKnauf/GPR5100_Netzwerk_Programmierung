using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ProjectileController : MonoBehaviour
{
    public PlayerController Owner;
    public bool IsFlying;

    [SerializeField] private Vector3 poolingPosition;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private CircleCollider2D circleTrigger;
    [SerializeField] private float flyingThreshold = 10;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private VisualEffect collisionVisualEffect;
    [SerializeField] private AudioClip collisionSFX;

    private Vector3 preCollisionVelocity;

    private void FixedUpdate()
    {
        if (IsFlying)
        {
            preCollisionVelocity = rb.velocity;
            if (Owner == null)
                Fly();
        }

    }

    private void Fly()
    {
        if (rb.velocity.magnitude < flyingThreshold)
        {
            StopFlying();
        }
    }

    private void StartFlying()
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

    public void PickUp(PlayerController _player)
    {
        Owner = _player;
        StopFlying();
        transform.position = poolingPosition;
    }

    public void Shoot(PlayerController _player, Vector2 _direction, float _force)
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
