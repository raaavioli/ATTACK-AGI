import re
import socket
import sys
import time


# This is the "main" function of the interactive client.
def startInteractiveClient():
    commands = ["add", "remove", "rotate", "quit", "status", "send", "clear", "help"]
    cards = []

    while True:
        line = input("$ ")
        tokens = line.rstrip().split(" ")
        command = tokens[0]

        udpSocket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
        
        if command == commands[0]:
            if processAdd(tokens, cards):
                print(f"added card {tokens[1]}, status \"{concatenateCards(cards)}\"")
            else:
                print("error adding card, usage \"add [12]:[1-5]:[1-6]:[01]\"")
        elif command == commands[1]:
            if processRemove(tokens, cards):
                print(f"removed card in position {tokens[1][0:3]}, status \"{concatenateCards(cards)}\"")
            else:
                print("error removing card, usage \"remove [12]:[1-5]\"")
        elif command == commands[2]:
            if processRotate(tokens, cards):
                print(f"rotated card in position {tokens[1][0:3]}, status \"{concatenateCards(cards)}\"")
            else:
                print("error rotating card, usage \"rotate [12]:[1-5]\"")
        elif command == commands[3]:
            print("")
            break
        elif command == commands[4]:
            print(concatenateCards(cards))
        elif command == commands[5]:
            if processSend(tokens, cards, udpSocket):
                print(f"successfully sent {tokens[1]} times")
            else:
                print("error sending data, usage \"send [0-9]+\"")
        elif command == commands[6]:
            cards.clear()
            print("cards were cleared")
        elif command == commands[7]:
            print(f"commands are {commands}")
        else:
            print("something went wrong, type \"help\" to get a list of commands")




# Input processing functions:

def processAdd(tokens, cards):
    if len(tokens) != 2 or not verifyCardInput(tokens[1]):
        return False
    
    card = tokens[1]

    foundCard, index = findCard(card, cards)

    if foundCard:
        cards[index] = card
    else:
        cards.append(card)

    return True

def processRemove(tokens, cards):
    if len(tokens) != 2 or (not verifyCardInput(tokens[1]) and not verifyPositionInput(tokens[1])):
        return False
    
    position = tokens[1]

    foundCard, index = findCard(position, cards)

    if foundCard:
        cards.pop(index)

    return foundCard

def processRotate(tokens, cards):
    if len(tokens) != 2 or (not verifyCardInput(tokens[1]) and not verifyPositionInput(tokens[1])):
        return False
    
    position = tokens[1]

    foundCard, index = findCard(position, cards)

    if foundCard:
        newRotation = not bool(int(cards[index][-1]))
        cards[index] = cards[index][:6] + str(int(newRotation))

    return foundCard

def processSend(tokens, cards, udpSocket):
    if len(tokens) != 2:
        return False

    repetitions = 1
    try:
        repetitions = int(tokens[1])
    except ValueError:
        return False

    result = concatenateCards(cards)

    if result == "":
        result = "empty"

    bytesToSend = result.encode("ascii")

    try:
        for _ in range(repetitions):
            udpSocket.sendto(bytesToSend, ("127.0.0.1", 50002))
            print("sent data")
            time.sleep(0.1)
    except socket.error:
        return False
    
    return True




# Helper functions:

def concatenateCards(cards):
    return ",".join(cards)

def findCard(position, cards):
    parsedPosition = parsePosition(position)
    
    for i, savedCard in enumerate(cards):
        parsedSavedCard = parseCard(savedCard)
        if parsedPosition["position"] == parsedSavedCard["position"] and parsedPosition["player"] == parsedSavedCard["player"]:
            return (True, i)

    return (False, -1)

def parseCard(cardString):
    cardParts = cardString.split(":")
    return { "player": cardParts[0], "position": cardParts[1], "rank": cardParts[2], "rotated": cardParts[3] }

def parsePosition(positionString):
    positionParts = positionString.split(":")
    return { "player": positionParts[0], "position": positionParts[1] }

def verifyCardInput(cardString):
    return bool(re.match("[12]:[1-5]:[1-6]:[01]", cardString))

def verifyPositionInput(positionString):
    return bool(re.match("[12]:[1-5]", positionString))
