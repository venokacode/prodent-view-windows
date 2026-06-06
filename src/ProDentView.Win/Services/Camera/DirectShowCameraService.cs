using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using ProDentView.Win.Services.Camera.DirectShow;
using DrawingSize = System.Drawing.Size;

namespace ProDentView.Win.Services.Camera;

public sealed class DirectShowCameraService : ICameraService, IDisposable
{
    private IGraphBuilder? graphBuilder;
    private ICaptureGraphBuilder2? captureGraphBuilder;
    private IMediaControl? mediaControl;
    private IVideoWindow? videoWindow;
    private IBaseFilter? sourceFilter;
    private IBaseFilter? sampleGrabberFilter;
    private IBaseFilter? nullRendererFilter;
    private ISampleGrabber? sampleGrabber;
    private IPin? stillPin;
    private IntPtr previewHandle;
    private bool disposed;

    public CameraCaptureRoute LastCaptureRoute { get; private set; } = CameraCaptureRoute.Idle;
    public string LastStatus { get; private set; } = "Idle";

    public Task<IReadOnlyList<CameraDeviceInfo>> EnumerateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<CameraDeviceInfo> devices = EnumerateVideoDevices();
        LastStatus = devices.Count == 0 ? "No UVC camera detected" : $"{devices.Count} camera(s) detected";
        return Task.FromResult(devices);
    }

    public Task StartPreviewAsync(CameraDeviceInfo device, IntPtr previewHandle, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        StopPreviewCore();

        this.previewHandle = previewHandle;
        BuildGraph(device);
        mediaControl = (IMediaControl)graphBuilder!;
        videoWindow = (IVideoWindow)graphBuilder!;
        ConfigureVideoWindow();
        NativeMethods.ThrowIfFailed(mediaControl.Run(), "Start DirectShow preview");
        ResizePreview(0, 0);

        LastCaptureRoute = sampleGrabber is null ? CameraCaptureRoute.PreviewFrameFallback : CameraCaptureRoute.StillPin;
        LastStatus = sampleGrabber is null
            ? "Preview live · still pin unavailable, using preview-frame fallback"
            : "Preview live · still pin ready";
        return Task.CompletedTask;
    }

    public void ResizePreview(int width, int height)
    {
        if (videoWindow is null || previewHandle == IntPtr.Zero)
        {
            return;
        }

        if (NativeMethods.GetClientRect(previewHandle, out var rect))
        {
            var targetWidth = Math.Max(1, rect.Right - rect.Left);
            var targetHeight = Math.Max(1, rect.Bottom - rect.Top);
            videoWindow.SetWindowPosition(0, 0, targetWidth, targetHeight);
        }
    }

    public Task StopPreviewAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StopPreviewCore();
        LastCaptureRoute = CameraCaptureRoute.Idle;
        LastStatus = "Preview stopped";
        return Task.CompletedTask;
    }

    public Task CaptureJpegAsync(string filePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        if (TryCaptureFromStillPin(filePath, cancellationToken))
        {
            LastCaptureRoute = CameraCaptureRoute.StillPin;
            LastStatus = "Captured from UVC still pin";
            return Task.CompletedTask;
        }

        CapturePreviewWindow(filePath);
        LastCaptureRoute = CameraCaptureRoute.PreviewFrameFallback;
        LastStatus = "Captured from preview-frame fallback";
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        StopPreviewCore();
        disposed = true;
    }

    private static IReadOnlyList<CameraDeviceInfo> EnumerateVideoDevices()
    {
        var devices = new List<CameraDeviceInfo>();
        ICreateDevEnum? devEnum = null;
        IEnumMoniker? enumMoniker = null;
        IBindCtx? bindCtx = null;

        try
        {
            devEnum = (ICreateDevEnum)NativeMethods.CreateComObject(DirectShowGuids.SystemDeviceEnum);
            var category = DirectShowGuids.VideoInputDeviceCategory;
            var hr = devEnum.CreateClassEnumerator(ref category, out enumMoniker, 0);
            if (hr == DirectShowConstants.SFalse || enumMoniker is null)
            {
                return devices;
            }
            NativeMethods.ThrowIfFailed(hr, "Enumerate video input devices");
            NativeMethods.ThrowIfFailed(NativeMethods.CreateBindCtx(0, out bindCtx), "Create bind context");
            var activeBindCtx = bindCtx ?? throw new InvalidOperationException("DirectShow bind context was not created.");

            var monikers = new IMoniker[1];
            while (enumMoniker.Next(1, monikers, IntPtr.Zero) == DirectShowConstants.SOk)
            {
                var moniker = monikers[0];
                var displayName = GetDisplayName(moniker, activeBindCtx);
                var friendlyName = GetFriendlyName(moniker, activeBindCtx) ?? displayName;
                devices.Add(new CameraDeviceInfo(displayName, friendlyName, displayName));
                NativeMethods.ReleaseComObject(moniker);
            }
        }
        finally
        {
            NativeMethods.ReleaseComObject(enumMoniker);
            NativeMethods.ReleaseComObject(bindCtx);
            NativeMethods.ReleaseComObject(devEnum);
        }

        return devices
            .OrderBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string GetDisplayName(IMoniker moniker, IBindCtx bindCtx)
    {
        moniker.GetDisplayName(bindCtx, null, out var displayName);
        return displayName;
    }

    private static string? GetFriendlyName(IMoniker moniker, IBindCtx bindCtx)
    {
        object? bagObject = null;
        try
        {
            var propertyBagId = typeof(IPropertyBag).GUID;
            moniker.BindToStorage(bindCtx, null, ref propertyBagId, out bagObject);
            if (bagObject is not IPropertyBag propertyBag)
            {
                return null;
            }

            var hr = propertyBag.Read("FriendlyName", out var value, IntPtr.Zero);
            return hr >= 0 ? value as string : null;
        }
        catch
        {
            return null;
        }
        finally
        {
            NativeMethods.ReleaseComObject(bagObject);
        }
    }

    private static IMoniker FindDeviceMoniker(string deviceId)
    {
        ICreateDevEnum? devEnum = null;
        IEnumMoniker? enumMoniker = null;
        IBindCtx? bindCtx = null;

        try
        {
            devEnum = (ICreateDevEnum)NativeMethods.CreateComObject(DirectShowGuids.SystemDeviceEnum);
            var category = DirectShowGuids.VideoInputDeviceCategory;
            var hr = devEnum.CreateClassEnumerator(ref category, out enumMoniker, 0);
            NativeMethods.ThrowIfFailed(hr, "Enumerate video input devices");
            if (enumMoniker is null)
            {
                throw new InvalidOperationException("No video input devices are available.");
            }
            NativeMethods.ThrowIfFailed(NativeMethods.CreateBindCtx(0, out bindCtx), "Create bind context");
            var activeBindCtx = bindCtx ?? throw new InvalidOperationException("DirectShow bind context was not created.");

            var monikers = new IMoniker[1];
            while (enumMoniker.Next(1, monikers, IntPtr.Zero) == DirectShowConstants.SOk)
            {
                var moniker = monikers[0];
                var displayName = GetDisplayName(moniker, activeBindCtx);
                if (string.Equals(displayName, deviceId, StringComparison.Ordinal))
                {
                    return moniker;
                }
                NativeMethods.ReleaseComObject(moniker);
            }

            throw new InvalidOperationException("Selected camera is no longer available.");
        }
        finally
        {
            NativeMethods.ReleaseComObject(enumMoniker);
            NativeMethods.ReleaseComObject(bindCtx);
            NativeMethods.ReleaseComObject(devEnum);
        }
    }

    private void BuildGraph(CameraDeviceInfo device)
    {
        graphBuilder = (IGraphBuilder)NativeMethods.CreateComObject(DirectShowGuids.FilterGraph);
        captureGraphBuilder = (ICaptureGraphBuilder2)NativeMethods.CreateComObject(DirectShowGuids.CaptureGraphBuilder2);
        NativeMethods.ThrowIfFailed(captureGraphBuilder.SetFiltergraph(graphBuilder), "Attach capture graph builder");

        var moniker = FindDeviceMoniker(device.Id);
        IBindCtx? bindCtx = null;
        try
        {
            var baseFilterId = DirectShowGuids.IidIBaseFilter;
            NativeMethods.ThrowIfFailed(NativeMethods.CreateBindCtx(0, out bindCtx), "Create source bind context");
            moniker.BindToObject(bindCtx, null, ref baseFilterId, out var sourceObject);
            sourceFilter = (IBaseFilter)sourceObject;
        }
        finally
        {
            NativeMethods.ReleaseComObject(bindCtx);
            NativeMethods.ReleaseComObject(moniker);
        }

        NativeMethods.ThrowIfFailed(graphBuilder.AddFilter(sourceFilter, device.Name), "Add camera source filter");
        TrySetPreferredPreviewFormat();
        TryBuildStillPinBranch();
        RenderPreviewStream();
    }

    private void TrySetPreferredPreviewFormat()
    {
        if (captureGraphBuilder is null || sourceFilter is null)
        {
            return;
        }

        if (TrySetPreferredFormatForCategory(DirectShowGuids.PinCategoryPreview))
        {
            return;
        }

        TrySetPreferredFormatForCategory(DirectShowGuids.PinCategoryCapture);
    }

    private bool TrySetPreferredFormatForCategory(Guid category)
    {
        if (captureGraphBuilder is null || sourceFilter is null)
        {
            return false;
        }

        object? configObject = null;
        try
        {
            var video = DirectShowGuids.MediaTypeVideo;
            var iid = DirectShowGuids.IidIAMStreamConfig;
            var hr = captureGraphBuilder.FindInterface(ref category, ref video, sourceFilter, ref iid, out configObject);
            if (hr < 0 || configObject is not IAMStreamConfig streamConfig)
            {
                return false;
            }

            return TrySetPreferredFormat(streamConfig);
        }
        catch
        {
            return false;
        }
        finally
        {
            NativeMethods.ReleaseComObject(configObject);
        }
    }

    private static bool TrySetPreferredFormat(IAMStreamConfig streamConfig)
    {
        var caps = IntPtr.Zero;
        try
        {
            var hr = streamConfig.GetNumberOfCapabilities(out var count, out var size);
            if (hr < 0 || count <= 0 || size <= 0)
            {
                return false;
            }

            caps = Marshal.AllocCoTaskMem(size);
            AMMediaType? fallback = null;
            for (var index = 0; index < count; index += 1)
            {
                hr = streamConfig.GetStreamCaps(index, out var mediaType, caps);
                if (hr < 0)
                {
                    continue;
                }

                if (TryReadVideoInfo(mediaType, out var info))
                {
                    var width = Math.Abs(info.BitmapInfoHeader.Width);
                    var height = Math.Abs(info.BitmapInfoHeader.Height);
                    if (width == 1280 && height == 720)
                    {
                        NativeMethods.ThrowIfFailed(streamConfig.SetFormat(mediaType), "Set 1280 x 720 preview format");
                        NativeMethods.FreeAMMediaType(fallback);
                        NativeMethods.FreeAMMediaType(mediaType);
                        return true;
                    }

                    if (fallback is null && width <= 1280 && height <= 720)
                    {
                        fallback = mediaType;
                        continue;
                    }
                }

                NativeMethods.FreeAMMediaType(mediaType);
            }

            if (fallback is not null)
            {
                NativeMethods.ThrowIfFailed(streamConfig.SetFormat(fallback), "Set fallback preview format");
                NativeMethods.FreeAMMediaType(fallback);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (caps != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(caps);
            }
        }
    }

    private static bool TryReadVideoInfo(AMMediaType mediaType, out VideoInfoHeader info)
    {
        info = default;
        if (mediaType.FormatPtr == IntPtr.Zero || mediaType.FormatSize < Marshal.SizeOf<VideoInfoHeader>())
        {
            return false;
        }

        info = Marshal.PtrToStructure<VideoInfoHeader>(mediaType.FormatPtr);
        return true;
    }

    private void TryBuildStillPinBranch()
    {
        if (graphBuilder is null || captureGraphBuilder is null || sourceFilter is null)
        {
            return;
        }

        try
        {
            sampleGrabberFilter = (IBaseFilter)NativeMethods.CreateComObject(DirectShowGuids.SampleGrabber);
            sampleGrabber = (ISampleGrabber)sampleGrabberFilter;
            nullRendererFilter = (IBaseFilter)NativeMethods.CreateComObject(DirectShowGuids.NullRenderer);

            var desiredType = new AMMediaType
            {
                MajorType = DirectShowGuids.MediaTypeVideo,
                SubType = DirectShowGuids.MediaSubTypeRgb24,
                FormatType = DirectShowGuids.FormatVideoInfo
            };
            NativeMethods.ThrowIfFailed(sampleGrabber.SetMediaType(desiredType), "Configure still pin sample format");
            NativeMethods.ThrowIfFailed(sampleGrabber.SetBufferSamples(true), "Enable still pin buffer samples");
            NativeMethods.ThrowIfFailed(sampleGrabber.SetOneShot(false), "Disable one-shot still sample mode");

            NativeMethods.ThrowIfFailed(graphBuilder.AddFilter(sampleGrabberFilter, "Still Sample Grabber"), "Add still sample grabber");
            NativeMethods.ThrowIfFailed(graphBuilder.AddFilter(nullRendererFilter, "Still Null Renderer"), "Add still null renderer");

            var still = DirectShowGuids.PinCategoryStill;
            var video = DirectShowGuids.MediaTypeVideo;
            var hr = captureGraphBuilder.RenderStream(ref still, ref video, sourceFilter, sampleGrabberFilter, nullRendererFilter);
            if (hr < 0)
            {
                sampleGrabber = null;
                LastStatus = "Still pin unavailable";
                return;
            }

            hr = captureGraphBuilder.FindPin(sourceFilter, PinDirection.Output, ref still, ref video, false, 0, out stillPin);
            if (hr < 0 || stillPin is null)
            {
                sampleGrabber = null;
                LastStatus = "Still pin trigger unavailable";
            }
        }
        catch
        {
            stillPin = null;
            sampleGrabber = null;
            LastStatus = "Still pin setup failed";
        }
    }

    private void RenderPreviewStream()
    {
        if (captureGraphBuilder is null || sourceFilter is null)
        {
            throw new InvalidOperationException("Camera graph has not been created.");
        }

        var preview = DirectShowGuids.PinCategoryPreview;
        var video = DirectShowGuids.MediaTypeVideo;
        var hr = captureGraphBuilder.RenderStream(ref preview, ref video, sourceFilter, null, null);
        if (hr >= 0)
        {
            return;
        }

        var capture = DirectShowGuids.PinCategoryCapture;
        NativeMethods.ThrowIfFailed(
            captureGraphBuilder.RenderStream(ref capture, ref video, sourceFilter, null, null),
            "Render camera preview stream"
        );
    }

    private void ConfigureVideoWindow()
    {
        if (videoWindow is null)
        {
            return;
        }

        videoWindow.put_Owner(previewHandle);
        videoWindow.put_WindowStyle(DirectShowConstants.WsChild | DirectShowConstants.WsClipChildren | DirectShowConstants.WsClipSiblings);
        videoWindow.put_AutoShow(DirectShowConstants.Oatrue);
        videoWindow.put_Visible(DirectShowConstants.Oatrue);
    }

    private bool TryCaptureFromStillPin(string filePath, CancellationToken cancellationToken)
    {
        if (captureGraphBuilder is null || sourceFilter is null || sampleGrabber is null || stillPin is null)
        {
            return false;
        }

        try
        {
            var iid = DirectShowGuids.IidIAMVideoControl;
            var still = DirectShowGuids.PinCategoryStill;
            var video = DirectShowGuids.MediaTypeVideo;
            var hr = captureGraphBuilder.FindInterface(ref still, ref video, sourceFilter, ref iid, out var controlObject);
            NativeMethods.ThrowIfFailed(hr, "Find IAMVideoControl for still pin");
            try
            {
                var videoControl = (IAMVideoControl)controlObject;
                NativeMethods.ThrowIfFailed(videoControl.SetMode(stillPin, VideoControlFlags.Trigger), "Trigger still pin");
            }
            finally
            {
                NativeMethods.ReleaseComObject(controlObject);
            }

            return TrySaveSampleGrabberBuffer(filePath, cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    private bool TrySaveSampleGrabberBuffer(string filePath, CancellationToken cancellationToken)
    {
        if (sampleGrabber is null)
        {
            return false;
        }

        for (var attempt = 0; attempt < 20; attempt += 1)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bufferSize = 0;
            var hr = sampleGrabber.GetCurrentBuffer(ref bufferSize, IntPtr.Zero);
            if (hr >= 0 && bufferSize > 0)
            {
                var buffer = Marshal.AllocCoTaskMem(bufferSize);
                try
                {
                    NativeMethods.ThrowIfFailed(sampleGrabber.GetCurrentBuffer(ref bufferSize, buffer), "Read still pin sample");
                    SaveSampleBufferAsJpeg(buffer, bufferSize, filePath);
                    return true;
                }
                finally
                {
                    Marshal.FreeCoTaskMem(buffer);
                }
            }

            Thread.Sleep(50);
        }

        return false;
    }

    private void SaveSampleBufferAsJpeg(IntPtr buffer, int bufferSize, string filePath)
    {
        if (sampleGrabber is null)
        {
            throw new InvalidOperationException("Still pin sample grabber is not configured.");
        }

        var mediaType = new AMMediaType();
        try
        {
            NativeMethods.ThrowIfFailed(sampleGrabber.GetConnectedMediaType(mediaType), "Read connected still media type");
            if (mediaType.FormatPtr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Still pin did not provide a video format.");
            }

            var videoInfo = Marshal.PtrToStructure<VideoInfoHeader>(mediaType.FormatPtr);
            using var bitmap = CreateBitmapFromRgb24Buffer(buffer, bufferSize, videoInfo.BitmapInfoHeader);
            bitmap.Save(filePath, ImageFormat.Jpeg);
        }
        finally
        {
            NativeMethods.FreeAMMediaType(mediaType);
        }
    }

    private static Bitmap CreateBitmapFromRgb24Buffer(IntPtr buffer, int bufferSize, BitmapInfoHeader bitmapInfo)
    {
        var width = Math.Abs(bitmapInfo.Width);
        var height = Math.Abs(bitmapInfo.Height);
        if (width == 0 || height == 0 || bitmapInfo.BitCount != 24)
        {
            throw new InvalidOperationException($"Unsupported still image format: {bitmapInfo.Width} x {bitmapInfo.Height}, {bitmapInfo.BitCount} bpp.");
        }

        var sourceStride = ((width * bitmapInfo.BitCount + 31) / 32) * 4;
        if (bufferSize < sourceStride * height)
        {
            throw new InvalidOperationException("Still image buffer is smaller than the reported video format.");
        }

        var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format24bppRgb
        );

        try
        {
            var row = new byte[sourceStride];
            var destinationStride = Math.Abs(bitmapData.Stride);
            var copyBytes = Math.Min(sourceStride, destinationStride);
            var bottomUp = bitmapInfo.Height > 0;
            for (var y = 0; y < height; y += 1)
            {
                var sourceY = bottomUp ? height - 1 - y : y;
                var destinationY = bitmapData.Stride < 0 ? height - 1 - y : y;
                Marshal.Copy(IntPtr.Add(buffer, sourceY * sourceStride), row, 0, sourceStride);
                Marshal.Copy(row, 0, IntPtr.Add(bitmapData.Scan0, destinationY * destinationStride), copyBytes);
            }
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }

        return bitmap;
    }

    private void CapturePreviewWindow(string filePath)
    {
        if (previewHandle == IntPtr.Zero)
        {
            LastCaptureRoute = CameraCaptureRoute.Unavailable;
            throw new InvalidOperationException("Preview is not running.");
        }

        if (!NativeMethods.GetWindowRect(previewHandle, out var rect))
        {
            LastCaptureRoute = CameraCaptureRoute.Unavailable;
            throw new InvalidOperationException("Unable to read preview window bounds.");
        }

        var width = Math.Max(1, rect.Right - rect.Left);
        var height = Math.Max(1, rect.Bottom - rect.Top);
        using var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new DrawingSize(width, height));
        bitmap.Save(filePath, ImageFormat.Jpeg);
    }

    private void StopPreviewCore()
    {
        try
        {
            mediaControl?.Stop();
            videoWindow?.put_Visible(DirectShowConstants.Oafalse);
            videoWindow?.put_Owner(IntPtr.Zero);
        }
        catch
        {
        }

        NativeMethods.ReleaseComObject(stillPin);
        NativeMethods.ReleaseComObject(nullRendererFilter);
        NativeMethods.ReleaseComObject(sampleGrabberFilter);
        NativeMethods.ReleaseComObject(sourceFilter);
        NativeMethods.ReleaseComObject(captureGraphBuilder);
        NativeMethods.ReleaseComObject(graphBuilder);

        stillPin = null;
        sampleGrabber = null;
        nullRendererFilter = null;
        sampleGrabberFilter = null;
        sourceFilter = null;
        videoWindow = null;
        mediaControl = null;
        captureGraphBuilder = null;
        graphBuilder = null;
        previewHandle = IntPtr.Zero;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }
}
