import socket
import sys
import signal
import cv2

def signal_handler(sig, frame):
    print('interrupted, shutting down server.')
    sys.exit(0)


def main():
    signal.signal(signal.SIGINT, signal_handler)

    # Create a TCP/IP socket.
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    # Bind the socket to the port.
    server_address = ("127.0.0.1", 50001)
    sock.bind(server_address)
    print("started server")

    # Listen for incoming connections.
    sock.listen(1)

    # Wait for a connection.
    connection, client_address = sock.accept()
    print("connection from " + str(client_address))

    # Create a datagram socket
    UDPClientSocket = socket.socket(
        family=socket.AF_INET, type=socket.SOCK_DGRAM)

    while True:
        data = connection.recv(518400)
        if data:
            print("get data")
            result = analyzeImage(data)
            dataSum = sum(data)
            result += str(dataSum)
            print(result)
            # Sending a message to server
            bytesToSend = result.encode("ascii")
            UDPClientSocket.sendto(bytesToSend, ("127.0.0.1", 50002))


def analyzeImage(image):
    return "test"

def splitImage(image):
    return None

def analyzeSubImage(subImage):
    return None

if __name__ == "__main__":
    main()
