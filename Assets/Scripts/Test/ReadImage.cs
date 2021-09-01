using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83.Image.BMP;

public class ReadImage : MonoBehaviour
{    
    [SerializeField]
    public GameObject obstacle;
    public GameObject ground;

    string imagePath = @"C:\Users\andre\Desktop\unity-autonomous-parking-ml-agents-master\Assets\Map\freespace_last.bmp";

    // Start is called before the first frame update
    void Start()
    {
        Texture2D img = LoadTexture(imagePath);

        Color[] pix = img.GetPixels();

        int imgWidth = img.width;
        int imgHeight = img.height;

        Vector3[] spawnPositions = new Vector3[pix.Length];
        Vector3 startingSpawnPosition = new Vector3(-Mathf.Round(imgWidth / 2), 0, -Mathf.Round(imgHeight / 2));
        Vector3 currentSpawnPos = startingSpawnPosition;

        int counter = 0;

      
        for (int z = 0; z < imgHeight; z++)
        {
            for (int x = 0; x < imgWidth; x++)
            {
                spawnPositions[counter] = currentSpawnPos;
                counter++;
                currentSpawnPos.x++;
            }
            currentSpawnPos.x = startingSpawnPosition.x;
            currentSpawnPos.z++;
        }

        counter = 0;

        foreach (Vector3 pos in spawnPositions)
        {
            Color c = pix[counter];

            if (c.Equals(Color.black))
            {
                Instantiate(obstacle, pos, Quaternion.identity);
            }
            else if(c.Equals(Color.white) || c.Equals(Color.gray))
            {
                Instantiate(ground, pos, Quaternion.identity);
            }
        }
    }

    public static Texture2D LoadTexture(string filePath)
    {
        Texture2D image = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);

            BMPLoader bmpLoader = new BMPLoader();
            bmpLoader.ForceAlphaReadWhenPossible = true; //Uncomment to read alpha too

            //Load the BMP data
            BMPImage bmpImg = bmpLoader.LoadBMP(fileData);

            //Convert the Color32 array into a Texture2D
            image = bmpImg.ToTexture2D();
        }
        return image;
    }
}
