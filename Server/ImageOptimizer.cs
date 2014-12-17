using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace TA.SharpBooru.Server
{
    public class ImageOptimizer : IDisposable
    {
        private class OptimizationProcess : IDisposable
        {
            private Logger Logger;
            private string ImgFile;
            private Process Process;
            private Thread Thread;

            public OptimizationProcess(Logger Logger, string ImgFile)
            {
                this.ImgFile = ImgFile;
                this.Logger = Logger;
                Thread = new Thread(Optimize) { Name = "OptiThread" };
            }

            private void Optimize()
            {
                if (Logger != null)
                    Logger.LogLine("Optimizing file {0}...", ImgFile);
                Process = new Process();
                Process.StartInfo = GetPSI(ImgFile);
                try
                {
                    Process.Start();
                    try { SyscallEx.setpriority(Process.Id, 19); }
                    catch { }
                    Process.WaitForExit();
                    if (Process.ExitCode == 0)
                    {
                        File.Delete(ImgFile);
                        File.Move(ImgFile + ".new", ImgFile);
                        Logger.LogLine("Optimization of file {0} successfull", ImgFile);
                    }
                    else throw new Exception("Optimization failed");
                }
                catch
                {
                    File.Delete(ImgFile + ".new");
                    Logger.LogLine("Optimization of file {0} failed", ImgFile);
                }
            }

            public void Start() { Thread.Start(); }

            public void Dispose()
            {
                Thread.Join();
                Process.Dispose();
            }
        }

        private const string OPTI_PNG_EXE = "optipng";
        private const string OPTI_JPG_EXE = "jpegtran";
        private const string OPTI_GIF_EXE = "gifsicle";

        private const string OPTI_PNG_ARGS = "-i 0 -o 7 -out {1} {0}";
        private const string OPTI_JPG_ARGS = "-progressive -verbose -optimize -outfile {1} {0}";
        private const string OPTI_GIF_ARGS = "-O2 {0} -V -o {1}";

        private List<OptimizationProcess> _OptiProcesses = new List<OptimizationProcess>();
        private Logger _Logger = null;

        public ImageOptimizer(Logger Logger) { _Logger = Logger ?? Logger.Null; }

        public void Optimize(string ImageFile)
        {
            OptimizationProcess optiProcess = new OptimizationProcess(_Logger, ImageFile);
            optiProcess.Start();
            lock (_OptiProcesses)
                _OptiProcesses.Add(optiProcess);
        }

        public void Dispose() { FinishAllOptimizationProcesses(); }
        public void FinishAllOptimizationProcesses()
        {
            lock (_OptiProcesses)
            {
                for (int i = 0; i < _OptiProcesses.Count; i++)
                    _OptiProcesses[i].Dispose();
            }
        }

        private static ProcessStartInfo GetPSI(string File)
        {
            using (BooruImage img = BooruImage.FromFile(File))
            {
                if (img.ImageFormat == ImageFormat.Png)
                    return GetPSI2(OPTI_PNG_EXE, OPTI_PNG_ARGS, File);
                else if (img.ImageFormat == ImageFormat.Jpeg)
                    return GetPSI2(OPTI_JPG_EXE, OPTI_JPG_ARGS, File);
                else if (img.ImageFormat == ImageFormat.Gif)
                    return GetPSI2(OPTI_GIF_EXE, OPTI_GIF_ARGS, File);
                else throw new BadImageFormatException("Optimization not supported for this ImageFormat");
            }
        }

        private static ProcessStartInfo GetPSI2(string exe, string args, string file)
        {
            return new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = string.Format(args, file, file + ".new"),
                UseShellExecute = false
            };
        }
    }
}
