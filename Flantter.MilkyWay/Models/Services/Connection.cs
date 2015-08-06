using Flantter.MilkyWay.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Flantter.MilkyWay.Models.Services
{
    public class Connection
    {
        private static Connection _Instance = new Connection();
        private Connection() { }

        public static Connection Instance
        {
            get { return _Instance; }
        }

        public event EventHandler CommandExecute;
        public void OnCommandExecute(object child)
        {
            if (CommandExecute != null)
                CommandExecute(child, EventArgs.Empty);
        }

        public async Task Initialize()
        {
            AppServiceConnection connection = new AppServiceConnection();
            connection.AppServiceName = "Flantter.MilkyWay.Service";
            connection.PackageFamilyName = "db427974-cce9-4f8b-8cfd-a7a21e241b03_4jbsbbz25cm9m";
            AppServiceConnectionStatus connectionStatus = await connection.OpenAsync();

            if (connectionStatus != AppServiceConnectionStatus.Success)
                throw new AppServiceConnectionException("Failed to establish a connection to AppService.", connectionStatus.ToString());

            this.AppServiceConnection = connection;

            var message = new ValueSet();
            message.Add("Command", "Initialize");
            AppServiceResponse response = await connection.SendMessageAsync(message);

        }

        public AppServiceConnection AppServiceConnection { get; set; }
    }
}
