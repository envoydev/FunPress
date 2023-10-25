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
using System.Windows;
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
        private readonly IUserSettingsService _userSettingsService;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly IApplicationService _applicationService;
        private readonly IPrinterService _printerService;
        private readonly IImageService _imageService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ISerializeService _serializeService;
        private readonly IJobService _jobService;
        private readonly IDelayService _delayService;
        private readonly IFileService _fileService;
        private readonly IViewFactory _viewFactory;

        private const string TrackFilesJobName = "TrackNewFiles";

        private IManageView _viewModelView;
        private bool _isActionAvailable = true;
        private bool _isNeedPrintingAfterImageIsAppear;
        private readonly string[] _validExtensions = { ".jpg", ".png", ".jpeg" };
        private DispatcherTimer _resultTextDispatcherTimer;
        private CancellationTokenSource _cancelPrintingCancellationTokenSource;

        #region Observable properties

        private ObservableCollection<PrinterName> _printerNames;
        public ObservableCollection<PrinterName> PrinterNames
        {
            get => _printerNames;
            private set
            {
                _printerNames = value;
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

        private ObservableCollection<PrinterAction> _printerActions;
        public ObservableCollection<PrinterAction> PrinterActions
        {
            get => _printerActions;
            private set
            {
                _printerActions = value;
                NotifyPropertyChanged();
            }
        }

        private PrinterName _selectedPrinter;
        public PrinterName SelectedPrinter
        {
            get => _selectedPrinter;
            set
            {
                _selectedPrinter = value;
                NotifyPropertyChanged();
            }
        }

        private PrinterAction _selectedPrinterAction;
        public PrinterAction SelectedPrinterAction
        {
            get => _selectedPrinterAction;
            set
            {
                _selectedPrinterAction = value;
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

        private Visibility _printerButtonVisibility;
        public Visibility PrinterButtonVisibility
        {
            get => _printerButtonVisibility;
            set
            {
                _printerButtonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _cancelPrinterButtonVisibility;
        public Visibility CancelPrinterButtonVisibility
        {
            get => _cancelPrinterButtonVisibility;
            set
            {
                _cancelPrinterButtonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private string _textResult;
        public string TextResult
        {
            get => _textResult;
            private set
            {
                _textResult = value;
                NotifyPropertyChanged();
            }
        }

        private string _applicationTitle;
        public string ApplicationTitle
        {
            get => _applicationTitle;
            private set
            {
                _applicationTitle = value;
                NotifyPropertyChanged();
            }
        }

        private string _applicationVersion;
        public string ApplicationVersion
        {
            get => _applicationVersion;
            private set
            {
                _applicationVersion = value;
                NotifyPropertyChanged();
            }
        }
        
        private string _selectedFolder;
        public string SelectedFolder
        {
            get => _selectedFolder;
            private set
            {
                _selectedFolder = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand GetImagesFromFolderCommand { get; private set; }
        public ICommand ApplicationShutdownCommandAsync { get; private set; }
        public ICommand PrintPressCommand { get; private set; }
        public ICommand CancelPrintPressCommand { get; private set; }
        public ICommand SaveSettingsCommand { get; private set; }

        #endregion

        public FunPressViewModel(
            ILogger<FunPressViewModel> logger, 
            IUserSettingsService userSettingsService,
            IApplicationEnvironment applicationEnvironment,
            IApplicationService applicationService, 
            IPrinterService printerService,
            IImageService imageService,
            IDateTimeService dateTimeService,
            ISerializeService serializeService,
            IJobService jobService,
            IDelayService delayService, 
            IFileService fileService,
            IViewFactory viewFactory
            )
        {
            _logger = logger;
            _userSettingsService = userSettingsService;
            _applicationEnvironment = applicationEnvironment;
            _applicationService = applicationService;
            _printerService = printerService;
            _imageService = imageService;
            _dateTimeService = dateTimeService;
            _serializeService = serializeService;
            _jobService = jobService;
            _delayService = delayService;
            _fileService = fileService;
            _viewFactory = viewFactory;
        }

        public void AssignView(IManageView manageView)
        {
            _viewModelView = manageView;

            _logger.LogInformation("View assigned");
        }

        public Task InitializeDataAsync(object param = null)
        {
            _resultTextDispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _resultTextDispatcherTimer.Tick += ResultTextDispatcherTimer_Tick;
            _cancelPrintingCancellationTokenSource = new CancellationTokenSource();

            var printerNames = GetPrinterNames().ToArray();
            var printerActions = GetPrinterActions().ToArray();

            ApplicationVersion = _applicationEnvironment.GetApplicationVersion().ToString(3);
            ApplicationTitle = ApplicationConstants.ApplicationName;
            PrinterButtonVisibility = Visibility.Visible;
            CancelPrinterButtonVisibility = Visibility.Hidden;
            SelectedFolder = string.Empty;
            SelectedPrinter = printerNames.First();
            SelectedPrinterAction = printerActions.First();
            SelectedImage = new FileImageInfo();
            AvailableImages = new ObservableCollection<FileImageInfo>();
            PrinterNames = new ObservableCollection<PrinterName>(printerNames);
            PrinterActions = new ObservableCollection<PrinterAction>(printerActions);

            ApplicationShutdownCommandAsync = new RelayAsyncCommand(ApplicationShutdownAsync, CanExecuteCommand);
            PrintPressCommand = new RelayAsyncCommand(PrintPressAsync, CanExecuteCommand);
            GetImagesFromFolderCommand = new RelayCommand(SelectFolder, CanExecuteCommand);
            SaveSettingsCommand = new RelayCommand(SaveSettings, CanExecuteCommand);
            CancelPrintPressCommand = new RelayCommand(CancelPrintPress);
            
            PropertyChanged += FunPressViewModel_PropertyChanged;
            
            ApplyUserSettings(printerNames, printerActions);
            
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

        private void SelectFolder(object param = null)
        {
            try
            {
                _isActionAvailable = false;

                _logger.LogInformation("Invoke in {Method}. Start invoking", 
                    nameof(SelectFolder));

                using (var folderBrowser = new FolderBrowserDialog())
                {
                    var result = folderBrowser.ShowDialog();

                    if (result != DialogResult.OK || string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                    {
                        return;
                    }

                    SelectedFolder = folderBrowser.SelectedPath;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(SelectFolder));
            }
            finally
            {
                _isActionAvailable = true;

                _logger.LogInformation("Invoke in {Method}. Finish invoking", 
                    nameof(SelectFolder));
            }
        }
        
        private void SaveSettings(object param = null)
        {
            try
            {
                _isActionAvailable = false;

                _logger.LogInformation("Invoke in {Method}. Start invoking", 
                    nameof(SaveSettings));

                var userSettings = _userSettingsService.GetUserSettings();
                userSettings.FolderPath = SelectedFolder;
                userSettings.PrinterName = SelectedPrinter.Name;
                userSettings.PrinterActionType = SelectedPrinterAction.Type;
                _userSettingsService.SaveSettings(userSettings);

                TextResult = "Settings saved";

                _resultTextDispatcherTimer.Start();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(SaveSettings));
            }
            finally
            {
                _isActionAvailable = true;

                _logger.LogInformation("Invoke in {Method}. Finish invoking", 
                    nameof(SaveSettings));
            }
        }

        private async Task PrintPressAsync(object param = null)
        {
            try
            {
                _isActionAvailable = false;

                CancelPrinterButtonVisibility = Visibility.Visible;

                _logger.LogInformation("Invoke in {Method}. Start invoking", 
                    nameof(PrintPressAsync));

                TextResult = string.Empty;

                if (string.IsNullOrWhiteSpace(SelectedImage.ImagePath))
                {
                    await GenerateErrorNotificationAsync("Image is not selected. Please, select the image.");

                    return;
                }

                _logger.LogInformation("Invoke in {Method}. Selected Printer: {SelectedPrinter}", 
                    nameof(PrintPressAsync), SelectedPrinter.Name);

                _logger.LogInformation("Invoke in {Method}. Selected image: {SelectedImage}", 
                    nameof(PrintPressAsync), SelectedImage.ImagePath);

                if (!_fileService.IsFileAvailable(SelectedImage.ImagePath))
                {
                    _logger.LogInformation("Invoke in {Method}. Image is not fully available", nameof(PrintPressAsync));

                    await _delayService.WaitForConditionAsync(
                        () => !_fileService.IsFileAvailable(SelectedImage.ImagePath),
                        TimeSpan.FromSeconds(1),
                        _cancelPrintingCancellationTokenSource.Token);

                    if (_cancelPrintingCancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                }

                var newImageFileName = Path.Combine(_applicationEnvironment.GetResultsPath(), 
                    $"{_dateTimeService.GetDateTimeNow():dd_MM_yyyy__HH_mm_ss_fff}.jpg");

                var imageGenerationResult = await _imageService.GenerateImageByTemplateOneAsync(SelectedImage.ImagePath, newImageFileName, 
                    _cancelPrintingCancellationTokenSource.Token);

                if (_cancelPrintingCancellationTokenSource.IsCancellationRequested)
                {
                    _logger.LogDebug("Invoke in {Method}. Cancellation token ", nameof(PrintPressAsync));

                    return;
                }

                if (!imageGenerationResult)
                {
                    await GenerateErrorNotificationAsync("Cannot generate the image. Please check logs.");

                    return;
                }

                if (SelectedPrinter == null || SelectedPrinter.Type == PrinterType.None)
                {
                    _logger.LogInformation("Invoke in {Method}. Selected printer type is {Type}", 
                        nameof(PrintPressAsync), SelectedPrinter?.Type);

                    TextResult = "Press is generated, but won't be printed because printer is not selected.";

                    return;
                }

                if (_cancelPrintingCancellationTokenSource.IsCancellationRequested)
                {
                    _logger.LogInformation("Invoke in {Method}. Cancellation is requested", 
                        nameof(PrintPressAsync));

                    return;
                }

                TextResult = "Press is generated!";

                var printingResult = _printerService.PrintImage(SelectedPrinter.Name, newImageFileName);
                if (!printingResult)
                {
                    await GenerateErrorNotificationAsync("Cannot print image. Please check logs.");

                    return;
                }

                TextResult = "Printing started!";

                _logger.LogInformation("Invoke in {Method}. Printing is finished", nameof(PrintPressAsync));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(PrintPressAsync));
            }
            finally
            {
                CancelPrinterButtonVisibility = Visibility.Hidden;

                _isActionAvailable = true;

                _logger.LogInformation("Invoke in {Method}. Finish invoking", 
                    nameof(PrintPressAsync));
            }
        }

        private void CancelPrintPress(object param = null)
        {
            try
            {
                _logger.LogInformation("Invoke in {Method}. Start invoking",
                    nameof(CancelPrintPress));

                _cancelPrintingCancellationTokenSource.Cancel();
                _cancelPrintingCancellationTokenSource = new CancellationTokenSource();

                _printerService.CancelCurrentPrinting();

                TextResult = "Printing cancelled";
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(CancelPrintPress));
            }
            finally
            {
                _logger.LogInformation("Invoke in {Method}. Finish invoking", 
                    nameof(CancelPrintPress));
            }
        }

        #endregion

        #region Events

        private void ResultTextDispatcherTimer_Tick(object sender, EventArgs args)
        {
            try
            {
                TextResult = string.Empty;
                _resultTextDispatcherTimer.Stop();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", 
                    nameof(ResultTextDispatcherTimer_Tick));
            }
        }

        private void FunPressViewModel_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(SelectedFolder):
                {
                    _logger.LogInformation("Selected folder: {SelectedImage}", SelectedFolder);
                    
                    SetImages(SelectedFolder);

                    StartTrackImagesInFolderJob(SelectedFolder);
                    
                    break;
                }
                case nameof(SelectedImage):
                {
                    _logger.LogInformation("Selected image: {SelectedImage}", SelectedImage.ImagePath);
                    
                    break;
                }
                case nameof(SelectedPrinter):
                {
                    _logger.LogInformation("Selected printer: {SelectedPrinter}", SelectedPrinter.Name);
                    
                    break;
                }
                case nameof(SelectedPrinterAction):
                {
                    _logger.LogInformation("Selected printer action: {SelectedPrinterActionType}", SelectedPrinterAction.Type);

                    if (SelectedPrinterAction.Type == PrinterActionType.Automatic)
                    {
                        _isNeedPrintingAfterImageIsAppear = true;

                        PrinterButtonVisibility = Visibility.Hidden;
                    }
                    else
                    {
                        _isNeedPrintingAfterImageIsAppear = false;

                        PrinterButtonVisibility = Visibility.Visible;
                    }
                    
                    break;
                }
            }
        }

        #endregion

        #region Private methods

        private void SetImages(string folderPath)
        {
            try
            {
                AvailableImages.Clear();
                
                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    _logger.LogWarning("Invoke in {Method}. Folder path is empty", 
                        nameof(SetImages));
                    
                    return;
                }
                
                var images = GetImages(folderPath);

                _logger.LogInformation("Invoke in {Method}. Images in folder: {Images}", 
                    nameof(SetImages), _serializeService.SerializeObject(images));
                
                foreach (var imagePath in images.OrderBy(File.GetLastWriteTime))
                {
                    AvailableImages.Add(new FileImageInfo
                    {
                        ImageName = Path.GetFileName(imagePath),
                        ImagePath = imagePath
                    });
                }

                if (!AvailableImages.Any())
                {
                    return;
                }

                SelectedImage = AvailableImages.First();

                _logger.LogInformation("Invoke in {Method}. Selected image: {Images}", 
                    nameof(SetImages), _serializeService.SerializeObject(SelectedImage));
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                _logger.LogDebug(unauthorizedAccessException, "Invoke in {Method}",
                    nameof(SetImages));
            }
        }

        private void StartTrackImagesInFolderJob(string pathToFolder)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pathToFolder) || Directory.Exists(pathToFolder))
                {
                    _logger.LogWarning("Invoke in {Method}. Job will not start because of invalid path to folder: {Path}", 
                        nameof(StartTrackImagesInFolderJob), pathToFolder);
                    
                    return;
                }
                
                if (_jobService.IsJobExist(TrackFilesJobName))
                {
                    _jobService.FinishJob(TrackFilesJobName);
                }

                _jobService.CreateJob(
                    TrackFilesJobName, 
                    TimeSpan.FromSeconds(2), 
                    async cancellationToken => await CheckNewImages(pathToFolder, cancellationToken)
                    );
                
                _jobService.StartJob(TrackFilesJobName, true);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(StartTrackImagesInFolderJob));
            }
        }
        
        private async Task CheckNewImages(string pathToFolder, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || string.IsNullOrWhiteSpace(pathToFolder))
            {
                return;
            }

            var imagesPath = GetImages(pathToFolder).ToArray();

            await _applicationService.DispatcherInvokeAsync(async () =>
            {
                if (imagesPath.Length > AvailableImages.Count)
                {
                    foreach (var imagePath in imagesPath)
                    {
                        if (AvailableImages.Any(x => x.ImageName == Path.GetFileName(imagePath)))
                        {
                            continue;
                        }

                        _logger.LogInformation("Invoke in {Method}. New image is appeared. Image path: {ImagePath}", 
                            nameof(CheckNewImages), imagePath);

                        AvailableImages.Add(new FileImageInfo
                        {
                            ImageName = Path.GetFileName(imagePath),
                            ImagePath = imagePath
                        });
                    }

                    if (_isNeedPrintingAfterImageIsAppear)
                    {
                        SelectedImage = AvailableImages.Last();

                        _logger.LogInformation("Invoke in {Method}. New image ig going to be printed appeared. Image path: {ImagePath}", 
                            nameof(CheckNewImages), SelectedImage.ImagePath);

                        await PrintPressAsync();
                    }
                }
                else if (imagesPath.Length < AvailableImages.Count)
                {
                    foreach (var imagePath in AvailableImages.Where(x => imagesPath.All(path => path != x.ImagePath)))
                    {
                        AvailableImages.Remove(imagePath);

                        _logger.LogInformation("Invoke in {Method}. Image is deleted. Image path: {ImagePath}", 
                            nameof(CheckNewImages), imagePath.ImagePath);
                    }
                }

            }, DispatcherPriority.Send, cancellationToken);
        }

        private void ApplyUserSettings(IEnumerable<PrinterName> printerNames, IEnumerable<PrinterAction> printerActions)
        {
            try
            {
                var useSettings = _userSettingsService.GetUserSettings();

                if (useSettings == null)
                {
                    useSettings = new UserSettings
                    {
                        FolderPath = SelectedFolder,
                        PrinterName = SelectedPrinter.Name,
                        PrinterActionType = SelectedPrinterAction.Type
                    };

                    _userSettingsService.SaveSettings(useSettings);
                }
                else
                {
                    var selectedPrinter = printerNames.FirstOrDefault(x => x.Name == useSettings.PrinterName);
                    var selectedPrinterAction = printerActions.FirstOrDefault(x => x.Type == useSettings.PrinterActionType);

                    if (selectedPrinter?.Name != SelectedPrinter.Name)
                    {
                        SelectedPrinter = selectedPrinter;
                    }

                    if (selectedPrinterAction?.Type != SelectedPrinterAction.Type)
                    {
                        SelectedPrinterAction = selectedPrinterAction;
                    }

                    if (!string.IsNullOrWhiteSpace(useSettings.FolderPath) && useSettings.FolderPath != SelectedFolder)
                    {
                        SelectedFolder = useSettings.FolderPath;   
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(ApplyUserSettings));
            }
        }

        private IEnumerable<string> GetImages(string directoryToCheck)
        {
            return Directory.GetFiles(directoryToCheck, "*.*", SearchOption.AllDirectories)
                .Where(file => _validExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));
        }

        private IEnumerable<PrinterName> GetPrinterNames()
        {
            var printers = new List<PrinterName>
            {
                new PrinterName
                {
                    Name = "Without printer",
                    Type = PrinterType.None
                }
            };

            var printersName = _printerService.GetPrinterNames()
                .Select(name => new PrinterName
                {
                    Name = name,
                    Type = PrinterType.Active
                });

            printers.AddRange(printersName);

            return printers;
        }

        private static IEnumerable<PrinterAction> GetPrinterActions()
        {
            return new[]
            {
                new PrinterAction
                {
                    Name = "Manually",
                    Type = PrinterActionType.Manually
                },
                new PrinterAction
                {
                    Name = "Automatic",
                    Type = PrinterActionType.Automatic
                }
            };
        }

        private async Task GenerateErrorNotificationAsync(string errorMessage)
        {
            var viewParameters = new CreateViewParameters
            {
                Parent = _viewModelView,
                AdditionalParameters = new MessageDialogParam(errorMessage, new[] { MessageDialogButton.Ok })
            };

            await _viewFactory.Get<IMessageDialogView>().ShowDialogViewAsync(viewParameters);
        }

        #endregion
    }
}
