using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using static System.Net.Mime.MediaTypeNames;

namespace AnimalBot
{
	class CommandHandler
	{
		readonly Emoji thumbs_up = new Emoji("👍");
		public static void Main(string[] args)
		 => new CommandHandler().MainAsync().GetAwaiter().GetResult();

		private DiscordSocketClient _client;
		public async Task MainAsync()
		{
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Running AnimalBot.exe");
			Console.WriteLine();
			Console.WriteLine($@"{DateTime.Now.ToString().Substring(11)} Discord     Initializing");
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Green;
			_client = new DiscordSocketClient();
			_client.MessageReceived += CommandHandlerFunc;
			_client.Log += LoginPrint;
			string token = File.ReadAllText("TOKEN.txt");
			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();
			await _client.SetActivityAsync(new Game(File.ReadAllText("PREFIX.txt")[0] + "help",
				ActivityType.Listening, ActivityProperties.None));
			await Task.Delay(-1);
		}

		private Task LoginPrint(LogMessage msg)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(msg.ToString());
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Green;
			return Task.CompletedTask;
		}

		private Task LogCommands(SocketMessage message, string command)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($@"NEW MESSAGE (ON {_client.CurrentUser.Username.ToString()}) FROM {message.Author} using {command} at {message.Timestamp.AddHours(3).ToString().Substring(0, 19)}");
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Green;
			message.AddReactionAsync(thumbs_up);
			return Task.CompletedTask;
		}

		private Task CommandHandlerFunc(SocketMessage message)
		{
			if (message.Author.IsBot) return Task.CompletedTask;
			char prefix = File.ReadAllText("PREFIX.txt")[0];
			string ADMIN_ID = File.ReadAllText("ADMIN.txt");
			if (!message.Content.StartsWith(prefix)) return Task.CompletedTask;
			int lengthOfCommand = message.Content.Length;
			if (message.Content.Contains(' ')) lengthOfCommand = message.Content.IndexOf(' ');
			string command = message.Content.Substring(1, lengthOfCommand - 1).ToLower();

			switch (command)
			{
				case "hello":
					message.Channel.SendMessageAsync($@"Hello {message.Author.Mention}");
					LogCommands(message, command);
					break;

				case "help":
					message.Channel.SendMessageAsync
					($@"THIS IS THE HELP SCREEN. CONSIDER YOURSELF HELPED

				List of **public** commands (last updated 20-06-2021):

				**{prefix}hello** is a friendly greeting
				**{prefix}help** gets you this screen

				**{prefix}dogfact** sends you a fact about dogs
				**{prefix}catfact** sends you a fact about cats
				**{prefix}pandafact** sends you a fact about panda
				**{prefix}foxfact** sends you a fact about foxes
				**{prefix}birdfact** sends you a fact about birds
				**{prefix}koalafact** sends you a fact about koalas

				**{prefix}dog** sends a dog photo
				**{prefix}cat** sends a cat photo
				**{prefix}panda** sends a panda photo
				**{prefix}redpanda** sends a photo of a red panda
				**{prefix}fox** sends a fox photo
				**{prefix}bird** sends a bird photo
				**{prefix}koala** sends a koala
				**{prefix}ping** or **{prefix}latency** sends the current latency for the bot

				Current latency (ping): {_client.Latency.ToString()} ms.
				
				Source Code: **https://github.com/Stormageddon37/AnimalBot**
				");
					LogCommands(message, command);
					break;

				case "dogfact":
				case "catfact":
				case "pandafact":
				case "foxfact":
				case "birdfact":
				case "koalafact":
					WebRequest fact_request = HttpWebRequest.Create("https://some-random-api.ml/facts/" + command.Substring(0, command.Length - 4));
					WebResponse fact_response = fact_request.GetResponse();
					StreamReader fact_reader = new StreamReader(fact_response.GetResponseStream());
					string fact = fact_reader.ReadToEnd().Substring(9).TrimEnd('}').TrimEnd('"');
					message.Channel.SendMessageAsync(fact);
					LogCommands(message, command);
					break;

				case "dog":
				case "cat":
				case "panda":
				case "redpanda":
				case "fox":
				case "bird":
				case "koala":
					WebRequest request = HttpWebRequest.Create($@"https://some-random-api.ml/img/" + command);
					if (command.Equals("redpanda")) request = HttpWebRequest.Create($@"https://some-random-api.ml/img/red_panda");
					WebResponse response = request.GetResponse();
					StreamReader reader = new StreamReader(response.GetResponseStream());
					string url = reader.ReadToEnd().Substring(9).TrimEnd('}').TrimEnd('"'); ;
					message.Channel.SendMessageAsync(url);
					LogCommands(message, command);
					break;

				case "ping":
				case "latency":
					message.Channel.SendMessageAsync("ping is " + _client.Latency.ToString() + "ms");
					LogCommands(message, command);
					break;

				case "restart":
				case "reboot":
				case "r":
					if (!message.Author.Id.ToString().Equals(ADMIN_ID))
					{
						message.Channel.SendMessageAsync("You do not have permission to use this command");
						return Task.CompletedTask;
					}
					message.Channel.SendMessageAsync("Restarting Animal Bot...");
					LogCommands(message, command);
					Process.Start("AnimalBot.bat");
					Thread.Sleep(500);
					_client.StopAsync();
					break;
			}
			return Task.CompletedTask;
		}
    }
}
