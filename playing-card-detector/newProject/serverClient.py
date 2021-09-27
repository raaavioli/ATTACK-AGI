import socket
import sys
import signal
import cv2
import numpy as np
from PIL import Image

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

            img = np.frombuffer(data, dtype='uint8')
            img = np.reshape(img, (540, 960))
            #img = Image.fromarray(img)
            #img.save("testImg.png")
            #img = cv2.imread("testImg.png")
            #cv2.imshow("testImg", img)
            #img = Image.fromarray(img.astype(np.uint8))
            #img = cv2.cvtColor(img, cv2.COLOR_GRAY2BGR)
            #img = cv2.imdecode(img, cv2.IMREAD_GRAYSCALE)
            #if img.any():
                #print(img)
            #cv2.imshow("testImg", img)
            #img = cv2.resize(img, (960, 540))
            #cv2.imshow("testImg", img)

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
