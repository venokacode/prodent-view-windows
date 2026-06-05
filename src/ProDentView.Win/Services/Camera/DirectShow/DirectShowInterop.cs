using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace ProDentView.Win.Services.Camera.DirectShow;

internal static class DirectShowGuids
{
    public static readonly Guid FilterGraph = new("e436ebb3-524f-11ce-9f53-0020af0ba770");
    public static readonly Guid CaptureGraphBuilder2 = new("bf87b6e1-8c27-11d0-b3f0-00aa003761c5");
    public static readonly Guid SystemDeviceEnum = new("62be5d10-60eb-11d0-bd3b-00a0c911ce86");
    public static readonly Guid VideoInputDeviceCategory = new("860bb310-5d01-11d0-bd3b-00a0c911ce86");
    public static readonly Guid SampleGrabber = new("c1f400a0-3f08-11d3-9f0b-006008039e37");
    public static readonly Guid NullRenderer = new("c1f400a4-3f08-11d3-9f0b-006008039e37");

    public static readonly Guid PinCategoryPreview = new("fb6c4282-0353-11d1-905f-0000c0cc16ba");
    public static readonly Guid PinCategoryCapture = new("fb6c4281-0353-11d1-905f-0000c0cc16ba");
    public static readonly Guid PinCategoryStill = new("fb6c428a-0353-11d1-905f-0000c0cc16ba");

    public static readonly Guid MediaTypeVideo = new("73646976-0000-0010-8000-00aa00389b71");
    public static readonly Guid MediaSubTypeRgb24 = new("e436eb7d-524f-11ce-9f53-0020af0ba770");
    public static readonly Guid FormatVideoInfo = new("05589f80-c356-11ce-bf01-00aa0055595a");

    public static readonly Guid IidIBaseFilter = new("56a86895-0ad4-11ce-b03a-0020af0ba770");
    public static readonly Guid IidIAMVideoControl = new("6a2e0670-28e4-11d0-a18c-00a0c9118956");
    public static readonly Guid IidIAMStreamConfig = new("c6e13340-30ac-11d0-a18c-00a0c9118956");
}

internal static class DirectShowConstants
{
    public const int SOk = 0;
    public const int SFalse = 1;
    public const int WsChild = 0x40000000;
    public const int WsClipChildren = 0x02000000;
    public const int WsClipSiblings = 0x04000000;
    public const int Oatrue = -1;
    public const int Oafalse = 0;
}

internal enum PinDirection
{
    Input = 0,
    Output = 1
}

[Flags]
internal enum VideoControlFlags
{
    None = 0x0,
    FlipHorizontal = 0x1,
    FlipVertical = 0x2,
    ExternalTriggerEnable = 0x4,
    Trigger = 0x8
}

