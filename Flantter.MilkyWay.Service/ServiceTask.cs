using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace Flantter.MilkyWay.Service
{
    public sealed class ServiceTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _Deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            this._Deferral = taskInstance.GetDeferral();

            var detail = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if (detail != null && detail.Name == "TaskService")
                detail.AppServiceConnection.RequestReceived += AppServiceConnection_RequestReceived;

            detail.
        }
    }
}
