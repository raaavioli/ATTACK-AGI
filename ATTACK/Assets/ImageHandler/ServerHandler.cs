using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

public class ServerHandler : MonoBehaviour {
    
    public enum Suit {
        SPADES,
        CLUBS,
        DIAMONDS,
        HEARTS
	}

    public struct CardPosition {
        int player;
        int position;
        Suit suit;
        int rank;

        public CardPosition(int player, int position, Suit suit, int rank) {
            this.player = player;
            this.position = position;
            this.suit = suit;
            this.rank = rank;
		}

        public override string ToString() {
            return $"player: {player}, position: {position}, suit: {suit}, rank: {rank}";
        }
    }
    
    private byte[] data;

    Socket socket;
    EndPoint senderRemote;

    void Awake() {
        data = new byte[128];

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50002);

        socket = new Socket(endPoint.Address.AddressFamily,
            SocketType.Dgram,
            ProtocolType.Udp);

        // Creates an IPEndPoint to capture the identity of the sending host.
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        senderRemote = sender;

        // Binding is required with ReceiveFrom calls.
        socket.Bind(endPoint);
        Console.WriteLine("Waiting to receive datagrams from client...");
        socket.Blocking = false;
    }

    void Update() {
        if (socket.Available > 0) {
            socket.ReceiveFrom(data, ref senderRemote);
            Debug.Log(socket.Available);
            string cards = Encoding.ASCII.GetString(data);
            Debug.Log(cards);
            CardPosition[] positions = ParseCards(cards);
            foreach (CardPosition position in positions) {
                Debug.Log(position);
			}
        }
    }

    private CardPosition[] ParseCards(string cards) {
        List<CardPosition> positions = new List<CardPosition>();
        string[] cardStrings = cards.Split(',');


        for (int i = 0; i < cardStrings.Length - 1; ++i) {
            string cardPosition = cardStrings[i];
            string[] parts = cardPosition.Split(':');
            Suit suit = Suit.SPADES;
            switch (parts[2]) {
                case "C":
                    suit = Suit.CLUBS;
                    break;
                case "D":
                    suit = Suit.DIAMONDS;
                    break;
                case "H":
                    suit = Suit.HEARTS;
                    break;
			}
            positions.Add(new CardPosition(int.Parse(parts[0]), int.Parse(parts[1]), suit, int.Parse(parts[3])));
		}

        return positions.ToArray();
	}

    private void OnDestroy() {
        socket.Close();
    }
}