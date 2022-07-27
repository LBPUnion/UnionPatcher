/*PS3 MANAGER API
 * Copyright (c) 2014-2015 _NzV_.
 *
 * This code is write by _NzV_ <donm7v@gmail.com>.
 * It may be used for any purpose as long as this notice remains intact on all
 * source code distributions.
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace LBPUnion.UnionPatcher.Communication;

public class PS3MAPI
{
    public readonly PS3Cmd PS3 = new();
    //public VSH_PLUGINS_CMD VSH_Plugin = new VSH_PLUGINS_CMD();

    public PS3MAPI()
    {
        this.PS3 = new PS3Cmd();
    }

    #region PS3MAPI_Client

    /// <summary>
    /// Indicates if PS3MAPI is connected
    /// </summary>
    public bool IsConnected => PS3MAPIClientServer.IsConnected;

    /// <summary>Connect the target with "ConnectDialog".</summary>
    /// <param name="ip">Ip</param>
    /// <param name="port">Port</param>
    public void ConnectTarget(string ip, int port = 7887)
    {
        try
        {
            PS3MAPIClientServer.Connect(ip, port);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public class PS3Cmd
    {
        /// <summary>Get PS3 Firmware Version.</summary>
        public uint GetFirmwareVersion()
        {
            try
            {
                return PS3MAPIClientServer.PS3_GetFwVersion();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        /// <summary>Get PS3 Firmware Version.</summary>
        public string GetFirmwareVersion_Str()
        {
            string ver = PS3MAPIClientServer.PS3_GetFwVersion().ToString("X4");
            string char1 = ver.Substring(1, 1) + ".";
            string char2 = ver.Substring(2, 1);
            string char3 = ver.Substring(3, 1);
            return char1 + char2 + char3;
        }

        /// <summary>Get PS3 Firmware Type.</summary>
        public string GetFirmwareType()
        {
            try
            {
                return PS3MAPIClientServer.PS3_GetFirmwareType();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>PS3 VSH Notify.</summary>
        /// <param name="msg)">Your message</param>
        public void Notify(string msg)
        {
            try
            {
                PS3MAPIClientServer.PS3_Notify(msg);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public enum BuzzerMode
        {
            Single = 1,
            Double = 2,
            Triple = 3,
        }

        /// <summary>Ring PS3 Buzzer.</summary>
        /// <param name="mode">Simple, Double, Continuous</param>
        public void RingBuzzer(BuzzerMode mode)
        {
            try
            {
                PS3MAPIClientServer.PS3_Buzzer((int)mode);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

    }

    #endregion PS3MAPI_Client

    #region PS3MAPI_Client_Server
    internal class PS3MAPIClientServer
    {
        #region Private Members

        static private int ps3m_api_server_minversion = 0x0120;
        static private PS3MAPI_ResponseCode eResponseCode;
        static private string sResponse;
        static private string sMessages = "";
        static private string sServerIP = "";
        static private int iPort = 7887;
        static private string sBucket = "";
        static private int iTimeout = 5000;	// 5 Second
        static private uint iPid = 0;
        static private uint[] iprocesses_pid = new uint[16];
        static private int[] imodules_prx_id = new int[64];
        static private string sLog = "";
        #endregion Private Members

        #region Internal Members

        static internal Socket main_sock;
        static internal Socket listening_sock;
        static internal Socket data_sock;
        static internal IPEndPoint main_ipEndPoint;
        static internal IPEndPoint data_ipEndPoint;
        internal enum PS3MAPI_ResponseCode
        {
            DataConnectionAlreadyOpen = 125,
            MemoryStatusOK = 150,
            CommandOK = 200,
            RequestSuccessful = 226,
            EnteringPassiveMode = 227,
            PS3MAPIConnected = 220,
            PS3MAPIConnectedOK = 230,
            MemoryActionCompleted = 250,
            MemoryActionPended = 350
        }

        #endregion Internal Members

        #region Public Properties

        /// <summary>
        /// Return all process_pid
        /// </summary>
        static public string Log => sLog;

        /// <summary>
        /// Return all process_pid
        /// </summary>
        static public uint[] Processes_Pid => iprocesses_pid;

        /// <summary>
        /// Attached process_pid
        /// </summary>
        static public uint Process_Pid
        {
            get => iPid;
            set => iPid = value;
        }

        /// <summary>
        /// Return all modules_prx_id
        /// </summary>
        static public int[] Modules_Prx_Id => imodules_prx_id;

        /// <summary>
        /// User Specified Timeout: Defaults to 5000 (5 seconds)
        /// </summary>
        static public int Timeout
        {
            get => iTimeout;
            set => iTimeout = value;
        }

        /// <summary>
        /// Indicates if PS3MAPI is connected
        /// </summary>
        static public bool IsConnected => ((main_sock != null) ? main_sock.Connected : false);

        /// <summary>
        /// Indicates if PS3MAPI is attached
        /// </summary>
        static public bool IsAttached => ((iPid != 0) ? true : false);

        #endregion Public Properties

        //SERVER---------------------------------------------------------------------------------
        internal static void Connect()
        {
            Connect(sServerIP, iPort);
        }
        internal static void Connect(string sServer, int Port)
        {
            sServerIP = sServer;
            iPort = Port;
            if (Port.ToString().Length == 0)
            {
                throw new Exception("Unable to Connect - No Port Specified.");
            }
            if (sServerIP.Length == 0)
            {
                throw new Exception("Unable to Connect - No Server Specified.");
            }
            if (main_sock != null)
            {
                if (main_sock.Connected)
                {
                    return;
                }
            }
            main_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            main_ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(sServerIP)[0], Port);
            try
            {
                main_sock.Connect(main_ipEndPoint);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            ReadResponse();
            if (eResponseCode != PS3MAPI_ResponseCode.PS3MAPIConnected)
            {
                Fail();
            }
            ReadResponse();
            if (eResponseCode != PS3MAPI_ResponseCode.PS3MAPIConnectedOK)
            {
                Fail();
            }
            if (Server_GetMinVersion() < ps3m_api_server_minversion)
            {
                Disconnect();
                throw new Exception("PS3M_API SERVER (webMAN-MOD) OUTDATED! PLEASE UPDATE.");
            }
            else if (Server_GetMinVersion() > ps3m_api_server_minversion)
            {
                Disconnect();
                throw new Exception("PS3M_API PC_LIB (PS3ManagerAPI.dll) OUTDATED! PLEASE UPDATE.");
            }
            return;
        }
        internal static void Disconnect()
        {
            CloseDataSocket();
            if (main_sock != null)
            {
                if (main_sock.Connected)
                {
                    SendCommand("DISCONNECT");
                    iPid = 0;
                    main_sock.Close();
                }
                main_sock = null;
            }
            main_ipEndPoint = null;
        }
        internal static uint Server_Get_Version()
        {
            if (IsConnected)
            {
                SendCommand("SERVER GETVERSION");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                return Convert.ToUInt32(sResponse);
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static uint Server_GetMinVersion()
        {
            if (IsConnected)
            {
                SendCommand("SERVER GETMINVERSION");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                return Convert.ToUInt32(sResponse);
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        //CORE-----------------------------------------------------------------------------------
        internal static uint Core_Get_Version()
        {
            if (IsConnected)
            {
                SendCommand("CORE GETVERSION");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                return Convert.ToUInt32(sResponse);
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static uint Core_GetMinVersion()
        {
            if (IsConnected)
            {
                SendCommand("CORE GETMINVERSION");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                return Convert.ToUInt32(sResponse);
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        //PS3------------------------------------------------------------------------------------
        internal static uint PS3_GetFwVersion()
        {
            if (IsConnected)
            {
                SendCommand("PS3 GETFWVERSION");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                return Convert.ToUInt32(sResponse);
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static string PS3_GetFirmwareType()
        {
            if (IsConnected)
            {
                SendCommand("PS3 GETFWTYPE");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                return sResponse;
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void PS3_Shutdown()
        {
            if (IsConnected)
            {
                SendCommand("PS3 SHUTDOWN");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        Disconnect();
                        break;
                    default:
                        Fail();
                        break;
                }

            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void PS3_Reboot()
        {
            if (IsConnected)
            {
                SendCommand("PS3 REBOOT");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        Disconnect();
                        break;
                    default:
                        Fail();
                        break;
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void PS3_Notify(string msg)
        {
            if (IsConnected)
            {
                SendCommand("PS3 NOTIFY  " + msg);
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void PS3_Buzzer(int mode)
        {
            if (IsConnected)
            {
                SendCommand("PS3 BUZZER" + mode);
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void PS3_Led(int color, int mode)
        {
            if (IsConnected)
            {
                SendCommand("PS3 LED " + color + " " + mode);
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void PS3_GetTemp(out uint cpu, out uint rsx)
        {
            cpu = 0; rsx = 0;
            if (IsConnected)
            {
                SendCommand("PS3 GETTEMP");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                string[] tmp = sResponse.Split(new char[] { '|' });
                cpu = System.Convert.ToUInt32(tmp[0], 10);
                rsx = System.Convert.ToUInt32(tmp[1], 10);
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void PS3_DisableSyscall(int num)
        {
            if (IsConnected)
            {
                SendCommand("PS3 DISABLESYSCALL " + num);
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void PS3_ClearHistory(bool include_directory)
        {
            if (IsConnected)
            {
                if (include_directory) SendCommand("PS3 DELHISTORY+D");
                else SendCommand("PS3 DELHISTORY");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static bool PS3_CheckSyscall(int num)
        {
            if (IsConnected)
            {
                SendCommand("PS3 CHECKSYSCALL " + num);
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                if (Convert.ToInt32(sResponse) == 0) return true;
                else return false;
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void PS3_PartialDisableSyscall8(int mode)
        {
            if (IsConnected)
            {
                SendCommand("PS3 PDISABLESYSCALL8 " + mode);
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static int PS3_PartialCheckSyscall8()
        {
            if (IsConnected)
            {
                SendCommand("PS3 PCHECKSYSCALL8");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                return Convert.ToInt32(sResponse);
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void PS3_RemoveHook()
        {
            if (IsConnected)
            {
                SendCommand("PS3 REMOVEHOOK");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static string PS3_GetIDPS()
        {
            if (IsConnected)
            {
                SendCommand("PS3 GETIDPS");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                return sResponse;
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        //PROCESS--------------------------------------------------------------------------------
        internal static string Process_GetName(uint pid)
        {
            if (IsConnected)
            {
                SendCommand("PROCESS GETNAME " + string.Format("{0}", pid));
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                return sResponse;
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static uint[] Process_GetPidList()
        {
            if (IsConnected)
            {
                SendCommand("PROCESS GETALLPID");
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                int i = 0;
                iprocesses_pid = new uint[16];
                foreach (string s in sResponse.Split(new char[] { '|' }))
                {
                    if (s.Length != 0 && s != null && s != "" && s != " " && s != "0") { iprocesses_pid[i] = Convert.ToUInt32(s, 10); i++; }
                }
                return iprocesses_pid;
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        //MEMORY--------------------------------------------------------------------------------
        internal static void Memory_Get(uint Pid, ulong Address, byte[] Bytes)
        {
            if (IsConnected)
            {
                SetBinaryMode(true);
                int BytesLength = Bytes.Length;
                long TotalBytes = 0;
                long lBytesReceived = 0;
                bool bComplete = false;
                OpenDataSocket();
                SendCommand("MEMORY GET " + string.Format("{0}", Pid) + " " + string.Format("{0:X16}", Address) + " " + string.Format("{0}", Bytes.Length));
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.DataConnectionAlreadyOpen:
                    case PS3MAPI_ResponseCode.MemoryStatusOK:
                        break;
                    default:
                        throw new Exception(sResponse);
                }
                ConnectDataSocket();
                byte[] buffer = new byte[Bytes.Length];
                while (bComplete != true)
                {
                    try
                    {
                        lBytesReceived = data_sock.Receive(buffer, BytesLength, 0);
                        if (lBytesReceived > 0)
                        {
                            Buffer.BlockCopy(buffer, 0, Bytes, (int)TotalBytes, (int)lBytesReceived);
                            TotalBytes += lBytesReceived;
                            if ((int)(((TotalBytes) * 100) / BytesLength) >= 100) bComplete = true;
                        }
                        else
                        {
                            bComplete = true;
                        }
                        if (bComplete)
                        {
                            CloseDataSocket();
                            ReadResponse();
                            switch (eResponseCode)
                            {
                                case PS3MAPI_ResponseCode.RequestSuccessful:
                                case PS3MAPI_ResponseCode.MemoryActionCompleted:
                                    break;
                                default:
                                    throw new Exception(sResponse);
                            }
                            SetBinaryMode(false);
                        }
                    }
                    catch
                    {
                        CloseDataSocket();
                        ReadResponse();
                        SetBinaryMode(false);
                        throw;
                    }
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void Memory_Set(uint Pid, ulong Address, byte[] Bytes)
        {
            if (IsConnected)
            {
                SetBinaryMode(true);
                int BytesLength = Bytes.Length;
                long TotalBytes = 0;
                long lBytesSended = 0;
                bool bComplete = false;
                OpenDataSocket();
                SendCommand("MEMORY SET " + string.Format("{0}", Pid) + " " + string.Format("{0:X16}", Address));
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.DataConnectionAlreadyOpen:
                    case PS3MAPI_ResponseCode.MemoryStatusOK:
                        break;
                    default:
                        throw new Exception(sResponse);
                }
                ConnectDataSocket();
                while (bComplete != true)
                {
                    try
                    {
                        byte[] buffer = new byte[BytesLength - (int)TotalBytes];
                        Buffer.BlockCopy(Bytes, (int)lBytesSended, buffer, 0, (BytesLength - (int)TotalBytes));
                        lBytesSended = data_sock.Send(buffer, (Bytes.Length - (int)TotalBytes), 0);
                        bComplete = false;
                        if (lBytesSended > 0)
                        {
                            TotalBytes += lBytesSended;
                            if ((int)(((TotalBytes) * 100) / BytesLength) >= 100) bComplete = true;
                        }
                        else
                        {
                            bComplete = true;
                        }
                        if (bComplete)
                        {
                            CloseDataSocket();
                            ReadResponse();
                            switch (eResponseCode)
                            {
                                case PS3MAPI_ResponseCode.RequestSuccessful:
                                case PS3MAPI_ResponseCode.MemoryActionCompleted:
                                    break;
                                default:
                                    throw new Exception(sResponse);
                            }
                            SetBinaryMode(false);
                        }
                    }
                    catch
                    {
                        CloseDataSocket();
                        ReadResponse();
                        SetBinaryMode(false);
                        throw;
                    }
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        //MODULES--------------------------------------------------------------------------------
        internal static int[] Module_GetPrxIdList(uint pid)
        {
            if (IsConnected)
            {
                SendCommand("MODULE GETALLPRXID " + string.Format("{0}", pid));
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                int i = 0;
                imodules_prx_id = new int[128];
                foreach (string s in sResponse.Split(new char[] { '|' }))
                {
                    if (s.Length != 0 && s != null && s != "" && s != " " && s != "0") { imodules_prx_id[i] = Convert.ToInt32(s, 10); i++; }
                }
                return imodules_prx_id;
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static string Module_GetName(uint pid, int prxid)
        {
            if (IsConnected)
            {
                SendCommand("MODULE GETNAME " + string.Format("{0}", pid) + " " + prxid);
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                return sResponse;
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static string Module_GetFilename(uint pid, int prxid)
        {
            if (IsConnected)
            {
                SendCommand("MODULE GETFILENAME " + string.Format("{0}", pid) + " " + prxid);
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                return sResponse;
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void Module_Load(uint pid, string path)
        {
            if (IsConnected)
            {
                SendCommand("MODULE LOAD " + string.Format("{0}", pid) + " " + path);
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void Module_Unload(uint pid, int prx_id)
        {
            if (IsConnected)
            {
                SendCommand("MODULE UNLOAD " + string.Format("{0}", pid) + " " + prx_id);
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        //VSH PLUGINS (MODULES)-------------------------------------------------------------------
        internal static void VSHPlugins_GetInfoBySlot(uint slot, out string name, out string path)
        {
            name = ""; path = "";
            if (IsConnected)
            {
                SendCommand("MODULE GETVSHPLUGINFO " + string.Format("{0}", slot));
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
                string[] tmp = sResponse.Split(new char[] { '|' });
                name = tmp[0];
                path = tmp[1];
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void VSHPlugins_Load(uint slot, string path)
        {
            if (IsConnected)
            {
                SendCommand("MODULE LOADVSHPLUG " + string.Format("{0}", slot) + " " + path);
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        internal static void VSHPlugins_Unload(uint slot)
        {
            if (IsConnected)
            {
                SendCommand("MODULE UNLOADVSHPLUGS " + string.Format("{0}", slot));
                switch (eResponseCode)
                {
                    case PS3MAPI_ResponseCode.RequestSuccessful:
                    case PS3MAPI_ResponseCode.CommandOK:
                        break;
                    default:
                        Fail();
                        break;
                }
            }
            else
            {
                throw new Exception("PS3MAPI not connected!");
            }
        }
        //----------------------------------------------------------------------------------------
        internal static void Fail()
        {
            Fail(new Exception("[" + eResponseCode + "] " + sResponse));
        }
        internal static void Fail(Exception e)
        {
            Disconnect();
            throw e;
        }
        internal static void SetBinaryMode(bool bMode)
        {
            SendCommand("TYPE" + ((bMode) ? " I" : " A"));
            switch (eResponseCode)
            {
                case PS3MAPI_ResponseCode.RequestSuccessful:
                case PS3MAPI_ResponseCode.CommandOK:
                    break;
                default:
                    Fail();
                    break;
            }
        }
        internal static void OpenDataSocket()
        {
            string[] pasv;
            string sServer;
            int iPort;
            Connect();
            SendCommand("PASV");
            if (eResponseCode != PS3MAPI_ResponseCode.EnteringPassiveMode)
            {
                Fail();
            }
            try
            {
                int i1, i2;
                i1 = sResponse.IndexOf('(') + 1;
                i2 = sResponse.IndexOf(')') - i1;
                pasv = sResponse.Substring(i1, i2).Split(',');
            }
            catch (Exception)
            {
                Fail(new Exception("Malformed PASV response: " + sResponse));
                throw new Exception("Malformed PASV response: " + sResponse);
            }

            if (pasv.Length < 6)
            {
                Fail(new Exception("Malformed PASV response: " + sResponse));
            }

            sServer = string.Format("{0}.{1}.{2}.{3}", pasv[0], pasv[1], pasv[2], pasv[3]);
            iPort = (int.Parse(pasv[4]) << 8) + int.Parse(pasv[5]);
            try
            {
                CloseDataSocket();
                data_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                data_ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(sServerIP)[0], iPort);
                data_sock.Connect(data_ipEndPoint);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to connect for data transfer: " + e.Message);
            }
        }
        internal static void ConnectDataSocket()
        {
            if (data_sock != null)		// already connected (always so if passive mode)
                return;
            try
            {
                data_sock = listening_sock.Accept();	// Accept is blocking
                listening_sock.Close();
                listening_sock = null;
                if (data_sock == null)
                {
                    throw new Exception("Winsock error: " +
                                        Convert.ToString(System.Runtime.InteropServices.Marshal.GetLastWin32Error()));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to connect for data transfer: " + ex.Message);
            }
        }
        internal static void CloseDataSocket()
        {
            if (data_sock != null)
            {
                if (data_sock.Connected)
                {
                    data_sock.Close();
                }
                data_sock = null;
            }
            data_ipEndPoint = null;
        }
        internal static void ReadResponse()
        {
            string sBuffer;
            sMessages = "";
            while (true)
            {
                sBuffer = GetLineFromBucket();
                if (Regex.Match(sBuffer, "^[0-9]+ ").Success)
                {
                    sResponse = sBuffer.Substring(4).Replace("\r", "").Replace("\n", "");
                    eResponseCode = (PS3MAPI_ResponseCode)int.Parse(sBuffer.Substring(0, 3));
                    sLog = sLog + "RESPONSE CODE: " + eResponseCode + Environment.NewLine;
                    sLog = sLog + "RESPONSE MSG: " + sResponse + Environment.NewLine + Environment.NewLine;
                    break;
                }
                else
                {
                    sMessages += Regex.Replace(sBuffer, "^[0-9]+-", "") + "\n";
                }
            }
        }
        internal static void SendCommand(string sCommand)
        {
            sLog = sLog + "COMMAND: " + sCommand + Environment.NewLine;
            Connect();
            byte[] byCommand = Encoding.ASCII.GetBytes((sCommand + "\r\n").ToCharArray());
            main_sock.Send(byCommand, byCommand.Length, 0);
            ReadResponse();
        }
        internal static void FillBucket()
        {
            byte[] bytes = new byte[512];
            long lBytesRecieved;
            int iMilliSecondsPassed = 0;
            while (main_sock.Available < 1)
            {
                System.Threading.Thread.Sleep(50);
                iMilliSecondsPassed += 50;

                if (iMilliSecondsPassed > Timeout) // Prevents infinite loop
                {
                    Fail(new Exception("Timed out waiting on server to respond."));
                }
            }
            while (main_sock.Available > 0)
            {
                // gives any further data not yet received, a small chance to arrive
                lBytesRecieved = main_sock.Receive(bytes, 512, 0);
                sBucket += Encoding.ASCII.GetString(bytes, 0, (int)lBytesRecieved);
                System.Threading.Thread.Sleep(50);
            }
        }
        internal static string GetLineFromBucket()
        {
            string sBuffer = "";
            int i = sBucket.IndexOf('\n');

            while (i < 0)
            {
                FillBucket();
                i = sBucket.IndexOf('\n');
            }

            sBuffer = sBucket.Substring(0, i);
            sBucket = sBucket.Substring(i + 1);

            return sBuffer;
        }
    }
    #endregion PS3MAPI_Client_Server

}