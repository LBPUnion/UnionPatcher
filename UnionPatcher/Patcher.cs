using System;
using System.IO;
using System.Text;

namespace LBPUnion.UnionPatcher; 

public static class Patcher {
    private static readonly string[] toBePatched = {
        // Normal LittleBigPlanet gameserver URLs
        "https://littlebigplanetps3.online.scee.com:10061/LITTLEBIGPLANETPS3_XML",
        "http://littlebigplanetps3.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
        // LittleBigPlanet 3 Presence URLs
        "http://live.littlebigplanetps3.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
        "http://presence.littlebigplanetps3.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
        #region Spinoff URLs
        // LittleBigPlanet PSP URLs
        "http://lbppsp.online.scee.com:10060/LITTLEBIGPLANETPSP_XML",
        "https://lbppsp.online.scee.com:10061/LITTLEBIGPLANETPSP_XML",
        // LittleBigPlanet Vita URLs
        "http://lbpvita.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
        "https://lbpvita.online.scee.com:10061/LITTLEBIGPLANETPS3_XML",
        #endregion
        #region Beta URLS
        // LittleBigPlanet 2 Beta URLs
        "http://lbp2ps3-beta.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
        "https://lbp2ps3-beta.online.scee.com:10061/LITTLEBIGPLANETPS3_XML",
        // LittleBigPlanet (3?) Beta URLs
        "http://littlebigplanetps3-beta.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
        "https://littlebigplanetps3-beta.online.scee.com:10061/LITTLEBIGPLANETPS3_XML",
        // LittleBigPlanet Vita Beta URLs
        "http://lbpvita-beta.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
        "https://lbpvita-beta.online.scee.com:10061/LITTLEBIGPLANETPS3_XML",
        #endregion
    };

    public static void PatchFile(string fileName, Uri serverUrl, string outputFileName) {
        PatchFile(fileName, serverUrl.ToString(), outputFileName);
    }

    public static void PatchFile(string fileName, string serverUrl, string outputFileName) {
        File.WriteAllBytes(outputFileName, PatchData(File.ReadAllBytes(fileName), serverUrl));
    }

    public static byte[] PatchData(byte[] data, Uri serverUrl) {
        return PatchData(data, serverUrl.ToString());
    }
        
    public static byte[] PatchData(byte[] data, string serverUrl) {
        #region Validation
        if(serverUrl.EndsWith('/')) {
            throw new ArgumentException("URL must not contain a trailing slash!");
        }

        // Attempt to create URI to see if it's valid
        if(!Uri.TryCreate(serverUrl, UriKind.RelativeOrAbsolute, out _)) {
            throw new Exception("URL must be valid.");
        }

        if(serverUrl.Length > data.Length) {
            throw new ArgumentException("URL cannot be bigger than the file to patch.");
        }
        #endregion
            
        string dataAsString = Encoding.ASCII.GetString(data);

        using MemoryStream ms = new(data);
        using BinaryWriter writer = new(ms);

        // Using writer.Write(string) writes the length as a byte beforehand
        // This is problematic because LBP (being written in C/C++) uses null-terminated strings,
        // as opposed to length-text written strings as C# likes to serialize.
        byte[] serverUrlAsBytes = Encoding.ASCII.GetBytes(serverUrl);

        bool wroteUrl = false;
        foreach(string url in toBePatched) {
            if(serverUrl.Length > url.Length) {
                throw new ArgumentOutOfRangeException(nameof(serverUrl), $"Server URL ({serverUrl.Length} characters long) is above maximum length {url.Length}");
            }
                
            int offset = dataAsString.IndexOf(url, StringComparison.Ordinal);
            if(offset < 1) continue;

            writer.BaseStream.Position = offset;
            for(int i = 0; i < url.Length; i++) {
                writer.Write((byte)0x00); // Zero out data
            }

            writer.BaseStream.Position = offset; // Reset position to beginning
            writer.Write(serverUrlAsBytes);

            wroteUrl = true;
        }

        if(!wroteUrl) {
            throw new Exception("No patchable URLs were detected in the " +
                                "provided file. Please make sure you are patching " +
                                "the correct file.");
        }

        writer.Flush();
        writer.Close();

        return data;
    }
}