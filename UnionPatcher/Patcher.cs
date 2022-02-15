using System;
using System.IO;
using System.Text;

namespace LBPUnion.UnionPatcher {
    public static class Patcher {
        private static readonly string[] ToBePatched = {
            "https://littlebigplanetps3.online.scee.com:10061/LITTLEBIGPLANETPS3_XML",
            "http://littlebigplanetps3.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
            // LittleBigPlanet 3-Specific URLs
            "http://live.littlebigplanetps3.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
            "http://presence.littlebigplanetps3.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
            // LittleBigPlanet PSP URLs
            "http://lbppsp.online.scee.com:10060/LITTLEBIGPLANETPSP_XML",
            "https://lbppsp.online.scee.com:10061/LITTLEBIGPLANETPSP_XML",
            // LittleBigPlanet 2 Beta URLs
            "http://lbp2ps3-beta.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
            "https://lbp2ps3-beta.online.scee.com:10061/LITTLEBIGPLANETPS3_XML",
            // LittleBigPlanet (3?) Beta URLs
            "http://littlebigplanetps3-beta.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
            "https://littlebigplanetps3-beta.online.scee.com:10061/LITTLEBIGPLANETPS3_XML",
            // LittleBigPlanet Vita URLs
            "http://lbpvita.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
            "https://lbpvita.online.scee.com:10061/LITTLEBIGPLANETPS3_XML",
            // LittleBigPlanet Vita Beta URLs
            "http://lbpvita-beta.online.scee.com:10060/LITTLEBIGPLANETPS3_XML",
            "https://lbpvita-beta.online.scee.com:10061/LITTLEBIGPLANETPS3_XML",
        };
        
        public static void PatchFile(string fileName, string serverUrl, string outputFileName) {
            File.WriteAllBytes(outputFileName, PatchData(File.ReadAllBytes(fileName), serverUrl));
        }
        
        public static byte[] PatchData(byte[] data, string serverUrl) {
            Console.WriteLine("NOTICE: As Union Patcher is still a work in progress, the patching method is not refined. " +
                              "Due to this, it may show that URLS are not found. These can be safely ignored." +
                              "Please check the EBOOT manually in a hex editor to make sure your specified url has been" +
                              "added properly. Thank you");
            if(serverUrl.EndsWith('/')) {
                throw new ArgumentException("URL must not contain a trailing slash!");
            }
            
            string dataAsString = Encoding.ASCII.GetString(data);

            using MemoryStream ms = new(data);
            using BinaryWriter writer = new(ms);

            // using writer.Write(string) writes the length as a byte beforehand which is problematic because
            // LBP uses null-terminated strings, not length-defined strings
            byte[] serverUrlAsBytes = Encoding.ASCII.GetBytes(serverUrl);
            
            foreach(string url in ToBePatched) {
                if(serverUrl.Length > url.Length) {
                    throw new ArgumentOutOfRangeException(nameof(serverUrl), $"Server URL ({serverUrl.Length} characters long) is above maximum length {url.Length}");
                }
                
                int offset = dataAsString.IndexOf(url, StringComparison.Ordinal);
                if(offset < 1) {
                    Console.WriteLine($"WARNING: URL {url} not found!");
                    continue;
                }

                writer.BaseStream.Position = offset;
                for(int i = 0; i < url.Length; i++) {
                    writer.Write((byte)0x00); // Zero out data
                }

                writer.BaseStream.Position = offset; // Reset position to beginning
                writer.Write(serverUrlAsBytes);
            }

            writer.Flush();
            writer.Close();

            return data;
        }
    }
}
