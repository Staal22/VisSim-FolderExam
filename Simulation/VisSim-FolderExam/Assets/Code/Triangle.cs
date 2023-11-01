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

    public float HeightAtPoint(Vector3 point)
    {
        var height = 0f;
        // Find plane height from barycentric coordinates.
        var barycentricCoordinates = Utilities.Barycentric(
            Vertices[0],
            Vertices[1],
            Vertices[2],
            point
        );
    
        height = barycentricCoordinates.x * Vertices[0].y +
                 barycentricCoordinates.y * Vertices[1].y +
                 barycentricCoordinates.z * Vertices[2].y;
        
        return height;
    }
    
    // public float HighestPoint
    // {
    //     get
    //     {
    //         var highestPoint = Vertices[0].y;
    //         foreach (var vertex in Vertices)
    //         {
    //             if (vertex.y > highestPoint)
    //                 highestPoint = vertex.y;
    //         }
    //         return highestPoint;
    //     }
    // }
    
}
