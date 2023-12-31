﻿using Microsoft.AspNetCore.SignalR;
using Server.Models;

namespace Server.Hubs
{
    public class PartyHub : Hub
    {
        private readonly Dictionary<string, Party> m_parties = new();

        public PartyHub(Dictionary<string, Party> parties)
            => m_parties = parties;

        public async Task CreateParty(string username)
        {
            if (string.IsNullOrEmpty(username))
                return;

            await Console.Out.WriteLineAsync($"{username} creating a party...");

            //Recreate while it's unique
            string partyId = CreateId();
            while (m_parties.ContainsKey(partyId))
                partyId = CreateId();

            var party = new Party(partyId);
            party.Players.Add(new Player(Context.ConnectionId)
            {
                Username = username,
                IsAdmin = true,
            });

            m_parties.Add(partyId, party);

            await Clients.Caller.SendAsync("PartyCreated", partyId);
            await Groups.AddToGroupAsync(Context.ConnectionId, partyId);
            await Clients.Group(party.Id).SendAsync("ReceivePlayers", party.Players);

            await Console.Out.WriteLineAsync($"{username} created a party ({partyId})");
        }

        public async Task StartParty(string partyId)
        {
            if (!await CheckAdminStatus(partyId))
                return;

            await Clients.Group(partyId).SendAsync("GameStarted");
        }

        public Task<bool> CheckAdminStatus(string partyId)
        {
            if (!m_parties.TryGetValue(partyId, out var party))
                return Task.FromResult(false);

            var player = party.Players.FirstOrDefault(x => x.Identifier == Context.ConnectionId);
            return Task.FromResult(player != null && player.IsAdmin); 
        }

        public async Task JoinParty(string partyId, string username)
        {
            Console.WriteLine("Joining...");

            if (string.IsNullOrEmpty(username))
                return;

            if (!m_parties.TryGetValue(partyId, out Party? party))
            {
                await SendError("Party doesn't exists");
                return;
            }

            if (m_parties.Count == 0)
            {
                await SendError("There is no party !");
                return;
            }

            await Console.Out.WriteLineAsync($"{username} try to join {party.Id}...");
/*
            if (!m_parties.TryGetValue(partyId, out var party))
            {
                await SendError("Party doesn't exists.");
                return;
            }
*/
            if (party.Players.Any(x => x.Username == username))
            {
                await SendError("Player with the same username exists.");
                return;
            }

            await Console.Out.WriteLineAsync($"{username} joined {party.Id}.");

            party.Players.Add(new Player(Context.ConnectionId)
            {
                IsAdmin = false,
                Username = username,
            });

            //Adding to group and sending a response to inform him that it successfully joined the party
            await Clients.Caller.SendAsync("PartyJoined", party.Id);

            //Notifying other users that a player joined the party
            await Groups.AddToGroupAsync(Context.ConnectionId, party.Id);
            await Clients.Group(party.Id).SendAsync("ReceivePlayers", party.Players);
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var party in m_parties.Values)
            {
                foreach (var player in party.Players.ToArray())
                {
                    if (player.Identifier == Context.ConnectionId)
                    {
                        await Console.Out.WriteLineAsync("Found to disconnect");
                        party.Players.Remove(player);

                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, party.Id);
                        await Clients.Group(party.Id).SendAsync("ReceivePlayers", party.Players);
                    }
                }
            }

            await Console.Out.WriteLineAsync($"Client disconnected ({Context.ConnectionId})");
        }

        private async Task SendError(string errorMessage)
        {
            await Console.Out.WriteLineAsync($"Error: {errorMessage}");
            await Clients.Caller.SendAsync("Error", errorMessage);
        }

        private static string CreateId()
        {
            string s = string.Empty;

            for (int i = 0; i < 10; i++)
            {
                int c = Random.Shared.Next(0, 2);
                switch (c)
                {
                    case 0:
                        s += (char)Random.Shared.Next('A', 'Z' + 1);
                        break;
                    case 1:
                        s += Random.Shared.Next(10);
                        break;
                }
            }

            return s;
        }

        public async Task StartGame(string partyId)
        {
            if (m_parties.TryGetValue(partyId, out var party))
            {
                if (party.Players.Count < 2)
                {
                    // Envoyer un message d'erreur au client
                    await Clients.Caller.SendAsync("SendError", "Vous devez être au moins deux joueurs pour démarrer le jeu.");
                }
                else
                {
                    // Exécuter la logique pour démarrer le jeu
                    // ...
                    await Clients.Group(partyId).SendAsync("GameStarted");
                }
            }
            else
            {
                // La partie n'existe pas, gérer l'erreur appropriée
                await Clients.Caller.SendAsync("SendError", "La partie spécifiée n'existe pas.");
            }
        }
    }
}
