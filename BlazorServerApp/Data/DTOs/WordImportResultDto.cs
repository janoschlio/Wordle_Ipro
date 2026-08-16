namespace BlazorServerApp.Data.DTOs;

public class WordImportResultDto
{
    public int Added { get; set; }
    public int Duplicates { get; set; }
    public int Rejected { get; set; }
    public List<string> RejectedSamples { get; set; } = [];
    public int TotalAfterImport { get; set; }
    public bool Replaced { get; set; }
}
