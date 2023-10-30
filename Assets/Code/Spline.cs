using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{
    private List<Vector3> _controlPoints = new();

    public void SetControlPoints(List<Vector3> controlPoints)
    {
        _controlPoints = controlPoints;
        // adjust y value of control points so the spline hovers above the ground
        for (var i = 0; i < _controlPoints.Count; i++)
        {
            _controlPoints[i] = new Vector3(_controlPoints[i].x, _controlPoints[i].y + 5f, _controlPoints[i].z);
        }
    }
    
    public void OnDrawGizmos()
    {
        if (_controlPoints.Count < 2) return;

        for (var i = 0; i < _controlPoints.Count - 1; i++)
        {
            // Redraw the Gizmos each frame so the control lines follow the controlPoints
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_controlPoints[i], _controlPoints[i + 1]);
        }

        // Draw the actual curve
        Gizmos.color = Color.red;
        for (var t = 0.0f; t <= 1.0; t += 0.01f)
        {
            Gizmos.DrawSphere(QuadraticBSpline(_controlPoints, t), 0.1f);
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
