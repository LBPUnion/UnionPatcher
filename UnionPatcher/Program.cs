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
            Patcher.PatchFile("EBOOT.elf", "https://lighthouse.lbpunion.com:10061/LITTLEBIGPLANETPS3_XML", "EBOOT.new.elf");
            if(args.Length < 3) {
                PrintHelp();
                return;
            }
            
        }

        public static void PrintHelp() {
            Console.WriteLine($"UnionPatcher {Version}");
            Console.WriteLine($"    Usage: {FileName} <Input EBOOT.elf> <Server URL> <Output filename>");
        }
    }
}