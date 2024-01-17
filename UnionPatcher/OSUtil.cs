using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LBPUnion.UnionPatcher
{
    using RuntimeOSPlatform = System.Runtime.InteropServices.OSPlatform;

    public enum OSPlatform
    {
        NotSupported,
        Windows,
        OSX,
        Linux,
    }
    public class OSUtil
    {
        private static IEnumerable
        <(OSPlatform Platform, RuntimeOSPlatform RuntimePlatform)?> EnumeratePlatforms()
        {
            yield return (OSPlatform.Windows, RuntimeOSPlatform.Windows);
            yield return (OSPlatform.OSX, RuntimeOSPlatform.OSX);
            yield return (OSPlatform.Linux, RuntimeOSPlatform.Linux);
        }

        public static OSPlatform GetPlatform()
        {
            return EnumeratePlatforms().FirstOrDefault(p
               => RuntimeInformation.IsOSPlatform(p.Value.RuntimePlatform))?.Platform ?? default;
        }

        public static string GetExecutablePath()
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrEmpty(path))
                path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
            if (string.IsNullOrEmpty(path))
                path = AppContext.BaseDirectory;
            
            return path;
        }
    }
}

