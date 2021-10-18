using System;
using System.Diagnostics;
using System.IO;

namespace UnionPatcher {
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
            
            Patcher.PatchFile(args[0], args[1], args[2]);
            Console.WriteLine($"Successfully patched Server URL to {args[1]}.");
        }

        public static void PrintHelp() {
            Console.WriteLine($"UnionPatcher {Version}");
            Console.WriteLine($"    Usage: {FileName} <Input EBOOT.elf> <Server URL> <Output filename>");
        }
    }
}