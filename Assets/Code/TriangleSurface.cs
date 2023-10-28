using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriangleSurface : MonoBehaviour
{
    public List<Triangle> Triangles;

    // GridWith * GridHeight must not exceed 65535, otherwise the mesh will not be generated
    private const int GridWidth = 50;
    private const int GridHeight = 50;
    
    private MeshFilter _meshFilter;
    
    private Vector3[] _points;
    private Vector3[] _vertices;
    private int[] _indices;
    
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
    
    public int[] GetTriangleInfo()
    {
        // output example
        // idx1, idx2, idx3, neighbour1, neighbour2, neighbour3
        var output = new int[Triangles.Count * 6];
        for (int i = 0; i < Triangles.Count; i++)
        {
            var triangle = Triangles[i];
            output[i * 6] = triangle.Indices[0];
            output[i * 6 + 1] = triangle.Indices[1];
            output[i * 6 + 2] = triangle.Indices[2];
            output[i * 6 + 3] = triangle.Neighbours[0];
            output[i * 6 + 4] = triangle.Neighbours[1];
            output[i * 6 + 5] = triangle.Neighbours[2];
        }

        return output;
    }
    
    public int FindTriangle(Vector3 point, int initialTriangleId)
    {
        if (initialTriangleId == -1)
        {
            // The initial triangle is not known/valid, perform the full search
            foreach (var triangle in Triangles)
            {
                Vector3 barycentricCoordinates = Utilities.Barycentric(
                    triangle.Vertices[0],
                    triangle.Vertices[1],
                    triangle.Vertices[2],
                    point
                );
                if (Utilities.IsInsideTriangle(barycentricCoordinates))
                {
                    return triangle.ID;
                }
            }

            return -1; // point is not within any triangle
        }
        else
        {
            // Initiate search from the initialTriangleId and check its neighbours
            Triangle initialTriangle = Triangles[initialTriangleId];
            List<Triangle> neighboringTriangles = initialTriangle.Neighbours
                .Where(neighbourId => neighbourId != -1)
                .Select(neighbourId => Triangles[neighbourId])
                .ToList();            neighboringTriangles.Add(initialTriangle); // Also add initial triangle in the list to check for it as well.

            foreach (var triangle in neighboringTriangles)
            {
                Vector3 barycentricCoordinates = Utilities.Barycentric(
                    triangle.Vertices[0],
                    triangle.Vertices[1],
                    triangle.Vertices[2],
                    point
                );
                if (Utilities.IsInsideTriangle(barycentricCoordinates))
                {
                    return triangle.ID;
                }
            }

            return -1; // point is not within any triangle
        }
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
        _indices = triangles.SelectMany(t => t.Indices).ToArray();
        var indices = _indices;
        
        GenerateNewMesh(indices); // Creates a new mesh and assigns it to _mesh and _meshFilter.mesh
        SetupCollision();
    }

    private void SetupCollision()
    {
        // only used for ray-casting collision to spawn the ball on the surface
        var meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = _meshFilter.mesh;
        }
    }
    
    private void SetUpVertices()
    {
        var vertices = GridHeight * GridWidth;
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
    
    private List<Triangle> GenerateTriangles()
    {
        Triangles = new List<Triangle>();
        int triangleId = 0;
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (i % GridWidth == GridWidth - 1 || i / GridWidth == GridHeight - 1) continue;
            
            Triangles.Add(new Triangle(new Vector3[3] {_vertices[i], _vertices[i + GridWidth], _vertices[i + 1]}, new int[3] {i, i + GridWidth, i + 1}, triangleId++));
            Triangles.Add(new Triangle(new Vector3[3] {_vertices[i + 1], _vertices[i + GridWidth], _vertices[i + GridWidth + 1]}, new int[3] {i + 1, i + GridWidth, i + GridWidth + 1}, triangleId++));
        }
        
        // map each vertex to the triangles it is part of
        var vertexToTriangles = new Dictionary<int, List<Triangle>>();
        foreach (var triangle in Triangles)
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
        foreach (var triangle in Triangles)
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
        
        print("Number of vertices: " + _vertices.Length);
        print("Number of triangles: " + Triangles.Count);
        return Triangles;
    }

    private void GenerateNewMesh(int[] triangles)
    {
        if (_meshFilter != null)
        {
            var newMesh = new Mesh {vertices = _vertices, triangles = triangles};
            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();
            _meshFilter.mesh = newMesh;
        }
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
