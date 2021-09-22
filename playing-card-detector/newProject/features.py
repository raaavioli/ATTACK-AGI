import cv2
import numpy as np
import matplotlib.pyplot as plt

template = cv2.imread("fullAceSmall.png")
orb = cv2.ORB_create()
kp1, des1 = orb.detectAndCompute(template, None)

capture = cv2.VideoCapture(0)

_, img = capture.read()

while True:
    _, frame = capture.read()

    cv2.imshow("Image", img)

    kp2, des2 = orb.detectAndCompute(img, None)
    bf = cv2.BFMatcher(cv2.NORM_HAMMING, crossCheck = True)
    matches = bf.match(des1, des2)
    matches = sorted(matches, key = lambda x: x.distance)
    matchImg = cv2.drawMatches(template, kp1, img, kp2, matches[:10], None, flags=2)
    cv2.imshow("Matches", matchImg)

    key = cv2.waitKey(0) & 0xFF

    if key == ord('p'):
        img = frame

    if key == ord('q'):
        break

capture.release()
cv2.destroyAllWindows()