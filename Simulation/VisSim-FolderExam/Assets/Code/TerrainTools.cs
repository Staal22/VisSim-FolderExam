using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class TerrainTools
{
    private const string XYZPath = "Assets/StreamingAssets/terrain.txt";
    private const string TriangleIndicesPath = "Assets/StreamingAssets/triangles.txt";
    
    public static Vector3[] GetPoints()
    {
        return LoadPositionsFromFile(XYZPath).ToArray();
    }
    
    private static Vector3[] LoadPositionsFromFile(string path)
    {
        string[] lines = File.ReadAllLines(path).Skip(1).ToArray();
        return lines.Select(line =>
        {
            var values = line.Split(' ');
            return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        }).ToArray();
    }
}