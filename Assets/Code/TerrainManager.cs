using System;
using System.Linq;
using System.IO;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance;
    public Mesh instanceMesh;
    public Material instanceMaterial;
    private const string OutputFilePath = "Assets/StreamingAssets/terrain.txt";
    
    private ComputeBuffer _matricesBuffer;
    private ComputeBuffer _positionBuffer1, _positionBuffer2;
    private ComputeBuffer _argsBuffer;
    private Matrix4x4[] matrices;

    private readonly uint[] _args = { 0, 0, 0, 0, 0 };
    private Vector3[] _points;
    private int _count;
    
    [field: SerializeField] public Color[] ColorArray { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializePointData();
        _count = _points.Length;
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    private void Update()
    {
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(new Vector3(900, 580, 5120), Vector3.one * 1000), _argsBuffer);
    }

    private void InitializePointData()
    {
        _points = LoadPositionsFromFile(OutputFilePath);
    }

    private void UpdateBuffers()
    {
        // positions
        _positionBuffer1?.Release();
        _positionBuffer2?.Release();
        _positionBuffer1 = new ComputeBuffer(_count, 16);
        _positionBuffer2 = new ComputeBuffer(_count, 16);
        
        // Create a matrix array to hold the position and rotation of each instance
        int numInstances = _points.Length;
        matrices = new Matrix4x4[numInstances];
        
        // Create a compute buffer to hold the matrix data on the GPU
        _matricesBuffer = new ComputeBuffer(numInstances, sizeof(float) * 16, ComputeBufferType.Default);
        for (int i = 0; i < _points.Length; i++)
        {
            var pos = _points[i];
            var unityPos = new Vector3(pos.x - 260000f, pos.z, pos.y - 6660000f);
            matrices[i] = Matrix4x4.TRS(unityPos, Quaternion.identity, Vector3.one);
        }
        _matricesBuffer.SetData(matrices);
        
        var vectors = new Vector4[matrices.Length];
        for (int i = 0; i < matrices.Length; i++)
        {
            vectors[i] = matrices[i].GetColumn(3);
        }
        _positionBuffer1.SetData(vectors);
        _positionBuffer2.SetData(vectors);
        instanceMaterial.SetBuffer("position_buffer_1", _positionBuffer1);
        instanceMaterial.SetBuffer("position_buffer_2", _positionBuffer2);
        instanceMaterial.SetColorArray("color_buffer", ColorArray);
        
        // vertices
        uint[] args = { instanceMesh.GetIndexCount(0), (uint)numInstances, instanceMesh.GetIndexStart(0), instanceMesh.GetBaseVertex(0), 0 };
        _argsBuffer.SetData(args);
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
        _matricesBuffer?.Dispose();
        _matricesBuffer = null;
        
        _positionBuffer1?.Release();
        _positionBuffer1 = null;

        _positionBuffer2?.Release();
        _positionBuffer2 = null;

        _argsBuffer?.Release();
        _argsBuffer = null;
    }
}