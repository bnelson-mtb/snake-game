using System.Net;
using System.Net.Sockets;
using System.Text;
using CS3500.DatabaseController;

namespace WebServer;

public class Program
{
    static DatabaseController db = new DatabaseController();

    public static void Main(string[] args)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 80);
        listener.Start();
        Console.WriteLine("Web server listening on port 80");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();

            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream, new UTF8Encoding(false));
            StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

            // Read the first line to get the request
            string requestLine = reader.ReadLine()!;
            Console.WriteLine(requestLine);

            // Read and ignore the rest of the headers
            while (reader.ReadLine() != "")
            {
            }

            // Get the path from the request
            string path = requestLine.Split(' ')[1];

            // Build the HTML response based on the path
            string html = "";

            if (path == "/")
            {
                html = "<html>\n<h3>Welcome to the Snake Games Database!</h3>\n<a href=\"/games\">View Games</a>\n</html>";
            }
            else if (path == "/games")
            {
                html = "<html>\n<table border=\"1\">\n<thead>\n<tr>\n<td>ID</td><td>Start</td><td>End</td>\n</tr>\n</thead>\n<tbody>\n";

                var games = db.GetAllGames();
                foreach (var game in games)
                {
                    html += "<tr>\n";
                    html += "<td><a href=\"/games?gid=" + game.Id + "\">" + game.Id + "</a></td>";
                    html += "<td>" + game.StartTime + "</td>";
                    html += "<td>" + (game.EndTime.HasValue ? game.EndTime.Value.ToString() : "") + "</td>\n";
                    html += "</tr>\n";
                }

                html += "</tbody>\n</table>\n</html>";
            }
            else if (path.StartsWith("/games?gid="))
            {
                int gameId = int.Parse(path.Substring("/games?gid=".Length));

                html = "<html>\n<h3>Stats for Game " + gameId + "</h3>\n";
                html += "<table border=\"1\">\n<thead>\n<tr>\n";
                html += "<td>Player ID</td><td>Player Name</td><td>Max Score</td><td>Enter Time</td><td>Leave Time</td>\n";
                html += "</tr>\n</thead>\n<tbody>\n";

                var players = db.GetPlayersForGame(gameId);
                foreach (var player in players)
                {
                    html += "<tr>\n";
                    html += "<td>" + player.PlayerId + "</td>";
                    html += "<td>" + player.Name + "</td>";
                    html += "<td>" + player.MaxScore + "</td>";
                    html += "<td>" + player.EnterTime + "</td>";
                    html += "<td>" + (player.LeaveTime.HasValue ? player.LeaveTime.Value.ToString() : "") + "</td>\n";
                    html += "</tr>\n";
                }

                html += "</tbody>\n</table>\n</html>";
            }

            // Send HTTP response
            int contentLength = Encoding.UTF8.GetBytes(html).Length;
            string response = "HTTP/1.1 200 OK\r\n";
            response += "Connection: close\r\n";
            response += "Content-Type: text/html; charset=UTF-8\r\n";
            response += "Content-Length: " + contentLength + "\r\n";
            response += "\r\n";
            response += html;

            writer.Write(response);
            client.Close();
        }
    }
}
