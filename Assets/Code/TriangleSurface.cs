using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleSurface : MonoBehaviour
{
    public List<Vector3[]> Triangles;
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private Vector3[] _points;
    private Vector3[] _vertices;
    private int _count;
    private const int SuperTriangleSize = 3;
    private const int TriangleSize = 3;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        Triangles = new List<Vector3[]>();
        _mesh = _meshFilter.mesh;
    }

    private void Start()
    {
        _points = TerrainTools.GetPoints();
        for (int i = 0; i < _points.Length; i++)
        {
            var pos = _points[i];
            var unityPos = new Vector3(pos.x - 260000f, pos.z, pos.y - 6664500f);
            _points[i] = unityPos;
        }
        CreateMesh();
    }

    private void CreateMesh()
    {
        Mesh newMesh = new Mesh();
        int vertices = 1000;
        int step = 4800000/vertices;
        // mesh vertex limit in Unity is 65536 - 16 bit index buffer
        _vertices = new Vector3[vertices];
        int j = 0;
        for (int i = 0; i < 1000; i++)
        {
            _vertices[i] = _points[j];
            j+=step;
        }
        
        newMesh.vertices = _vertices;
        // newMesh.triangles = Triangulation(_vertices.ToList());
        
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
        
        _mesh = newMesh;
        _meshFilter.mesh = newMesh;

        // for (int i = 0; i < _mesh.vertices.Length; i++)
        // {
        //     Debug.Log(_mesh.vertices[i].x + " " + _mesh.vertices[i].y + " " + _mesh.vertices[i].z);
        // }
        
    }
}
