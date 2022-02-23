using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeCheckpoints : MonoBehaviour
{
    GameObject objToSpawn;

    void FixedUpdate ()
    {
        objToSpawn = new GameObject("Checkpoint");
        objToSpawn.tag = "Checkpoint";
        objToSpawn.transform.position = this.transform.GetChild(2).position;
    }

    public void ResetObjectToSpawn () 
    {
        GameObject.Destroy(objToSpawn);
    }
}
