using System;
using System.Collections.Generic;
using OpenCvSharp;
using VideoFileStorage.Utils;

namespace VideoFileStorage.Core
{
    public class Decoder
    {
        /// <summary>
        /// Reads block video frame by frame to extract and decode the underlying original file.
        /// </summary>
        public void DecodeVideoToFile(string inputVideoPath, string outputFolder, IProgress<int> progress, Action<string> log)
        {
            log("Opening video file for decoding...");
            List<bool> allBits = new List<bool>();

            using (var capture = new VideoCapture(inputVideoPath))
            {
                if (!capture.IsOpened())
                    throw new Exception("Failed to open video file for reading.");

                int totalFrames = (int)capture.Get(VideoCaptureProperties.FrameCount);
                int frameIdx = 0;
                Mat frame = new Mat();

                log($"Extracting binary data from {totalFrames} frames...");

                while (capture.Read(frame) && !frame.Empty())
                {
                    bool[] frameBits = VideoProcessor.ExtractBits(frame);
                    allBits.AddRange(frameBits);

                    frameIdx++;
                    // Prevent divide-by-zero or over-reporting
                    if (totalFrames > 0)
                    {
                        int currentProgress = (int)((frameIdx / (double)totalFrames) * 90); // Use 90% for extraction
                        progress?.Report(currentProgress);
                    }
                }
            }

            log("Converting binary bits back to raw bytes...");
            byte[] decodedBytes = BitHelper.ToByteArray(allBits);

            log("Verifying checksum and writing output file...");
            FileHelper.ExtractPayload(decodedBytes, outputFolder);

            log($"Decoding Complete! Extracted file dumped into: {outputFolder}");
            progress?.Report(100);
        }
    }
}
