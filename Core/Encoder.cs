using System;
using System.IO;
using OpenCvSharp;
using VideoFileStorage.Utils;

namespace VideoFileStorage.Core
{
    public class Encoder
    {
        /// <summary>
        /// Encodes a generic file into an MP4 video storage format.
        /// </summary>
        public void EncodeFileToVideo(string inputFilePath, string outputVideoPath, IProgress<int> progress, Action<string> log)
        {
            log($"Reading file: {Path.GetFileName(inputFilePath)}...");
            byte[] fileData = File.ReadAllBytes(inputFilePath);

            log("Building payload and applying SHA256 checksum...");
            byte[] payload = FileHelper.CreatePayload(inputFilePath, fileData);

            log("Converting payload to binary blocks...");
            bool[] bits = BitHelper.ToBitArray(payload);

            int totalFrames = (int)Math.Ceiling((double)bits.Length / VideoProcessor.BitsPerFrame);
            int fps = 10; // Low FPS helps minimize errors

            log($"Writing Video ({totalFrames} frames)...");

            // OpenCvSharp VideoWriter using standard MP4V codec
            using (var writer = new VideoWriter(outputVideoPath, FourCC.MP4V, fps, new OpenCvSharp.Size(VideoProcessor.FrameWidth, VideoProcessor.FrameHeight), true))
            {
                if (!writer.IsOpened())
                    throw new Exception("Failed to initialize OpenCvSharp VideoWriter. Ensure output path is accessible.");

                for (int frameIdx = 0; frameIdx < totalFrames; frameIdx++)
                {
                    int startIndex = frameIdx * VideoProcessor.BitsPerFrame;

                    using (Mat frame = VideoProcessor.CreateFrame(bits, startIndex))
                    {
                        writer.Write(frame);
                    }

                    progress?.Report((int)((frameIdx + 1) / (double)totalFrames * 100));
                }
            }

            log($"Encoding Complete! File saved to: {outputVideoPath}");
            progress?.Report(100);
        }
    }
}
