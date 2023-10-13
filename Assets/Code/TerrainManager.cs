using System;
using System.IO;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance;
    [SerializeField] private GameObject pointPrefab;
    private const string OutputFilePath = "Assets/StreamingAssets/terrain.txt";
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        DrawPointCloud();
    }
    
    private void DrawPointCloud()
    {
        try
        {
            string[] lines = File.ReadAllLines(OutputFilePath);
            
            if (lines.Length == 0)
            {
                Debug.LogError("Terrain file is empty!");
                return;
            }

            // Assuming the first line in the file contains the count of points, skipping it
            for (int i = 1; i < lines.Length; i += 50) // draw 100k points instead of 5M
            {
                var line = lines[i];
                var values = line.Split(' ');
                if (values.Length < 3)
                {
                    Debug.LogError("Invalid line in terrain file: " + line);
                    continue;
                }
                var xPos = float.Parse(values[0]);
                var yPos = float.Parse(values[1]);
                var zPos = float.Parse(values[2]);
                
                var point = Instantiate(pointPrefab);
                point.transform.position = new Vector3(xPos - 260000, zPos, yPos - 6660000f);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to draw point cloud: " + e);
        }
    }
}