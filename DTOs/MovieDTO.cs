namespace RandomAPIApp.DTOs;

public class MovieDTO
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Director { get; set; }
    public string? Producer { get; set; }
    public string? ReleaseDate { get; set; }
    public string? RunningTime { get; set; }
    public string? Rating { get; set; }
    public string? Created { get; set; }
    public string? Edited { get; set; }
    public string? Url { get; set; }
}