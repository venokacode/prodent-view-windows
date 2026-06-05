using System.IO;
using ProDentView.Win.Models;

namespace ProDentView.Win.Services;

public sealed class LocalImageStore
{
    private readonly string rootPath;

    public LocalImageStore()
    {
        rootPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ProDENT",
            "ProDENT View",
            "Images"
        );
        Directory.CreateDirectory(rootPath);
    }

    public string RootPath => rootPath;

    public string GetPatientDateFolder(PatientRecord patient, DateTime date)
    {
        var folder = Path.Combine(GetPatientFolder(patient), date.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    public IReadOnlyList<CapturedImageRecord> GetImages(PatientRecord patient)
    {
        var patientFolder = GetPatientFolder(patient);
        if (!Directory.Exists(patientFolder))
        {
            return [];
        }

        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".bmp",
            ".tif",
            ".tiff"
        };

        return Directory
            .EnumerateFiles(patientFolder, "*.*", SearchOption.AllDirectories)
            .Where(path => extensions.Contains(Path.GetExtension(path)))
            .Select(path => new CapturedImageRecord
            {
                PatientId = patient.Id,
                FileName = Path.GetFileName(path),
                FilePath = path,
                CapturedAt = File.GetCreationTime(path)
            })
            .OrderByDescending(image => image.CapturedAt)
            .ToArray();
    }

    private string GetPatientFolder(PatientRecord patient)
    {
        return Path.Combine(rootPath, patient.Id.ToString("N"));
    }

    public CapturedImageRecord ReserveCapturePath(PatientRecord patient)
    {
        var folder = GetPatientDateFolder(patient, DateTime.Now);
        var fileName = $"{patient.Id:N}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.jpg";
        return new CapturedImageRecord
        {
            PatientId = patient.Id,
            FileName = fileName,
            FilePath = Path.Combine(folder, fileName),
            CapturedAt = DateTime.Now
        };
    }

    public string ReserveImportPath(PatientRecord patient, string sourcePath)
    {
        var folder = GetPatientDateFolder(patient, DateTime.Now);
        var sourceFileName = Path.GetFileName(sourcePath);
        var safeFileName = MakeSafeFileName(sourceFileName);
        var extension = Path.GetExtension(safeFileName);
        var baseName = Path.GetFileNameWithoutExtension(safeFileName);
        var candidate = Path.Combine(folder, safeFileName);

        var suffix = 1;
        while (File.Exists(candidate))
        {
            candidate = Path.Combine(folder, $"{baseName}-{suffix}{extension}");
            suffix += 1;
        }

        return candidate;
    }

    private static string MakeSafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "Patient" : cleaned.Trim();
    }
}
