using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaterBody : MonoBehaviour
{
    private TriangleSurface _triangleSurface;
    private float _expandHeight = 0.3f;
    private float _xScale = 0.3f;
    private float _zScale = 0.3f;


    private void Awake() 
    {
        _triangleSurface = TriangleSurface.Instance;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z);
        if (!HasWalls(false))
        {
            Destroy(gameObject);
        }
    }

    private void Expand()
    {
        var transform1 = transform;
        var scale = transform1.localScale;
        scale.x += _xScale;
        scale.z += _zScale;
        // _xScale /= 2;
        // _zScale /= 2;
        transform1.localScale = scale;
        if (!HasWalls(true))
            return;
        var pos = transform1.position;
        pos.y += _expandHeight;
        // _expandHeight /= 2;
        transform1.position = pos;
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
            height = _expandHeight;
        }
        
        var transform1 = transform;
        var position1 = transform1.position;
        var scale1 = transform1.localScale;
        var distance = scale1.x * 5; // * 10 / 2
        
        var corner1 = position1 + new Vector3(-distance, height, -distance);
        var corner2 = position1 + new Vector3(distance, height, distance);
        var corner3 = position1 + new Vector3(-distance, height, distance);
        var corner4 = position1 + new Vector3(distance, height, -distance);
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
    
}
