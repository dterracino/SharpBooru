using System;
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace TA.SharpBooru
{
    public class ImageOptimizer
    {
        private const string OPTI_PNG_EXE = "optipng";
        private const string OPTI_JPG_EXE = "jpegtran";
        private const string OPTI_GIF_EXE = "gifsicle";

        private const string OPTI_PNG_ARGS = "-i 0 -o 7 -out {1} {0}";
        private const string OPTI_JPG_ARGS = "-progressive -verbose -optimize -outfile {1} {0}";
        private const string OPTI_GIF_ARGS = "-O2 {0} -V -o {1}";

        public static void TryOptimizeSilent(string File, Logger Logger = null) { TryOptimize(File, Logger); }

        public static bool TryOptimize(string File, Logger Logger = null)
        {
            try
            {
                if (Logger != null)
                    Logger.LogLine("Optimizing file {0}...", File);
                Optimize(File);
                if (Logger != null)
                    Logger.LogLine("Optimization of file {0} finished", File);
                return true;
            }
            catch (Exception ex)
            {
                if (Logger != null)
                    Logger.LogException("ImageOptimization", ex);
                return false;
            }
        }

        public static void Optimize(string File)
        {
            using (BooruImage img = BooruImage.FromFile(File))
            {
                if (img.ImageFormat == ImageFormat.Png)
                    OptimizeStage2(OPTI_PNG_EXE, OPTI_PNG_ARGS, File);
                else if (img.ImageFormat == ImageFormat.Jpeg)
                    OptimizeStage2(OPTI_JPG_EXE, OPTI_JPG_ARGS, File);
                else if (img.ImageFormat == ImageFormat.Gif)
                    OptimizeStage2(OPTI_GIF_EXE, OPTI_GIF_ARGS, File);
                else throw new BadImageFormatException("Optimization not supported for this ImageFormat");
            }
        }

        private static void OptimizeStage2(string Exe, string Args, string ImgFile)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = Exe,
                Arguments = string.Format(Args, ImgFile + ".bak", ImgFile),
                UseShellExecute = false
            };
            File.Copy(ImgFile, ImgFile + ".bak");
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                File.Move(ImgFile + ".bak", ImgFile);
                throw new Exception("Optimization failed");
            }
            else File.Delete(ImgFile + ".bak");
        }
    }
}