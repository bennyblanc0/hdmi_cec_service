using System;
using System.Text;
using CecSharp;

namespace hdmi_cec_service
{
    public class CecSharpClient : CecCallbackMethods
    {
        public CecSharpClient()
        {
            Config = new LibCECConfiguration();
            Config.DeviceTypes.Types[0] = CecDeviceType.RecordingDevice;
            Config.DeviceName = "CEC Tester";
            Config.ClientVersion = LibCECConfiguration.CurrentVersion;
            Config.SetCallbacks(this);
            LogLevel = (int)CecLogLevel.All;
            Lib = new LibCecSharp(Config);
            Lib.InitVideoStandalone();
        }

        public override int ReceiveCommand(CecCommand command)
        {
            return 1;
        }

        public override int ReceiveKeypress(CecKeypress key)
        {
            return 1;
        }

        public override int ReceiveLogMessage(CecLogMessage message)
        {
            if (((int)message.Level & LogLevel) == (int)message.Level)
            {
                string strLevel = "";
                switch (message.Level)
                {
                    case CecLogLevel.Error:
                        strLevel = "ERROR:   ";
                        break;
                    case CecLogLevel.Warning:
                        strLevel = "WARNING: ";
                        break;
                    case CecLogLevel.Notice:
                        strLevel = "NOTICE:  ";
                        break;
                    case CecLogLevel.Traffic:
                        strLevel = "TRAFFIC: ";
                        break;
                    case CecLogLevel.Debug:
                        strLevel = "DEBUG:   ";
                        break;
                    default:
                        break;
                }
                string strLog = string.Format("{0} {1,16} {2}", strLevel, message.Time, message.Message);
                Console.WriteLine(strLog);
            }
            return 1;
        }

        public bool Connect(int timeout)
        {
            CecAdapter[] adapters = Lib.FindAdapters(string.Empty);
            if (adapters.Length > 0)
                return Connect(adapters[0].ComPort, timeout);
            else
            {
                Console.WriteLine("Did not find any CEC adapters");
                return false;
            }
        }

        public bool Connect(string port, int timeout)
        {
            return Lib.Open(port, timeout);
        }

        public void Close()
        {
            Lib.Close();
        }

        public void ListDevices()
        {
            int iAdapter = 0;
            foreach (CecAdapter adapter in Lib.FindAdapters(string.Empty))
            {
                Console.WriteLine("Adapter:  " + iAdapter++);
                Console.WriteLine("Path:     " + adapter.Path);
                Console.WriteLine("Com port: " + adapter.ComPort);
            }
        }

        public string AdaptorDetails() {
            CecAdapter[] adapters = Lib.FindAdapters(string.Empty);
            return "Path: " + adapters[0].Path + " Com: " + adapters[0].ComPort;
        }

        void ShowConsoleHelp()
        {
            Console.WriteLine(
              "================================================================================" + Environment.NewLine +
              "Available commands:" + Environment.NewLine +
              Environment.NewLine +
              "[tx] {bytes}              transfer bytes over the CEC line." + Environment.NewLine +
              "[txn] {bytes}             transfer bytes but don't wait for transmission ACK." + Environment.NewLine +
              "[on] {address}            power on the device with the given logical address." + Environment.NewLine +
              "[standby] {address}       put the device with the given address in standby mode." + Environment.NewLine +
              "[la] {logical_address}    change the logical address of the CEC adapter." + Environment.NewLine +
              "[pa] {physical_address}   change the physical address of the CEC adapter." + Environment.NewLine +
              "[osd] {addr} {string}     set OSD message on the specified device." + Environment.NewLine +
              "[ver] {addr}              get the CEC version of the specified device." + Environment.NewLine +
              "[ven] {addr}              get the vendor ID of the specified device." + Environment.NewLine +
              "[lang] {addr}             get the menu language of the specified device." + Environment.NewLine +
              "[pow] {addr}              get the power status of the specified device." + Environment.NewLine +
              "[poll] {addr}             poll the specified device." + Environment.NewLine +
              "[scan]                    scan the CEC bus and display device info" + Environment.NewLine +
              "[mon] {1|0}               enable or disable CEC bus monitoring." + Environment.NewLine +
              "[log] {1 - 31}            change the log level. see cectypes.h for values." + Environment.NewLine +
              "[ping]                    send a ping command to the CEC adapter." + Environment.NewLine +
              "[bl]                      to let the adapter enter the bootloader, to upgrade" + Environment.NewLine +
              "                          the flash rom." + Environment.NewLine +
              "[r]                       reconnect to the CEC adapter." + Environment.NewLine +
              "[h] or [help]             show this help." + Environment.NewLine +
              "[q] or [quit]             to quit the CEC test client and switch off all" + Environment.NewLine +
              "                          connected CEC devices." + Environment.NewLine +
              "================================================================================");
        }

