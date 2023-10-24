using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle
{
    public int ID;
    
    public Vector3[] Vertices;
    public int[] Indices;
    public int[] Neighbours;
    
    public Triangle(Vector3[] vertices, int[] indices, int id)
    {
        Vertices = vertices;
        Indices = indices;
        Neighbours = new int[3] {-1, -1, -1};
        ID = id;
    }
}
