# This file is for developing the cv functions offline, where there is no server to worry about.

import cv2
import numpy as np
import os

file_path = os.path.dirname(__file__)

CARD_AREAS = 5
HEIGHT = 540
WIDTH = 960
RECT_HEIGHT = HEIGHT / CARD_AREAS
RECT_WIDTH = RECT_HEIGHT * 8.8 / 5.8 #dimension of playing card

CARD_AREA_MIN = 5000
CARD_AREA_MAX = 6000
SUIT_AREA_MIN = 100
SUIT_AREA_MAX = 1000

clubs = cv2.imread(file_path + "\..\sur40screenshots\club2.png", cv2.IMREAD_GRAYSCALE)

def main():
    path = file_path + "\..\sur40screenshots\correctedImage3-2.png"
    image = suitAreaToSquare(cv2.imread(path, cv2.IMREAD_GRAYSCALE))
    print(analyzeImage(image))
    cv2.waitKey(0)

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
    median = cv2.medianBlur(sharpen, 9)
    

    size = (6, 6)
    shape = cv2.MORPH_RECT
    kernel = cv2.getStructuringElement(shape, size)
    max_image = cv2.dilate(median, kernel)

    size = (9, 9)
    shape = cv2.MORPH_RECT
    kernel = cv2.getStructuringElement(shape, size)
    min_image = cv2.erode(max_image, kernel)

    cv2.imshow("kernel result", min_image)
    return min_image
    

# Takes a full-size image of the SUR40 screen and returns the string that represents the positioning of the cards.
# The returned string is formatted as follows: "{player}:{position}:{suit}:{rank},{player}:{position}:{suit}:{rank},..."
# Where player is 1 for the left-side player and 2 is for the right-side player,
# position is a number 1-6 where 1 is the position at the top of the screen and 6 is the position at the bottom,
# suit is S for spades, C for clubs, D for diamonds and H for hearts,
# rank is a number 1-13 where 1 is an ace and 13 is a king.
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
# for each card, a string is created containing player_id, position, suit, rank, angle.
# If no card is found, an empty string is returned.
def analyzeSubImage(subImage, player):
    _, thresh = cv2.threshold(subImage,100,255,cv2.THRESH_BINARY)
    contours, hier = cv2.findContours(thresh,cv2.RETR_TREE,cv2.CHAIN_APPROX_SIMPLE)
    index_sort = sorted(range(len(contours)), key=lambda i : cv2.contourArea(contours[i]),reverse=True)

    # If there are no contours or no card is found, do nothing.
    if len(contours) == 0:
        return ""

    resultString = ""
    foundCards = 0
    currentCardIndex = 0
    while(foundCards < CARD_AREAS and currentCardIndex < len(contours) and isCardContour(contours[currentCardIndex])):
        _, y, w, h = cv2.boundingRect(contours[currentCardIndex])
        rotated = int(w < h)
        position = int((y + h / 2) / RECT_HEIGHT)
        rank = 0
        suitMarkerIndex = currentCardIndex + 1
        while(suitMarkerIndex < len(contours) and isSuitContour(contours[suitMarkerIndex])): 
            rank+=1
            suitMarkerIndex+=1
        resultString += "%d:%d:%d:%d," % (player, position, rank, rotated)
        currentCardIndex = suitMarkerIndex
        foundCards +=1
    
    return resultString

def isCardContour(contour):
    return (CARD_AREA_MIN <= cv2.contourArea(contour) <= CARD_AREA_MAX)

def isSuitContour(contour):
    return (SUIT_AREA_MIN <= cv2.contourArea(contour) <= SUIT_AREA_MAX)

if __name__ == "__main__":
    main()