# This program receives image data from a client over TCP, then analyzes the image to recognize cards, then finally send the interpretation to Unity via UDP.
import socket
import sys
import signal
import cv2
import numpy as np
import os
from interactiveClient import startInteractiveClient
import traceback

file_path = os.path.dirname(__file__)

CARD_AREAS = 5
HEIGHT = 540
WIDTH = 960
RECT_HEIGHT = HEIGHT / CARD_AREAS
RECT_WIDTH = RECT_HEIGHT * 8.8 / 5.8 #dimension of playing card

CARD_AREA_MIN = 3500
CARD_AREA_MAX = 6000
SUIT_AREA_MIN = 90
SUIT_AREA_MAX = 300

def signal_handler(sig, frame):
    print('interrupted, shutting down server.')
    sys.exit(0)

def main():
    signal.signal(signal.SIGINT, signal_handler)

    printing = False
    if "-v" in sys.argv:
        printing = True

    if "-i" in sys.argv:
        startInteractiveClient()
        return

    cv2.startWindowThread()
    cv2.namedWindow("server_image")

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
            if printing:
                print("get data")

            try:
                img = np.frombuffer(data, dtype='uint8')
                img = np.reshape(img, (540, 960))
                result = analyzeImage(suitAreaToSquare(img))

                if result == "":
                    result = "empty"
                
                if printing:
                    print(result)

                # Send the message to the Unity server.
                bytesToSend = result.encode("ascii")
                UDPClientSocket.sendto(bytesToSend, ("127.0.0.1", 50002))
            except:
                print("something went wrong, nothing was processed or sent")
                traceback.print_exc()

def recvFull(connection, fullSize):
    fullData = b''
    full = False
    while not full:
        data = connection.recv(fullSize)
        fullData += data
        if len(fullData) >= fullSize:
            full = True
    
    return fullData

# Takes an image and applies: 
# sharpening -> median filter -> max filter -> min filter
# @returns an image where all suit shapes are transformed into equally sized squares
def suitAreaToSquare(image):
    sharpen_kernel = np.array(
        [[-1,-1,-1,-1,-1],
        [-1,-1,-1,-1,-1],
        [-1,-1,25,-1,-1],
        [-1,-1,-1,-1,-1],
        [-1,-1,-1,-1,-1]])
    sharpen = cv2.filter2D(image, -1, sharpen_kernel)
    median = cv2.medianBlur(sharpen, 7)
    
    size = (6, 6)
    shape = cv2.MORPH_RECT
    kernel = cv2.getStructuringElement(shape, size)
    max_image = cv2.dilate(median, kernel)

    size = (9, 9)
    shape = cv2.MORPH_RECT
    kernel = cv2.getStructuringElement(shape, size)
    min_image = cv2.erode(max_image, kernel)

    _, thresh = cv2.threshold(min_image,128,255,cv2.THRESH_BINARY)
    cv2.imshow("server_image", thresh)
    cv2.waitKey(20)

    return thresh

# Takes a full-size image of the SUR40 screen and returns the string that represents the positioning of the cards.
# The returned string is formatted as follows: "{player}:{position}:{rank}:{rotated},{player}:{position}:{rank}:{rotated},..."
# Where player is 1 for the left-side player and 2 is for the right-side player,
# position is a number 0-5 where 1 is the position at the top of the screen and 6 is the position at the bottom,
# rank is a number 1-5 where 1 is an ace and 5 is a 5,
# rotated is a number 0 or 1 where 0 is when the card is not rotated.
def analyzeImage(image):
    subImages = []
    subImages.append(image[:, 0:int(RECT_WIDTH)])
    subImages.append(image[:, int(WIDTH-RECT_WIDTH) : WIDTH])

    resultString = ""
    for i, subImage in enumerate(subImages):
        player = i + 1
        resultString += analyzeSubImage(subImage, player)

    return resultString

# Analyzes one player's part of the image with up to "CARD_AREAS" number of cards.
# for each card, a string is created containing player_id, position, rank, rotated.
# If no card is found, an empty string is returned.
def analyzeSubImage(subImage, player):
    contours, hier = cv2.findContours(subImage,cv2.RETR_TREE,cv2.CHAIN_APPROX_SIMPLE)
    index_sort = sorted(range(len(contours)), key=lambda i : cv2.contourArea(contours[i]),reverse=True)

    # If there are no contours or no card is found, do nothing.
    if len(contours) == 0:
        return ""

    foundCards = 0
    currentCardIndex = 0
    cardInfos = []
    seenPositions = []
    while((foundCards < CARD_AREAS) and (currentCardIndex < len(contours))):
        if not isCardContour(contours[currentCardIndex]):
            currentCardIndex+=1
            continue
        _, y, w, h = cv2.boundingRect(contours[currentCardIndex])
        rotated = int(w < h)
        position = int((y + h / 2.0) / float(RECT_HEIGHT))
        rank = 0
        suitMarkerIndex = currentCardIndex + 1
        while(suitMarkerIndex < len(contours) and not isCardContour(contours[suitMarkerIndex])):
            if isSuitContour(contours[suitMarkerIndex]):
                rank+=1
            suitMarkerIndex+=1

        if rank == 0:
            rank = 1

        # Make sure there are no duplicate positions
        while position in seenPositions:
            position += 1

        if position <= CARD_AREAS:
            seenPositions.append(position)
            cardInfos.append((player, position, rank, rotated))
            currentCardIndex = suitMarkerIndex
            foundCards +=1
        
    
    resultString = ""
    for cardInfo in cardInfos:
        resultString += "%d:%d:%d:%d," % cardInfo

    return resultString

def isCardContour(contour):
    return (CARD_AREA_MIN <= cv2.contourArea(contour) <= CARD_AREA_MAX)

def isSuitContour(contour):
    return (SUIT_AREA_MIN <= cv2.contourArea(contour) <= SUIT_AREA_MAX)

if __name__ == "__main__":
    main()
