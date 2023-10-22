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
    private ComputeBuffer _positionBuffer;
    private ComputeBuffer _argsBuffer;
    private Matrix4x4[] _matrices;

    private readonly uint[] _args = { 0, 0, 0, 0, 0 };
    private Vector3[] _points;
    private int _count;
    
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
        _positionBuffer?.Release();
        _positionBuffer = new ComputeBuffer(_count, 16);
        
        // Create a matrix array to hold the position and rotation of each instance
        int numInstances = _points.Length;
        _matrices = new Matrix4x4[numInstances];
        
        // Create a compute buffer to hold the matrix data on the GPU
        _matricesBuffer = new ComputeBuffer(numInstances, sizeof(float) * 16, ComputeBufferType.Default);
        for (int i = 0; i < _points.Length; i++)
        {
            var pos = _points[i];
            var unityPos = new Vector3(pos.x - 260000f, pos.z, pos.y - 6660000f);
            _matrices[i] = Matrix4x4.TRS(unityPos, Quaternion.identity, Vector3.one);
        }
        _matricesBuffer.SetData(_matrices);
        
        var vectors = new Vector4[_matrices.Length];
        for (int i = 0; i < _matrices.Length; i++)
        {
            vectors[i] = _matrices[i].GetColumn(3);
        }
        _positionBuffer.SetData(vectors);
        instanceMaterial.SetBuffer("position_buffer", _positionBuffer);
        
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
        
        _positionBuffer?.Release();
        _positionBuffer = null;

        _argsBuffer?.Release();
        _argsBuffer = null;
    }
}