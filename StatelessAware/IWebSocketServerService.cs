using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketServerService
{
    public interface IWebSocketServerService : IService
    {
        /// <summary>
        /// Called from another service to route to not found clients.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<DtoResult> RouteFromService(ArraySegment<byte> message, Guid reqGuid, string from, CancellationToken token);

        /// <summary>
        /// Called for initial request from API gateway
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<DtoResult> RouteFromApiGateway(ArraySegment<byte> message, Guid reqGuid);
    }

    public class DtoResult
    {
        // Return this so that it's clear which service had the connection and serviced the request to the remote device. 
        public string PartitionName { get; set; }
    }

    public class WebSocket
    {
        // Just stub this out doing nothing. 
        public void Send(ArraySegment<byte> payload)
        {

        }
    }
}
