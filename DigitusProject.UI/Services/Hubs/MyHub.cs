using DigitusProject.WebUI.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitusProject.WebUI.Services.Hubs
{
    public class MyHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var user = Context.User;
            if(user.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress") is not null)
            {
                var email = user.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;
                Client client = new Client()
                {
                    Email = email,
                    ConnectionId = connectionId
                };
                ClientSource.Clients.Add(client);
            }
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var user = Context.User;
            if (user.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress") is not null)
            {
                var email = user.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;

                var client = ClientSource.Clients.FirstOrDefault(x => x.Email == email);
                ClientSource.Clients.Remove(client);
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
