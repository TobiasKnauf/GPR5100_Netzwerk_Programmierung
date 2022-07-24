using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public PlayerController owner;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private CircleCollider2D circleTrigger;
    [SerializeField] float flyingThreshold = 10;

    public bool isFlying;

    private void Update()
    {
        if (owner == null && isFlying)
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
        isFlying = true;
        circleCollider.enabled = true;
        rb.isKinematic = false;
    }

    private void StopFlying()
    {
        isFlying = false;
        circleCollider.enabled = false;
        rb.isKinematic = true;
    }

    public void PickUp(PlayerController _player)
    {
        owner = _player;
        rb.transform.SetParent(_player.transform, false);
        rb.transform.position = _player.transform.position;
    }

    public void Drop()
    {
        rb.transform.SetParent(null);
    }

    public void Shoot(PlayerController _player, Vector2 _direction, float _force)
    {
        StartFlying();
        rb.transform.SetParent(null);
        rb.AddForce(_direction.normalized * _force, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        owner = null;
    }
}
