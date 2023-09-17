using FunPress.ViewModels.Mvvm;
using System.Threading.Tasks;
using System.Windows.Input;
using FunPress.ViewModels.Contracts;
using FunPress.Views.Mvvm;
using Microsoft.Extensions.Logging;
using FunPress.Views.DialogViews;
using FunPress.Views.Factory;
using FunPress.Views.Mvvm.Parameters;
using FunPress.Views.Params;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using FunPress.Common.Constants;
using FunPress.Common.Types.Enums;
using FunPress.Core.Services;
using FunPress.Common.Types.Models;

namespace FunPress.ViewModels.Implementations
{
    public class FunPressViewModel : ObservableObject, IFunPressViewModel
    {
        private readonly ILogger<FunPressViewModel> _logger;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly IApplicationService _applicationService;
        private readonly IPrinterService _printerService;
        private readonly IImageService _imageService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ISerializeService _serializeService;
        private readonly IJobService _jobService;
        private readonly IViewFactory _viewFactory;

        private const string TrackFilesJobName = "TrackNewFiles";

        private IManageView _viewModelView;
        private DispatcherTimer _clearPrintingResultMessageTimer;
        private bool _isActionAvailable = true;
        private string _currentDirectory;
        private readonly string[] _validExtensions = { ".jpg", ".png", ".jpeg" };

        #region Observable properties

