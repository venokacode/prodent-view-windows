using System.Text.Json;
using ProDentView.Win.Models;

namespace ProDentView.Win.Services;

public sealed class PatientStore
{
    private readonly string filePath;
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        WriteIndented = true
    };

    public PatientStore()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ProDENT",
            "ProDENT View"
        );
        Directory.CreateDirectory(folder);
        filePath = Path.Combine(folder, "patients.json");
    }

    public IReadOnlyList<PatientRecord> Load()
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<PatientRecord>>(json, jsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IEnumerable<PatientRecord> patients)
    {
        var json = JsonSerializer.Serialize(patients, jsonOptions);
        File.WriteAllText(filePath, json);
    }
}
