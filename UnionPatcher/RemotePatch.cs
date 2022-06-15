using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using LBPUnion.UnionPatcher.Communication;

namespace LBPUnion.UnionPatcher;

public class RemotePatch
{
    private readonly PS3MAPI _ps3Mapi = new();

    private static Dictionary<string, string> GetUsers(string ps3Ip, string user, string pass)
    {
        Console.WriteLine("Getting users...");

        Dictionary<string, string> users = new();

        string[] userFolders = FTP.ListDirectory($"ftp://{ps3Ip}/dev_hdd0/home/", user, pass);

        string username = "";

        for (int i = 0; i < userFolders.Length; i++)
        {
            username = FTP.ReadFile($"ftp://{ps3Ip}/dev_hdd0/home/{userFolders[i]}/localusername", user,
                pass);
            users.Add(userFolders[i], username);

            Console.WriteLine("User found: " + username + $" <{userFolders[i]}>");
        }

        return users;
    }
    
    public static void LaunchSCETool(string args)
    {
        string platformExecutable = "";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            platformExecutable = "scetool/win64/scetool.exe";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            platformExecutable = "scetool/linux64/scetool";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) platformExecutable = "";

        if (platformExecutable != "")
        {
            ProcessStartInfo startInfo = new();
            startInfo.UseShellExecute = false;
            startInfo.FileName = Path.GetFullPath(platformExecutable);
            startInfo.WorkingDirectory = Path.GetFullPath(".");
            startInfo.Arguments = args;
            startInfo.RedirectStandardOutput = true;

            Console.WriteLine("\n\n===== START SCETOOL =====\n");
            using (Process proc = Process.Start(startInfo))
            {
                while (!proc.StandardOutput.EndOfStream) Console.WriteLine(proc.StandardOutput.ReadLine());
                proc.WaitForExit();
            }

            Console.WriteLine("\n===== END SCETOOL =====\n\n");
        }
        else
        {
            throw new Exception("Error starting SCETool. Your platform may not be supported yet.");
        }
    }

    public void RevertEBOOT(string ps3ip, string gameID, string serverURL, string user, string pass)
    {
        Console.WriteLine("Restoring original EBOOT.BIN from EBOOT.BIN.BAK");
        
        // Create a simple directory structure
        Directory.CreateDirectory(@"eboot");
        Directory.CreateDirectory($@"eboot/{gameID}");
        Directory.CreateDirectory($@"eboot/{gameID}/original");
        
        // Now we'll check and see if a backup exists on the server, if so download it and then upload it back as EBOOT.BIN
        if (FTP.FileExists($"ftp://{ps3ip}/dev_hdd0/game/{gameID}/USRDIR/EBOOT.BIN.BAK", user, pass))
        {
            FTP.DownloadFile($"ftp://{ps3ip}/dev_hdd0/game/{gameID}/USRDIR/EBOOT.BIN.BAK", @$"eboot/{gameID}/original/EBOOT.BIN.BAK", user, pass);
            FTP.UploadFile(@$"eboot/{gameID}/original/EBOOT.BIN.BAK", $"ftp://{ps3ip}/dev_hdd0/game/{gameID}/USRDIR/EBOOT.BIN", user, pass);
        }
        else
        {
            throw new WebException("Could not find EBOOT.BIN.BAK on server.");
        }
    }

    public void PSNEBOOTRemotePatch(string ps3ip, string gameID, string serverURL, string user, string pass)
    {
        Console.WriteLine("Detected Digital Copy - Running in Full Mode");
        
        string idps = "";
        string contentID = "";
        Dictionary<string, string> users;

        this._ps3Mapi.ConnectTarget(ps3ip);
        this._ps3Mapi.PS3.RingBuzzer(PS3MAPI.PS3Cmd.BuzzerMode.Double);
        this._ps3Mapi.PS3.Notify("UnionRemotePatcher Connected! Patching...");

        // Create simple directory structure
        Directory.CreateDirectory(@"rifs");
        Directory.CreateDirectory(@"eboot");
        Directory.CreateDirectory($@"eboot/{gameID}");
        Directory.CreateDirectory($@"eboot/{gameID}/original");
        Directory.CreateDirectory($@"eboot/{gameID}/patched");

        // Let's grab and backup our EBOOT
        FTP.DownloadFile($"ftp://{ps3ip}/dev_hdd0/game/{gameID}/USRDIR/EBOOT.BIN",
            @$"eboot/{gameID}/original/EBOOT.BIN", user, pass);

        // Now we'll check and see if a backup exists on the server or not, if we don't have one on the server, then upload one
        if (!FTP.FileExists($"ftp://{ps3ip}/dev_hdd0/game/{gameID}/USRDIR/EBOOT.BIN.BAK", user, pass))
            FTP.UploadFile(@$"eboot/{gameID}/original/EBOOT.BIN",
                $"ftp://{ps3ip}/dev_hdd0/game/{gameID}/USRDIR/EBOOT.BIN.BAK", user, pass);

        // Start getting idps and act.dat - these will help us decrypt a PSN eboot
        idps = PS3MAPI.PS3MAPIClientServer.PS3_GetIDPS();

        File.WriteAllBytes(@"data/idps", IDPSHelper.StringToByteArray(idps));

        // Scan the users on the system
        users = GetUsers(ps3ip, user, pass);

        // Scan the system for a license for the game
        foreach (string currentUser in users.Keys.ToArray())
        {
            if (FTP.FileExists($"ftp://{ps3ip}/dev_hdd0/home/{currentUser}/exdata", user, pass))
            {
                foreach (string fileName in FTP.ListDirectory(
                             $"ftp://{ps3ip}/dev_hdd0/home/{currentUser}/exdata/", user, pass))
                    if (fileName.Contains(gameID))
                    {
                        FTP.DownloadFile($"ftp://{ps3ip}/dev_hdd0/home/{currentUser}/exdata/act.dat", @"data/act.dat",
                            user,
                            pass);
                        
                        FTP.DownloadFile($"ftp://{ps3ip}/dev_hdd0/home/{currentUser}/exdata/{fileName}",
                            @$"rifs/{fileName}", user, pass);
                        
                        contentID = fileName.Substring(0, fileName.Length - 4);
                        
                        Console.WriteLine($"Got content ID {contentID}");
                    }
            }
        }

        // Finally, let's decrypt the EBOOT.BIN
        LaunchSCETool($" -v -d \"{Path.GetFullPath(@$"eboot/{gameID}/original/EBOOT.BIN")}\" \"{Path.GetFullPath(@$"eboot/{gameID}/original/EBOOT.ELF")}\"");

        // Now, patch the EBOOT;
        Patcher.PatchFile($"eboot/{gameID}/original/EBOOT.ELF", serverURL, $"eboot/{gameID}/patched/EBOOT.ELF");

        // Encrypt the EBOOT (PSN)
        LaunchSCETool($"--verbose " +
                      $"--sce-type=SELF" +
                      $" --skip-sections=FALSE" +
                      $" --self-add-shdrs=TRUE" +
                      $" --compress-data=TRUE" +
                      $" --key-revision=0A" +
                      $" --self-app-version=0001000000000000" +
                      $" --self-auth-id=1010000001000003" +
                      $" --self-vendor-id=01000002" +
                      $" --self-ctrl-flags=0000000000000000000000000000000000000000000000000000000000000000" +
                      $" --self-cap-flags=00000000000000000000000000000000000000000000003B0000000100040000" +
                      $" --self-type=NPDRM" +
                      $" --self-fw-version=0003005500000000" +
                      $" --np-license-type=FREE" +
                      $" --np-app-type=SPRX" +
                      $" --np-content-id={contentID}" +
                      $" --np-real-fname=EBOOT.BIN" +
                      $" --encrypt eboot/{gameID}/patched/EBOOT.ELF eboot/{gameID}/patched/EBOOT.BIN");

        // And upload the encrypted, patched EBOOT to the system.
        FTP.UploadFile(@$"eboot/{gameID}/patched/EBOOT.BIN",
            $"ftp://{ps3ip}/dev_hdd0/game/{gameID}/USRDIR/EBOOT.BIN", user, pass);
    }

    // Cut-down version that only patches disc copies
    public void DiscEBOOTRemotePatch(string ps3ip, string gameID, string serverURL, string user, string pass)
    {
        Console.WriteLine("Detected Disc Copy - Running in Simplified Mode");
        
        // Create a simple directory structure
        Directory.CreateDirectory(@"eboot");
        Directory.CreateDirectory($@"eboot/{gameID}");
        Directory.CreateDirectory($@"eboot/{gameID}/original");
        Directory.CreateDirectory($@"eboot/{gameID}/patched");

        // Let's grab and backup our EBOOT
        FTP.DownloadFile($"ftp://{ps3ip}/dev_hdd0/game/{gameID}/USRDIR/EBOOT.BIN",
            @$"eboot/{gameID}/original/EBOOT.BIN", user, pass);

        // Now we'll check and see if a backup exists on the server or not, if we don't have one on the server, then upload one
        if (!FTP.FileExists($"ftp://{ps3ip}/dev_hdd0/game/{gameID}/USRDIR/EBOOT.BIN.BAK", user, pass))
            FTP.UploadFile(@$"eboot/{gameID}/original/EBOOT.BIN",
                $"ftp://{ps3ip}/dev_hdd0/game/{gameID}/USRDIR/EBOOT.BIN.BAK", user, pass);

        // Check for keys in the data directory
        if (!File.Exists("data/keys"))
            throw new FileNotFoundException(
                "UnionRemotePatcher cannot find the keys, ldr_curves, or vsh_curves files required to continue. Please make sure you have copies of these files placed in the data directory where you found the executable to run UnionRemotePatcher. Without them, we can't patch your game.");

        // Decrypt the EBOOT
        LaunchSCETool($"-v -d eboot/{gameID}/original/EBOOT.BIN eboot/{gameID}/original/EBOOT.ELF");

        // Now, patch the EBOOT;
        Patcher.PatchFile($"eboot/{gameID}/original/EBOOT.ELF", serverURL, $"eboot/{gameID}/patched/EBOOT.ELF");

        // Encrypt the EBOOT (Disc)
        LaunchSCETool(
            $" -v --sce-type=SELF --skip-sections=FALSE --key-revision=0A --self-app-version=0001000000000000 --self-auth-id=1010000001000003 --self-vendor-id=01000002 --self-ctrl-flags=0000000000000000000000000000000000000000000000000000000000000000 --self-cap-flags=00000000000000000000000000000000000000000000003B0000000100040000 --self-type=APP --self-fw-version=0003005500000000 --compress-data true --encrypt \"{Path.GetFullPath(@$"eboot/{gameID}/patched/EBOOT.ELF")}\" \"{Path.GetFullPath(@$"eboot/{gameID}/patched/EBOOT.BIN")}\"");

        // And upload the encrypted, patched EBOOT to the system.
        FTP.UploadFile(@$"eboot/{gameID}/patched/EBOOT.BIN",
            $"ftp://{ps3ip}/dev_hdd0/game/{gameID}/USRDIR/EBOOT.BIN", user, pass);
    }
}