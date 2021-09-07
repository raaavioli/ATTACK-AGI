import random
import signal
import socket
import sys
import time


def generateMessages():
    players = {"P1", "P2"}
    cardPositions = {"C1", "C2", "C3"}
    suits = {"H", "D", "C", "S"}
    possibleMessages = []
    for player in players:
        for cardPosition in cardPositions:
            for suit in suits:
                for x in range(2, 5):
                    possibleMessages.append(
                        player + cardPosition + suit + str(x))

    return possibleMessages


def signal_handler(sig, frame):
    print('interrupted, shutting down client.')
    sys.exit(0)


def processPixelsenseData(possibleMessages):
    time.sleep(1)
    return random.choice(possibleMessages)


def main():
    signal.signal(signal.SIGINT, signal_handler)

    possibleMessages = generateMessages()

    # Create a datagram socket
    UDPClientSocket = socket.socket(
        family=socket.AF_INET, type=socket.SOCK_DGRAM)

    print("UDP client up and sending")

    # Process pixelsense data and send datagrams
    while True:
        # Sending a message to server
        bytesToSend = str.encode(processPixelsenseData(possibleMessages))
        UDPClientSocket.sendto(bytesToSend, ("127.0.0.1", 20001))


if __name__ == "__main__":
    main()
