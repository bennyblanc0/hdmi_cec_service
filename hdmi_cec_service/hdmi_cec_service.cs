using System;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace hdmi_cec_service
{
    public partial class hdmi_cec_service : ServiceBase
    {
        // Declare threading processes
        public Thread thread;
        public TcpListener tcpListener;
        public Timer timer;

        // Declare flags
        public bool threadRunning = false;
        public string ping = "False";

        // Instantiate CEC client instance 
        public CecSharpClient cecSharpclient = new CecSharpClient();

        // Declare passed argument variables
        public string pActiveSource;
        public string pTvHDMIPort;
        public string pAudioSystemHDMIPort;

        public hdmi_cec_service()
        {
            // Mandatory initialisation for designer 
            InitializeComponent();

            // Set up event logging in Windows Application log
            eventLog = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("hdmi_cec_service"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "hdmi_cec_service", "Application");
            }
            eventLog.Source = "hdmi_cec_service";
            eventLog.Log = "Application";
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // Check if CEC device is present/responsive and if not then attempt restart 
            if (threadRunning)
            {
                ping = cecSharpclient.SendCommand("ping");
            }
            else
            {
                ping = "False";
            }
            if (ping == "False")
            {
                threadRunning = false; // Must set flag to false before attempting to stop TCP listener so main thread can exit gracefully
                tcpListener.Stop();
                while (thread.IsAlive)
                {
                    thread.Abort(); // Probably don't need to do this
                }                
                tcpListener.Start();
                thread = new Thread(MainThread);
                thread.Start();
            }
        }

        public void MainThread() {
            if (cecSharpclient.Connect(10000))
            {
                eventLog.WriteEntry("Successfully connected to CEC device");
                threadRunning = true;

                // Set default device ports and active source.  I seems Libcec always sets HDMI ports to 1 by default... annoying.
                eventLog.WriteEntry(cecSharpclient.SendCommand("setActiveSource " + pActiveSource));
                eventLog.WriteEntry(cecSharpclient.SendCommand("setDeviceHDMIPort Tv " + pTvHDMIPort));
                eventLog.WriteEntry(cecSharpclient.SendCommand("setDeviceHDMIPort AudioSystem " + pAudioSystemHDMIPort));
                
                while (threadRunning)
                {
                    Socket socket = tcpListener.AcceptSocket();
                    byte[] bytes = new Byte[256];
                    Array.Clear(bytes, 0, bytes.Length);
                    int result = socket.Receive(bytes);
                    ASCIIEncoding ascen = new ASCIIEncoding();
                    string str = ascen.GetString(bytes).Replace("\0", "");
                    eventLog.WriteEntry("Received: " + str);
                    eventLog.WriteEntry(cecSharpclient.SendCommand(str));
                    socket.Close();
                }              
                return;
            }
            else
            {
                eventLog.WriteEntry("Could not open a connection to the CEC adapter");
            }
            return;
        }

        protected override void OnStart(string[] args)
        {
            // Set any passed parameters 
            args = Environment.GetCommandLineArgs();

            pActiveSource = args[1];
            pTvHDMIPort = args[2];
            pAudioSystemHDMIPort = args[3];

            StringBuilder output = new StringBuilder();
            output.AppendLine("Passed arguments");
            output.AppendLine("===================");
            output.AppendLine("pActiveSource: " + pActiveSource);
            output.AppendLine("pTvHDMIPort: " + pTvHDMIPort);
            output.AppendLine("pAudioSystemHDMIPort: " + pAudioSystemHDMIPort);
            eventLog.WriteEntry(output.ToString());

            // Set up a timer to trigger every minute.
            eventLog.WriteEntry("Starting timer");
            try
            {
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 20000; // 20 seconds
                timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
                timer.Start();
            }
            catch
            {
                eventLog.WriteEntry("Failed to start timer");               
            }

            // Start TCP listner 
            eventLog.WriteEntry("Starting TCP listener");
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
                tcpListener.Start();
            }
            catch
            {
                eventLog.WriteEntry("Failed to start TCP listener");
            }

            // Start main thread
            eventLog.WriteEntry("Starting main thread");            
            try
            {
                thread = new Thread(MainThread);
                thread.Start();
            }
            catch
            {
                eventLog.WriteEntry("Failed to start main thread");
            }
        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("Stopping service");
        }
    }
}