using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public PlayerController Owner;

    [SerializeField] private Vector3 poolingPosition;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private CircleCollider2D circleTrigger;
    [SerializeField] float flyingThreshold = 10;

    public bool IsFlying;

    private void Update()
    {
        if (Owner == null && IsFlying)
            Fly();
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
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private void StopFlying()
    {
        IsFlying = false;
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
        Owner = null;
    }
}
