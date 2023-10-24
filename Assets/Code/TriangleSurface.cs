using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriangleSurface : MonoBehaviour
{
    private MeshFilter _meshFilter;

    private const int VertexLimit = 65535;
    private const int GridWidth = 100;
    private const int GridHeight = 100;
    private Vector3[] _points;
    private Vector3[] _vertices;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
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
        SetUpVertices(); // Constructs _vertices[]

        // Find bounds of vertices.
        var (xMin, xMax, zMin, zMax) = FindVerticesBounds();

        // Top left corner, we construct the grid from there
        var topLeft = new Vector3(xMin, 0, zMax);

        var grid = GenerateGrid(xMin, xMax, zMin, zMax, topLeft); // Generates a grid of Vector3 points

        _vertices = grid.ToArray();

        var triangles = GenerateTriangles(); // Generates the triangulation
        var indices = triangles.SelectMany(t => t.Indices).ToArray();
        
        GenerateNewMesh(indices); // Creates a new mesh and assigns it to _mesh and _meshFilter.mesh
    }

    private void SetUpVertices()
    {
        var vertices = VertexLimit;
        var step = _points.Length / vertices;
        _vertices = new Vector3[vertices];

        int index = 0;
        for (int i = 0; i < vertices; i++)
        {
            _vertices[i] = _points[index];
            index += step;
        }
    }

    private (float, float, float, float) FindVerticesBounds()
    {
        float xMax = float.MinValue, zMax = float.MinValue, xMin = float.MaxValue, zMin = float.MaxValue;

        foreach (Vector3 vertex in _vertices)
        {
            if (vertex.x > xMax) xMax = vertex.x;
            if (vertex.z > zMax) zMax = vertex.z;
            if (vertex.x < xMin) xMin = vertex.x;
            if (vertex.z < zMin) zMin = vertex.z;
        }

        return (xMin, xMax, zMin, zMax);
    }

    private List<Vector3> GenerateGrid(float xMin, float xMax, float zMin, float zMax, Vector3 topLeft)
    {
        var grid = new List<Vector3>();
        var gridStepX = (xMax - xMin) / GridWidth;
        var gridStepZ = (zMax - zMin) / GridHeight;
        for (int i = 0; i < GridWidth; i++)
        {
            for (int k = 0; k < GridHeight; k++)
            {
                var yAvg = GetAverageYValue(i, k, topLeft, gridStepX, gridStepZ);
                var point = new Vector3(topLeft.x + i * gridStepX, yAvg, topLeft.z - k * gridStepZ);
                grid.Add(point);
            }
        }

        return grid;
    }
    
    // private List<int> GenerateTriangles()
    private List<Triangle> GenerateTriangles()
    {
        // var triangles = new List<int>();
        var triangles = new List<Triangle>();
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (i % GridWidth == GridWidth - 1 || i / GridWidth == GridHeight - 1) continue;
            
            triangles.Add(new Triangle(new Vector3[3] {_vertices[i], _vertices[i + GridWidth], _vertices[i + 1]}, new int[3] {i, i + GridWidth, i + 1}, i));
            triangles.Add(new Triangle(new Vector3[3] {_vertices[i + 1], _vertices[i + GridWidth], _vertices[i + GridWidth + 1]}, new int[3] {i + 1, i + GridWidth, i + GridWidth + 1}, i + 1));
        }

        
        // map each vertex to the triangles it is part of
        var vertexToTriangles = new Dictionary<int, List<Triangle>>();
        foreach (var triangle in triangles)
        {
            foreach (var idx in triangle.Indices)
            {
                if (!vertexToTriangles.ContainsKey(idx))
                {
                    vertexToTriangles[idx] = new List<Triangle>();
                }
                vertexToTriangles[idx].Add(triangle);
            }
        }
        // find neighbours
        foreach (var triangle in triangles)
        {
            for (int i = 0; i < 3; i++)
            {
                var idx1 = triangle.Indices[i];
                var idx2 = triangle.Indices[(i + 1) % 3];
                var neighbour = vertexToTriangles[idx1].Intersect(vertexToTriangles[idx2]).FirstOrDefault(t => t != triangle);
                if (neighbour != null)
                {
                    triangle.Neighbours[i] = neighbour.ID;
                }
            }
        }
        
        return triangles;
    }

    private void GenerateNewMesh(IReadOnlyCollection<int> triangles)
    {
        var newMesh = new Mesh {vertices = _vertices, triangles = triangles.ToArray()};
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
        _meshFilter.mesh = newMesh;
    }
    
    private float GetAverageYValue(int i, int k, Vector3 topLeft, float gridStepX, float gridStepZ, bool useNeighbouringSquares)
    {
        var pointsInSquare = new List<Vector3>();
        var yAvg = 0f;
        for (int j = 0; j < _vertices.Length; j++)
        {
            if (_vertices[j].x >= topLeft.x + (i - (useNeighbouringSquares ? 1 : 0)) * gridStepX && 
                _vertices[j].x < topLeft.x + (i + (useNeighbouringSquares ? 2 : 1)) * gridStepX &&
                _vertices[j].z <= topLeft.z - (k - (useNeighbouringSquares ? 1 : 0)) * gridStepZ && 
                _vertices[j].z > topLeft.z - (k + (useNeighbouringSquares ? 2 : 1)) * gridStepZ)
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
        return yAvg;
    }

    private float GetAverageYValue(int i, int k, Vector3 topLeft, float gridStepX, float gridStepZ)
    {
        var yAvg = GetAverageYValue(i, k, topLeft, gridStepX, gridStepZ, false);

        if (Math.Abs(yAvg) > float.Epsilon)
        {
            return yAvg;
        }

        return GetAverageYValue(i, k, topLeft, gridStepX, gridStepZ, true);
    }
}
