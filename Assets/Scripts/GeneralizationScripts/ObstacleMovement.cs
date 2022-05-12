using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMovement : MonoBehaviour
{
    private float speed;
    private float rotation;

    void Start() 
    {
        speed = Random.Range(0.1f,2);
        rotation = Random.Range(0,360);
        transform.rotation = Quaternion.Euler(0, rotation, 0);
    }
    
    //Moves this GameObject 2 units a second in the forward direction
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    //Upon collision with another GameObject, this GameObject will reverse direction
    // private void OnTriggerEnter(Collider other) 
    // {
    //     speed *= -1;
    //     Debug.Log("Trigger!!!");
    // }       

    private void OnCollisionEnter(Collision other) 
    {  
        if (other.gameObject.CompareTag("barrier"))  
        {
            speed *= -1;
            Debug.Log("Collisione Obstacle-Barrier");
        } 
    }
}
