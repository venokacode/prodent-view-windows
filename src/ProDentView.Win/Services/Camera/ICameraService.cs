namespace ProDentView.Win.Services.Camera;

public interface ICameraService
{
    CameraCaptureRoute LastCaptureRoute { get; }
    string LastStatus { get; }
    Task<IReadOnlyList<CameraDeviceInfo>> EnumerateAsync(CancellationToken cancellationToken = default);
    Task StartPreviewAsync(CameraDeviceInfo device, IntPtr previewHandle, CancellationToken cancellationToken = default);
    void ResizePreview(int width, int height);
    Task StopPreviewAsync(CancellationToken cancellationToken = default);
    Task CaptureJpegAsync(string filePath, CancellationToken cancellationToken = default);
}

public sealed record CameraDeviceInfo(string Id, string Name, string DevicePath = "");

public enum CameraCaptureRoute
{
    Idle,
    StillPin,
    PreviewFrameFallback,
    Unavailable
}
