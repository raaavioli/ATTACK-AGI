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

CARD_AREA_MIN = 5500
CARD_AREA_MAX = 6000
SUIT_AREA_MIN = 50
SUIT_AREA_MAX = 500

clubs = cv2.imread(file_path + "\..\sur40screenshots\club2.png", cv2.IMREAD_GRAYSCALE)

def main():
    path = file_path + "\..\sur40screenshots\correctedImage5.png"
    image = cv2.imread(path, cv2.IMREAD_GRAYSCALE)
    sharpen_kernel = np.array(
        [[-1,-1,-1,-1,-1], 
        [-1,-1,-1,-1,-1], 
        [-1,-1,25,-1,-1], 
        [-1,-1,-1,-1,-1],
        [-1,-1,-1,-1,-1]])
    sharpen = cv2.filter2D(image, -1, sharpen_kernel)
    
    median = cv2.medianBlur(sharpen, 5)

    size = (3, 3)
    shape = cv2.MORPH_RECT
    kernel = cv2.getStructuringElement(shape, size)
    min_image = cv2.dilate(sharpen, kernel)

    cv2.imshow("image", min_image)

    #print(analyzeImage(median))
    cv2.waitKey(0)

# Takes a full-size image of the SUR40 screen and returns the string that represents the positioning of the cards.
# The returned string is formatted as follows: "{player}:{position}:{suit}:{rank},{player}:{position}:{suit}:{rank},..."
# Where player is 1 for the left-side player and 2 is for the right-side player,
# position is a number 0-CARD_AREAS where 0 is the position at the top of the screen and 6 is the position at the bottom,
# suit is S for spades, C for clubs, D for diamonds and H for hearts,
# rank is a number 1-13 where 1 is an ace and 13 is a king.
def analyzeImage(image):
    subImages = splitImage(image)

    resultString = ""
    for i, subImage in enumerate(subImages):
        player = 1
        if i >= CARD_AREAS: player = 2
        position = (i % CARD_AREAS)
        resultString += analyzeSubImage(subImage, player, position)

    return resultString

# Splits a full-size image into 12 separate sub-images, one for each position.
def splitImage(image):
    subImages = []

    for i in range((2 * CARD_AREAS)):
        xOffset = 0
        if i >= CARD_AREAS: xOffset = WIDTH - RECT_WIDTH
        yOffset = (i % CARD_AREAS) * RECT_HEIGHT
        x1 = int(xOffset)
        x2 = int(xOffset + RECT_WIDTH)
        y1 = int(yOffset)
        y2 = int(yOffset + RECT_HEIGHT)
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

    img_contours = np.zeros(subImage.shape, dtype=np.uint8)
    cv2.drawContours(img_contours, contours, -1, 255, 1)
    #cv2.imshow("thresh%d%d" % (player, position), thresh)
    cv2.imshow("img_contours%d%d" % (player, position), img_contours)

    H, W = clubs.shape 
    result = cv2.matchTemplate(img_contours, clubs, cv2.TM_CCORR)
    min_val, max_val, min_loc, max_loc = cv2.minMaxLoc(result)
    
    suit = "S"
    location = max_loc
    bottom_right = (location[0] + W, location[1] + H)
    if (bottom_right[0] > 30):
        suit = "C"
        cv2.rectangle(img_contours, location,bottom_right, 255, 1)
        cv2.imshow("img%d%d" % (player, position), img_contours)

    # Sobel testing
    # sobel = cv2.Sobel(subImage,cv2.CV_64F,1,1,ksize=15)
    # cv2.imshow("sobel%d%d" % (player, position), sobel)

    rank = max(1, min(rank, 6))
    return "%d:%d:%s:%d," % (player, position, suit, rank)

if __name__ == "__main__":
    main()