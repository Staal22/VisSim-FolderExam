using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private string _xyzFilePath = "Assets/StreamingAssets/terrain.txt";

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

    public void PointCloudScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
