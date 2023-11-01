using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private string _xyzFilePath = "Assets/StreamingAssets/terrain.txt";
    private string _triangulationFilePath = "Assets/StreamingAssets/triangles.txt";

    private TriangleSurface _triangleSurface;
    
    private void Awake()
    {
        _triangleSurface = GetComponent<TriangleSurface>();
    }

    public void OpenXYZTextFile()
    {
        // make absolute path
        _xyzFilePath = Path.GetFullPath(_xyzFilePath);
        
        // detect operating system
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // open the file natively on windows
            System.Diagnostics.Process.Start(_xyzFilePath);
        #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            // open the file natively on mac
            _xyzFilePath = _xyzFilePath.Replace("\\", "/");
            System.Diagnostics.Process.Start("open", XYZFilePath);
        #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            // open the file natively on linux
            _xyzFilePath = _xyzFilePath.Replace("\\", "/");
            System.Diagnostics.Process.Start("xdg-open", XYZFilePath);
        #endif
    }

    public void OpenTriangulationTextFile()
    {
        var triangleInfo = _triangleSurface.GetTriangleInfo();
        
        // write to file
        // format:
        // v1 v2 v3 | n1 n2 n3
        // v2 v3 v4 | n2 n3 n4
        using (StreamWriter sw = new StreamWriter(_triangulationFilePath))
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
        _triangulationFilePath = Path.GetFullPath(_triangulationFilePath);
        
        // detect operating system
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // open the file natively on windows
            System.Diagnostics.Process.Start(_triangulationFilePath);
        #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            // open the file natively on mac
            _triangulationFilePath = _triangulationFilePath.Replace("\\", "/");
            System.Diagnostics.Process.Start("open", TriangulationFilePath);
        #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            // open the file natively on linux
            _triangulationFilePath = _triangulationFilePath.Replace("\\", "/");
            System.Diagnostics.Process.Start("xdg-open", TriangulationFilePath);
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
