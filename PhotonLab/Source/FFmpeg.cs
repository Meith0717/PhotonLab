// FFmpeg.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Diagnostics;

namespace FlowLab.Core
{
    internal class FFmpeg : IDisposable
    {
        private Process _ffmpegProcess;

        public FFmpeg(int width, int height, int frameRate, string outputPath)
        {
            // Start FFmpeg process
            _ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-f rawvideo -pix_fmt rgb24 -s {width}x{height} -r {frameRate} -i - -c:v libx264 -pix_fmt yuv420p {outputPath}",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            _ffmpegProcess.Start();

            // Optional: Log FFmpeg output
            _ffmpegProcess.ErrorDataReceived += (sender, e) => { if (e.Data != null) Debug.WriteLine(e.Data); };
            _ffmpegProcess.BeginErrorReadLine();
        }

        public void WriteFrame(byte[] colorData)
        {
            // Write the frame to FFmpeg
            _ffmpegProcess.StandardInput.BaseStream.Write(colorData, 0, colorData.Length);
        }

        public void Finish()
        {
            _ffmpegProcess.StandardInput.Close();
            _ffmpegProcess.WaitForExit();
            Console.WriteLine($"FFmpeg exited with code {_ffmpegProcess.ExitCode}");
        }

        public void Dispose()
        {
            if (_ffmpegProcess == null) return;
            if (!_ffmpegProcess.HasExited)
                _ffmpegProcess.Kill();
            _ffmpegProcess.Dispose();
        }
    }
}
