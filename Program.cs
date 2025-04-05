using System.IO.Compression;
using System.Text;

namespace NBT_Finder;

public class Program

{
    /// <summary>
    /// Gzip magic (first bytes that define an .gz file)
    /// </summary>
    private static readonly byte[] Magic = [0x1F, 0x8B, 0x08];

    /// <summary>
    /// Bytes to search for inside the uncompressed .gz file that might define a Minecraft .nbt file (UTF-8 representation of string "minecraft")
    /// </summary>
    //private static readonly byte[] Keyword = [0x6D, 0x69, 0x6E, 0x65, 0x63, 0x72, 0x61, 0x66, 0x74];
    private static readonly byte[] Keyword = [0x0A, 0x00, 0x00, 0x0A, 0x00, 0x04, 0x44, 0x61, 0x74, 0x61, 0x01, 0x00, 0x0A, 0x44, 0x69, 0x66];

    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("At least one argument is required");
            return;
        }
        string root = args[0];
        string[] makedirs = ["Temp", "Recovered_NBT"];
        foreach (string makedir in makedirs)
        {
            if (Directory.Exists(Environment.CurrentDirectory + "/" + makedir)) continue;
            Directory.CreateDirectory(Environment.CurrentDirectory + "/" + makedir);
        }
        Console.WriteLine($"Root directory: {root}");
        DirectoryInfo di = new(root);
        Console.WriteLine("Scan started (copying found files to Recovered_NBT)");
        foreach (DirectoryInfo sdi in di.GetDirectories())
        {
            if (!sdi.Name.StartsWith("recup_dir.")) continue;
            foreach (FileInfo fi in sdi.GetFiles())
            {
                if (fi.Name == "report.xml") continue;
                using FileStream fs = new(fi.FullName, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[Magic.Length];
                fs.Read(buffer);
                fs.Close();
                bool match = true;
                for (int i = 0; i < Magic.Length; i++)
                {
                    if (Magic[i] != buffer[i])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    Decompress(fi, fi.Name.Split('.')[0] + ".dat");
                    File.Move(fi.Name.Split('.')[0] + ".dat", Environment.CurrentDirectory + "/Temp/" + fi.Name.Split('.')[0] + ".dat");
                    using (FileStream uncompressed_stream = new(Environment.CurrentDirectory + "/Temp/" + fi.Name.Split('.')[0] + ".dat", FileMode.Open, FileAccess.Read))
                    {
                        int offset = 0;
                        buffer = new byte[Keyword.Length];
                        match = false;
                        while ((offset = uncompressed_stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            byte[] data = buffer;
                            match = true;
                            for (int i = 0; i < Magic.Length; i++)
                            {
                                if (Keyword[i] != data[i])
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                            {
                                break;
                            }
                        }
                        if (match)
                        {
                            Console.WriteLine("Match found! Saving as " + Environment.CurrentDirectory + "/Recovered_NBT/" + fi.Name.Split('.')[0] + ".nbt");
                            File.Copy(Environment.CurrentDirectory + "/Temp/" + fi.Name.Split('.')[0] + ".dat", Environment.CurrentDirectory + "/Recovered_NBT/" + fi.Name.Split('.')[0] + ".nbt");
                        }
                        uncompressed_stream.Close();
                    }
                    File.Delete(Environment.CurrentDirectory + "/Temp/" + fi.Name.Split('.')[0] + ".dat");
                }
            }
        }
    }

    public static void Decompress(FileInfo fileToDecompress, string newName)
    {
        using (FileStream originalFileStream = fileToDecompress.OpenRead())
        {
            string currentFileName = fileToDecompress.FullName;

            using (FileStream decompressedFileStream = File.Create(newName))
            {
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                {
                    try
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    } catch
                    {
                        Console.WriteLine("Failed to extract: {0}");
                    }
                }
            }
        }
    }
}
