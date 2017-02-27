﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace PSO2ProxyLauncherNew.Classes.PSO2
{
    public static class CommonMethods
    {

        public static bool IsPSO2Folder(string path)
        {
            if (File.Exists(Path.Combine(path, "pso2launcher.exe")))
                if (File.Exists(Path.Combine(path, "pso2.exe")))
                    if (Directory.Exists(Path.Combine(path, "data", "win32")))
                        if (Directory.Exists(Path.Combine(path, "gameguard")))
                            return true;
            return false;
        }

        public static Exception LaunchPSO2(bool waitForExit = true)
        {
            return LaunchPSO2(MySettings.PSO2Dir, waitForExit);
        }
        public static Exception LaunchPSO2(string path, bool waitForExit = true)
        {
            Exception result = null;
            try
            {
                string pso2exefullpath = Path.Combine(path, "pso2.exe");
                if (File.Exists(pso2exefullpath))
                {
                    PSO2HexStartInfo hexInfo = DefaultValues.RandomStartGameHexCode;
                    ProcessStartInfo myProcInfo = new ProcessStartInfo(pso2exefullpath, hexInfo.ImageString);
                    myProcInfo.UseShellExecute = false;
                    myProcInfo.WorkingDirectory = path;
                    var myProcInfo_env = myProcInfo.EnvironmentVariables;
                    if (myProcInfo_env.ContainsKey(DefaultValues.EnviromentKey))
                        myProcInfo_env.Add(DefaultValues.EnviromentKey, hexInfo.EnviromentString);
                    else if (myProcInfo_env[DefaultValues.EnviromentKey] != hexInfo.EnviromentString)
                        myProcInfo_env[DefaultValues.EnviromentKey] = hexInfo.EnviromentString;

                    //if ((Infos.OSVersionInfo.Name.ToLower() != "windows xp") && Classes.MySettings.LaunchPSO2AsAdmin)
                    if ((Infos.OSVersionInfo.Name.ToLower() != "windows xp"))
                        myProcInfo.Verb = "runas";

                    if (myProcInfo_env.ContainsKey("COMPLUS_NoGuiFromShim"))
                        myProcInfo_env.Remove("COMPLUS_NoGuiFromShim");
                    if (myProcInfo_env.ContainsKey("_NO_DEBUG_HEAP"))
                        myProcInfo_env.Remove("_NO_DEBUG_HEAP");
                    if (myProcInfo_env.ContainsKey("VS110COMNTOOLS"))
                        myProcInfo_env.Remove("VS110COMNTOOLS");
                    if (myProcInfo_env.ContainsKey("VS120COMNTOOLS"))
                        myProcInfo_env.Remove("VS120COMNTOOLS");
                    if (myProcInfo_env.ContainsKey("VS140COMNTOOLS"))
                        myProcInfo_env.Remove("VS140COMNTOOLS");
                    if (myProcInfo_env.ContainsKey("MSBuildLoadMicrosoftTargetsReadOnly"))
                        myProcInfo_env.Remove("MSBuildLoadMicrosoftTargetsReadOnly");
                    if (myProcInfo_env.ContainsKey("VisualStudioDir"))
                        myProcInfo_env.Remove("VisualStudioDir");
                    if (myProcInfo_env.ContainsKey("VisualStudioEdition"))
                        myProcInfo_env.Remove("VisualStudioEdition");
                    if (myProcInfo_env.ContainsKey("VisualStudioVersion"))
                        myProcInfo_env.Remove("VisualStudioVersion");
                    if (myProcInfo_env.ContainsKey("VSLANG"))
                        myProcInfo_env.Remove("VSLANG");
                    using (Process myProc = new Process())
                    {
                        myProc.StartInfo = myProcInfo;
                        myProc.Start();
                        if (waitForExit)
                            myProc.WaitForExit();
                    }
                    result = null;
                }
                else
                    result = new FileNotFoundException("PSO2 executable file is not found", pso2exefullpath);
            }
            catch (Exception ex) { result = ex; }
            return result;
        }
    }
}
