using OpenCvSharp;
using System;

namespace VideoFileStorage.Core
{
    public static class VideoProcessor
    {
        // Low Resolution config: 640x480
        public const int FrameWidth = 640;
        public const int FrameHeight = 480;

        // 8x8 blocks will produce robust encodings resistant to video compression algorithms
        public const int BlockSize = 8;

        public const int BlocksPerRow = FrameWidth / BlockSize;      // 80
        public const int BlocksPerCol = FrameHeight / BlockSize;     // 60
        public const int BitsPerFrame = BlocksPerRow * BlocksPerCol; // 4800 bits per frame

        /// <summary>
        /// Renders a black and white frame from a chunk of boolean bits.
        /// </summary>
        public static Mat CreateFrame(bool[] bits, int startIndex)
        {
            // Create pure black base frame
            Mat frame = new Mat(FrameHeight, FrameWidth, MatType.CV_8UC3, new Scalar(0, 0, 0));
            int bitCount = Math.Min(BitsPerFrame, bits.Length - startIndex);

            for (int i = 0; i < bitCount; i++)
            {
                if (bits[startIndex + i])
                {
                    int row = i / BlocksPerRow;
                    int col = i % BlocksPerRow;

                    // Define area of the block
                    var rect = new Rect(col * BlockSize, row * BlockSize, BlockSize, BlockSize);

                    // Draw a white block (1)
                    Cv2.Rectangle(frame, rect, new Scalar(255, 255, 255), -1);
                }
            }
            return frame;
        }

        /// <summary>
        /// Reads a frame and converts the average block brightness back to bits.
        /// </summary>
        public static bool[] ExtractBits(Mat frame)
        {
            bool[] bits = new bool[BitsPerFrame];
            using Mat grayFrame = new Mat();

            if (frame.Channels() == 3)
                Cv2.CvtColor(frame, grayFrame, ColorConversionCodes.BGR2GRAY);
            else
                frame.CopyTo(grayFrame);

            for (int i = 0; i < BitsPerFrame; i++)
            {
                int row = i / BlocksPerRow;
                int col = i % BlocksPerRow;

                // Sample the direct center of the block to avoid edge compression artifacts
                int y = row * BlockSize + (BlockSize / 2);
                int x = col * BlockSize + (BlockSize / 2);

                byte pixelValue = grayFrame.At<byte>(y, x);

                // Threshold 128 (0 is black, 255 is white)
                bits[i] = pixelValue > 128;
            }

            return bits;
        }
    }
}
