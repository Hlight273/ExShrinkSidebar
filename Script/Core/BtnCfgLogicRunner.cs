using ExShrinkSidebar.Script.Model;
using System.Diagnostics;

namespace ExShrinkSidebar.Script.Core {
    public static class BtnCfgLogicRunner
    {
        public static void Run(ButtonConfig cfg)
        {
            DoLogic(cfg);
        }

        private static bool DoLogic(ButtonConfig cfg)
        {
            if (cfg.configType == ExConfigType.Combine)
            {
                for (int i = 0; i < cfg.logicChain.Count; i++)
                {
                    return DoLogic(cfg.logicChain[i]);
                }
            }
            else
            {
                switch (cfg.configType)
                {
                    case ExConfigType.Notepad:
                        return DoProcess("notepad.exe");
                    case ExConfigType.OpenFolder:
                        return DoProcess("explorer.exe " + cfg?.arg?.path);
                    case ExConfigType.Cmd:
                        return DoProcess(cfg?.arg?.script);
                }
            }
            return true;
        }

        private static bool DoProcess(string script)
        {
            if(string.IsNullOrEmpty(script)) return false;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c " + script,
                    CreateNoWindow = true
                });
                return true;
            }
            catch (Exception ex) { 
                Debug.WriteLine("DoProcess"+ex.Message);
                return false;
            }
        }
    }
}
