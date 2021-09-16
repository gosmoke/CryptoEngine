using System;
using System.Collections.Generic;
using Crypto.Models;
using Newtonsoft.Json;
using WebSocketSharp;

namespace TerdyCoin.Services.P2P
{
    public class P2PClient
    {
        IDictionary<string, WebSocket> wsDict = new Dictionary<string, WebSocket>();

        public void Connect(string url)
        {
            if (!wsDict.ContainsKey(url))
            {
                WebSocket ws = new WebSocket(url);
                ws.OnMessage += Ws_OnMessage;

                ws.Connect();
                ws.Send("Hi Server");
                ws.Send(JsonConvert.SerializeObject(Program.TurdyCoin));

                wsDict.Add(url, ws);
            }
        }

        public void Send(string url, string data)
        {
            foreach (var item in wsDict)
            {
                if (item.Key == url)
                    item.Value.Send(data);
            }
        }

        public void Broadcast(string data)
        {
            foreach (var item in wsDict)
                item.Value.Send(data);
        }

        public IList<string> GetServers()
        {
            IList<string> servers = new List<string>();
            foreach (var item in wsDict)
                servers.Add(item.Key);

            return servers;
        }

        public void Close()
        {
            foreach (var item in wsDict)
                item.Value.Close();
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Data == "Hi Client")
            {
                Console.Write(e.Data);
            }
            else
            {
                BlockChain newChain = JsonConvert.DeserializeObject<BlockChain>(e.Data);
                if (newChain.IsValid() && newChain.Chain.Count > Program.TurdyCoin.Chain.Count)
                {
                    List<Transaction> newTransactions = new List<Transaction>();
                    newTransactions.AddRange(newChain.PendingTransactions);
                    newTransactions.AddRange(Program.TurdyCoin.PendingTransactions);

                    newChain.PendingTransactions = newTransactions;
                    Program.TurdyCoin = newChain;
                }
            }
        }
    }
}
