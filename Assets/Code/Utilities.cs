using System;
using System.IO;
using UnityEngine;

public static class Utilities
{
    public static Vector3 Barycentric(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
    {
        Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;

        Vector3 result = new Vector3();
        result.y = (d11 * d20 - d01 * d21) / denom;
        result.z = (d00 * d21 - d01 * d20) / denom;
        result.x = 1.0f - result.y - result.z;

        return result;
    }

    // Check if Barycentric coordinates are inside the triangle
    public static bool IsInsideTriangle(Vector3 barycentricCoordinates)
    {
        // Barycentric coordinates inside triangle when all values are greater than 0 and less than 1
        return barycentricCoordinates.x >= 0f && barycentricCoordinates.y >= 0f && barycentricCoordinates.z >= 0f
               && barycentricCoordinates.x <= 1f && barycentricCoordinates.y <= 1f && barycentricCoordinates.z <= 1f;
    }
}