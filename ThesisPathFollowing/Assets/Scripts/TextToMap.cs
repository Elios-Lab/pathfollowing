using UnityEngine;
using System.Text.RegularExpressions;

public class TextToMap : MonoBehaviour
{
    public TextMapping[] mappingData;
    public TextAsset mapText;

    private Vector3 currentPosition = new Vector3(0.25f, 0.5f, 0.25f);

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateMap()
    {
        string[] rows = Regex.Split(mapText.text, "\r\n|\r|\n");

        foreach(string row in rows)
        {
            foreach(char c in row)
            {
                foreach(TextMapping tm in mappingData)
                {
                    if(c == tm.character)
                    {
                        Instantiate(tm.prefab, currentPosition, Quaternion.identity, transform);
                    }
                }
                currentPosition = new Vector3(currentPosition.x + 0.25f, 0.5f, currentPosition.z);
            }
            currentPosition = new Vector3(0.25f, 0.5f, currentPosition.z - 0.25f);
        }
    }
}
