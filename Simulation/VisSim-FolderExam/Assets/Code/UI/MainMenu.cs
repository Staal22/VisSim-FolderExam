using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private TriangleSurface _triangleSurface;
    
    private void Awake()
    {
        _triangleSurface = GetComponent<TriangleSurface>();
    }

    public void OpenXYZTextFile()
    {
        // make absolute path
        var xyzFilePath = Path.GetFullPath(TerrainTools.XYZPath);
        
        // detect operating system
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // open the file natively on windows
            System.Diagnostics.Process.Start(xyzFilePath);
        #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            // open the file natively on mac
            xyzFilePath = xyzFilePath.Replace("\\", "/");
            System.Diagnostics.Process.Start("open", xyzFilePath);
        #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            // open the file natively on linux
            xyzFilePath = xyzFilePath.Replace("\\", "/");
            System.Diagnostics.Process.Start("xdg-open", xyzFilePath);
        #endif
    }

    public void OpenTriangulationTextFile()
    {
        var triangleInfo = _triangleSurface.GetTriangleInfo();
        
        // write to file
        // format:
        // v1 v2 v3 | n1 n2 n3
        // v2 v3 v4 | n2 n3 n4
        var triangulationFilePath = TerrainTools.TriangleIndicesPath;
        using (StreamWriter sw = new StreamWriter(triangulationFilePath))
        {
            for (int i = 0; i < triangleInfo.Length / 6; i++)
            {
                var v1 = triangleInfo[i * 6];
                var v2 = triangleInfo[i * 6 + 1];
                var v3 = triangleInfo[i * 6 + 2];
                var n1 = triangleInfo[i * 6 + 3];
                var n2 = triangleInfo[i * 6 + 4];
                var n3 = triangleInfo[i * 6 + 5];
                sw.WriteLine($"{v1} {v2} {v3} | {n1} {n2} {n3}");
            }
        }
        
        // make absolute path
        triangulationFilePath = Path.GetFullPath(triangulationFilePath);
        
        // detect operating system
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // open the file natively on windows
            System.Diagnostics.Process.Start(triangulationFilePath);
        #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            // open the file natively on mac
            triangulationFilePath = triangulationFilePath.Replace("\\", "/");
            System.Diagnostics.Process.Start("open", triangulationFilePath);
        #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            // open the file natively on linux
            triangulationFilePath = triangulationFilePath.Replace("\\", "/");
            System.Diagnostics.Process.Start("xdg-open", triangulationFilePath);
        #endif
    }
    
    public void PointCloudScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    public void TriangleSurfaceScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }
    
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
