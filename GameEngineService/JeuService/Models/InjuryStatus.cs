namespace JeuService.Models
{
    public class InjuryStatus
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public string Status { get; set; } = string.Empty; 
        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }
}
