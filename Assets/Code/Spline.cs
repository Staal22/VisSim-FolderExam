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

        const float step = 0.1f;
        int stepsPerSegment = (int)(1.0f / step) + 1; // Steps for each segment including the last point
        int segmentCount = _controlPoints.Count - 2; // Calculate amount of segments
        _lineRenderer.positionCount = segmentCount * stepsPerSegment; // Total position count
    
        int index = 0;
        for (var i = 0; i < segmentCount; i++)
        {
            for (var t = 0.0f; t <= 1.0; t += step)
            {
                Vector3 position = QuadraticBSpline(_controlPoints.GetRange(i, 3), t);
                _lineRenderer.SetPosition(index++, position);
            }
        }
    }
    
    private Vector3 QuadraticBSpline(List<Vector3> controlPoints, float t)
    {
        // This B-Spline uses a quadratic uniform function for calculation
        var P0 = controlPoints[0];
        var P1 = controlPoints[1];
        var P2 = controlPoints[2];
        
        var part1 = (1 - t) * (1 - t) * P0;
        var part2 = 2 * (1 - t) * t * P1;
        var part3 = t * t * P2;

        return part1 + part2 + part3;
    }
}
