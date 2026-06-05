using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ProDentView.Win.Models;
using ProDentView.Win.Services;
using ProDentView.Win.Services.Camera;

namespace ProDentView.Win;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<PatientRecord> patients = [];
    private readonly ObservableCollection<CapturedImageRecord> images = [];
    private readonly ObservableCollection<CameraDeviceInfo> cameraDevices = [];
    private readonly LocalImageStore imageStore = new();
    private readonly PatientStore patientStore = new();
    private readonly ICameraService cameraService = new DirectShowCameraService();

    private PatientRecord? selectedPatient;
    private bool isPopulatingCameras;

    public MainWindow()
    {
        InitializeComponent();
        PatientList.ItemsSource = patients;
        ImageList.ItemsSource = images;
        CameraComboBox.ItemsSource = cameraDevices;
        LoadPatients();

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await RefreshCamerasAsync(autoStart: true);
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        cameraService.StopPreviewAsync().GetAwaiter().GetResult();
        if (cameraService is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private void AddPatientButton_Click(object sender, RoutedEventArgs e)
    {
        var name = PatientNameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(this, "Patient name is required.", "ProDENT View", MessageBoxButton.OK, MessageBoxImage.Information);
            PatientNameBox.Focus();
            return;
        }

        var patient = new PatientRecord
        {
            Name = name,
            ChartNumber = ChartNumberBox.Text.Trim(),
            Phone = PhoneBox.Text.Trim(),
            Email = EmailBox.Text.Trim(),
            Notes = NotesBox.Text.Trim()
        };
        patients.Add(patient);
        PatientList.SelectedItem = patient;
        SavePatients();
    }

    private void SavePatientButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedPatient is null)
        {
            MessageBox.Show(this, "Select a patient first.", "ProDENT View", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var name = PatientNameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(this, "Patient name is required.", "ProDENT View", MessageBoxButton.OK, MessageBoxImage.Information);
            PatientNameBox.Focus();
            return;
        }

        selectedPatient.Name = name;
        selectedPatient.ChartNumber = ChartNumberBox.Text.Trim();
        selectedPatient.Phone = PhoneBox.Text.Trim();
        selectedPatient.Email = EmailBox.Text.Trim();
        selectedPatient.Notes = NotesBox.Text.Trim();
        SavePatients();
        PatientList.Items.Refresh();
        SelectedPatientText.Text = selectedPatient.Name;
    }

    private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        PatientList.ItemsSource = string.IsNullOrWhiteSpace(SearchBox.Text)
            ? patients
            : patients.Where(patient => patient.Name.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    private void PatientList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        selectedPatient = PatientList.SelectedItem as PatientRecord;
        SelectedPatientText.Text = selectedPatient?.Name ?? "Select a patient";
        PopulatePatientFields(selectedPatient);
        images.Clear();
        if (selectedPatient is not null)
        {
            foreach (var image in imageStore.GetImages(selectedPatient))
            {
                images.Add(image);
            }
        }
    }

    private async void RefreshCameraButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshCamerasAsync(autoStart: true);
    }

    private async void CameraComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (isPopulatingCameras || CameraComboBox.SelectedItem is not CameraDeviceInfo device)
        {
            return;
        }

        await StartPreviewAsync(device);
    }

    private void PreviewFrame_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        cameraService.ResizePreview((int)e.NewSize.Width, (int)e.NewSize.Height);
    }

    private async void CaptureButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedPatient is null)
        {
            MessageBox.Show(this, "Select a patient first.", "ProDENT View", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var record = imageStore.ReserveCapturePath(selectedPatient);
        try
        {
            await cameraService.CaptureJpegAsync(record.FilePath);
            images.Add(record);
            CameraStatusText.Text = cameraService.LastStatus;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Capture failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedPatient is null)
        {
            MessageBox.Show(this, "Select a patient first.", "ProDENT View", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff"
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        foreach (var sourcePath in dialog.FileNames)
        {
            var targetPath = imageStore.ReserveImportPath(selectedPatient, sourcePath);
            System.IO.File.Copy(sourcePath, targetPath, overwrite: false);
            images.Add(new CapturedImageRecord
            {
                PatientId = selectedPatient.Id,
                FileName = System.IO.Path.GetFileName(targetPath),
                FilePath = targetPath,
                CapturedAt = DateTime.Now
            });
        }
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (images.Count == 0)
        {
            MessageBox.Show(this, "No images to export.", "ProDENT View", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select export folder",
            UseDescriptionForTitle = true
        };
        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return;
        }

        foreach (var image in images)
        {
            if (!System.IO.File.Exists(image.FilePath))
            {
                continue;
            }

            var targetPath = ReserveExportPath(dialog.SelectedPath, image.FileName);
            System.IO.File.Copy(image.FilePath, targetPath, overwrite: false);
        }

        MessageBox.Show(this, "Export complete.", "ProDENT View", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (ImageList.SelectedItem is not CapturedImageRecord record)
        {
            return;
        }

        images.Remove(record);
        if (System.IO.File.Exists(record.FilePath))
        {
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                record.FilePath,
                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin
            );
        }
    }

    private void ImageList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (ImageList.SelectedItem is not CapturedImageRecord record || !System.IO.File.Exists(record.FilePath))
        {
            return;
        }

        var image = new System.Windows.Controls.Image
        {
            Source = new BitmapImage(new Uri(record.FilePath)),
            Stretch = System.Windows.Media.Stretch.Uniform,
            Margin = new Thickness(12)
        };

        var viewer = new Window
        {
            Title = record.FileName,
            Width = 1100,
            Height = 780,
            MinWidth = 720,
            MinHeight = 520,
            Content = image,
            Owner = this,
            Background = System.Windows.Media.Brushes.Black
        };
        viewer.Show();
    }

    private void LoadPatients()
    {
        patients.Clear();
        foreach (var patient in patientStore.Load())
        {
            patients.Add(patient);
        }

        if (patients.Count > 0)
        {
            PatientList.SelectedItem = patients[0];
        }
    }

    private void SavePatients()
    {
        patientStore.Save(patients);
    }

    private void PopulatePatientFields(PatientRecord? patient)
    {
        PatientNameBox.Text = patient?.Name ?? "";
        ChartNumberBox.Text = patient?.ChartNumber ?? "";
        PhoneBox.Text = patient?.Phone ?? "";
        EmailBox.Text = patient?.Email ?? "";
        NotesBox.Text = patient?.Notes ?? "";
    }

    private async Task RefreshCamerasAsync(bool autoStart)
    {
        try
        {
            var devices = await cameraService.EnumerateAsync();
            isPopulatingCameras = true;
            cameraDevices.Clear();
            foreach (var device in devices)
            {
                cameraDevices.Add(device);
            }

            CameraComboBox.SelectedItem = cameraDevices.FirstOrDefault();
            isPopulatingCameras = false;

            if (devices.Count == 0)
            {
                PreviewStatusText.Visibility = Visibility.Visible;
                CameraStatusText.Text = "No UVC camera detected";
                return;
            }

            CameraStatusText.Text = $"{devices.Count} camera(s) detected";
            if (autoStart && CameraComboBox.SelectedItem is CameraDeviceInfo selectedDevice)
            {
                await StartPreviewAsync(selectedDevice);
            }
        }
        catch (Exception ex)
        {
            isPopulatingCameras = false;
            CameraStatusText.Text = "Camera enumeration failed";
            MessageBox.Show(this, ex.Message, "Camera enumeration failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async Task StartPreviewAsync(CameraDeviceInfo device)
    {
        try
        {
            PreviewStatusText.Visibility = Visibility.Hidden;
            await cameraService.StartPreviewAsync(device, PreviewPanel.Handle);
            cameraService.ResizePreview((int)PreviewFrame.ActualWidth, (int)PreviewFrame.ActualHeight);
            CameraStatusText.Text = cameraService.LastStatus;
        }
        catch (Exception ex)
        {
            PreviewStatusText.Visibility = Visibility.Visible;
            CameraStatusText.Text = "Preview failed";
            MessageBox.Show(this, ex.Message, "Preview failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static string ReserveExportPath(string folder, string fileName)
    {
        var target = System.IO.Path.Combine(folder, fileName);
        var extension = System.IO.Path.GetExtension(fileName);
        var baseName = System.IO.Path.GetFileNameWithoutExtension(fileName);
        var suffix = 1;
        while (System.IO.File.Exists(target))
        {
            target = System.IO.Path.Combine(folder, $"{baseName}-{suffix}{extension}");
            suffix += 1;
        }

        return target;
    }
}
