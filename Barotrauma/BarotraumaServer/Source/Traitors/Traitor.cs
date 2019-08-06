using Barotrauma.Networking;
using Lidgren.Network;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
    partial class Traitor
    {
        public readonly Character Character;

        public string Role { get; private set; }
        public TraitorMission Mission { get; private set; }
        public Objective CurrentObjective => Mission.GetCurrentObjective(this);

        public Traitor(TraitorMission mission, string role, Character character)
        {
            Mission = mission;
            Role = role;
            Character = character;
            Character.IsTraitor = true;
            GameMain.NetworkMember.CreateEntityEvent(Character, new object[] { NetEntityEvent.Type.Status });
        }

        public void Greet(GameServer server, string codeWords, string codeResponse)
        {
            string greetingMessage = TextManager.FormatServerMessage(Mission.StartText, new string[] {
                "[codewords]", "[coderesponse]"
            }, new string[] {
                codeWords, codeResponse
            });

            SendChatMessage(greetingMessage);
            SendChatMessageBox(greetingMessage);

            Client traitorClient = server.ConnectedClients.Find(c => c.Character == Character);
            Client ownerClient = server.ConnectedClients.Find(c => c.Connection == server.OwnerConnection);
            if (traitorClient != ownerClient && ownerClient != null && ownerClient.Character == null)
            {
                var ownerMsg = ChatMessage.Create(
                    null,//TextManager.Get("NewTraitor"),
                    CurrentObjective.StartMessageServerText,
                    ChatMessageType.ServerMessageBox,
                    null
                );
                GameMain.Server.SendDirectChatMessage(ownerMsg, ownerClient);
            }
        }

        public void SendChatMessage(string serverText)
        {
            Client client = GameMain.Server.ConnectedClients.Find(c => c.Character == Character);
            GameMain.Server.SendDirectChatMessage(ChatMessage.Create("", serverText, ChatMessageType.Server, null), client);
        }

        public void SendChatMessageBox(string serverText)
        {
            Client client = GameMain.Server.ConnectedClients.Find(c => c.Character == Character);
            GameMain.Server.SendDirectChatMessage(ChatMessage.Create("", serverText, ChatMessageType.ServerMessageBox, null), client);
        }

        public void UpdateCurrentObjective(string objectiveText)
        {
            Client traitorClient = GameMain.Server.ConnectedClients.Find(c => c.Character == Character);
            Character.TraitorCurrentObjective = objectiveText;
            GameMain.Server.SendTraitorCurrentObjective(traitorClient, Character.TraitorCurrentObjective);
        }
    }
}
