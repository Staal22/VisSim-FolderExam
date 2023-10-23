using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;

public class TriangleSurface : MonoBehaviour
{
    public List<Vector3[]> Triangles;
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private Vector3[] _points;
    private Vector3[] _vertices;
    private int _count;

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
        // mesh vertex limit in Unity is 65536 - 16 bit index buffer
        int vertices = 65535;
        // we need to get to the final index of points[], while only stepping through vertices[] one at a time
        int step = 4800000/vertices;
        _vertices = new Vector3[vertices];
        int j = 0;
        for (int i = 0; i < vertices; i++)
        {
            _vertices[i] = _points[j];
            j+=step;
        }
        
        // we have vertices, now we get triangles(indices)
        var points = new IPoint[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            points[i] = new Point(_vertices[i].x, _vertices[i].z);
        }
        Delaunator delaunator = new Delaunator(points);
        int[] triangles = delaunator.Triangles;
        
        newMesh.vertices = _vertices;
        newMesh.triangles = triangles;
        
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
        
        _mesh = newMesh;
        _meshFilter.mesh = newMesh;
    }
}
