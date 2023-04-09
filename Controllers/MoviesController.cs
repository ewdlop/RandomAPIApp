using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomAPIApp.DTOs;

namespace RandomAPIApp.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class MoviesController : ControllerBase
{
    private IEnumerable<MovieDTO> _movies = new List<MovieDTO>()
    {
        new MovieDTO
        {
            Id = 1,
            Title = "The Godfather",
            Description = "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant son.",
            Director = "Francis Ford Coppola",
            Producer = "Albert S. Ruddy",
            ReleaseDate = "1972-03-24",
            RunningTime = "175 minutes",
            Rating = "9.2",
            Created = "2021-05-12",
            Edited = "2021-05-12",
            Url = "https://www.imdb.com/title/tt0068646/"
        },
        new MovieDTO
        {
            Id = 2,
            Title = "The Shawshank Redemption",
            Description = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.",
            Director = "Frank Darabont",
            Producer = "Niki Marvin",
            ReleaseDate = "1994-09-23",
            RunningTime = "142 minutes",
            Rating = "9.3",
            Created = "2021-05-12",
            Edited = "2021-05-12",
            Url = "https://www.imdb.com/title/tt0111161/"
        },
        new MovieDTO
        {
            Id = 3,
            Title = "The Dark Knight",
            Description = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.",
            Director = "Christopher Nolan",
            Producer = "Emma Thomas",
            ReleaseDate = "2008-07-18",
            RunningTime = "152 minutes",
            Rating = "9.0",
            Created = "2021-05-12",
            Edited = "2021-05-12",
            Url = "https://www.imdb.com/title/tt0468569/"
        },
        new MovieDTO
        {
            Id = 4,
            Title = "The Lord of the Rings: The Fellowship of the Ring",
            Description = "A meek Hobbit from the Shire and eight companions set out on a journey to destroy the powerful One Ring and save Middle-earth from the Dark Lord Sauron.",
            Director = "Peter Jackson",
            Producer = "Peter Jackson",
            ReleaseDate = "2001-12-19",
            RunningTime = "178 minutes",
            Rating = "8.8",
            Created = "2021-05-12",
            Edited = "2021-05-12",
            Url = "https://www.imdb.com/title/tt0120737/"
        }
    };


    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_movies);
    }
}