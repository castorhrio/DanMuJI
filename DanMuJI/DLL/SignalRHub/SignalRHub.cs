using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DanMuJI.DLL.SignalRHub
{
    public class SignalRHub : Hub
    {
        [HubMethodName("sendMsg")]
        public void SendMsg(string flag, string message)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<SignalRHub>();
            hubContext.Clients.All.getMessage(flag, message);
        }
    }
}