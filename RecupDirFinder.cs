using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBT_Finder
{
    class RecupDirFinder
    {

        /// <summary>
        /// Finds potential Minecraft world files using the recup.dir folder structure
        /// </summary>
        /// <param name="root">Root directory for PhotoRec recovery</param>
        public static void FindMcData(string root)
        {
            Utils.MakeDirs();
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
                    byte[] buffer = new byte[Program.Magic.Length];
                    fs.Read(buffer);
                    fs.Close();
                    bool match = true;
                    for (int i = 0; i < Program.Magic.Length; i++)
                    {
                        if (Program.Magic[i] != buffer[i])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        Utils.Decompress(fi, fi.Name.Split('.')[0] + ".dat");
                        File.Move(fi.Name.Split('.')[0] + ".dat", Environment.CurrentDirectory + "/Temp/" + fi.Name.Split('.')[0] + ".dat");
                        using (FileStream uncompressed_stream = new(Environment.CurrentDirectory + "/Temp/" + fi.Name.Split('.')[0] + ".dat", FileMode.Open, FileAccess.Read))
                        {
                            int offset = 0;
                            buffer = new byte[Program.Keyword.Length];
                            match = false;
                            while ((offset = uncompressed_stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                byte[] data = buffer;
                                match = true;
                                for (int i = 0; i < Program.Keyword.Length; i++)
                                {
                                    if (Program.Keyword[i] != data[i])
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
                            if (/*match*/ true)
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
    }
}
