using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Components
    private Rigidbody2D rb;
    #endregion

    #region Movement
    private Vector2 moveVal;
    [SerializeField]
    private float moveSpeed;
    #endregion

    private void Awake()
    {

    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
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
    #endregion
}