        public string SetActiveSource(string activeSource)
        {
            if (activeSource == "AudioSystem")
            {
                Lib.SetActiveSource(CecDeviceType.AudioSystem);
                return "setting active source to audio system";
            }
            if (activeSource == "PlaybackDevice")
            {
                Lib.SetActiveSource(CecDeviceType.PlaybackDevice);
                return "setting active source to playback device";
            }
            if (activeSource == "RecordingDevice")
            {
                Lib.SetActiveSource(CecDeviceType.RecordingDevice);
                return "setting active source to Recording device";
            }
            if (activeSource == "Reserved")
            {
                Lib.SetActiveSource(CecDeviceType.Reserved);
                return "setting active source to Reserved device";
            }
            if (activeSource == "Tuner")
            {
                Lib.SetActiveSource(CecDeviceType.Tuner);
                return "setting active source to Tuner";
            }
            if (activeSource == "Tv")
            {
                Lib.SetActiveSource(CecDeviceType.Tv);
                return "setting active source to Tv";
            }
            return "Failed to set active source";
        }
        public string SetTvHDMIPort(string HDMIPort)
        {
            Lib.SetHDMIPort(CecLogicalAddress.AudioSystem, byte.Parse(HDMIPort));
            return "Set Tv HDMI port to " + HDMIPort;
        }
        public string SetAudioSystemHDMIPort(string HDMIPort)
        {
            Lib.SetHDMIPort(CecLogicalAddress.AudioSystem, byte.Parse(HDMIPort));
            return "Set device AudioSystem HDMI port to " + HDMIPort;
        }
        public string SendCommand(string command)
        {

            if (command == null || command.Length == 0)
                return "No command received";
                string[] splitCommand = command.Split(' ');
            if (splitCommand[0] == "tx" || splitCommand[0] == "txn")
            {
                CecCommand bytes = new CecCommand();
                for (int iPtr = 1; iPtr < splitCommand.Length; iPtr++)
                {
                    bytes.PushBack(byte.Parse(splitCommand[iPtr], System.Globalization.NumberStyles.HexNumber));
                }

                if (command == "txn")
                    bytes.TransmitTimeout = 0;

                Lib.Transmit(bytes);
            }
            else if (splitCommand[0] == "default")
            {
                Lib.SetActiveSource(CecDeviceType.PlaybackDevice);
                return "Set default playback device as active";
            }
            else if (splitCommand[0] == "rescan")
            {
                Lib.RescanActiveDevices();
                return "Rescan active devices";
            }
            else if (splitCommand[0] == "vol")
            {
                if (splitCommand[1] == "up")
                {
                    Lib.VolumeUp(true);
                    return "vol up";
                }
                else if (splitCommand[1] == "down")
                {
                    Lib.VolumeDown(true);
                    return "vol down";
                }
                else if (splitCommand[1] == "mute")
                {
                    Lib.MuteAudio(true);
                    return "Vol mute";
                }
                return "Vol sub command not understood";
            }
            else if (splitCommand[0] == "on")
            {
                if (splitCommand.Length > 1)
                {
                    Lib.PowerOnDevices((CecLogicalAddress)byte.Parse(splitCommand[1], System.Globalization.NumberStyles.HexNumber));
                    return "Signalled ON for device: " + splitCommand[1];
                }
                else
                {
                    Lib.PowerOnDevices(CecLogicalAddress.Broadcast);
                    return "Signalled broadcast ON";
                }
            }
            else if (splitCommand[0] == "standby")
            {
                if (splitCommand.Length > 1)
                {
                    Lib.StandbyDevices((CecLogicalAddress)byte.Parse(splitCommand[1], System.Globalization.NumberStyles.HexNumber));
                    return "Signalled STANDBY for device: " + splitCommand[1];
                }
                else
                {
                    Lib.StandbyDevices(CecLogicalAddress.Broadcast);
                    return "Signalled broadcast STANDBY";
                }
            }
            else if (splitCommand[0] == "setDeviceHDMIPort")
            {
                if (splitCommand.Length > 2)
                {
                    if (splitCommand[1] == "Tv")
                    {
                        Lib.SetHDMIPort(CecLogicalAddress.Tv, byte.Parse(splitCommand[2]));
                        return "Set device " + splitCommand[1] + " to HDMI port " + splitCommand[2];
                    }
                    if (splitCommand[1] == "AudioSystem")
                    {
                        Lib.SetHDMIPort(CecLogicalAddress.AudioSystem, byte.Parse(splitCommand[2]));
                        return "Set device " + splitCommand[1] + " to HDMI port " + splitCommand[2];
                    }
                }
                return "Incorrect use of setDeviceHDMIPort";
            }
            else if (splitCommand[0] == "poll")
            {
                bool bSent = false;
                if (splitCommand.Length > 1)
                    bSent = Lib.PollDevice((CecLogicalAddress)byte.Parse(splitCommand[1], System.Globalization.NumberStyles.HexNumber));
                else
                    bSent = Lib.PollDevice(CecLogicalAddress.Broadcast);
                if (bSent)
                    Console.WriteLine("POLL message sent");
                else
                    Console.WriteLine("POLL message not sent");
            }
            else if (splitCommand[0] == "la")
            {
                if (splitCommand.Length > 1)
                    Lib.SetLogicalAddress((CecLogicalAddress)byte.Parse(splitCommand[1], System.Globalization.NumberStyles.HexNumber));
            }
            else if (splitCommand[0] == "pa")
            {
                if (splitCommand.Length > 1)
                    Lib.SetPhysicalAddress(ushort.Parse(splitCommand[1], System.Globalization.NumberStyles.HexNumber));
            }
            else if (splitCommand[0] == "osd")
            {
                if (splitCommand.Length > 2)
                {
                    StringBuilder osdString = new StringBuilder();
                    for (int iPtr = 1; iPtr < splitCommand.Length; iPtr++)
                    {
                        osdString.Append(splitCommand[iPtr]);
                        if (iPtr != splitCommand.Length - 1)
                            osdString.Append(" ");
                    }
                    Lib.SetOSDString((CecLogicalAddress)byte.Parse(splitCommand[1], System.Globalization.NumberStyles.HexNumber), CecDisplayControl.DisplayForDefaultTime, osdString.ToString());
                }
            }
            else if (splitCommand[0] == "ping")
            {
                return Lib.PingAdapter().ToString();
            }
            else if (splitCommand[0] == "mon")
            {
                bool enable = splitCommand.Length > 1 ? splitCommand[1] == "1" : false;
                Lib.SwitchMonitoring(enable);
            }
            else if (splitCommand[0] == "bl")
            {
                Lib.StartBootloader();
            }
            else if (splitCommand[0] == "lang")
            {
                if (splitCommand.Length > 1)
                {
                    string language = Lib.GetDeviceMenuLanguage((CecLogicalAddress)byte.Parse(splitCommand[1], System.Globalization.NumberStyles.HexNumber));
                    return "Menu language: " + language;
                }
            }
            else if (splitCommand[0] == "ven")
            {
                if (splitCommand.Length > 1)
                {
                    CecVendorId vendor = Lib.GetDeviceVendorId((CecLogicalAddress)byte.Parse(splitCommand[1], System.Globalization.NumberStyles.HexNumber));
                    return "Vendor ID: " + Lib.ToString(vendor);
                }
            }
            else if (splitCommand[0] == "ver")
            {
                if (splitCommand.Length > 1)
                {
                    CecVersion version = Lib.GetDeviceCecVersion((CecLogicalAddress)byte.Parse(splitCommand[1], System.Globalization.NumberStyles.HexNumber));
                    return "CEC version: " + Lib.ToString(version);
                }
            }
            else if (splitCommand[0] == "pow")
            {
                if (splitCommand.Length > 1)
                {
                    CecPowerStatus power = Lib.GetDevicePowerStatus((CecLogicalAddress)byte.Parse(splitCommand[1], System.Globalization.NumberStyles.HexNumber));
                    return "power status: " + Lib.ToString(power);
                }
            }
            else if (splitCommand[0] == "r")
            {
                Console.WriteLine("closing the connection");
                Lib.Close();

                Console.WriteLine("opening a new connection");
                Connect(10000);

                Console.WriteLine("setting active source");
                Lib.SetActiveSource(CecDeviceType.AudioSystem);
            }
            else if (splitCommand[0] == "setActiveSource")
            {
                if (splitCommand.Length > 1)
                {
                    if (splitCommand[1] == "AudioSystem")
                    {
                        Lib.SetActiveSource(CecDeviceType.AudioSystem);
                        return "setting active source to audio system";
                    }
                    if (splitCommand[1] == "PlaybackDevice")
                    {
                        Lib.SetActiveSource(CecDeviceType.PlaybackDevice);
                        return "setting active source to playback device";
                    }
                    if (splitCommand[1] == "RecordingDevice")
                    {
                        Lib.SetActiveSource(CecDeviceType.RecordingDevice);
                        return "setting active source to Recording device";
                    }
                    if (splitCommand[1] == "Reserved")
                    {
                        Lib.SetActiveSource(CecDeviceType.Reserved);
                        return "setting active source to Reserved device";
                    }
                    if (splitCommand[1] == "Tuner")
                    {
                        Lib.SetActiveSource(CecDeviceType.Tuner);
                        return "setting active source to Tuner";
                    }
                    if (splitCommand[1] == "Tv")
                    {
                        Lib.SetActiveSource(CecDeviceType.Tv);
                        return "setting active source to Tv";
                    }
                }
                return "in correct use of setActiveSource";
            }
            else if (splitCommand[0] == "scan")
            {
                StringBuilder output = new StringBuilder();
                output.AppendLine("CEC bus information");
                output.AppendLine("===================");
                CecLogicalAddresses addresses = Lib.GetActiveDevices();
                for (int iPtr = 0; iPtr < addresses.Addresses.Length; iPtr++)
                {
                    CecLogicalAddress address = (CecLogicalAddress)iPtr;
                    if (!addresses.IsSet(address))
                        continue;

                    CecVendorId iVendorId = Lib.GetDeviceVendorId(address);
                    bool bActive = Lib.IsActiveDevice(address);
                    ushort iPhysicalAddress = Lib.GetDevicePhysicalAddress(address);
                    string strAddr = Lib.PhysicalAddressToString(iPhysicalAddress);
                    CecVersion iCecVersion = Lib.GetDeviceCecVersion(address);
                    CecPowerStatus power = Lib.GetDevicePowerStatus(address);
                    string osdName = Lib.GetDeviceOSDName(address);
                    string lang = Lib.GetDeviceMenuLanguage(address);

                    
                    output.AppendLine("device #" + iPtr + ": " + Lib.ToString(address));
                    output.AppendLine("address:       " + strAddr);
                    output.AppendLine("active source: " + (bActive ? "yes" : "no"));
                    output.AppendLine("vendor:        " + Lib.ToString(iVendorId));
                    output.AppendLine("osd string:    " + osdName);
                    output.AppendLine("CEC version:   " + Lib.ToString(iCecVersion));
                    output.AppendLine("power status:  " + Lib.ToString(power));
                    if (!string.IsNullOrEmpty(lang))
                        output.AppendLine("language:      " + lang);
                    output.AppendLine("");
                }
                return output.ToString();
            }
            return "CEC command not understood";
        }

        private int LogLevel;
        private LibCecSharp Lib;
        private LibCECConfiguration Config;
    }
}