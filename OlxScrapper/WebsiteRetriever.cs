using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace OlxScrapper;

public class WebsiteRetriever : IDisposable
{
    private HtmlDocument _htmlDocument;
    private readonly ChromeDriver _browser;

    public WebsiteRetriever(string url)
    {
        _htmlDocument = InitiateHtmlDocument(url);
        _browser = InitiateBrowserTorProxy(url);
    }

    public HtmlDocument GetHtmlDocument() => _htmlDocument;
    public ChromeDriver GetBrowser() => _browser;

    private HtmlDocument InitiateHtmlDocument(string url)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(CallUrl(url).Result);
        return htmlDoc;
    }

    public void NavigateTo(string url)
    {
        //_browser.Manage().Cookies.DeleteAllCookies();
        _htmlDocument = InitiateHtmlDocument(url);
        _browser.Navigate().GoToUrl(url);
    }
    
    private ChromeDriver InitiateBrowser(string url)
    {
        var options = new ChromeOptions()
        {
            BinaryLocation = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
        };
        options.AddArguments(new List<string>() { "headless", "disable-gpu" });
        
        var browser = new ChromeDriver(options);
        browser.Navigate().GoToUrl(url);
        
        return browser;
    }

    private ChromeDriver InitiateBrowserTorProxy(string url)
    {
        var options = new ChromeOptions()
        {
            BinaryLocation = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
            Proxy = new Proxy()
            {
                Kind = ProxyKind.Manual,
                IsAutoDetect = false,
                HttpProxy = "socks5://127.0.0.1:9150",
                SslProxy = "socks5://127.0.0.1:9150"
            }
        };

        var service = ChromeDriverService.CreateDefaultService(options.BinaryLocation);
        service.SuppressInitialDiagnosticInformation = true;
        service.HideCommandPromptWindow = true;
        options.AddArguments(new List<string>() { "headless", "disable-gpu" });
        
        var browser = new ChromeDriver(service, options);
        browser.Navigate().GoToUrl(url);

        return browser;
    }
    
    private static async Task<string> CallUrl(string url)
    {
        var client = new HttpClient();
        var response = await client.GetStringAsync(url);
        return response;
    }

    public void Dispose()
    {
        _browser.Dispose();
    }
}