using OpenQA.Selenium;

namespace OlxScrapper;

/**
 * Configuration
 * 1. Install TOR Browser                                       https://www.torproject.org/download/
 * 2. Put two lines that are below into torrc file inside       C:\Users\USERNAME\Desktop\Tor Browser\Browser\TorBrowser\Data\Tor
 *
 * ControlPort 9051
 * HashedControlPassword 16:271E55CBD958440A609C288C2BDB5A2798AD5579D2997EDD98A7A63A03
 *
 * The hash is for password 'dupa'. You can create your own hash using      tor.exe --hash-password 'your-password'
 *
 * 3. Put list of links divided by newline into input.txt inside folder with OlxScrapper .exe
 * 4. Run .exe
 * 5. Wait and Have fun ;)
 */
public class Program
{
    public static void Main()
    {
        var inputFile = "input.txt";
        var outputFile = "output.txt";

        // >>> -------------------------------- <<<
        // >>> Process input to output normally <<<
        // >>> -------------------------------- <<<
        // var offers = File.ReadAllLines(inputFile).Select((value, index) => new {value, index})
        //     .ToDictionary(pair => pair.index, pair => pair.value);
        // ProcessAllOffers(outputFile, offers).Wait();

        // >>> -------------------------------- <<<
        // >>> Process only unprocessed records <<<
        // >>> -------------------------------- <<<
        var allOffers = File.ReadAllLines(inputFile).Where(s => !string.IsNullOrEmpty(s))
            .Select((value, index) => new {value, index})
            .ToDictionary(pair => pair.index, pair => pair.value);
        var offers = FilterOffersByUnprocessed(outputFile, allOffers);
        ProcessAllOffers(outputFile, offers).Wait();

        // >>> ----------------------- <<<
        // >>> Reprocess failed output <<<
        // >>> ----------------------- <<<
        // var offers = GetOutputRecordsToRetry(outputFile);
        // ProcessAllOffers(outputFile, offers).Wait();
        
        Console.WriteLine("Done!");
    }


    // Process all offers (multiple browser async processes)
    private static async Task ProcessAllOffers(string outputFile, Dictionary<int, string> offers)
    {
        foreach (var offerBatch in DivideDictionary(offers, 25))
        {
            var tasks = new List<Task>();
            
            foreach (var offerAsyncBatch in DivideDictionary(offerBatch, 5))
                tasks.Add(ProcessOffers(outputFile, offerAsyncBatch));

            await Task.WhenAll(tasks);
            
            Console.WriteLine("---<<==============>>---");
            Console.WriteLine("Batch completed.");
            Console.WriteLine("Changing IP to: " + TorController.ChangeIp());
            Console.WriteLine("---<<==============>>---");
        }
    }
    
    // Process single offer batch (one browser async process)
    private static async Task ProcessOffers(string outputFile, Dictionary<int, string> offers)
    {
        Console.WriteLine($"Started website process.");
        var wr = new WebsiteRetriever("https://google.com");
        var os = new OfferScrapper(wr);
        
        foreach (var (id, url) in offers)
        {
            Console.WriteLine($"Processing {id} => {url}");

            try { wr.NavigateTo(url); } 
            catch (Exception e)
            {
                await File.AppendAllTextAsync(outputFile, new OutputCsvRecord(
                    id: id, 
                    url: url,
                    title: "navigation-error",
                    price: -1, 
                    phoneNumber: "null")
                    .ToString()
                );
                continue;
            }
            
            await File.AppendAllTextAsync(outputFile, new OutputCsvRecord(
                id: id, 
                url: url,
                title: os.GetTitle(),
                price: os.GetPrice(),
                phoneNumber: await os.GetPhoneNumber(id))
                .ToString()
            );
        }
        
        wr.Dispose();
    }

    private static Dictionary<int, string> GetOutputRecordsToRetry(string outputFile)
    {
        var lines = File.ReadAllLines(outputFile).Where(s => !string.IsNullOrEmpty(s)).ToList();
        var csv = lines.Select(line => new OutputCsvRecord(line)).ToList();

        var outputLines = lines.ToList();
        var dict = new Dictionary<int, string>();
        
        foreach (var line in csv)
        {
            if (line.PhoneNumber == "timeout")
            {
                dict.Add(line.Id, line.Url);
                outputLines.Remove(lines.First(l => int.Parse(l.Split(',')[0]) == line.Id));
            }
        }

        File.WriteAllLines(outputFile, outputLines);

        return dict;
    }
    
    private static Dictionary<int, string> FilterOffersByUnprocessed(string outputFile, Dictionary<int, string> offers)
    {
        var lines = File.ReadAllLines(outputFile).Where(s => !string.IsNullOrEmpty(s)).ToList();
        var csv = lines.Select(line => new OutputCsvRecord(line)).ToList();
        
        foreach (var line in csv.Where(line => offers.ContainsKey(line.Id)))
        {
            if (offers[line.Id] != line.Url)
                throw new Exception("Corrupted output file.");

            offers.Remove(line.Id);
        }

        return offers;
    }

    private static IEnumerable<Dictionary<int, string>> DivideDictionary(Dictionary<int, string> dict, int maxGroupSize)
    {
        for (var i = 0; i < Math.Ceiling((decimal) dict.Count / maxGroupSize); i++)
            yield return dict.Skip(i*maxGroupSize).Take(maxGroupSize).ToDictionary(pair => pair.Key, pair => pair.Value);
    }
    
    public static void DebugIps()
    {
        for (var j = 0; j < 5; j++)
        {
            var wr = new WebsiteRetriever("https://api.ipify.org/");
            for (var i = 0; i < 3; i++)
            {
                wr.NavigateTo("https://api.ipify.org/");
                var ip = wr.GetBrowser().FindElement(By.TagName("body")).GetAttribute("innerText");
                Console.WriteLine(ip);
            }
        
            wr.Dispose();
        }
    }
}
