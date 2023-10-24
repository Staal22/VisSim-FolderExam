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
        int step = _points.Length/vertices;
        _vertices = new Vector3[vertices];
        int index = 0;
        for (int i = 0; i < vertices; i++)
        {
            _vertices[i] = _points[index];
            index+=step;
        }
        
        // we have vertices, now we get triangles(indices)
        // create a regular triangulation (vector grid with a certain resolution) - xz grid due to y being up in Unity
        // find xMax and zMax in vertices, also xMin and zMin
        float xMax = float.MinValue;
        float zMax = float.MinValue;
        float xMin = float.MaxValue;
        float zMin = float.MaxValue;
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (_vertices[i].x > xMax)
            {
                xMax = _vertices[i].x;
            }
            if (_vertices[i].z > zMax)
            {
                zMax = _vertices[i].z;
            }
            if (_vertices[i].x < xMin)
            {
                xMin = _vertices[i].x;
            }
            if (_vertices[i].z < zMin)
            {
                zMin = _vertices[i].z;
            }
        }
        
        // top left corner, we construct the grid from there
        var topLeft = new Vector3(xMin, 0, zMax);
        
        // create a grid of points
        var grid = new List<Vector3>();
        var gridWidth = 100;
        var gridHeight = 100;
        var gridStepX = (xMax - xMin) / gridWidth;
        var gridStepZ = (zMax - zMin) / gridHeight;
        for (int i = 0; i < gridWidth; i++)
        {
            for (int k = 0; k < gridHeight; k++)
            {
                // find points within current square and take average of their y values
                var pointsInSquare = new List<Vector3>();
                var yAvg = 0f;
                for (int j = 0; j < _vertices.Length; j++)
                {
                    if (_vertices[j].x >= topLeft.x + i * gridStepX && _vertices[j].x < topLeft.x + (i + 1) * gridStepX &&
                        _vertices[j].z <= topLeft.z - k * gridStepZ && _vertices[j].z > topLeft.z - (k + 1) * gridStepZ)
                    {
                        pointsInSquare.Add(_vertices[j]);
                    }
                }
                foreach (var pointSq in pointsInSquare)
                {
                    yAvg += pointSq.y;
                }

                if (pointsInSquare.Count != 0)
                {
                    yAvg /= pointsInSquare.Count;
                }
                else
                {
                    // get average from neighbouring squares
                    var pointsInNeighbouringSquares = new List<Vector3>();
                    var yAvgNeighbouring = 0f;
                    for (int j = 0; j < _vertices.Length; j++)
                    {
                        if (_vertices[j].x >= topLeft.x + (i - 1) * gridStepX && _vertices[j].x < topLeft.x + (i + 2) * gridStepX &&
                            _vertices[j].z <= topLeft.z - (k - 1) * gridStepZ && _vertices[j].z > topLeft.z - (k + 2) * gridStepZ)
                        {
                            pointsInNeighbouringSquares.Add(_vertices[j]);
                        }
                    }
                    foreach (var pointSq in pointsInNeighbouringSquares)
                    {
                        yAvgNeighbouring += pointSq.y;
                    }
                    yAvgNeighbouring /= pointsInNeighbouringSquares.Count;
                    if (pointsInNeighbouringSquares.Count != 0)
                    {
                        yAvg = yAvgNeighbouring;
                    }
                }
                var point = new Vector3(topLeft.x + i * gridStepX, yAvg, topLeft.z - k * gridStepZ);
                grid.Add(point);
            }
        }
        _vertices = grid.ToArray();
        
        // create triangles
        var triangles = new List<int>();
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (i % gridWidth == gridWidth - 1 || i / gridWidth == gridHeight - 1)
            {
                continue;
            }
            triangles.Add(i);
            triangles.Add(i + gridWidth);
            triangles.Add(i + 1);
            
            triangles.Add(i + 1);
            triangles.Add(i + gridWidth);
            triangles.Add(i + gridWidth + 1);
        }
        
        newMesh.vertices = _vertices;
        newMesh.triangles = triangles.ToArray();
        
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
        
        _mesh = newMesh;
        _meshFilter.mesh = newMesh;
    }
}
