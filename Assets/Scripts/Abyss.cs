using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Abyss : MonoBehaviourPunCallbacks, IPunObservable
{
    private ProjectileController projectile;
    private Rigidbody2D projectileRb;
    private PolygonCollider2D polygonCollider2D;
    private Vector3 startSize;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
        }
        else
        {
            // Network player, receive data
        }
    }

    void Start()
    {
        FindComponents();
        startSize = transform.localScale;
    }
    private void FindComponents()
    {
        if (!projectile)
            projectile = GameManager.Instance.Projectile;
        if (!projectileRb)
            projectileRb = projectile.rb;
        if (!polygonCollider2D)
            polygonCollider2D = GetComponent<PolygonCollider2D>();
    }

    void FixedUpdate()
    {
        FindComponents();

        if (polygonCollider2D.bounds.Contains(projectile.transform.position) && !projectile.IsFlying)
        {
            if (transform.localScale.x > 0.1f)
            {
                transform.localScale -= Vector3.one * Time.fixedDeltaTime;
            }
        }
        else if (transform.localScale.x < startSize.x)
        {
            transform.localScale += Vector3.one * Time.fixedDeltaTime;
        }

        //Alternative

        if (polygonCollider2D.bounds.Contains(projectile.transform.position))
        {
            projectileRb.AddForce(projectileRb.velocity);
        }
    }
}