        private ObservableCollection<string> _availablePrinterNames;
        public ObservableCollection<string> AvailablePrinterNames
        {
            get => _availablePrinterNames;
            private set
            {
                _availablePrinterNames = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<FileImageInfo> _availableImages;
        public ObservableCollection<FileImageInfo> AvailableImages
        {
            get => _availableImages;
            private set
            {
                _availableImages = value;
                NotifyPropertyChanged();
            }
        }

        private FileImageInfo _selectedImage;
        public FileImageInfo SelectedImage
        {
            get => _selectedImage;
            set
            {
                _selectedImage = value;
                NotifyPropertyChanged();
            }
        }

        private string _applicationTitle;
        public string ApplicationTitle
        {
            get => _applicationTitle;
            set
            {
                _applicationTitle = value;
                NotifyPropertyChanged();
            }
        }

        private string _selectedPrinterName;
        public string SelectedPrinterName
        {
            get => _selectedPrinterName;
            set
            {
                _selectedPrinterName = value;
                NotifyPropertyChanged();
            }
        }

        private string _printResult;
        public string PrintResult
        {
            get => _printResult;
            set
            {
                _printResult = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand GetImagesFromFolderCommand { get; private set; }
        public ICommand ApplicationShutdownCommandAsync { get; private set; }
        public ICommand PrintImageCommand { get; private set; }

        #endregion

        public FunPressViewModel(
            ILogger<FunPressViewModel> logger, 
            IApplicationEnvironment applicationEnvironment,
            IApplicationService applicationService, 
            IPrinterService printerService,
            IImageService imageService,
            IDateTimeService dateTimeService,
            ISerializeService serializeService,
            IJobService jobService,
            IViewFactory viewFactory
            )
        {
            _logger = logger;
            _applicationEnvironment = applicationEnvironment;
            _applicationService = applicationService;
            _printerService = printerService;
            _imageService = imageService;
            _dateTimeService = dateTimeService;
            _serializeService = serializeService;
            _jobService = jobService;
            _viewFactory = viewFactory;
        }

        public void AssignView(IManageView manageView)
        {
            _viewModelView = manageView;

            _logger.LogInformation("View assigned");
        }

        public Task InitializeDataAsync(object param = null)
        {
            _clearPrintingResultMessageTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _clearPrintingResultMessageTimer.Tick += ClearPrintingResultMessageTimer_Tick;

            ApplicationTitle = ApplicationConstants.ApplicationName;
            SelectedImage = new FileImageInfo();
            AvailablePrinterNames = new ObservableCollection<string>(_printerService.GetPrinterNames());
            AvailableImages = new ObservableCollection<FileImageInfo>();

            ApplicationShutdownCommandAsync = new RelayAsyncCommand(ApplicationShutdownAsync, CanExecuteCommand);
            PrintImageCommand = new RelayAsyncCommand(PrintImageAsync, CanExecuteCommand);
            GetImagesFromFolderCommand = new RelayCommand(GetImagesFromFolder, CanExecuteCommand);

            PropertyChanged += FunPressViewModel_PropertyChanged;

            _logger.LogInformation("Model data initialized");

            return Task.CompletedTask;
        }

        public void ClearData()
        {
            if (_jobService.IsJobExist(TrackFilesJobName))
            {
                _jobService.FinishJob(TrackFilesJobName);
            }

            PropertyChanged -= FunPressViewModel_PropertyChanged;

            _logger.LogInformation("Model data cleared");
        }

        #region Command methods

        private bool CanExecuteCommand(object param = null)
        {
            return _isActionAvailable;
        }

        private async Task ApplicationShutdownAsync(object param = null)
        {
            try
            {
                _isActionAvailable = false;

                var viewParameters = new CreateViewParameters
                {
                    Parent = _viewModelView,
                    AdditionalParameters = new MessageDialogParam("Do you want to close the app?",
                        new[] { MessageDialogButton.Yes, MessageDialogButton.No })
                };

                var dialogResult = await _viewFactory.Get<IMessageDialogView>()
                    .ShowDialogViewAsync(viewParameters);

                if (!dialogResult)
                {
                    return;
                }

                var closeViewParameters = new CloseViewParameters { CloseViewResult = false };

                await _viewModelView.CloseAsync(closeViewParameters);

                _applicationService.ApplicationShutdown();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(ApplicationShutdownAsync));
            }
            finally
            {
                _isActionAvailable = true;
            }
        }

        private void GetImagesFromFolder(object param = null)
        {
            try
            {
                _isActionAvailable = false;

                _logger.LogInformation("Invoke in {Method}. Start invoking", 
                    nameof(GetImagesFromFolder));

                using (var folderBrowser = new FolderBrowserDialog())
                {
                    var result = folderBrowser.ShowDialog();

                    if (result != DialogResult.OK)
                    {
                        return;
                    }

                    _currentDirectory = folderBrowser.SelectedPath;

                    _logger.LogInformation("Invoke in {Method}. Folder selected. Current folder: {Folder}", 
                        nameof(GetImagesFromFolder), _currentDirectory);

                    AvailableImages.Clear();

                    if (_jobService.IsJobExist(TrackFilesJobName))
                    {
                        _jobService.FinishJob(TrackFilesJobName);
                    }

                    _jobService.CreateJob(TrackFilesJobName, TimeSpan.FromSeconds(2), CheckNewImages);

                    try
                    {
                        var images = GetImages(_currentDirectory);

                        _logger.LogInformation("Invoke in {Method}. Images in folder: {Images}", 
                            nameof(GetImagesFromFolder), _serializeService.SerializeObject(images));

                        foreach (var imagePath in images.OrderBy(File.GetLastWriteTime))
                        {
                            AvailableImages.Add(new FileImageInfo
                            {
                                ImageName = Path.GetFileName(imagePath),
                                ImagePath = imagePath
                            });
                        }

                        _jobService.StartJob(TrackFilesJobName, false);

                        if (!AvailableImages.Any())
                        {
                            return;
                        }

                        SelectedImage = AvailableImages.First();

                        _logger.LogInformation("Invoke in {Method}. Selected image: {Images}", 
                            nameof(GetImagesFromFolder), _serializeService.SerializeObject(SelectedImage));
                    }
                    catch (UnauthorizedAccessException unauthorizedAccessException)
                    {
                        _logger.LogDebug(unauthorizedAccessException, "Invoke in {Method}",
                            nameof(GetImagesFromFolder));
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(GetImagesFromFolder));
            }
            finally
            {
                _isActionAvailable = true;

                _logger.LogInformation("Invoke in {Method}. Finish invoking", 
                    nameof(GetImagesFromFolder));
            }
        }

        private async Task PrintImageAsync(object param = null)
        {
            try
            {
                _isActionAvailable = false;

                _logger.LogInformation("Invoke in {Method}. Start invoking", 
                    nameof(PrintImageAsync));

                if (string.IsNullOrWhiteSpace(SelectedPrinterName))
                {
                    await GenerateErrorNotificationAsync("Printer does not selected. Please, select printer name.");

                    return;
                }

                if (string.IsNullOrWhiteSpace(SelectedImage.ImagePath))
                {
                    await GenerateErrorNotificationAsync("Image is not selected. Please, select the image.");

                    return;
                }

                _logger.LogInformation("Invoke in {Method}. Selected Printer: {SelectedPrinter}", 
                    nameof(PrintImageAsync), SelectedPrinterName);

                _logger.LogInformation("Invoke in {Method}. Selected image: {SelectedImage}", 
                    nameof(PrintImageAsync), SelectedImage.ImagePath);

                var newImageFileName = Path.Combine(_applicationEnvironment.GetResultsPath(), 
                    $"{_dateTimeService.GetDateTimeNow():dd_MM_yyyy__HH_mm_ss_fff}.jpg");

                var imageGenerationResult = _imageService.GenerateImageByTemplateOne(SelectedImage.ImagePath, newImageFileName);
                if (!imageGenerationResult)
                {
                    await GenerateErrorNotificationAsync("Cannot generate the image. Please check logs.");

                    return;
                }

                //var printingResult = _printerService.PrintImage(SelectedPrinterName, newImageFileName);
                //if (!printingResult)
                //{
                //    await GenerateErrorNotificationAsync("Cannot print image. Please check logs.");

                //    return;
                //}

                PrintResult = "Printing started!";

                _clearPrintingResultMessageTimer.Start();

                _logger.LogInformation("Invoke in {Method}. Printing is finished", nameof(PrintImageAsync));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(PrintImageAsync));
            }
            finally
            {
                _isActionAvailable = true;

                _logger.LogInformation("Invoke in {Method}. Finish invoking", 
                    nameof(GetImagesFromFolder));
            }
        }

        #endregion

        #region Events

        private void ClearPrintingResultMessageTimer_Tick(object sender, EventArgs args)
        {
            PrintResult = string.Empty;

            _clearPrintingResultMessageTimer.Stop();
        }

        private void FunPressViewModel_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(SelectedImage):
                    _logger.LogInformation("Selected image: {SelectedImage}", SelectedImage.ImagePath);
                    break;
                case nameof(SelectedPrinterName):
                    _logger.LogInformation("Selected printer: {SelectedPrinter}", SelectedPrinterName);
                    break;
            }
        }

        #endregion

        #region Private methods

        private async Task GenerateErrorNotificationAsync(string errorMessage)
        {
            var viewParameters = new CreateViewParameters
            {
                Parent = _viewModelView,
                AdditionalParameters = new MessageDialogParam(errorMessage, new[] { MessageDialogButton.Ok })
            };

            await _viewFactory.Get<IMessageDialogView>().ShowDialogViewAsync(viewParameters);
        }

        private IEnumerable<string> GetImages(string directoryToCheck)
        {
            return Directory.GetFiles(directoryToCheck, "*.*", SearchOption.AllDirectories)
                .Where(file => _validExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));
        }

        private async Task CheckNewImages(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || string.IsNullOrWhiteSpace(_currentDirectory))
            {
                return;
            }

            var imagesPath = GetImages(_currentDirectory).ToArray();

            await _applicationService.DispatcherInvokeAsync(() =>
            {
                if (imagesPath.Length > AvailableImages.Count)
                {
                    foreach (var imagePath in imagesPath)
                    {
                        if (AvailableImages.Any(x => x.ImageName == Path.GetFileName(imagePath)))
                        {
                            continue;
                        }

                        AvailableImages.Add(new FileImageInfo
                        {
                            ImageName = Path.GetFileName(imagePath),
                            ImagePath = imagePath
                        });
                    }
                }
                else if (imagesPath.Length < AvailableImages.Count)
                {
                    foreach (var imagePath in AvailableImages.Where(x => imagesPath.All(path => path != x.ImagePath)))
                    {
                        AvailableImages.Remove(imagePath);
                    }
                }

            }, DispatcherPriority.Send, cancellationToken);
        }

        #endregion
    }
}
