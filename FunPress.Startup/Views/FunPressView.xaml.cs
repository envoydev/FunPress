﻿using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FunPress.ViewModels.Contracts;
using FunPress.Views.Mvvm.Parameters;
using FunPress.Views.Views;
using Microsoft.Extensions.Logging;

namespace FunPress.Startup.Views
{
    public partial class FunPressView : IFunPressView
    {
        private readonly ILogger<FunPressView> _logger;
        private readonly IFunPressViewModel _viewModel;

        public FunPressView(
            ILogger<FunPressView> logger, 
            IFunPressViewModel viewModel
            )
        {
            _logger = logger;
            _viewModel = viewModel;
        }

        public async Task ShowViewAsync(CreateViewParameters param = null)
        {
            _viewModel.AssignView(this);
            await _viewModel.InitializeDataAsync();
            DataContext = _viewModel;

            InitializeComponent();

            Show();

            _logger.LogInformation("View showed");
        }

        public Task CloseAsync(CloseViewParameters param = null)
        {
            _logger.LogInformation("Invoke in {Method}", nameof(CloseAsync));

            Close();

            return Task.CompletedTask;
        }

        protected override void OnClosing(CancelEventArgs args)
        {
            _viewModel.ClearData();

            base.OnClosing(args);

            _logger.LogInformation("View closed");
        }

        #region Events

        private void OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        #endregion
    }
}
