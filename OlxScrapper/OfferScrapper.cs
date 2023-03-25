using OpenQA.Selenium;

namespace OlxScrapper;

public class OfferScrapper
{
    private readonly WebsiteRetriever _retriever;

    public OfferScrapper(WebsiteRetriever retriever)
    {
        _retriever = retriever;
    }

    public string GetTitle()
    {
        try
        {
            return _retriever.GetHtmlDocument().DocumentNode
                .Descendants("h1")
                .First(n => n.GetAttributeValue("data-cy", "")
                    .Contains("ad_title"))
                .GetDirectInnerText(); 
        }
        catch (Exception e)
        {
            return "unknown-error";
        }
    }

    public int GetPrice()
    {
        try
        {
            return int.Parse(_retriever.GetHtmlDocument().DocumentNode
                .Descendants("h3")
                .First(n => n.LastChild.InnerText.Contains("zł"))
                .FirstChild.GetDirectInnerText()
                .Replace(" ", ""));
        }
        catch (Exception e)
        {
            return -1;
        }
    }

    public async Task<string> GetPhoneNumber(int id)
    {
        if (!IsNumberScrapeable())
        {
            Console.WriteLine($"[{id}] This offer number can be revealed only when authorized.");
            return "no-auth";
        }
        
        try
        {
            _retriever.GetBrowser()
                .FindElement(By.XPath("//button[contains(@id, 'onetrust-accept-btn-handler')]"))
                .Click();
        }
        catch (Exception e)
        {
            Console.WriteLine("[DEBUG] Cannot accept OLX privacy policy. Probably it is not required.");
        }

        await Task.Delay(1000);
        RevealNumber();
        await Task.Delay(1000);
        var phoneNumber = await ScrapNumber();

        return phoneNumber;
    }

    private async Task<string> ScrapNumber()
    {
        for (var i = 0; i < 5; i++)
        {
            try
            {
                return _retriever.GetBrowser()
                    .FindElement(By.XPath("//div[contains(@data-testid, 'phones-container')]"))
                    .FindElement(By.TagName("a"))
                    .GetAttribute("innerText");
            } catch (Exception e)
            {
                await Task.Delay(2000);
            }
        }

        return "timeout";
    }
    
    private bool IsNumberScrapeable()
    {
        try
        {
            _retriever.GetBrowser()
                .FindElement(By.XPath("//div[contains(@data-testid, 'prompt-box')]"));
            return false;
        }
        catch (Exception e)
        {
            return true;
        }
    }
    
    private void RevealNumber()
    {
        _retriever.GetBrowser()
            .FindElement(By.XPath("//div[contains(@data-testid, 'phones-container')]"))
            .FindElement(By.TagName("button"))
            .Click();
    }
}