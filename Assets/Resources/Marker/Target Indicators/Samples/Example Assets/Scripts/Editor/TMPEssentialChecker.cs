#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace TargetIndicators.Samples
{
    [InitializeOnLoad]
    public class TMPEssentialsChecker
    {
        static ListRequest s_listRequest;

        static TMPEssentialsChecker()
        {
            EditorApplication.delayCall += CheckUGUIAndTMPEssentials;
        }

        static void CheckUGUIAndTMPEssentials()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            s_listRequest = Client.List();
            EditorApplication.update += ListProgress;
        }

        static void ListProgress()
        {
            if (!s_listRequest.IsCompleted)
                return;

            EditorApplication.update -= ListProgress;

            switch (s_listRequest.Status)
            {
                case StatusCode.Success:
                {
                    var uguiPackage = s_listRequest.Result.FirstOrDefault(p => p.name == "com.unity.ugui");

                    if (uguiPackage == null)
                    {
                        EditorUtility.DisplayDialog(
                            "UGUI Package Required",
                            "This package's samples starter assets requires the 'Unity UI (UGUI)' package for text and UI elements to display correctly. " +
                            "Please install it via Window > Package Manager.",
                            "OK"
                        );
                        Debug.LogError("The 'com.unity.ugui' package is not installed.");
                        return;
                    }

                    if (!IsTMPEssentialsImported())
                    {
                        var import = EditorUtility.DisplayDialog(
                            "TextMesh Pro Essentials Missing",
                            "Target Indicator's starter assets require TextMesh Pro Essential Resources " +
                            "to display text in the sample scenes correctly.\n\nThe 'Unity UI (UGUI)' package is " +
                            "installed, but TextMesh Pro Essential Resources appear to be missing.\n\n" +
                            "Would you like to try importing the TextMesh Pro Essentials now?",
                            "Import TMP Essential Resources",
                            "Not Now"
                        );

                        if (import)
                            AttemptTMPEssentialImport();
                        else
                            Debug.LogWarning("TextMesh Pro Essential Resources not imported. Text might not display correctly.");
                    }

                    break;
                }
                case >= StatusCode.Failure:
                    Debug.LogError("Error listing packages: " + s_listRequest.Error.message);
                    EditorUtility.DisplayDialog(
                        "Package Manager Error",
                        "Could not check package status. Please ensure your Package Manager is working correctly.",
                        "OK"
                    );
                    break;
            }
        }

        static bool IsTMPEssentialsImported()
        {
            const string assetPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
            return AssetDatabase.LoadAssetAtPath<Object>(assetPath) != null;
        }

        static void AttemptTMPEssentialImport()
        {
            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
            Debug.Log("Attempting to import TextMesh Pro Essential Resources via menu item.");
        }
    }
}
#endif
