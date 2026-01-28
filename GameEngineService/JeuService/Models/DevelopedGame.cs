namespace JeuService.Models
{
    public class DevelopedGame
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Developer { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string GameUrl { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
