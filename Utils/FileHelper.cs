using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace VideoFileStorage.Utils
{
    public static class FileHelper
    {
        /// <summary>
        /// Packages the raw file data with necessary metadata (Magic number, Checksum, Filename, Size).
        /// </summary>
        public static byte[] CreatePayload(string filePath, byte[] fileData)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            // 1. Magic Header "VIDF" (4 bytes)
            bw.Write(Encoding.ASCII.GetBytes("VIDF"));

            // 2. Original File Size (8 bytes)
            bw.Write((long)fileData.Length);

            // 3. SHA256 Checksum (32 bytes)
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(fileData);
                bw.Write(hash);
            }

            // 4. Filename (Length prefixed)
            string fileName = Path.GetFileName(filePath);
            byte[] nameBytes = Encoding.UTF8.GetBytes(fileName);
            bw.Write(nameBytes.Length); // 4 bytes
            bw.Write(nameBytes);        // Variable bytes

            // 5. Actual File Data
            bw.Write(fileData);

            return ms.ToArray();
        }

        /// <summary>
        /// Extracts metadata, verifies checksum, and saves the file to the output folder.
        /// </summary>
        public static void ExtractPayload(byte[] fullData, string outputFolder)
        {
            using var ms = new MemoryStream(fullData);
            using var br = new BinaryReader(ms);

            // 1. Validate Magic Header
            byte[] magic = br.ReadBytes(4);
            string magicStr = Encoding.ASCII.GetString(magic);
            if (magicStr != "VIDF")
                throw new InvalidOperationException("Invalid video encoding format. Magic header missing.");

            // 2. Read File Size & Checksum
            long fileSize = br.ReadInt64();
            byte[] originalHash = br.ReadBytes(32);

            // 3. Read Filename
            int nameLen = br.ReadInt32();
            byte[] nameBytes = br.ReadBytes(nameLen);
            string fileName = Encoding.UTF8.GetString(nameBytes);

            // 4. Read File Data ensuring we don't read padding
            byte[] fileData = br.ReadBytes((int)fileSize);

            // 5. Verify Checksum
            using (var sha = SHA256.Create())
            {
                byte[] computedHash = sha.ComputeHash(fileData);
                if (!computedHash.SequenceEqual(originalHash))
                    throw new InvalidDataException("SHA-256 Checksum verification failed! The video may be corrupted.");
            }

            // 6. Save original file
            string outPath = Path.Combine(outputFolder, fileName);
            File.WriteAllBytes(outPath, fileData);
        }
    }
}
