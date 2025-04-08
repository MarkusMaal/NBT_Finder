using System.Diagnostics;
using System.IO.Compression;

namespace NBT_Finder
{
    class Utils
    {
        /// <summary>
        /// Decompresses a Gzip archive
        /// </summary>
        /// <param name="fileToDecompress">The file that we need to decompress</param>
        /// <param name="newName">Full path to decompressed file that we want to create</param>
        public static void Decompress(FileInfo fileToDecompress, string newName)
        {
            using FileStream originalFileStream = fileToDecompress.OpenRead();
            string currentFileName = fileToDecompress.FullName;

            using FileStream decompressedFileStream = File.Create(newName);
            using GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress);
            try
            {
                decompressionStream.CopyTo(decompressedFileStream);
            }
            catch{ /*Console.WriteLine("Failed to extract: {0}");*/ }
            decompressionStream.Close();
            decompressedFileStream.Close();
            originalFileStream.Close();
        }

        /// <summary>
        /// Creates directories for recovery purposes
        /// </summary>
        public static void MakeDirs()
        {
            string[] makedirs = ["Temp", "Recovered_NBT"];
            foreach (string makedir in makedirs)
            {
                if (Directory.Exists(Environment.CurrentDirectory + "/" + makedir)) continue;
                Directory.CreateDirectory(Environment.CurrentDirectory + "/" + makedir);
            }
        }

        /// <summary>
        /// Find current CPU usage for this process
        /// </summary>
        /// <returns>Task, which returns current CPU usage percentage as double</returns>
        public static async Task<double> GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime; await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime; var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds; var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed); return cpuUsageTotal * 100;
        }
    }
}
