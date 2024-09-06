using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace Chain
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: dotnet run <listening-port> <next-host> <next-port> [true]");
                return;
            }

            int listeningPort, nextPort;
            string nextHost;
            bool isInitiator = false;

            if (!int.TryParse(args[0], out listeningPort))
            {
                Console.WriteLine("Invalid listening port. Please provide a valid integer.");
                return;
            }

            nextHost = args[1];

            if (!int.TryParse(args[2], out nextPort))
            {
                Console.WriteLine("Invalid next port. Please provide a valid integer.");
                return;
            }

            if (args.Length == 4)
            {
                if (args[3].ToLower() == "true")
                {
                    isInitiator = true;
                }
                else if (args[3].ToLower() != "false")
                {
                    Console.WriteLine("Invalid value for isInitiator. Please provide 'true' or 'false'.");
                    return;
                }
            }

            int localX = GetUserInput();

            if (isInitiator)
            {
                await ExecuteAlgorithm(listeningPort, nextHost, nextPort, localX, true);
            }
            else
            {
                await ExecuteAlgorithm(listeningPort, nextHost, nextPort, localX, false);
            }
        }

        private static async Task ExecuteAlgorithm(int listeningPort, string nextHost, int nextPort, int localX, bool isInitiator)
        {
            Socket listener = SetupListener(listeningPort);
            Socket sender = await SetupSender(nextHost, nextPort);
            Socket handlerSocket = listener.Accept();

            if (isInitiator)
            {
                await ProcessInitiator(handlerSocket, sender, localX);
            }
            else
            {
                await ProcessParticipant(handlerSocket, sender, localX);
            }

            CleanupSockets(handlerSocket, sender);
        }

        private static int GetUserInput()
        {
            int localX;
            Console.Write("Enter the value of X: ");
            while (!int.TryParse(Console.ReadLine(), out localX))
            {
                Console.WriteLine("Invalid input. Please enter a valid integer.");
            }
            return localX;
        }

        private static async Task ProcessInitiator(Socket handlerSocket, Socket sender, int localX)
        {
            SendData(sender, localX);
            localX = await ReceiveDataAsync(handlerSocket);
            SendData(sender, localX);
            localX = await ReceiveDataAsync(handlerSocket);
            Console.WriteLine(localX);
        }

        private static async Task ProcessParticipant(Socket handlerSocket, Socket sender, int localX)
        {
            int receivedX = await ReceiveDataAsync(handlerSocket);
            int maxX = Math.Max(localX, receivedX);
            SendData(sender, maxX);
            localX = await ReceiveDataAsync(handlerSocket);
            SendData(sender, localX);
            Console.WriteLine(localX);
        }

        private static Socket SetupListener(int port)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            Socket listener = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Bind(localEndPoint);
            listener.Listen(10);
            return listener;
        }

        private static async Task<Socket> SetupSender(string host, int port)
        {
            IPAddress ipAddress = host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ? IPAddress.Loopback : IPAddress.Parse(host);
            IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, port);
            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            while (true)
            {
                try
                {
                    await sender.ConnectAsync(remoteEndPoint);
                    return sender;
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Failed to connect to {host}:{port}. Retrying in 2 second...");
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    throw;
                }
            }
        }

        private static void SendData(Socket socket, int data)
        {
            byte[] buffer = BitConverter.GetBytes(data);
            socket.Send(buffer);
        }

        private static async Task<int> ReceiveDataAsync(Socket socket)
        {
            byte[] buffer = new byte[4];
            await socket.ReceiveAsync(buffer, SocketFlags.None);
            return BitConverter.ToInt32(buffer, 0);
        }

        private static void CleanupSockets(Socket handlerSocket, Socket sender)
        {
            handlerSocket.Shutdown(SocketShutdown.Both);
            handlerSocket.Close();
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
    }
}
