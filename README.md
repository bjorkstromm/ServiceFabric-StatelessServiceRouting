# ServiceFabric-StatelessServiceRouting
Sample trying to work around the inability to target a specific StatelessService on Service Fabric. 

## The Problem

So you want to have a service which will listen on an external endpoint for a client request to open a persistent connection.

You'd like to expose those connections via a front end but when you go to resolve a StatelessService you get a random one by default, which gives you only a 1/N chance of finding the correct instance. 

Service Fabric doesn't consider this use case very well where you have no persistent state and don't want replica semantics of a StatefulService but still have open socket connections that can't be replicated. 

That's a problem if you are scaling this service horizontally and your needs are too simple to justify taking a backplane into use like Redis or Azure Service Bus. 

## Potential Workaround

Stateless Services can be partitioned not only by numeric range but by name. Instead of defining an InstanceCount of -1, which creates an instance on each Node, we specify an InstanceCount of 1 but a number of named partitions. 

When we instantiate the StatelessService we query the ServiceFabric naming service for the partition info and create proxies to each named partition instance. 

Then each partitioned instance has the capability to broadcast RPC to the other partitioned instances when it does not have the open persistent connection. Then whichever partition has the open socket can service the request. 

We can also return the partition back in the result so that we can cache this at the front end and make calls directly to the service proxy holding the open connection which avoids the broadcast RPC overhead. 

## Inspiration

This is very roughly how Address Resolution Protocol works. The network device has no way of knowing on which port a specific IP Address is connected, it only knows about MAC addresses. So it broadcasts an ARP request to all devices on the local network, and the one device that has the IP Address in the request will respond. The network device can then cache this result and map the IP Address to the MAC in the ARP table. 

Partition Names = MAC   
Unique Client GUID = IP Address   
Service Partition = Network Port   

Main difference is that we don't do this from a central device (service) as that wouldn't work in a HA environment with many instances, but rather allow the Service Partitions to broadcast in peer-to-peer fashion. 

## Still to do

Implement real external persistant connections and endpoints to test the behavior, rather than just faking those. 

Test more in the cloud, sometimes life is different from local. 

Find way to generalize and wrap into a Nuget package?
