using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaterBody : MonoBehaviour
{
    private TriangleSurface _triangleSurface;
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    
    private List<Vector3> _vertices = new();
    
    private const float ExpandHeight = 0.2f;
    private const float ExpandX = 1f;
    private const float ExpandZ = 1f;


    private void Awake() 
    {
        _triangleSurface = TriangleSurface.Instance;
        _meshFilter = GetComponent<MeshFilter>();
        
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
        var indices = new int[] {2, 1, 0, 3, 0, 1};
        _mesh = new Mesh
        {
            vertices = _vertices.ToArray(),
            triangles = indices
        };
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _meshFilter.mesh = _mesh;
    }

    private void Expand()
    {
        // update vertices x and z
        _vertices[0] = new Vector3(_vertices[0].x - ExpandX, _vertices[0].y, _vertices[0].z - ExpandZ);
        _vertices[1] = new Vector3(_vertices[1].x + ExpandX, _vertices[1].y, _vertices[1].z + ExpandZ);
        _vertices[2] = new Vector3(_vertices[2].x - ExpandX, _vertices[2].y, _vertices[2].z + ExpandZ);
        _vertices[3] = new Vector3(_vertices[3].x + ExpandX, _vertices[3].y, _vertices[3].z - ExpandZ);
        UpdateMesh();
        if (!HasWalls(true))
            return;
        // update y
        _vertices[0] = new Vector3(_vertices[0].x, _vertices[0].y + ExpandHeight, _vertices[0].z);
        _vertices[1] = new Vector3(_vertices[1].x, _vertices[1].y + ExpandHeight, _vertices[1].z);
        _vertices[2] = new Vector3(_vertices[2].x, _vertices[2].y + ExpandHeight, _vertices[2].z);
        _vertices[3] = new Vector3(_vertices[3].x, _vertices[3].y + ExpandHeight, _vertices[3].z);
        UpdateMesh();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var rollingBall = other.gameObject.GetComponent<RollingBall>();
        if (rollingBall != null && rollingBall.isRainDrop)
        {
            Expand();
            rollingBall.BecomeWaterBody(true);
            return;

        }
        var waterBody = other.gameObject.GetComponent<WaterBody>();
        if (waterBody != null)
        {
            Expand();
            Destroy(waterBody.gameObject);
        }
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
    
}
