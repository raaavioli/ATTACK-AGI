import socket
import sys
import signal
import cv2
import numpy as np

HEIGHT = 540
WIDTH = 960
RECT_HEIGHT = 90
RECT_WIDTH = 120

CARD_AREA_MIN = 5500
CARD_AREA_MAX = 6000
SUIT_AREA_MIN = 50
SUIT_AREA_MAX = 500

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
        data = recvFull(connection, 518400)
        if data:
            print("get data")

            img = np.frombuffer(data, dtype='uint8')
            img = np.reshape(img, (540, 960))
            result = analyzeImage(img)
            print(result)

            # Send the message to the Unity server.
            bytesToSend = result.encode("ascii")
            UDPClientSocket.sendto(bytesToSend, ("127.0.0.1", 50002))

def recvFull(connection, fullSize):
    fullData = b''
    full = False
    while not full:
        data = connection.recv(fullSize)
        fullData += data
        if len(fullData) >= fullSize:
            full = True
    
    return fullData


# Takes a full-size image of the SUR40 screen and returns the string that represents the positioning of the cards.
# The returned string is formatted as follows: "{player}:{position}:{suit}:{rank},{player}:{position}:{suit}:{rank},..."
# Where player is 1 for the left-side player and 2 is for the right-side player,
# position is a number 1-6 where 1 is the position at the top of the screen and 6 is the position at the bottom,
# suit is S for spades, C for clubs, D for diamonds and H for hearts,
# rank is a number 1-13 where 1 is an ace and 13 is a king.
def analyzeImage(image):
    subImages = splitImage(image)

    resultString = ""
    for i, subImage in enumerate(subImages):
        player = 1
        if i > 5: player = 2
        position = (i % 6) + 1
        resultString += analyzeSubImage(subImage, player, position)

    #cv2.imshow("image", image)
    cv2.waitKey(0)

    return resultString

# Splits a full-size image into 12 separate sub-images, one for each position.
def splitImage(image):
    subImages = []

    for i in range(12):
        xOffset = 0
        if i > 5: xOffset = WIDTH - RECT_WIDTH
        yOffset = (i % 6) * RECT_HEIGHT
        x1 = xOffset
        x2 = xOffset + RECT_WIDTH
        y1 = yOffset
        y2 = yOffset + RECT_HEIGHT
        subImages.append(image[y1:y2, x1:x2])

    return subImages

# Analyzes a single sub-image for the given player and position and returns the string for that particular sub-image.
# If nothing is found, an empty string is returned.
def analyzeSubImage(subImage, player, position):
    _, thresh = cv2.threshold(subImage,100,255,cv2.THRESH_BINARY)
    contours, hier = cv2.findContours(thresh,cv2.RETR_TREE,cv2.CHAIN_APPROX_SIMPLE)
    index_sort = sorted(range(len(contours)), key=lambda i : cv2.contourArea(contours[i]),reverse=True)

    # If there are no contours or no card is found, do nothing.
    if len(contours) == 0 or not (CARD_AREA_MIN <= cv2.contourArea(contours[0]) <= CARD_AREA_MAX):
        return ""

    # Count contours that are large enough but not too small to be a suit marker.
    rank = 0
    for contour in contours:
        if SUIT_AREA_MIN <= cv2.contourArea(contour) <= SUIT_AREA_MAX: rank += 1

    # Debug drawing.
    #img_contours = np.zeros(subImage.shape)
    #cv2.drawContours(img_contours, contours, -1, 255, 1)
    #cv2.imshow("thresh%d%d" % (player, position), thresh)
    #cv2.imshow("img_contours%d%d" % (player, position), img_contours)

    suit = "S"
    rank = max(1, min(rank, 6))
    return "%d:%d:%s:%d," % (player, position, suit, rank)

if __name__ == "__main__":
    main()
