using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SimpleExternalUnityConsole.Model
{
    public class UnityBuild
    {
        public Process Process {  get; set; }
        public string GameDirectory { get; set; }
        public string BepInExDirectory { get; set; }
        public string PluginsDirectory { get; set; }
        public string TargetDLLPath {  get; set; }

        public UnityBuild(string selectedGamePath)
        {
            GameDirectory = Path.GetDirectoryName(selectedGamePath);
            BepInExDirectory = Path.Combine(GameDirectory, "BepInEx");
            PluginsDirectory = Path.Combine(BepInExDirectory, "plugins");
            TargetDLLPath = Path.Combine(PluginsDirectory, "PipelineTemplateMod.dll");
        }
    }
}
