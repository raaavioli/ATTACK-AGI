import signal
import socket
import sys
from time import sleep
import numpy as np
import base64


def signal_handler(sig, frame):
    print('interrupted, shutting down client.')
    sys.exit(0)

def main():
    signal.signal(signal.SIGINT, signal_handler)

    # Create a datagram socket
    UDPClientSocket = socket.socket(
        family=socket.AF_INET, type=socket.SOCK_DGRAM )

    print("UDP client up and sending")

    while True:
        # Sending a message to server
        bytesToSend = np.random.bytes(16384)
        UDPClientSocket.sendto(bytesToSend, ("127.0.0.1", 50001))
        sleep(1)

if __name__ == "__main__":
    main()
