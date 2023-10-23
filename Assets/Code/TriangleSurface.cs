using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleSurface : MonoBehaviour
{
    private Vector3[][] _triangles;
    private Vector3[] _points;
    private int _count;
    
    private void Start()
    {
        _points = TerrainTools.GetPoints();
    }

    private void CreateMeshTriangulation()
    {
        
    }
}
