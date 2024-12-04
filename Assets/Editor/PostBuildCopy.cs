#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEngine;

public class PostBuildCopy : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPostprocessBuild(BuildReport report)
    {
        string sourceFolder = Path.Combine(Application.dataPath, "Resources/Maps");

        string buildFolder = Path.GetDirectoryName(report.summary.outputPath);
        string destinationFolder = Path.Combine(buildFolder, "Maps");
        Debug.Log($"Source folder: {sourceFolder}");
        Debug.Log($"Destination folder: {destinationFolder}");

        if (!Directory.Exists(sourceFolder))
        {
            Debug.LogError($"Source folder does not exist: {sourceFolder}");
            return;
        }

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
            Debug.Log($"Created folder: {destinationFolder}");
        }

        foreach (string file in Directory.GetFiles(sourceFolder))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destinationFolder, fileName);

            try
            {
                File.Copy(file, destFile, true);
                Debug.Log($"Copied {file} to {destFile}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error copying {file} to {destFile}: {ex.Message}");
            }
        }
    }
}
#endif
