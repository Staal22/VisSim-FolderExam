using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.Serialization;

public class MeshToPointCloud : MonoBehaviour
{
    [Header("Static Setups")]
    [SerializeField] protected  ComputeShader computeShader;
    [SerializeField] private Material material;
    [SerializeField] private Mesh pointMesh;
    
    private const string XYZFilePath = "Assets/StreamingAssets/terrain.txt";
    
    private ComputeBuffer _positionsBuffer;

    private Bounds _bounds;
    private Vector3[] _vertices;
    
    private static readonly int Positions = Shader.PropertyToID("_Positions");
    private static readonly int UseNormals = Shader.PropertyToID("_UseNormals");

    private void Awake()
    {
        InitializeFromMeshData();
        SetBound();
    }

    protected void Update()
    {
        DrawInstanceMeshes();
    }
    
    private void InitializeFromMeshData()
    {
        GetPositionsDataFromMesh();
        SetStaticMaterialData();
    }

    private void GetPositionsDataFromMesh()
    {
        _vertices = LoadPositionsFromFile(XYZFilePath);

        var positions = new Vector3[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            positions[i] = new Vector3(_vertices[i].x - 260000, _vertices[i].z, _vertices[i].y - 6660000f);
        }
        
        _positionsBuffer = new ComputeBuffer(positions.Length, sizeof(float) * 3);
        _positionsBuffer.SetData(positions);
        Debug.Log("vertices Count :" + positions.Length);
        computeShader.SetBuffer(0, Positions, _positionsBuffer);
    }

    private void SetStaticMaterialData()
    {
        material.SetBuffer(Positions, _positionsBuffer);
        material.SetFloat(UseNormals, 0);
    }

    private void DrawInstanceMeshes()
    {
        Graphics.DrawMeshInstancedProcedural(pointMesh, 0, material, _bounds, _positionsBuffer.count);
    }

    private void SetBound()
    {
        _bounds = new Bounds(Vector3.zero, Vector3.one * 200000);
    }
    
    protected virtual void OnDisable()
    {
        _positionsBuffer.Release();
        _positionsBuffer = null;
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
