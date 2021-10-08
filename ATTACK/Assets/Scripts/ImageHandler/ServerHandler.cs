using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine.Events;
using static GameManager;

public class ServerHandler : MonoBehaviour {
    
    public enum Suit {
        SPADES,
        CLUBS,
        DIAMONDS,
        HEARTS
	}

    public struct CardPosition {
        public Team team;
        public int position;
        public Suit suit;
        public int rank;

        public CardPosition(Team team, int position, Suit suit, int rank) {
            this.team = team;
            this.position = position;
            this.suit = suit;
            this.rank = rank;
		}

        public override string ToString() {
            return $"team: {team}, position: {position}, suit: {suit}, rank: {rank}";
        }
    }

    public static CardPosition[] cardInformation { get; private set; }
    public static UnityAction onCardDataReceived;

    private byte[] data;
    private EndPoint senderRemote;
    private ServerHandler instance;
    private Socket socket;

    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(this);
        }

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
        socket.Blocking = false;
    }

    void Update() {
        if (socket.Available > 0) {
            socket.ReceiveFrom(data, ref senderRemote);
            string cards = Encoding.ASCII.GetString(data);
            Debug.Log(cards);
            cardInformation = ParseCards(cards);
            if (onCardDataReceived != null) {
                onCardDataReceived();
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
            positions.Add(new CardPosition((Team) int.Parse(parts[0]) - 1, int.Parse(parts[1]), suit, int.Parse(parts[3])));
		}

        return positions.ToArray();
	}

    private void OnDestroy() {
        socket.Close();
    }
}