[ComImport]
[Guid("29840822-5b84-11d0-bd3b-00a0c911ce86")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ICreateDevEnum
{
    [PreserveSig]
    int CreateClassEnumerator(
        [In] ref Guid type,
        out IEnumMoniker? enumMoniker,
        int flags
    );
}

[ComImport]
[Guid("55272a00-42cb-11ce-8135-00aa004bb851")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPropertyBag
{
    [PreserveSig]
    int Read(
        [MarshalAs(UnmanagedType.LPWStr)] string propertyName,
        [MarshalAs(UnmanagedType.Struct)] out object value,
        IntPtr errorLog
    );

    [PreserveSig]
    int Write(
        [MarshalAs(UnmanagedType.LPWStr)] string propertyName,
        [MarshalAs(UnmanagedType.Struct)] ref object value
    );
}

[ComImport]
[Guid("56a8689f-0ad4-11ce-b03a-0020af0ba770")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IFilterGraph
{
    [PreserveSig]
    int AddFilter(IBaseFilter filter, [MarshalAs(UnmanagedType.LPWStr)] string name);

    [PreserveSig]
    int RemoveFilter(IBaseFilter filter);

    [PreserveSig]
    int EnumFilters(out IntPtr enumFilters);

    [PreserveSig]
    int FindFilterByName([MarshalAs(UnmanagedType.LPWStr)] string name, out IBaseFilter filter);

    [PreserveSig]
    int ConnectDirect(IPin outputPin, IPin inputPin, IntPtr mediaType);

    [PreserveSig]
    int Reconnect(IPin pin);

    [PreserveSig]
    int Disconnect(IPin pin);

    [PreserveSig]
    int SetDefaultSyncSource();
}

[ComImport]
[Guid("56a868a9-0ad4-11ce-b03a-0020af0ba770")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGraphBuilder : IFilterGraph
{
    [PreserveSig]
    new int AddFilter(IBaseFilter filter, [MarshalAs(UnmanagedType.LPWStr)] string name);

    [PreserveSig]
    new int RemoveFilter(IBaseFilter filter);

    [PreserveSig]
    new int EnumFilters(out IntPtr enumFilters);

    [PreserveSig]
    new int FindFilterByName([MarshalAs(UnmanagedType.LPWStr)] string name, out IBaseFilter filter);

    [PreserveSig]
    new int ConnectDirect(IPin outputPin, IPin inputPin, IntPtr mediaType);

    [PreserveSig]
    new int Reconnect(IPin pin);

    [PreserveSig]
    new int Disconnect(IPin pin);

    [PreserveSig]
    new int SetDefaultSyncSource();

    [PreserveSig]
    int Connect(IPin outputPin, IPin inputPin);

    [PreserveSig]
    int Render(IPin outputPin);

    [PreserveSig]
    int RenderFile([MarshalAs(UnmanagedType.LPWStr)] string file, [MarshalAs(UnmanagedType.LPWStr)] string playList);

    [PreserveSig]
    int AddSourceFilter([MarshalAs(UnmanagedType.LPWStr)] string fileName, [MarshalAs(UnmanagedType.LPWStr)] string filterName, out IBaseFilter filter);

    [PreserveSig]
    int SetLogFile(IntPtr fileHandle);

    [PreserveSig]
    int Abort();

    [PreserveSig]
    int ShouldOperationContinue();
}

[ComImport]
[Guid("bf87b6e1-8c27-11d0-b3f0-00aa003761c5")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ICaptureGraphBuilder2
{
    [PreserveSig]
    int SetFiltergraph(IGraphBuilder graphBuilder);

    [PreserveSig]
    int GetFiltergraph(out IGraphBuilder graphBuilder);

    [PreserveSig]
    int SetOutputFileName(ref Guid type, [MarshalAs(UnmanagedType.LPWStr)] string fileName, out IBaseFilter mux, out IntPtr sink);

    [PreserveSig]
    int FindInterface(ref Guid category, ref Guid type, [MarshalAs(UnmanagedType.IUnknown)] object source, ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object result);

    [PreserveSig]
    int RenderStream(ref Guid category, ref Guid type, [MarshalAs(UnmanagedType.IUnknown)] object source, IBaseFilter? compressor, IBaseFilter? renderer);

    [PreserveSig]
    int ControlStream(ref Guid category, ref Guid type, [MarshalAs(UnmanagedType.Interface)] IBaseFilter filter, long start, long stop, short startCookie, short stopCookie);

    [PreserveSig]
    int AllocCapFile([MarshalAs(UnmanagedType.LPWStr)] string fileName, long size);

    [PreserveSig]
    int CopyCaptureFile([MarshalAs(UnmanagedType.LPWStr)] string oldFile, [MarshalAs(UnmanagedType.LPWStr)] string newFile, [MarshalAs(UnmanagedType.Bool)] bool allowEscAbort, IntPtr callback);

    [PreserveSig]
    int FindPin([MarshalAs(UnmanagedType.IUnknown)] object source, PinDirection direction, ref Guid category, ref Guid type, [MarshalAs(UnmanagedType.Bool)] bool unconnected, int zeroBasedIndex, out IPin pin);
}

[ComImport]
[Guid("56a86895-0ad4-11ce-b03a-0020af0ba770")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IBaseFilter
{
    [PreserveSig]
    int GetClassID(out Guid classId);

    [PreserveSig]
    int Stop();

    [PreserveSig]
    int Pause();

    [PreserveSig]
    int Run(long start);

    [PreserveSig]
    int GetState(int timeout, out int state);

    [PreserveSig]
    int SetSyncSource(IntPtr clock);

    [PreserveSig]
    int GetSyncSource(out IntPtr clock);

    [PreserveSig]
    int EnumPins(out IntPtr enumPins);

    [PreserveSig]
    int FindPin([MarshalAs(UnmanagedType.LPWStr)] string id, out IPin pin);

    [PreserveSig]
    int QueryFilterInfo(out FilterInfo filterInfo);

    [PreserveSig]
    int JoinFilterGraph(IFilterGraph graph, [MarshalAs(UnmanagedType.LPWStr)] string name);

    [PreserveSig]
    int QueryVendorInfo([MarshalAs(UnmanagedType.LPWStr)] out string vendorInfo);
}

[ComImport]
[Guid("56a86891-0ad4-11ce-b03a-0020af0ba770")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPin
{
    [PreserveSig]
    int Connect(IPin receivePin, IntPtr mediaType);

    [PreserveSig]
    int ReceiveConnection(IPin connector, IntPtr mediaType);

    [PreserveSig]
    int Disconnect();

    [PreserveSig]
    int ConnectedTo(out IPin pin);

    [PreserveSig]
    int ConnectionMediaType(IntPtr mediaType);

    [PreserveSig]
    int QueryPinInfo(out PinInfo pinInfo);

    [PreserveSig]
    int QueryDirection(out PinDirection pinDirection);

    [PreserveSig]
    int QueryId([MarshalAs(UnmanagedType.LPWStr)] out string id);

    [PreserveSig]
    int QueryAccept(IntPtr mediaType);

    [PreserveSig]
    int EnumMediaTypes(out IntPtr enumMediaTypes);

    [PreserveSig]
    int QueryInternalConnections(IntPtr pins, ref int count);

    [PreserveSig]
    int EndOfStream();

    [PreserveSig]
    int BeginFlush();

    [PreserveSig]
    int EndFlush();

    [PreserveSig]
    int NewSegment(long start, long stop, double rate);
}

[ComImport]
[Guid("56a868b3-0ad4-11ce-b03a-0020af0ba770")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
internal interface IBasicAudio
{
}

[ComImport]
[Guid("56a868b1-0ad4-11ce-b03a-0020af0ba770")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
internal interface IMediaControl
{
    [PreserveSig]
    int Run();

    [PreserveSig]
    int Pause();

    [PreserveSig]
    int Stop();

    [PreserveSig]
    int GetState(int msTimeout, out int filterState);
}

[ComImport]
[Guid("56a868b4-0ad4-11ce-b03a-0020af0ba770")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
internal interface IVideoWindow
{
    [PreserveSig]
    int put_Caption([MarshalAs(UnmanagedType.BStr)] string caption);

    [PreserveSig]
    int get_Caption([MarshalAs(UnmanagedType.BStr)] out string caption);

    [PreserveSig]
    int put_WindowStyle(int windowStyle);

    [PreserveSig]
    int get_WindowStyle(out int windowStyle);

    [PreserveSig]
    int put_WindowStyleEx(int windowStyleEx);

    [PreserveSig]
    int get_WindowStyleEx(out int windowStyleEx);

    [PreserveSig]
    int put_AutoShow(int autoShow);

    [PreserveSig]
    int get_AutoShow(out int autoShow);

    [PreserveSig]
    int put_WindowState(int windowState);

    [PreserveSig]
    int get_WindowState(out int windowState);

    [PreserveSig]
    int put_BackgroundPalette(int backgroundPalette);

    [PreserveSig]
    int get_BackgroundPalette(out int backgroundPalette);

    [PreserveSig]
    int put_Visible(int visible);

    [PreserveSig]
    int get_Visible(out int visible);

    [PreserveSig]
    int put_Left(int left);

    [PreserveSig]
    int get_Left(out int left);

    [PreserveSig]
    int put_Width(int width);

    [PreserveSig]
    int get_Width(out int width);

    [PreserveSig]
    int put_Top(int top);

    [PreserveSig]
    int get_Top(out int top);

    [PreserveSig]
    int put_Height(int height);

    [PreserveSig]
    int get_Height(out int height);

    [PreserveSig]
    int put_Owner(IntPtr owner);

    [PreserveSig]
    int get_Owner(out IntPtr owner);

    [PreserveSig]
    int put_MessageDrain(IntPtr drain);

    [PreserveSig]
    int get_MessageDrain(out IntPtr drain);

    [PreserveSig]
    int get_BorderColor(out int color);

    [PreserveSig]
    int put_BorderColor(int color);

    [PreserveSig]
    int get_FullScreenMode(out int fullScreenMode);

    [PreserveSig]
    int put_FullScreenMode(int fullScreenMode);

    [PreserveSig]
    int SetWindowForeground(int focus);

    [PreserveSig]
    int NotifyOwnerMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);

    [PreserveSig]
    int SetWindowPosition(int left, int top, int width, int height);

    [PreserveSig]
    int GetWindowPosition(out int left, out int top, out int width, out int height);
}

[ComImport]
[Guid("6a2e0670-28e4-11d0-a18c-00a0c9118956")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAMVideoControl
{
    [PreserveSig]
    int GetCaps(IPin pin, out VideoControlFlags flags);

    [PreserveSig]
    int SetMode(IPin pin, VideoControlFlags mode);

    [PreserveSig]
    int GetMode(IPin pin, out VideoControlFlags mode);

    [PreserveSig]
    int GetCurrentActualFrameRate(IPin pin, out long actualFrameRate);

    [PreserveSig]
    int GetMaxAvailableFrameRate(IPin pin, int index, Size dimensions, out long maxAvailableFrameRate);

    [PreserveSig]
    int GetFrameRateList(IPin pin, int index, Size dimensions, out int listSize, out IntPtr frameRates);
}

[ComImport]
[Guid("c6e13340-30ac-11d0-a18c-00a0c9118956")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAMStreamConfig
{
    [PreserveSig]
    int SetFormat(AMMediaType mediaType);

    [PreserveSig]
    int GetFormat(out AMMediaType mediaType);

    [PreserveSig]
    int GetNumberOfCapabilities(out int count, out int size);

    [PreserveSig]
    int GetStreamCaps(int index, out AMMediaType mediaType, IntPtr streamConfigCaps);
}

[ComImport]
[Guid("56a868b2-0ad4-11ce-b03a-0020af0ba770")]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
internal interface IMediaEvent
{
}

[ComImport]
[Guid("c1f400a0-3f08-11d3-9f0b-006008039e37")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISampleGrabber
{
    [PreserveSig]
    int SetOneShot([MarshalAs(UnmanagedType.Bool)] bool oneShot);

    [PreserveSig]
    int SetMediaType(AMMediaType mediaType);

    [PreserveSig]
    int GetConnectedMediaType(AMMediaType mediaType);

    [PreserveSig]
    int SetBufferSamples([MarshalAs(UnmanagedType.Bool)] bool bufferThem);

    [PreserveSig]
    int GetCurrentBuffer(ref int bufferSize, IntPtr buffer);

    [PreserveSig]
    int GetCurrentSample(out IntPtr sample);

    [PreserveSig]
    int SetCallback(IntPtr callback, int whichMethodToCallback);
}

[StructLayout(LayoutKind.Sequential)]
internal struct FilterInfo
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string AchName;
    public IntPtr Graph;
}

[StructLayout(LayoutKind.Sequential)]
internal struct PinInfo
{
    public IBaseFilter Filter;
    public PinDirection Direction;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Name;
}

[StructLayout(LayoutKind.Sequential)]
internal struct Size
{
    public int Width;
    public int Height;
}

[StructLayout(LayoutKind.Sequential)]
internal struct Rect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

[StructLayout(LayoutKind.Sequential)]
internal struct VideoInfoHeader
{
    public Rect Source;
    public Rect Target;
    public int BitRate;
    public int BitErrorRate;
    public long AvgTimePerFrame;
    public BitmapInfoHeader BitmapInfoHeader;
}

[StructLayout(LayoutKind.Sequential)]
internal struct VideoStreamConfigCaps
{
    public Guid Guid;
    public uint VideoStandard;
    public Size InputSize;
    public Size MinCroppingSize;
    public Size MaxCroppingSize;
    public int CropGranularityX;
    public int CropGranularityY;
    public int CropAlignX;
    public int CropAlignY;
    public Size MinOutputSize;
    public Size MaxOutputSize;
    public int OutputGranularityX;
    public int OutputGranularityY;
    public int StretchTapsX;
    public int StretchTapsY;
    public int ShrinkTapsX;
    public int ShrinkTapsY;
    public long MinFrameInterval;
    public long MaxFrameInterval;
    public int MinBitsPerSecond;
    public int MaxBitsPerSecond;
}

[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal struct BitmapInfoHeader
{
    public int Size;
    public int Width;
    public int Height;
    public short Planes;
    public short BitCount;
    public int Compression;
    public int SizeImage;
    public int XPelsPerMeter;
    public int YPelsPerMeter;
    public int ClrUsed;
    public int ClrImportant;
}

[StructLayout(LayoutKind.Sequential)]
internal sealed class AMMediaType
{
    public Guid MajorType;
    public Guid SubType;
    [MarshalAs(UnmanagedType.Bool)]
    public bool FixedSizeSamples;
    [MarshalAs(UnmanagedType.Bool)]
    public bool TemporalCompression;
    public int SampleSize;
    public Guid FormatType;
    public IntPtr Unknown;
    public int FormatSize;
    public IntPtr FormatPtr;
}

internal static class NativeMethods
{
    [DllImport("ole32.dll")]
    public static extern int CreateBindCtx(int reserved, out IBindCtx bindCtx);

    [DllImport("ole32.dll")]
    public static extern int CreateItemMoniker(
        [MarshalAs(UnmanagedType.LPWStr)] string delimiter,
        [MarshalAs(UnmanagedType.LPWStr)] string item,
        out IMoniker moniker
    );

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetClientRect(IntPtr hwnd, out Rect rect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);

    public static object CreateComObject(Guid clsid)
    {
        var type = Type.GetTypeFromCLSID(clsid, throwOnError: true);
        return Activator.CreateInstance(type!) ?? throw new InvalidOperationException($"Unable to create COM object {clsid}.");
    }

    public static void ThrowIfFailed(int hr, string operation)
    {
        if (hr < 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }
    }

    public static void FreeAMMediaType(AMMediaType? mediaType)
    {
        if (mediaType is null)
        {
            return;
        }

        if (mediaType.FormatSize != 0 && mediaType.FormatPtr != IntPtr.Zero)
        {
            Marshal.FreeCoTaskMem(mediaType.FormatPtr);
            mediaType.FormatPtr = IntPtr.Zero;
            mediaType.FormatSize = 0;
        }

        if (mediaType.Unknown != IntPtr.Zero)
        {
            Marshal.Release(mediaType.Unknown);
            mediaType.Unknown = IntPtr.Zero;
        }
    }

    public static void ReleaseComObject(object? value)
    {
        if (value is not null && Marshal.IsComObject(value))
        {
            Marshal.FinalReleaseComObject(value);
        }
    }
}
