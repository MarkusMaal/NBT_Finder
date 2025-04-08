namespace NBT_Finder
{
    class DeepSearch
    {

        /// <summary>
        /// List of files that match the criteria
        /// </summary>
        private static List<string> foundFiles = [];

        /// <summary>
        /// Number of scanned files
        /// </summary>
        private static long TotalScanned = 0;

        /// <summary>
        /// Number of active threads
        /// </summary>
        private static int CurThreads = 1;

        /// <summary>
        /// Tracked directory (created from another thread)
        /// </summary>
        private static string CurrentDir = "";

        /// <summary>
        /// Tracked filename (created from another thread)
        /// </summary>
        private static string CurrentFile = "";

        /// <summary>
        /// Tracked recursion depth (created from another thread)
        /// </summary>
        private static int Stack = 0;

        /// <summary>
        /// Tracked CPU usage (created from another thread)
        /// </summary>
        private static int Cpu = 0;

        /// <summary>
        /// Width of the command prompt window
        /// </summary>
        private static int bufferWidth = Console.BufferWidth;

        /// <summary>
        /// Search for NBT data on all drives
        /// </summary>
        public static void WalkTrees(string root = "")
        {
            Console.WriteLine("Initializing...");
            Utils.MakeDirs();
            if (root == "")
            {
                foreach (DriveInfo d in DriveInfo.GetDrives())
                {
                    if (!d.IsReady) continue;
                    if (d.DriveType == DriveType.Network) continue; // avoid network drives, because we can't determine symlinks easily
                    new Thread(() =>
                    {
                        CurThreads++;
                        RecurseTree(d.RootDirectory.FullName);
                        CurThreads--;
                    }
                    ).Start();
                }
            } else
            {
                new Thread(() =>
                {
                    CurThreads++;
                    RecurseTree(root);
                    CurThreads--;
                }).Start();
            }
            Thread CLThread = new(CLIThread);
            CLThread.Start();
            new Thread(() =>
            {
                while (CurThreads > 3)
                {
                    var cpuUsage = Utils.GetCpuUsageForProcess().Result;
                    Cpu = (int)cpuUsage;
                    Thread.Sleep(100);
                }
            }).Start();
            CLThread.Join();
            using StreamWriter sw = new("foundfiles.log");
            foreach (string foundFile in foundFiles)
            {
                sw.WriteLine(foundFile);
            }
            sw.Close();
            Console.Clear();
            Console.WriteLine("Finished!");
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey();
            Environment.Exit(0);
        }

        /// <summary>
        /// Thread for displaying information during the search process
        /// </summary>
        private static void CLIThread()
        {
            Console.Clear();
            CurThreads++;
            while (CurThreads > 2)
            {
                if (Console.BufferWidth !=  bufferWidth)
                {
                    bufferWidth = Console.BufferWidth;
                    Console.Clear();
                }
                string root_friendly = $"CPU: {Cpu}%".PadLeft(9, ' ');
                if (root_friendly.Length > Console.BufferWidth / 2)
                {
                    root_friendly = root_friendly[..(Console.BufferWidth / 2)];
                }
                Console.SetCursorPosition(Console.BufferWidth - root_friendly.Length, 0);
                Console.WriteLine(root_friendly);
                Console.SetCursorPosition(0, 2);
                Console.WriteLine("Current directory:");
                Console.SetCursorPosition(0, 6);
                Console.WriteLine("Current file:");
                Console.SetCursorPosition(0, 11);
                Console.WriteLine("Total files scanned:");
                Console.SetCursorPosition(0, 13);
                Console.WriteLine("Found files:");
                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"Stack usage: {Stack}%   ");
                Console.SetCursorPosition(21, 11);
                Console.Write(TotalScanned);
                Console.SetCursorPosition(13, 13);
                Console.Write(foundFiles.Count);
                SetPos(0, 3, CurrentDir.Length);
                Console.WriteLine($"{CurrentDir}");
                SetPos(0, 7, CurrentFile.Length);
                Console.WriteLine($"{CurrentFile}");
                Console.SetCursorPosition(0, 12);
                Console.WriteLine($"Threads: {CurThreads}   ");
                SetPos(0, 3, CurrentDir.Length);
                Console.WriteLine($"{CurrentDir}");
            }
            CurThreads--;
        }

        /// <summary>
        /// Recursive function for walking the filesystem tree
        /// </summary>
        /// <param name="path">Current path to search</param>
        /// <param name="depth">Current recursion depth (default: 0)</param>
        private static void RecurseTree(string path, int depth = 0)
        {
            // for emergencies only, helps avoid stack overflows...
            if (depth >= 100) { return; }
            Stack = depth;
            CurrentDir = path;
            DirectoryInfo thisDir = new(path);
            new Thread(() =>
            {
                CurThreads += 1;
                try
                {
                    foreach (FileInfo fi in thisDir.GetFiles())
                    {
                        if (fi.Length > 1048576) continue; // too big, don't waste time
                        //if (!(fi.Extension == ".dat" || fi.Extension == ".gz" || fi.Extension == ".nbt" || fi.Extension == ".dd")) continue; // file extension filters, uncomment for slightly better performance at the cost of potentially missing some files
                        CurrentFile = fi.FullName;

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
                        TotalScanned++;
                        string decompressed = Environment.CurrentDirectory + "/Temp/" + fi.Name + ".uncompressed";
                        if (!match) {
                            // not a Gzip archive
                            //File.Copy(fi.FullName, decompressed);
                            continue;
                        } else
                        {
                            Utils.Decompress(fi, decompressed);
                        }
                        using FileStream uncompressed_stream = new(decompressed, FileMode.Open, FileAccess.Read);
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
                        if (match)
                        {
                            foundFiles.Add(fi.FullName);
                            File.Copy(decompressed, Environment.CurrentDirectory + "/Recovered_NBT/" + foundFiles.Count + "_" + fi.Name.Replace(".gz", ""));
                        }
                        uncompressed_stream.Close();
                        File.Delete(decompressed);
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
                CurThreads -= 1;
            }).Start();
            try
            {
                foreach (DirectoryInfo di in thisDir.GetDirectories())
                {
                    // skip symlinks to avoid infinite loops
                    if (di.Attributes.HasFlag(FileAttributes.ReparsePoint) || (di.LinkTarget != null)) continue;
                    RecurseTree(di.FullName, depth + 1);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }
        }

        /// <summary>
        /// Set console caret position and pads a specific number of spaces depending on the length of the string you want to output
        /// </summary>
        /// <param name="left">X coordinate</param>
        /// <param name="top">Y coordinate</param>
        /// <param name="skipchars">Number of characters to skip for padding (usually the length of the string)</param>
        private static void SetPos(int left, int top, int skipchars = 0)
        {
            int skiplines = skipchars / Console.BufferWidth;
            if (skiplines > 0)
            {
                skipchars -= skiplines * Console.BufferWidth;
            }
            Console.SetCursorPosition(left + skipchars, top + skiplines);
            string empty = "";
            for (int i = 0; i < 255 - skipchars - (skiplines * Console.BufferWidth); i++)
            {
                empty += " ";
            }
            Console.WriteLine(empty);
            Console.SetCursorPosition(left, top);
        }
    }
}
