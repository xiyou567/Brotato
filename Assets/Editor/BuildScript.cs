using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    private const string OutputDir = "Build/Windows";
    private const string ExeName = "Brotato.exe";

    [MenuItem("Build/Build Windows x64")]
    public static void BuildWindows()
    {

        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled) scenes.Add(scene.path);
        }

        if (scenes.Count == 0)
        {
            Debug.LogError("Build Settings 里没有启用的场景，检查 ProjectSettings/EditorBuildSettings.asset");
            if (Application.isBatchMode) EditorApplication.Exit(1);
            return;
        }

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string outputPath = Path.Combine(projectRoot, OutputDir, ExeName);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes.ToArray(),
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log(string.Format("打包成功：{0}（{1:F1} MB，耗时 {2:F1}s）",
                outputPath, summary.totalSize / 1024f / 1024f, summary.totalTime.TotalSeconds));
        }
        else
        {
            Debug.LogError(string.Format("打包失败：{0}，错误 {1} 条", summary.result, summary.totalErrors));

            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }
}
