import cv2

arr = []
for index in range(10):
    cap = cv2.VideoCapture(index)
    if cap.read()[0]:
        arr.append(index)
    cap.release()
    index += 1
print(arr)