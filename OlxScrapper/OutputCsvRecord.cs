namespace OlxScrapper;

public class OutputCsvRecord
{
    public int Id { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }
    public int Price { get; set; }
    public string PhoneNumber { get; set; }
    
    public OutputCsvRecord(int id, string url, string title, int price, string phoneNumber)
    {
        Id = id;
        Url = url;
        Title = title;
        Price = price;
        PhoneNumber = phoneNumber;
    }
    
    public OutputCsvRecord(string csvLine)
    {
        var splitCsvLine = csvLine.TrimEnd(Environment.NewLine.ToCharArray()).Split('|');
        Id = int.Parse(splitCsvLine[0]);
        Url = splitCsvLine[1];
        Title = splitCsvLine[2];
        Price = int.Parse(splitCsvLine[3]);
        PhoneNumber = splitCsvLine[4];
    }

    public override string ToString()
    {
        return $"{Id}|{Url}|{Title}|{Price}|{PhoneNumber}" + Environment.NewLine;
    }
}