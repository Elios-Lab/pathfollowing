using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMovement : MonoBehaviour
{
    public float speed;

    void Start() 
    {
        
    }
    
    //Moves this GameObject 2 units a second in the forward direction
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }     

    private void OnCollisionEnter(Collision other) 
    {  
        if (other.gameObject.CompareTag("barrier"))          
            speed *= -1;
    }
}
