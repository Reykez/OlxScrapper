using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OlxScrapper;

public static class TorController
{
    public static string ChangeIp()
    {
        var ipBefore = CheckCurrentIp();
        var ipAfter = ipBefore;
        
        while (ipBefore.Equals(ipAfter))
        {
            RefreshTor();
            ipAfter = CheckCurrentIp();
            Thread.Sleep(250);
        }

        return ipAfter;
    }
    
    private static void RefreshTor()
    {
        var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9151);
        var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            server.Connect(ip);
        }
        catch (SocketException e)
        {
            Console.WriteLine("Unable to connect to TOR control server.");
            RefreshTor();
            return;
        }

        server.Send(Encoding.ASCII.GetBytes("AUTHENTICATE \"dupa\"\n"));
        var data = new byte[1024];
        var receivedDataLength = server.Receive(data);
        var stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength);

        if (stringData.Contains("250"))
        {
            server.Send(Encoding.ASCII.GetBytes("SIGNAL NEWNYM\r\n"));
            data = new byte[1024];
            receivedDataLength = server.Receive(data);
            stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength);
            if (!stringData.Contains("250"))
            {
                Console.WriteLine("Unable to signal new user to server.");
                server.Shutdown(SocketShutdown.Both);
                server.Close();
                RefreshTor();
            }
        }
        else
        {
            Console.WriteLine("Unable to authenticate to server.");
            server.Shutdown(SocketShutdown.Both);
            server.Close();
            RefreshTor();
        }
        server.Shutdown(SocketShutdown.Both);
        server.Close();
    }

    private static string CheckCurrentIp()
    { 
        var proxy = new WebProxy
        {
            Address = new Uri("socks5://127.0.0.1:9150/"),
            BypassProxyOnLocal = false,
            UseDefaultCredentials = false
        };
        
        var clientHandler = new HttpClientHandler
        {
            Proxy = proxy
        };
        
        var client = new HttpClient(handler: clientHandler, disposeHandler: true);
        var result = client.GetStringAsync("https://api.ipify.org/").Result;
        
        client.Dispose();
        
        return result;
    }
}