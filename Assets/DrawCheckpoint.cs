using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCheckpoint : MonoBehaviour
{
    public GameObject[] checkpoints;

    void FixedUpdate ()
    {
        checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        if(checkpoints.Length > 1) {

            for(int i = 0; i < checkpoints.Length-1; i++) {
                //Vector3 frontAxlePosition = 
                DrawLine(checkpoints[i].transform.position, checkpoints[i+1].transform.position, 2f);
                //Debug.DrawLine(checkpoints[i].transform.position, checkpoints[i+1].transform.position, Color.green, 0.05f, false);
            }
        }
    }

    public void ResetCheckpoints()
    {
        foreach(GameObject checkpoint in checkpoints)
        GameObject.Destroy(checkpoint);
    }

    public static void DrawLine(Vector3 p1, Vector3 p2, float width)
    {
        int count = 1 + Mathf.CeilToInt(width); // how many lines are needed.
        if (count == 1)
        {
            Debug.DrawLine(p1, p2, Color.green, 0.05f, false);
        }
        else
        {
            Camera c = Camera.main;
            if (c == null)
            {
                Debug.LogError("Camera.main is null");
                return;
            }
            var scp1 = c.WorldToScreenPoint(p1);
            var scp2 = c.WorldToScreenPoint(p2);
    
            Vector3 v1 = (scp2 - scp1).normalized; // line direction
            Vector3 n = Vector3.Cross(v1, Vector3.forward); // normal vector
    
            for (int i = 0; i < count; i++)
            {
                Vector3 o = 0.99f * n * width * ((float)i / (count - 1) - 0.5f);
                Vector3 origin = c.ScreenToWorldPoint(scp1 + o);
                Vector3 destiny = c.ScreenToWorldPoint(scp2 + o);
                Debug.DrawLine(origin, destiny, Color.green, 0.05f, false);
            }
        }
    }
    
}
