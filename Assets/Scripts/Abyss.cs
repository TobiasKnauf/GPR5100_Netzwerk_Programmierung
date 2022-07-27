using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abyss : MonoBehaviour
{
    private ProjectileController projectile;
    private Rigidbody2D projectileRb;
    private PolygonCollider2D polygonCollider2D;
    private Vector3 startSize;

    void Start()
    {
        projectile = GameObject.Find("Projectile").GetComponent<ProjectileController>();
        projectileRb = projectile.GetComponent<Rigidbody2D>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        startSize = transform.localScale;
    }

    void FixedUpdate()
    {
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
