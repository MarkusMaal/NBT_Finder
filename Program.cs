using System.Diagnostics;
using System.Text;

namespace NBT_Finder;

public class Program

{
    /// <summary>
    /// Gzip magic (first bytes that define an .gz file)
    /// </summary>
    public static readonly byte[] Magic = [0x1F, 0x8B, 0x08];

    /// <summary>
    /// Bytes to search for inside the uncompressed .gz file that might define a Minecraft .nbt file (UTF-8 representation of string "minecraft")
    /// </summary>
    public static readonly byte[] Keyword = Encoding.UTF8.GetBytes("minecraft");
    //public static readonly byte[] Keyword = [0x6D, 0x69, 0x6E, 0x65, 0x63, 0x72, 0x61, 0x66, 0x74];
    //private static readonly byte[] Keyword = [0x0A, 0x00, 0x00, 0x0A, 0x00, 0x04, 0x44, 0x61, 0x74, 0x61, 0x01, 0x00, 0x0A, 0x44, 0x69, 0x66];
    [STAThread]
    public static void Main(string[] args)
    {
        Console.WriteLine("NBT search tool");
        if (Debugger.IsAttached)
        {
            Console.WriteLine("\nYou are running this program through a debugger.\nPlease note that for best performance,\nyou should run this program without debugging.");
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey();
            Console.WriteLine();
        }
        DeepSearch.WalkTrees((args.Length == 0) ? "" : args[0]);
        /*RecupDirFinder.FindMcData(args[0]);*/
    }
}
