using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockData
{
    public static class ConnectedClients
    {
        // Generate 20 GUIDs, we'll add 5 to each WebSocketServerService to simulate connected clients. 
        // We're parsing here so we can use this referenced in both WebApi and WebSocketServerService
        private static List<Guid> _mockConnectedClients = new List<Guid>()
        {
            { Guid.Parse("e7474f1b-8d92-40d5-baea-48b7dc48c56a") },
            { Guid.Parse("c9c14f26-c894-4ca9-9757-32d4bde07cce") },
            { Guid.Parse("ca7aac1d-3a82-4594-b3d5-e2ea434f3179") },
            { Guid.Parse("2c41af36-056e-40db-8fc0-2e8f038ae5bf") },
            { Guid.Parse("0e8c3555-af0e-4d6a-9823-2d294b64a3a0") },
            { Guid.Parse("40af78b1-83b8-4d22-aa31-9a87c1b91cd2") },
            { Guid.Parse("c4cfab1a-6e60-4e39-a4c3-f428f0c23082") },
            { Guid.Parse("05f63f28-8b91-4a0c-8163-0050d3abeee4") },
            { Guid.Parse("68138bfd-76f2-4000-bb99-9d95fd39517a") },
            { Guid.Parse("918b9342-00fd-4d4c-9466-5acf1379b96f") },
            { Guid.Parse("85132d1d-5ae5-4eb7-839d-019107984e8c") },
            { Guid.Parse("a0d16fe5-a459-4dab-ae6f-ee3fb73b32c3") },
            { Guid.Parse("015b8694-18b5-4065-9172-36c676753506") },
            { Guid.Parse("ab93dbe3-ca20-4cb1-9edb-3d13f93c4ec1") },
            { Guid.Parse("b0c1c5ca-a646-4967-835c-65362df9d032") },
            { Guid.Parse("5ddf1047-e0a1-48ad-9f56-ce4d728328bf") },
            { Guid.Parse("4907de1d-7ac6-47bf-bb3d-2f3e52a11954") },
            { Guid.Parse("e547e9bf-f0af-4d1a-8170-b50188ea8cc7") },
            { Guid.Parse("164fe8d7-940d-4d86-aa2d-4191f99d70d0") },
            { Guid.Parse("796294c7-a4b0-4823-8d2f-264e1bbc7637") }
        };

        public static Guid GetClientGuid(int index)
        {
           return _mockConnectedClients.ElementAt(index);
        }
    }
}
