# Requirements

| Requirement      | Download URL                                           | Reason                                                |
|------------------|--------------------------------------------------------|-------------------------------------------------------|
| **Tor Browser**  | https://www.torproject.org/download/                   | Proxy. You can use other also plain TOR proxy client. |
| **.NET 6.0**     | https://dotnet.microsoft.com/en-us/download/dotnet/6.0 | Compile and run project.                              |

# Setup
Turn on Tor Browser and wait for it to connect to the Tor network. Then run the project.
Be aware to port that Tor browser uses. Default for proxy is 9150, and for control port is 9151. This values is hardcoded. If you use other ports, you need to change them in the code. 
