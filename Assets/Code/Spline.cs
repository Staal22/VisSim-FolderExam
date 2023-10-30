using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{
    private List<Vector3> _controlPoints = new();
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetControlPoints(List<Vector3> controlPoints)
    {
        _controlPoints = controlPoints;
        
        if(_controlPoints.Count < 3) return; // At least 3 points are required for B-Spline

        const float step = 0.01f;
        int stepsPerSegment = (int)(1.0f / step) + 1; // Steps for each segment including the last point
        int segmentCount = _controlPoints.Count - 2; // Calculate amount of segments
        _lineRenderer.positionCount = segmentCount * stepsPerSegment; // Total position count
    
        int index = 0;
        for (var i = 0; i < segmentCount; i++)
        {
            for (var t = 0.0f; t <= 1.0; t += step)
            {
                Vector3 position = QuadraticBSpline(_controlPoints[i], _controlPoints[i+1], _controlPoints[i+2], t);
                _lineRenderer.SetPosition(index++, position);
            }
        }
    }
    
    private Vector3 QuadraticBSpline(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        // This B-Spline uses a quadratic uniform function for calculation
        var tSq = t * t;
        var oneMinusT = 1 - t;
        
        var part1 = oneMinusT * oneMinusT * p0;
        var part2 = 2 * oneMinusT * t * p1;
        var part3 = tSq * p2;

        return part1 + part2 + part3;
    }
}
