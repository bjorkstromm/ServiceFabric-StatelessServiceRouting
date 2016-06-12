using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using System.Collections.Concurrent;
using Microsoft.ServiceFabric.Services.Communication.Client;
using MockData;

namespace WebSocketServerService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WebSocketServerService : StatelessService, IWebSocketServerService
    {
        /// <summary>
        /// Holds the local connections from WebSocket upgrades. 
        /// </summary>
        private ConcurrentDictionary<Guid, WebSocket> ClientConnections = new ConcurrentDictionary<Guid, WebSocket>();
        
        /// <summary>
        /// Holds the references to proxies of the other service instances. Those proxies can auto resolve if a node goes down
        /// and the location of the named partition changes. Note that in this case any open connections held by that instance
        /// are lost because open sockets aren't location transparent. Fact of life in the cloud. 
        /// </summary>
        private ConcurrentDictionary<string, IWebSocketServerService> ServerPartitions = new ConcurrentDictionary<string, IWebSocketServerService>();

        /// <summary>
        /// Used to query the ServiceFabric application
        /// </summary>
        private FabricClient _fabricClient = new FabricClient();

        /// <summary>
        /// Uri for this service
        /// </summary>
        private Uri _thisServiceUri = new Uri("fabric:/ServiceFabricHost/WebSocketServerService");

        /// <summary>
        /// The partition name of this instance
        /// </summary>
        private string _partitionName;

        public const string ServiceEventSourceName = nameof(WebSocketServerService);

        public WebSocketServerService(StatelessServiceContext context)
            : base(context)
        {   

            // First get the partition list for this service
            var servicePartitions = _fabricClient.QueryManager.GetPartitionListAsync(_thisServiceUri).Result;

            // Now identify this one based on the partition information we have.
            var thisPartition = servicePartitions.Where(p => context.PartitionId.Equals(p.PartitionInformation.Id)).SingleOrDefault();

            // Get the partition name for this instance
            // Why isn't this included in the context information for Named Partitions? Why?
            _partitionName = (thisPartition.PartitionInformation as NamedPartitionInformation)?.Name;

            // Initialize mock connected clients based on partition name
            Initialize(_partitionName);

            // Now loop through the named instances and resolve a service proxy for each by the partition key and add them to the dictionary
            foreach(var partition in servicePartitions)
            {
                var key = (partition.PartitionInformation as NamedPartitionInformation)?.Name;

                // Don't include this partition instance, don't need to RPC ourselves.
                if (key == null || key.Equals(_partitionName)) continue;

                var resolvedPartition = ServiceProxy.Create<IWebSocketServerService>(_thisServiceUri, new ServicePartitionKey(key), TargetReplicaSelector.Default, "InternalWSRouting");
                ServerPartitions.AddOrUpdate(key, resolvedPartition, (oldKey, oldValue) => resolvedPartition);
            }
        }

        /// <summary>
        /// There will be (in this demo) 4 partitions of this service, Alpha, Bravo, Charlie, Delta
        /// Here we initialize some fake "connections" to them. 
        /// </summary>
        /// <param name="_partitionName"></param>
        private void Initialize(string _partitionName)
        {
            if ("Alpha".Equals(_partitionName))
            {
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(0), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(1), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(2), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(3), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(4), new WebSocket());
                

            }
            else if ("Beta".Equals(_partitionName))
            {
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(5), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(6), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(7), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(8), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(9), new WebSocket());
                

            }
            else if ("Charlie".Equals(_partitionName))
            {
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(10), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(11), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(12), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(13), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(14), new WebSocket());
            }
            else if ("Delta".Equals(_partitionName))
            {
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(15), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(16), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(17), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(18), new WebSocket());
                ClientConnections.TryAdd(ConnectedClients.GetClientGuid(19), new WebSocket());
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context), "InternalWSRouting")
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this, $"{_partitionName} working {++iterations} minute.");

                await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
            }
        }

        /// <summary>
        /// This method handles incoming routing from other instances of this service. 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="guid"></param>
        /// <param name="from"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<DtoResult> RouteFromService(ArraySegment<byte> message, Guid guid, string from, CancellationToken token)
        {
            ServiceEventSource.Current.ServiceMessage(this, $"Internal Routing from {from} to {_partitionName} : {guid}");

            // If this instance has the connection, get the result from the connected device
            if (ClientConnections.ContainsKey(guid))
            {
                ClientConnections[guid].Send(message);
                // Your way of getting the result here
                return Task.FromResult(new DtoResult() {
                    PartitionName = _partitionName
                });
            }
            else
            {
                // Otherwise if we don't have it return null to the calling service instance.
                // Hopefully another instance has the connection.
                return Task.FromResult<DtoResult>(null);
            }
        }

        public Task<DtoResult> RouteFromApiGateway(ArraySegment<byte> payload, Guid clientGuid)
        {
            ServiceEventSource.Current.ServiceMessage(this, $"API Request to {_partitionName} : {clientGuid}");

            // In the case that the WS open socket connection is to this instance...
            if (ClientConnections.ContainsKey(clientGuid))
            {
                ClientConnections[clientGuid].Send(payload);
                
                // Get your result from the device on the open WS socket connection and tell the ApiGateway that 
                // this service instance has the connection for that client.
                return Task.FromResult(new DtoResult() { PartitionName = _partitionName });
            }
            else 
            {
                // broadcast RPC to all service instances and let whoever has the connection handle it
                ServiceEventSource.Current.ServiceMessage(this, $"API Request to {_partitionName} not found. Broadcasting to peers.");

                // Create a collection of tasks of RPC calls to service partition instances and then await them. 
                var taskList = new List<Task<DtoResult>>();
                foreach (var server in ServerPartitions.Values)
                {
                    var task = server.RouteFromService(payload, clientGuid, _partitionName, CancellationToken.None);
                    taskList.Add(task);
                }

                // The one with the connection, if any, will be the last one to return a completed Task. 
                // Instances without the connection return successful null, the one with the connection needs a WS network round trip.
                var result = Task.WhenAll(taskList).Result;

                var retVal = result.ToList().SingleOrDefault(t => !ReferenceEquals(t, null));

                // Return the completed Task from the correct instance.
                return Task.FromResult(retVal);
            }
        }
    }
}
