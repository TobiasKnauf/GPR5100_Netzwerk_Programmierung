using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Abyss : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]private PolygonCollider2D polygonCollider2D;
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
        startSize = transform.localScale;
    }
    void FixedUpdate()
    {
        if (polygonCollider2D.bounds.Contains(GameManager.Instance.Projectile.transform.position) && !GameManager.Instance.Projectile.IsFlying)
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

        if (polygonCollider2D.bounds.Contains(GameManager.Instance.Projectile.transform.position))
        {
            GameManager.Instance.Projectile.rb.AddForce(GameManager.Instance.Projectile.rb.velocity);
        }
    }
}
