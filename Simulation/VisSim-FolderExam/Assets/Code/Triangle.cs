using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle
{
    public int ID;
    
    public Vector3[] Vertices;
    public int[] Indices;
    public int[] Neighbours;
    
    private Vector3 _normal = Vector3.zero;
    
    public Triangle(Vector3[] vertices, int[] indices, int id)
    {
        Vertices = vertices;
        Indices = indices;
        Neighbours = new int[3] {-1, -1, -1};
        ID = id;
    }
    
    public Vector3 Normal
    {
        get
        {
            if (_normal == Vector3.zero)
            {
                var edge1 = Vertices[1] - Vertices[0];
                var edge2 = Vertices[2] - Vertices[0];
                _normal = Vector3.Cross(edge1, edge2).normalized;
            }

            return _normal;
        }
    }

    public Vector3 BaryCentricCoordinates(Vector2 point)
    {
        return Utilities.Barycentric(
            new Vector2(Vertices[0].x, Vertices[0].z),
            new Vector2(Vertices[1].x, Vertices[1].z),
            new Vector2(Vertices[2].x, Vertices[2].z),
            point
        );
    }
    
    public float HeightAtPoint(Vector2 point)
    {
        // Find plane height from barycentric coordinates.
        var barycentricCoordinates = BaryCentricCoordinates(point);
    
        var height = barycentricCoordinates.x * Vertices[0].y +
                     barycentricCoordinates.y * Vertices[1].y +
                     barycentricCoordinates.z * Vertices[2].y;
        
        return height;
    }
}
