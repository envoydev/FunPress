using FunPress.Common.Types.Enums;
using FunPress.ViewModels.Contracts;
using FunPress.ViewModels.Mvvm;
using FunPress.Views.Mvvm.Parameters;
using FunPress.Views.Mvvm;
using FunPress.Views.Params;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using FunPress.Core.Services;

namespace FunPress.ViewModels.Implementations
{
    public class MessageDialogViewModel : ObservableObject, IMessageDialogViewModel
    {
        private readonly ILogger<MessageDialogViewModel> _logger;
        private readonly ISerializeService _serializeService;

        private IManageView _viewModelView;
        private bool _isActionAvailable = true;

        #region Observable properties

        private string _text;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _okButtonVisibility;

        public Visibility OkButtonVisibility
        {
            get => _okButtonVisibility;
            set
            {
                _okButtonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _yesButtonVisibility;

        public Visibility YesButtonVisibility
        {
            get => _yesButtonVisibility;
            set
            {
                _yesButtonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _noButtonVisibility;

        public Visibility NoButtonVisibility
        {
            get => _noButtonVisibility;
            set
            {
                _noButtonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _cancelButtonVisibility;

        public Visibility CancelButtonVisibility
        {
            get => _cancelButtonVisibility;
            set
            {
                _cancelButtonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand PositiveResultCommandAsync { get; private set; }
        public ICommand NegativeResultCommandAsync { get; private set; }

        #endregion

        public MessageDialogViewModel(
            ILogger<MessageDialogViewModel> logger,
            ISerializeService serializeService
            )
        {
            _logger = logger;
            _serializeService = serializeService;
        }

        public void AssignView(IManageView manageView)
        {
            _viewModelView = manageView;

            _logger.LogInformation("View assigned");
        }

        public Task InitializeDataAsync(object param = null)
        {
            PositiveResultCommandAsync = new RelayAsyncCommand(PositiveResultAsync, CanExecuteCommand);
            NegativeResultCommandAsync = new RelayAsyncCommand(NegativeResultAsync, CanExecuteCommand);
            OkButtonVisibility = Visibility.Collapsed;
            YesButtonVisibility = Visibility.Collapsed;
            NoButtonVisibility = Visibility.Collapsed;
            CancelButtonVisibility = Visibility.Collapsed;

            var messageModel = (MessageDialogParam)param ?? throw new ArgumentNullException(nameof(param));

            Text = messageModel.Message;

            foreach (var button in messageModel.Buttons)
            {
                switch (button)
                {
                    case MessageDialogButton.Yes:
                        YesButtonVisibility = Visibility.Visible;
                        break;
                    case MessageDialogButton.Ok:
                        OkButtonVisibility = Visibility.Visible;
                        break;
                    case MessageDialogButton.No:
                        NoButtonVisibility = Visibility.Visible;
                        break;
                    case MessageDialogButton.Cancel:
                        CancelButtonVisibility = Visibility.Visible;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _logger.LogInformation("Model data initialized. Data: {Data}",
                _serializeService.SerializeObject(messageModel));

            return Task.CompletedTask;
        }

        public void ClearData()
        {
            _logger.LogInformation("Model data cleared");
        }

        #region Command methods

        private bool CanExecuteCommand(object param = null)
        {
            return _isActionAvailable;
        }

        private async Task PositiveResultAsync(object param = null)
        {
            try
            {
                _isActionAvailable = false;

                await _viewModelView.CloseAsync(new CloseViewParameters
                {
                    CloseViewResult = true
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(PositiveResultAsync));
            }
            finally
            {
                _isActionAvailable = true;
            }
        }

        private async Task NegativeResultAsync(object param = null)
        {
            try
            {
                _isActionAvailable = false;

                var closeViewParameters = new CloseViewParameters
                {
                    CloseViewResult = false
                };

                await _viewModelView.CloseAsync(closeViewParameters);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(NegativeResultAsync));
            }
            finally
            {
                _isActionAvailable = true;
            }
        }

        #endregion
    }
}
