using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace hdmi_cec_service
{
    static class hdmi_cec_program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new hdmi_cec_service()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
