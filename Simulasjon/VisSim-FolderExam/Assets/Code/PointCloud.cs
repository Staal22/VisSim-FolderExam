using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PointCloud : MonoBehaviour
{
    [SerializeField] private Mesh instanceMesh;
    [SerializeField] private Material instanceMaterial;
    // [SerializeField] private GameObject testPrefab;
    
    private ComputeBuffer _matricesBuffer;
    private ComputeBuffer _positionBuffer;
    private ComputeBuffer _argsBuffer;
    private Matrix4x4[] _matrices;

    private readonly uint[] _args = { 0, 0, 0, 0, 0 };
    private Vector3[] _points;
    private int _count;

    private void Start()
    {
        _points = TerrainTools.GetPoints();
        _count = _points.Length;
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
        print("Rendering " + _count + " points as cubes");
        
        // Instantiate test prefabs at corners xMin,zMax and xMax,zMin
        // var xMin = _points.Min(p => p.x)  - TerrainTools.XOffset;
        // var xMax = _points.Max(p => p.x)  - TerrainTools.XOffset;
        // var zMin = _points.Min(p => p.y)  - TerrainTools.YOffset;
        // var zMax = _points.Max(p => p.y)  - TerrainTools.YOffset;
        // var y1 = _points.Min(p => p.z);
        // var y2 = _points.Max(p => p.z);
        // Instantiate(testPrefab, new Vector3(xMin, y1, zMax), Quaternion.identity);
        // Instantiate(testPrefab, new Vector3(xMax, y2, zMin), Quaternion.identity);
    }

    private void Update()
    {
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(new Vector3(900, 580, 5120), Vector3.one * 100000), _argsBuffer);
    }

    private void UpdateBuffers()
    {
        // Release old resources and calculate new size
        _matricesBuffer?.Release();
        _positionBuffer?.Release();
        _positionBuffer = new ComputeBuffer(_count, 16);
        
        // create an array to hold Vector4 data
        var vectors = new Vector4[_points.Length];

        Parallel.For(0, vectors.Length, i => {
            var pos = _points[i];
            var unityPos = new Vector3(pos.x - TerrainTools.XOffset, pos.z, pos.y - TerrainTools.YOffset);
            vectors[i] = Matrix4x4.TRS(unityPos, Quaternion.identity, Vector3.one).GetColumn(3);
        });

        _positionBuffer.SetData(vectors);
        instanceMaterial.SetBuffer("position_buffer", _positionBuffer);

        // vertices
        uint[] args = { instanceMesh.GetIndexCount(0), (uint)_points.Length, instanceMesh.GetIndexStart(0), instanceMesh.GetBaseVertex(0), 0 };
        _argsBuffer.SetData(args);
    }
    
    private void OnDisable()
    {
        _matricesBuffer?.Release();
        _matricesBuffer = null;
        
        _positionBuffer?.Release();
        _positionBuffer = null;

        _argsBuffer?.Release();
        _argsBuffer = null;
    }
}
