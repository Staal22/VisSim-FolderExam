using System;
using System.Linq;
using System.IO;
using UnityEngine;
using Unity.Serialization;



public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance;
    public Mesh instanceMesh;
    public Material instanceMaterial;
    private const string OutputFilePath = "Assets/StreamingAssets/terrain.txt";
    
    [DontSerialize] public bool renderPointCloud = false;

    private ComputeBuffer _matricesBuffer;
    private ComputeBuffer _argsBuffer;
    

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // var world = World.DefaultGameObjectInjectionWorld;
        // world.GetOrCreateSystem<MeshRenderingSystem>();
        
        InitializePointData();
        instanceMaterial.SetBuffer("position_buffer", _matricesBuffer);
    }

    private void Update()
    {
        
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(Vector3.zero, Vector3.one * 50000), _argsBuffer);
    }

    private void InitializePointData()
    {
        renderPointCloud = true;
        
        // var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var positions = LoadPositionsFromFile(OutputFilePath);
        
        // Create a matrix array to hold the position and rotation of each instance
        int numInstances = positions.Length;
        Matrix4x4[] matrices = new Matrix4x4[numInstances];
        
        // Create a compute buffer to hold the matrix data on the GPU
        _matricesBuffer = new ComputeBuffer(numInstances, sizeof(float) * 16, ComputeBufferType.Default);
        for (int i = 0; i < positions.Length; i+=50)
        {
            var pos = positions[i];
            var unityPos = new Vector3(pos.x - 260000f, pos.z, pos.y - 6660000f);
            matrices[i] = Matrix4x4.TRS(unityPos, Quaternion.identity, Vector3.one);
        }
        _matricesBuffer.SetData(matrices);
        
        // Create an args buffer which will hold the number of instances to draw
        uint[] args = { instanceMesh.GetIndexCount(0), (uint)numInstances, instanceMesh.GetIndexStart(0), instanceMesh.GetBaseVertex(0), 0 };
        _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        _argsBuffer.SetData(args);
        
        // //Buffer size
        // int bufferSize = 1023;
        //
        // // Calculate the number of batches we'll need to render all instances
        // int batchCount = Mathf.CeilToInt((float)positions.Length / bufferSize);
        
        // for each batch...
        // for (int batchIndex = 0; batchIndex < batchCount; ++batchIndex)
        // {
        //     // calculate the number of instances to render in this batch
        //     // (for the last batch this might be less than bufferSize!)
        //     // int instanceCount = Mathf.Min(bufferSize, positions.Length - batchIndex * bufferSize);
        //     // var mainEntity = entityManager.CreateEntity(typeof(LinkedEntityGroup));
        //     // var entityGroup = entityManager.AddBuffer<LinkedEntityGroup>(mainEntity);
        //
        //     // prepare instance data for this batch
        //     // for (int i = 0; i < instanceCount; i+=50)
        //     // {
        //     //     // var entity = entityManager.CreateEntity(typeof(LocalToWorld));
        //     //     
        //     //     // calculate the index into our positions array
        //     //     // int positionIndex = batchIndex * bufferSize + i;
        //     //
        //     //     // convert position to unity coords
        //     //     // var pos = positions[positionIndex];
        //     //     // var unityPos = new Vector3(pos.x - 260000, pos.z, pos.y - 6660000f);
        //     //
        //     //     // entityManager.SetComponentData(entity, new LocalToWorld { Value = float4x4.TRS(unityPos, quaternion.identity, new float3(1,1,1)) });
        //     //     // entityGroup.Add(entity);
        //     // }
        // }
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

    private void OnDisable()
    {
        renderPointCloud = false;
        _matricesBuffer.Dispose();
        _argsBuffer.Dispose();
    }
}