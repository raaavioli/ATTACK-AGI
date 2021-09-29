import numpy as np
import cv2
import socket
import sys
import signal
import time

sock = None

def signal_handler(sig, frame):
    print('interrupted, shutting down client.')
    if sock != None:
        sock.close()
    sys.exit(0)

def main():
    signal.signal(signal.SIGINT, signal_handler)

    path = "playing-card-detector/newProject/sur40screenshots/correctedImage.png"
    image = cv2.imread(path, cv2.IMREAD_GRAYSCALE)
    bytesToSend = image.tobytes()

    # Create a TCP/IP socket
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    # Connect the socket to the port where the server is listening
    server_address = ('127.0.0.1', 50001)
    sock.connect(server_address)

    # Send data
    while True:
        sock.sendall(bytesToSend)
        time.sleep(0.1)

if __name__ == "__main__":
    main()