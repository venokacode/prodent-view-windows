namespace ProDentView.Win.Models;

public sealed class PatientRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Sex { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string ChartNumber { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
