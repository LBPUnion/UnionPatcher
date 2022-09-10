using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LBPUnion.UnionPatcher; 

public static class Patcher {

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

        // Find a string including http or https and LITTLEBIGPLANETPS3_XML or LITTLEBIGPLANETPSP_XML, 
        // then match any additional NULL characters to dynamically gague the maximum length on a per-title basis 
        // without a hardcoded array of known server URLs
        MatchCollection urls = Regex.Matches(dataAsString, "http?[^\x00]*?LITTLEBIGPLANETPS(3|P)_XML\x00*");
        foreach(Match urlMatch in urls) {
            string url = urlMatch.Value;

            if(serverUrl.Length > url.Length) {
                throw new ArgumentOutOfRangeException(nameof(serverUrl), $"Server URL ({serverUrl.Length} characters long) is above maximum length {url.Length}");
            }
            int offset = urlMatch.Index;

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