namespace ProDentView.Win.Models;

public sealed class CapturedImageRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid PatientId { get; init; }
    public required string FileName { get; init; }
    public required string FilePath { get; init; }
    public DateTime CapturedAt { get; init; } = DateTime.Now;
}
