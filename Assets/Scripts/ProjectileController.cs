using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;

    [SerializeField] float flyingThreshold = 10;

    [SerializeField] float flyingDrag = 0.2f;

    [SerializeField] float groundDrag = 6f;


    void Update()
    {
        if (rb.velocity.magnitude < flyingThreshold)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = flyingDrag;
        }
    }
}
