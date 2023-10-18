using System;
using System.Linq;
using System.IO;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance;
    // [SerializeField] private GameObject pointPrefab;
    [SerializeField] private Mesh instanceMesh;
    [SerializeField] private Material instanceMaterial;
    private const int BufferSize = 1023; // must not exceed 1023
    private Matrix4x4[][] _batches;
    private const string OutputFilePath = "Assets/StreamingAssets/terrain.txt";
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializePointData();
    }
    
    private void Update()
    {
        // draw all batches
        for (int batchIndex = 0; batchIndex < _batches.Length; ++batchIndex)
        {
            Graphics.DrawMeshInstanced(instanceMesh, 0, instanceMaterial, _batches[batchIndex], _batches[batchIndex].Length);
        }
    }
    
    private void InitializePointData()
    {
        var positions = LoadPositionsFromFile(OutputFilePath);

        //Buffer size
        int bufferSize = 1023;
    
        // Calculate the number of batches we'll need to render all instances
        int batchCount = Mathf.CeilToInt((float)positions.Length / bufferSize);
    
        // allocate array for all batches
        _batches = new Matrix4x4[batchCount][];

        // for each batch...
        for (int batchIndex = 0; batchIndex < batchCount; ++batchIndex)
        {
            // calculate the number of instances to render in this batch
            // (for the last batch this might be less than bufferSize!)
            int instanceCount = Mathf.Min(bufferSize, positions.Length - batchIndex * bufferSize);
            _batches[batchIndex] = new Matrix4x4[instanceCount];

            // prepare instance data for this batch
            for (int i = 0; i < instanceCount; ++i)
            {
                // calculate the index into our positions array
                int positionIndex = batchIndex * bufferSize + i;

                // convert position to unity coords
                var pos = positions[positionIndex];
                var unityPos = new Vector3(pos.x - 260000, pos.z, pos.y - 6660000f);

                // create transformation matrix
                _batches[batchIndex][i] = Matrix4x4.TRS(unityPos, Quaternion.identity, Vector3.one);
            }
        }
    }
    
    private Vector3[] LoadPositionsFromFile(string path)
    {
        string[] lines = File.ReadAllLines(path).Skip(1).ToArray();
        return lines.Select(line =>
        {
            var values = line.Split(' ');
            return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        }).ToArray();
    }
}