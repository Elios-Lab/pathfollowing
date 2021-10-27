using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializerScript : MonoBehaviour
{
    [SerializeField] int numberInstances;
    [SerializeField] int instancesPerRow;
    [SerializeField] GameObject instancePrefab;
    [SerializeField] float timeScale;

    // Start is called before the first frame update
    void Start()
    {
        if (timeScale != 0)
            Time.timeScale = timeScale;

        for (int i = 0; i < numberInstances; i++)
        {
            float x, z;
            x = (i % instancesPerRow) * 100 + 1;
            z = (int)(i / instancesPerRow) * 100 + 1;
            Instantiate(instancePrefab, new Vector3(x, 0, z), Quaternion.identity);
        }
    }
}
