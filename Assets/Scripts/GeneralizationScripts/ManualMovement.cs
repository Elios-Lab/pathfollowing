using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualMovement : MonoBehaviour
{
    //private float speed = 2f;
    private const float SPEED = 2f;

    //Moves this GameObject 2 units a second in the forward direction
    void Update()
    {
        //transform.Translate(Vector3.forward * Time.deltaTime * speed);

        HandleMovement();
    }

    private void HandleMovement()
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveY = +1f;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveY = -1f;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1f;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveX = +1f;
        }

        Vector3 moveDir = new Vector3(moveX, moveY).normalized;
        transform.position += moveDir * SPEED * Time.deltaTime;
    }


    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerEnter(Collider other) 
    {
        Debug.Log("Trigger!!!");
    }
}
