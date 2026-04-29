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
networking library from the "Chatting" solution. Once connected, the client listens for continuous game-state updates
from the server, deserializes those JSON messages into model objects, and draws the entire world on an HTML canvas at
high frame rates. At the same time, it captures the player’s keyboard input (WASD/Arrow Keys) and sends movement
commands back to the server. This creates a full client-server gameplay loop where the server controls the game logic
and the client focuses on rendering and user input. This solution also includes a simple web server that stores and
displays game statistics.

# Time Expenditures:
1. Assignment Eight - Predicted Hours: 5 - Actual Hours: 5
2. Assignment Nine - Predicted Hours: 12 - Actual Hours: 15
3. Assignment Ten - Predicted Hours: 6 - Actual Hours: 7

# Use of AI*
**Google NotebookLM**: Used to rephrase and better explain assignment instructions and lectures slides.
**ChatGPT & Google Gemini**: Used to improve syntax and understanding of C#.
**Claude**: Used to improve understanding of specifically the web server and it's implementation.

```
* See individual project READMEs for more detailed descriptions on the use of AI.
```

---

# Running the Project

> **Note:** This is a university course project (CS 3500, University of Utah). Because of that, it is **not fully plug-and-play**. The game server is a pre-compiled course-provided binary (Windows-only), and the stats database requires manual setup with credentials that are not included in this repo.

## Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows OS (the provided `GameServer/Server.exe` is Windows-only)
- A SQL Server instance (for game statistics — optional, but the app will error on connect/disconnect without it)

## Steps

### 1. Start the Game Server
Run the pre-compiled server from the `GameServer/` directory:
```
GameServer\Server.exe
```
This starts the game server on port `11000` (localhost).

### 2. (Optional) Configure the Database
The client records player stats to a SQL Server database. To enable this, set up [.NET user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) in the `Snake/` project:
```
cd Snake
dotnet user-secrets set "DbSecrets:Server" "<your-server>"
dotnet user-secrets set "DbSecrets:Database" "<your-database>"
dotnet user-secrets set "DbSecrets:User" "<your-username>"
dotnet user-secrets set "DbSecrets:Password" "<your-password>"
```
If you skip this step, the app will still run but will throw errors when players connect or disconnect.

### 3. Run the Blazor Client
```
cd Snake
dotnet run
```
Then open your browser to the URL shown in the terminal (e.g., `https://localhost:5001`). Navigate to `/snake` to play.

### 4. (Optional) Run the Web Server
The web server displays per-session player statistics:
```
cd WebServer
dotnet run
```
