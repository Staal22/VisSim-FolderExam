using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class WaterBody : MonoBehaviour
{
    private TriangleSurface _triangleSurface;
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    private BoxCollider _boxCollider;
    
    private readonly List<Vector3> _vertices = new();
    private readonly List<RollingBall> _balls = new();
    
    private const float ExpandHeight = 0.2f;
    private const float ExpandX = 1f;
    private const float ExpandZ = 1f;
    private static readonly int[] Indices = {2, 1, 0, 3, 0, 1};

    private float _initTime;
    private bool _isActive;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _boxCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        _initTime = Time.fixedTime;
        _triangleSurface = TriangleSurface.Instance;
        
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z);
        // find corner vertices
        const float distance = 5f;
        _vertices.Add(new Vector3(-distance, 0, -distance));
        _vertices.Add(new Vector3(distance, 0, distance));
        _vertices.Add(new Vector3(-distance, 0, distance));
        _vertices.Add(new Vector3(distance, 0, -distance));
        if (!HasWalls(false))
        {
            Destroy(gameObject);
        }
        _mesh = new Mesh
        {
            vertices = _vertices.ToArray(),
            triangles = Indices
        };
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _meshFilter.mesh = _mesh;
    }

    private void FixedUpdate()
    {
        if (!_isActive)
            return;

        // iterate backwards so we can remove elements
        for (int i = _balls.Count - 1; i > -1; i--)
        {
            var ball = _balls[i];
            if (ball == null || ball.gameObject == null)
            {
                Debug.Log("Ball has been destroyed. Removing it from list.");
                _balls.RemoveAt(i);
                continue;
            }
            if (_balls[i].isRainDrop)
            {
                // if within one of the two triangles of water mesh, consume it and expand
                if (WithinTriangles(_balls[i].gameObject.transform.position))
                {
                    Expand();
                    ball.BecomeWaterBody(true);
                }
            }
            else if (WithinTriangles(_balls[i].gameObject.transform.position)) // ekstrem-vær effekt
            {
                // print("Floating ball" + ball.gameObject.name);
                
                // push away normal balls (non-raindrops)
                var height = _vertices[0].y;
                // to world space
                height += transform.position.y;
                ball.DoFloat(height);
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // stop weird logic where we detect the original ball that created this object
        if (Time.fixedTime - _initTime < 0.1f)
            return;
        
        var rollingBall = other.gameObject.GetComponent<RollingBall>();
        if (rollingBall != null)
        {
            _balls.Add(rollingBall);
            _isActive = true;
            rollingBall.onRollingBallDestruction.AddListener(() => RemoveRollingBall(rollingBall));
        }
        var waterBody = other.gameObject.GetComponent<WaterBody>();
        if (waterBody != null)
        {
            // merge water bodies
            Expand();
            Destroy(waterBody.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var rollingBall = other.gameObject.GetComponent<RollingBall>();
        if (rollingBall != null)
        {
            if (!rollingBall.isRainDrop)
                rollingBall.StopFloating();
            rollingBall.onRollingBallDestruction.RemoveListener(() => RemoveRollingBall(rollingBall));
            _balls.Remove(rollingBall);
        }
        if (_balls.Count == 0)
            _isActive = false;
    }
    
    private void Expand()
    {
        // update vertices x and z
        _vertices[0] = new Vector3(_vertices[0].x - ExpandX, _vertices[0].y, _vertices[0].z - ExpandZ);
        _vertices[1] = new Vector3(_vertices[1].x + ExpandX, _vertices[1].y, _vertices[1].z + ExpandZ);
        _vertices[2] = new Vector3(_vertices[2].x - ExpandX, _vertices[2].y, _vertices[2].z + ExpandZ);
        _vertices[3] = new Vector3(_vertices[3].x + ExpandX, _vertices[3].y, _vertices[3].z - ExpandZ);
        UpdateMesh();
        _boxCollider.size = new Vector3(_boxCollider.size.x + ExpandX * 2, _boxCollider.size.y, _boxCollider.size.z + ExpandZ * 2);
        if (!HasWalls(true))
            return;
        // update y
        _vertices[0] = new Vector3(_vertices[0].x, _vertices[0].y + ExpandHeight, _vertices[0].z);
        _vertices[1] = new Vector3(_vertices[1].x, _vertices[1].y + ExpandHeight, _vertices[1].z);
        _vertices[2] = new Vector3(_vertices[2].x, _vertices[2].y + ExpandHeight, _vertices[2].z);
        _vertices[3] = new Vector3(_vertices[3].x, _vertices[3].y + ExpandHeight, _vertices[3].z);
        UpdateMesh();
        _boxCollider.size = new Vector3(_boxCollider.size.x, _boxCollider.size.y + ExpandHeight, _boxCollider.size.z);
    }

    private bool WithinTriangles(Vector3 point)
    {
        for (int i = 0; i < Indices.Length; i += 3)
        {
            var worldVertex1 = _vertices[Indices[i]] + transform.position;
            var worldVertex2 = _vertices[Indices[i + 1]] + transform.position;
            var worldVertex3 = _vertices[Indices[i + 2]] + transform.position;
            Vector3 barycentricCoordinates = Utilities.Barycentric(
                worldVertex1,
                worldVertex2,
                worldVertex3,
                point
            );
            if (Utilities.IsInsideTriangle(barycentricCoordinates))
            {
                return true;
            }
        }
        return false; // point is not within any triangle
    }

    private bool HasWalls(bool predict)
    {
        var height = 0f;
        if (predict)
        {
            height = ExpandHeight;
        }
        
        var corner1 =  new Vector3(_vertices[0].x, _vertices[0].y + height, _vertices[0].z);
        var corner2 =  new Vector3(_vertices[1].x, _vertices[1].y + height, _vertices[1].z);
        var corner3 =  new Vector3(_vertices[2].x, _vertices[2].y + height, _vertices[2].z);
        var corner4 =  new Vector3(_vertices[3].x, _vertices[3].y + height, _vertices[3].z);
        var corners = new List<Vector3> {corner1, corner2, corner3, corner4};
        var cornerTriangles = new List<Triangle>();
        foreach (var corner in corners)
        {
            var triangleID = _triangleSurface.FindTriangle(corner, -1);
            if (triangleID != -1)
            {
                var triangle = _triangleSurface.Triangles[triangleID];
                cornerTriangles.Add(triangle);
            }
        }
        for (int i = 0; i < cornerTriangles.Count; i++)
        {
            if (cornerTriangles[i].HeightAtPoint(corners[i]) < corners[i].y)
                return false;
        }
        return true;
    }

    private void UpdateMesh()
    {
        _mesh.vertices = _vertices.ToArray();
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _meshFilter.mesh = _mesh;
    }
    
    private void RemoveRollingBall(RollingBall ball)
    {
        if (_balls != null && _balls.Contains(ball))
        {
            _balls.Remove(ball);
        }
    }
}