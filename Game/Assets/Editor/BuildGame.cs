using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CoopFighter.UnityGame.Editor
{
    public static class BuildGame
    {
        public static void BuildVerifiedWeb() {
            ScenarioValidation.Run();
            BuildWeb();
        }
        [MenuItem("Co-op Fighter/Build Web Game")]
        public static void BuildWeb()
        {
            ProjectValidation.Run();
            PlayerSettings.companyName = "Beaconbreak Studio";
            PlayerSettings.productName = "Beaconbreak";
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.WebGL.template = "PROJECT:Beaconbreak";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.initialMemorySize = 128;
            PlayerSettings.runInBackground = true;
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { "Assets/Scenes/Main.unity" }, locationPathName = "../Web/public",
                target = BuildTarget.WebGL, options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Web build failed");
            Debug.Log("BEACONBREAK_WEB_BUILD_PASS");
        }
        [MenuItem("Co-op Fighter/Build Windows Game")]
        public static void BuildWindows()
        {
            ProjectValidation.Run();
            Directory.CreateDirectory("Builds");
            BuildPlayerOptions settings = new BuildPlayerOptions {
                scenes = new[] { "Assets/Scenes/Main.unity" },
                locationPathName = "Builds/Beaconbreak.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(settings);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed: " + report.summary.result);
            Debug.Log("Beaconbreak Windows build succeeded: " + settings.locationPathName);
        }
    }
}
