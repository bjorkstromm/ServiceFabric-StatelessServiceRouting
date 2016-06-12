using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using WebSocketServerService;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using MockData;

namespace WebApi
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WebApi : StatelessService
    {
        private Dictionary<string, IWebSocketServerService> _serviceInstances = new Dictionary<string, IWebSocketServerService>();

        private Dictionary<Guid, string> _routingCache = new Dictionary<Guid, string>();

        private FabricClient _fabricClient = new FabricClient();

        public WebApi(StatelessServiceContext context)
            : base(context)
        {
            var wsServiceUri = new Uri("fabric:/ServiceFabricHost/WebSocketServerService");

            // First get the partition list for this service, should be 1 named partition/instance per node
            var servicePartitions = _fabricClient.QueryManager.GetPartitionListAsync(wsServiceUri).Result;

            /*
             * Now loop through all the partitions and resolve the service instance for each individual key and add them to some data structure.
             * 
             * In a WebApi implementation we can store a caching Factory in the application which we can inject into the ApiControllers that can
             * either get the specific service instance if we know it or just get a random one. 
             */

            foreach (var partition in servicePartitions)
            {
                var key = (partition.PartitionInformation as NamedPartitionInformation)?.Name;

                var resolvedPartition = ServiceProxy.Create<IWebSocketServerService>(wsServiceUri, new ServicePartitionKey(key), TargetReplicaSelector.Default, "InternalWSRouting");
                _serviceInstances.Add(key, resolvedPartition);
            }
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {

            long iterations = 0;

            // Just needed to pull random GUIDs from our "ConnectedClients" which is faked. 
            var random = new Random();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this, "Web API Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                // Get a faked client connection that we want to request data from. It will exist on one of the instances and we don't know which at this point.
                var clientGuid = ConnectedClients.GetClientGuid(random.Next(0, 20));

                // If we've just gotten a response from a specific instance get that one, this could be a factory with caching instead in an ApiController
                IWebSocketServerService serviceInstance = (_routingCache.ContainsKey(clientGuid)) ? _serviceInstances[_routingCache[clientGuid]] : _serviceInstances.GetRandom();

                // Get the result. Either the instance we call has the connected client or it will broadcast RPC to all the other instances to get it. 
                // The instance having the connection comes back in the dtoResult
                var dtoResult = serviceInstance.RouteFromApiGateway(new ArraySegment<byte>(), clientGuid);

                // So if we actually got anything, could not have if there is a disconnect, etc.. 
                if(!ReferenceEquals(dtoResult.Result, null))
                {
                    ServiceEventSource.Current.ServiceMessage(this, $"Result from {dtoResult.Result.PartitionName} for {clientGuid}");

                    // Add it to the routing cache so we can get directly the right service instance for subsequent requests.
                    // This saves internal network I/O for the RPC broadcasting if we can call the right one directly.
                    if (_routingCache.ContainsKey(clientGuid))
                    {
                        _routingCache[clientGuid] = dtoResult.Result.PartitionName;
                    }
                    else
                    {
                        _routingCache.Add(clientGuid, dtoResult.Result.PartitionName);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }
    }
}
