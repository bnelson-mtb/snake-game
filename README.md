```
Authors: Brady Nelson and Charles Adair
Start Date: 11/5/25
Course: CS 3500, University of Utah, School of Computing
GitHub IDs: bnelson-mtb & big-chuz
Repo: https://github.com/uofu-cs3500-20-fall2025/assignment-eight-chatting-brady_charlie_game
Commit Date: 12/4/25
Solution: Chatting
Copyright: CS 3500, Brady Nelson, Charles Adair
This work may not be copied for use in Academic Coursework.
```

# Overview of the Snake Client
The Snake Client is a Blazor app written in C# that connects to a remote Snake game server using a custom
networking library from the "Chatting" solution.
Once connected, the client listens for continuous game-state updates from the server, deserializes those JSON messages
into model objects, and draws the entire world on an HTML canvas at high frame rates. At the same time, it captures the
player’s keyboard input (WASD/Arrow Keys) and sends movement commands back to the server. This creates a full
client-server gameplay loop where the server controls the game logic and the client focuses on rendering and user input.

# Time Expenditures:
1. Assignment Eight - Predicted Hours: 5 - Actual Hours: 5
2. Assignment Nine - Predicted Hours: 12 - Actual Hours: 15
3. Assignment Ten - Predicted Hours: 6 - Actual Hours:

# Use of AI*
**Google NotebookLM**: Used to rephrase and better explain assignment instructions and lectures slides.
**ChatGPT & Google Gemini**: Used to improve syntax and understanding of C#.

```
* See individual project READMEs for more specific descriptions on use of AI.
```
