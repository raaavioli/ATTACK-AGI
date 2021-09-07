import signal
import socket
import sys


def signal_handler(sig, frame):
    print('interrupted, shutting down server.')
    sys.exit(0)


def main():
    signal.signal(signal.SIGINT, signal_handler)

    localIP = "127.0.0.1"
    localPort = 20001
    bufferSize = 1024

    # Create a datagram socket
    UDPServerSocket = socket.socket(
        family=socket.AF_INET, type=socket.SOCK_DGRAM)

    # Bind to address and ip
    UDPServerSocket.bind((localIP, localPort))

    print("UDP server up and listening")

    # Listen for incoming datagrams
    while True:
        bytesAddressPair = UDPServerSocket.recvfrom(bufferSize)

        message = bytesAddressPair[0]
        address = bytesAddressPair[1]

        clientMsg = "Message from Client: {}".format(message)
        clientIP = "Client IP Address: {}".format(address)

        print(clientMsg)
        print(clientIP)


if __name__ == "__main__":
    main()
