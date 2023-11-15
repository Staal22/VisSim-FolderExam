using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class TriangleSurface : MonoBehaviour
{
    public static TriangleSurface Instance;
    public List<Triangle> Triangles;

    // GridWith * GridHeight must not exceed 65535, otherwise the mesh will not be generated
    [SerializeField] private bool runSilent;
    private const int GridResolution = 60;
    private const int ReducedPointCount = 100000;
    private int _gridWidth;
    private int _gridHeight;
    private bool _trianglesGenerated;
    private float _gridStepX;
    private float _gridStepZ;
    private Vector3 _topLeft = Vector3.zero;
    
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    
    private Vector3[] _points;
    private Vector3[] _reducedPoints;
    private Vector3[] _vertices;
    private int[] _indices;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        _gridWidth = GridResolution;
        _gridHeight = GridResolution;
    }

    private void Start()
    {
        _points = TerrainTools.GetPoints();
        for (int i = 0; i < _points.Length; i++)
        {
            var pos = _points[i];
            var unityPos = new Vector3(pos.x - TerrainTools.XOffset, pos.z, pos.y - TerrainTools.YOffset);
            _points[i] = unityPos;
        }
        CreateMesh();
    }

    public void GetTriangleInfo(Action<int[]> callback)
    {
        StartCoroutine(FetchTriangleInfo(callback));
    }
    
    private IEnumerator FetchTriangleInfo(Action<int[]> callback)
    {
        yield return new WaitUntil(() => _trianglesGenerated);

        var output = GenerateOutput();

        callback?.Invoke(output);
    }
    
    private int[] GenerateOutput()
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
    
    public int FindTriangle(Vector2 point, int initialTriangleId)
    {
        if (initialTriangleId == -1)
        {
            // The initial triangle is not known/valid, perform the full search
            foreach (var triangle in Triangles)
            {
                Vector3 barycentricCoordinates = triangle.BaryCentricCoordinates(point);
                if (Utilities.IsInsideTriangle(barycentricCoordinates))
                {
                    return triangle.ID;
                }
            }

            return -1; // point is not within any triangle
        }

        // Initiate search from the initialTriangleId and check its neighbours
        Triangle initialTriangle = Triangles[initialTriangleId];
        List<Triangle> neighboringTriangles = initialTriangle.Neighbours
            .Where(neighbourId => neighbourId != -1)
            .Select(neighbourId => Triangles[neighbourId])
            .ToList();            neighboringTriangles.Add(initialTriangle); // Also add initial triangle in the list to check for it as well.

        foreach (var triangle in neighboringTriangles)
        {
            Vector3 barycentricCoordinates = triangle.BaryCentricCoordinates(point);
            if (Utilities.IsInsideTriangle(barycentricCoordinates))
            {
                return triangle.ID;
            }
        }

        return -1; // point is not within any triangle

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
        if (runSilent)
        {
            return;
        }
        _indices = triangles.SelectMany(t => t.Indices).ToArray();
        var indices = _indices;
        
        GenerateNewMesh(indices); // Creates a new mesh and assigns it to _mesh and _meshFilter.mesh
    }
    
    private void SetUpVertices()
    {
        int points = ReducedPointCount;
        int step = _points.Length / points;
        _reducedPoints = new Vector3[points];

        int index = 0;
        for (int i = 0; i < points; i++)
        {
            _reducedPoints[i] = _points[index];
            index += step;
        }
    }

    private (float, float, float, float) FindVerticesBounds()
    {
        float xMax = float.MinValue, zMax = float.MinValue, xMin = float.MaxValue, zMin = float.MaxValue;

        foreach (Vector3 vertex in _reducedPoints)
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
        _topLeft = topLeft;
        var grid = new List<Vector3>();
        _gridStepX = (xMax - xMin) / _gridWidth;
        _gridStepZ = (zMax - zMin) / _gridHeight;
        for (int i = 0; i < _gridWidth; i++)
        {
            for (int k = 0; k < _gridHeight; k++)
            {
                var yAvg = GetAverageYValue(i, k, _topLeft, _gridStepX, _gridStepZ);
                var point = new Vector3(_topLeft.x + i * _gridStepX, yAvg, _topLeft.z - k * _gridStepZ);
                grid.Add(point);
            }
        }

        return grid;
    }
    
    private IEnumerable<Triangle> GenerateTriangles()
    {
        Triangles = new List<Triangle>();
        int triangleId = 0;
        for (int i = 0; i < _vertices.Length; i++)
        {
            if (i % _gridWidth == _gridWidth - 1 || i / _gridWidth == _gridHeight - 1) continue;
            
            Triangles.Add(new Triangle(new [] {_vertices[i], _vertices[i + _gridWidth], _vertices[i + 1]}, new int[] {i, i + _gridWidth, i + 1}, triangleId++));
            Triangles.Add(new Triangle(new [] {_vertices[i + 1], _vertices[i + _gridWidth], _vertices[i + _gridWidth + 1]}, new int[] {i + 1, i + _gridWidth, i + _gridWidth + 1}, triangleId++));
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

        if (!runSilent)
        {
            print("Number of vertices: " + _vertices.Length);
            print("Number of triangles: " + Triangles.Count);
            print("Number of data points " + _reducedPoints.Length);
        }
        _trianglesGenerated = true;
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
        if (_meshCollider != null)
        {
            _meshCollider.sharedMesh = _meshFilter.mesh;
        }
    }
    
    private float GetAverageYValue(int i, int k, Vector3 topLeft, float gridStepX, float gridStepZ, bool useNeighbouringSquares)
    {
        var pointsInSquare = new List<Vector3>();
        var yAvg = 0f;
        for (int j = 0; j < _reducedPoints.Length; j++)
        {
            if (_reducedPoints[j].x >= topLeft.x + (i - (useNeighbouringSquares ? 1 : 0)) * gridStepX && 
                _reducedPoints[j].x < topLeft.x + (i + (useNeighbouringSquares ? 2 : 1)) * gridStepX &&
                _reducedPoints[j].z <= topLeft.z - (k - (useNeighbouringSquares ? 1 : 0)) * gridStepZ && 
                _reducedPoints[j].z > topLeft.z - (k + (useNeighbouringSquares ? 2 : 1)) * gridStepZ)
            {
                pointsInSquare.Add(_reducedPoints[j]);
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