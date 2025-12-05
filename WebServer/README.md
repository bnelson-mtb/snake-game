```
Authors: Brady Nelson and Charles Adair
Course: CS 3500, University of Utah, School of Computing
GitHub ID: bnelson-mtb
Repo: https://github.com/uofu-cs3500-20-fall2025/assignment-eight-chatting-brady_charlie_game
Date: 12/4/25
Project: WebServer
Copyright: CS 3500 and Brady Nelson - This work may not be copied for
use in Academic Coursework.
```

# Comments to Evaluators:
This is a simple web server which displays snake game session player data,
including player ID, score, leave time, and enter time.

# Assignment Specific Topics
- Networking
- Multithreading

# Consulted Peers:
n/a

# References:
n/a

# Use of AI
- Claude and Gemini were used.
- We were confused how to use multi-threading with the TcpListener and experienced some issues with the webpage when we tried to make
multiple requests. Following a prompt from Claude, we ended up putting all of our logic within a `new Thread( ).Start()` call.
- We were confused about some of the SQL syntax. Learned a few things like `SELECT` and `ORDER BY` from Google Gemini.
- We learned we could simplify our method calls by getting a `List` from the database with an expected output and using this to iterate
through and add data into our web page.
- Gemini was used to help understand how to handle URL paths and seperate logic for `/`, `/games` and `/games?gid=`