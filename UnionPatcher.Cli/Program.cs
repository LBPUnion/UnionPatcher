using System;
using System.Diagnostics;
using System.IO;

namespace LBPUnion.UnionPatcher.Cli; 

public static class Program {
    public const string Version = "1.0";

    private static string fileName;

    public static string FileName {
        get {
            if(fileName != null) return fileName;

            return fileName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName);
        }
    }

    public static void Main(string[] args) {
        if(args.Length < 3) {
            PrintHelp();
            return;
        }

        ElfFile eboot = new(new FileInfo(args[0]));

        if(!eboot.IsValid) {
            Console.WriteLine($"{eboot.Name} is not a valid ELF file (magic number mismatch)");
            Console.WriteLine("The EBOOT must be decrypted before using this tool");
            return;
        }

        if(eboot.Is64Bit == null) {
            Console.WriteLine($"{eboot.Name} does not target a valid system");
            return;
        }

        if(string.IsNullOrWhiteSpace(eboot.Architecture)) {
            Console.WriteLine($"{eboot.Name} does not target a valid architecture (PowerPC or ARM)");
            return;
        }

        Console.WriteLine($"{eboot.Name} targets {eboot.Architecture}");

        Patcher.PatchFile(args[0], args[1], args[2]);
        Console.WriteLine($"Successfully patched Server URL to {args[1]}.");
    }

    public static void PrintHelp() {
        Console.WriteLine($"UnionPatcher {Version}");
        Console.WriteLine($"    Usage: {FileName} <Input EBOOT.elf> <Server URL> <Output filename>");
    }
}