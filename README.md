# ATTACK
This project aims to develop an interactive game played on a SUR40 monitor.
Development is mainly done as part of the course DH2413 Advanced Graphics and Interaction 
given at KTH, Royal Institute of Technology.

## Gameplay
The game is played using cards, similar to how you would play traditional card 
games such as "Yu Gi Oh!" or "Magic the Gathering", by placing them on the SUR40 monitor.

When cards are placed onto the screen, characters are summoned for each of the two players.
The characters then fight, and the last man standing wins!

## Game website

[https://raaavioli.github.io/ATTACK-AGI/](https://raaavioli.github.io/ATTACK-AGI/)

## Installation instructions

To run the program, download the "Final demo version" release and run one of the batch files.

There are four batch files:
1. play.bat
2. debug.bat
3. playInteractive.bat
4. debugInteractive.bat

The first and second one will only work when running on a SUR-40 device, since it runs the software that reads from the Microsoft Pixelsense. The third and fourth ones work on any windows computer (within reason), since the Pixelsense reading is simulated using an interactive client.

The running of the game is really based on three programs. First, the ATTACK.exe is the game as built from Unity. The RawImageVisualizer.exe is a retrofitted demo program for the SUR-40, with modifications for sending the data to Unity. The serverClient.py is a python program that sits between the two executables. In normal operating mode on the SUR-40, it receives the image from RawImageVisualizer over TCP on localhost, then analyzes the picture and sends the interpreted card information to ATTACK over UDP on localhost. If the -i flag is sent to serverClient, nothing is read from RawImageVisualizer, instead the used inputs the card information and chooses to send it on to ATTACK. If the -v flag is sent to serverClient, it will be verbose and output all kinds of debug information.

## Tools
- Unity
- SUR40 (PixelSense)
- Blender
- OpenCV

## Group members
- Anders Steen (Interaction & Art)
- Samuel Westman Granlund (Interaction)
- Tobias Hansson (Gameplay)
- Filip Berendt (Gameplay, Art & Graphics)
- Oliver Eriksson (Graphics/VFX)