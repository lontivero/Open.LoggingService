using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Open.LoggingService
{
    [RunInstaller(true)]
    public partial class ServiceInstaller : Installer
    {
        public ServiceInstaller()
        {
            InitializeComponent();
        }

        protected override void OnCommitted(IDictionary savedState)
        {
            base.OnCommitted(savedState);

            using (var serviceController = new ServiceController(serviceInstaller.ServiceName, Environment.MachineName))
            {
                if (serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    serviceController.Start();
                }
            }
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            using (var serviceController = new ServiceController(serviceInstaller.ServiceName, Environment.MachineName))
            {
                serviceController.Start();
            }
        }
    }
}
