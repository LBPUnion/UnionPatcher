#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

#pragma warning disable SYSLIB0014 // the FtpWebRequest is needed in this case

namespace LBPUnion.UnionPatcher.Communication;
public static class FTP
{
    public static bool FileExists(string url, string user, string pass)
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);

        request.Credentials = new NetworkCredential(user, pass);
        request.Method = WebRequestMethods.Ftp.GetDateTimestamp;

        try
        {
            Console.Write($"FTP: Checking if file {url} exists... ");
            request.GetResponse();
        }
        catch (WebException ex)
        {
            FtpWebResponse? response = (FtpWebResponse?)ex.Response;
            if (response == null || response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                Console.WriteLine("No");
                return false;
            }
        }

        Console.WriteLine("Yes");
        return true;
    }

    public static string[] ListDirectory(string url, string user, string pass)
    {

        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
        request.Credentials = new NetworkCredential(user, pass);
        request.Method = WebRequestMethods.Ftp.ListDirectory;

        try
        {
            Console.WriteLine($"FTP: Listing directory {url}");

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            string names = new StreamReader(response.GetResponseStream()).ReadToEnd();

            List<string> dirs = names
                .Split("\r\n")
                .Where(dir => !string.IsNullOrWhiteSpace(dir) && dir != "." && dir != "..")
                .ToList();

            foreach (string dir in dirs.ToArray())
            {
                Console.WriteLine($"/{dir}");
            }

            Console.WriteLine("");

            return dirs.ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static void UploadFile(string source, string destination, string user, string pass)
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(destination);
        request.Credentials = new NetworkCredential(user, pass);
        request.Method = WebRequestMethods.Ftp.UploadFile;

        byte[] fileContents = File.ReadAllBytes(source);

        request.ContentLength = fileContents.Length;

        try
        {
            Console.WriteLine($"FTP: Uploading file {source} to {destination}");

            using Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
        }
        catch
        {
            throw new WebException("Could not upload file");
        }

    }

    public static void DownloadFile(string source, string destination, string user, string pass)
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(source);
        request.Credentials = new NetworkCredential(user, pass);
        request.Method = WebRequestMethods.Ftp.DownloadFile;

        try
        {
            Console.WriteLine($"FTP: Downloading file {source} to {destination}");

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();

            using Stream s = File.Create(destination);
            responseStream.CopyTo(s);
        }
        catch
        {
            throw new WebException("Could not download file");
        }
    }

    public static string ReadFile(string url, string user, string pass)
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
        request.Credentials = new NetworkCredential(user, pass);
        request.Method = WebRequestMethods.Ftp.DownloadFile;

        try
        {
            Console.WriteLine($"FTP: Reading file {url}");

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();

            using StreamReader reader = new(responseStream);
            return reader.ReadToEnd();

        }
        catch
        {
            return "<Error Downloading File>";
        }
    }
}