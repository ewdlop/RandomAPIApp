namespace RandomAPIApp.DTOs;

public class PartOfSpeechTaggerDTO
{
    public required string Input { get; set; }
    public string[]? Output { get; set; }
}
