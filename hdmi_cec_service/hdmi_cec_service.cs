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
        public Thread thread;
        public TcpListener tcpListener;
        public Timer timer;
        public bool threadsRunning = false;
        public CecSharpClient p = new CecSharpClient();
        public string pActiveSource = "AudioSystem";
        public string pTvHDMIPort = "2";
        public string pAudioSystemHDMIPort = "1";

        public hdmi_cec_service()
        {
            InitializeComponent();
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
            eventLog.WriteEntry("Ping result: " + p.SendCommand("ping"));
            if (p.SendCommand("ping") == "False")
            {
                threadsRunning = false;
                p.Close();
            }
        }

        public void MainThread() {
            eventLog.WriteEntry("Connecting to CEC device");
            if (p.Connect(10000))
            {
                // p.SetDefaultHDMI();
                eventLog.WriteEntry("Successfully connected to CEC device");       
                while (threadsRunning)
                {
                    Socket socket = tcpListener.AcceptSocket();
                    byte[] bytes = new Byte[256];
                    Array.Clear(bytes, 0, bytes.Length);
                    int result = socket.Receive(bytes);
                    ASCIIEncoding ascen = new ASCIIEncoding();
                    string str = ascen.GetString(bytes).Replace("\0", "");
                    eventLog.WriteEntry("Receved: " + str);
                    eventLog.WriteEntry(p.SendCommand(str));
                    socket.Close();
                }
            }
            else
            {
                eventLog.WriteEntry("Could not open a connection to the CEC adapter");
            }
        }

        protected override void OnStart(string[] args)
        {
            eventLog.WriteEntry("Starting service");

            // Set any passed parameters 
            pActiveSource = args[0];
            pTvHDMIPort = args[1];
            pAudioSystemHDMIPort = args[2];

            // Mark threads as running 
            threadsRunning = true;

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

            // Start TCP listner thread
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