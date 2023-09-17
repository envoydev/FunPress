using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using FunPress.ViewModels.Contracts;
using FunPress.Views.DialogViews;
using FunPress.Views.Mvvm.Parameters;
using Microsoft.Extensions.Logging;

namespace FunPress.Startup.Views.Common
{
    /// <summary>
    /// Interaction logic for MessageDialogView.xaml
    /// </summary>
    public partial class MessageDialogView : IMessageDialogView
    {
        private readonly ILogger<MessageDialogView> _logger;
        private readonly IMessageDialogViewModel _viewModel;

        public bool IsClosed { get; private set; } = true;
        public object DialogViewResult { get; private set; }

        public MessageDialogView(
            ILogger<MessageDialogView> logger,
            IMessageDialogViewModel viewModel
            )
        {
            _logger = logger;
            _viewModel = viewModel;
        }

        public async Task<bool> ShowDialogViewAsync(CreateViewParameters param = null)
        {
            _viewModel.AssignView(this);
            await _viewModel.InitializeDataAsync(param?.AdditionalParameters);
            DataContext = _viewModel;

            Owner = (Window)param?.Parent;
            if (Owner != null)
            {
                Owner.Activated += Owner_Activated;
            }

            InitializeComponent();

            IsClosed = false;

            _logger.LogInformation("View dialog showed");

            return ShowDialog() ?? false;
        }

        public Task CloseAsync(CloseViewParameters param = null)
        {
            _logger.LogInformation("Invoke in {Method}. Dialog result is: {Result}", 
                nameof(CloseAsync), param?.CloseViewResult);

            DialogResult = param?.CloseViewResult;
            DialogViewResult = param?.ObjectResult;

            Close();

            return Task.CompletedTask;
        }

        protected override void OnClosing(CancelEventArgs args)
        {
            if (Owner != null)
            {
                Owner.Activated -= Owner_Activated;
            }

            _viewModel.ClearData();

            base.OnClosing(args);

            IsClosed = true;

            _logger.LogInformation("View dialog closed");
        }

        #region Events

        private void Owner_Activated(object sender, EventArgs args)
        {
            Activate();
        }

        #endregion
    }
}
