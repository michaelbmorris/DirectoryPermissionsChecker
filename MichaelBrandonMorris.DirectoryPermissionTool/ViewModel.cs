﻿using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Extensions.PrimitiveExtensions;
using GalaSoft.MvvmLight.CommandWpf;
using MichaelBrandonMorris.DynamicText;

namespace MichaelBrandonMorris.DirectoryPermissionTool
{
    internal class ViewModel : INotifyPropertyChanged
    {
        private AboutWindow _aboutWindow;
        private Visibility _cancelButonVisibility;
        private bool _combinedPathLevelsIsChecked;
        private PermissionChecker _directoryPermissionsChecker;
        private bool _includeFilesIsChecked;
        private bool _isBusy;
        private string _message;
        private bool _messageIsVisible;
        private int _messageZIndex;
        private Visibility _progressBarVisibility;
        private bool _searchDepthAllIsChecked;
        private bool _searchDepthChildrenIsChecked;
        private bool _searchDepthCurrentIsChecked;

        private bool _splitPathLevelsIsChecked;

        internal ViewModel()
        {
            SetUpProperties();
        }

        public ICommand AddExcludedGroup => new RelayCommand(
            ExecuteAddExcludedGroup, CanAddExcludedGroup);

        public ICommand AddExcludedPath => new RelayCommand(
            ExecuteAddExcludedPath, CanAddExcludedPath);

        public ICommand AddSearchPath => new RelayCommand(
            ExecuteAddSearchPath, CanAddSearchPath);

        public ICommand Cancel => new RelayCommand(ExecuteCancel, CanCancel);

        public Visibility CancelButtonVisibility
        {
            get
            {
                return _cancelButonVisibility;
            }
            set
            {
                if (_cancelButonVisibility == value)
                {
                    return;
                }

                _cancelButonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public bool CombinedPathLevelsIsChecked
        {
            get
            {
                return _combinedPathLevelsIsChecked;
            }
            set
            {
                if (_combinedPathLevelsIsChecked == value)
                {
                    return;
                }
                _combinedPathLevelsIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public DynamicTextCollection ExcludedGroups
        {
            get;
        } = new DynamicTextCollection();

        public DynamicDirectoryPathCollection ExcludedPaths
        {
            get;
        } = new DynamicDirectoryPathCollection();

        public ICommand GetDirectoryPermissions => new RelayCommand(
            ExecuteGetDirectoryPermissions,
            CanGetDirectoryPermissions);

        public bool IncludeFilesIsChecked
        {
            get
            {
                return _includeFilesIsChecked;
            }
            set
            {
                if (_includeFilesIsChecked == value)
                {
                    return;
                }

                _includeFilesIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                if (_isBusy == value)
                {
                    return;
                }

                _isBusy = value;
                NotifyPropertyChanged();
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (_message != null &&
                    value != null &&
                    _message.Equals(value))
                {
                    return;
                }

                _message = value;
                NotifyPropertyChanged();
            }
        }

        public bool MessageIsVisible
        {
            get
            {
                return _messageIsVisible;
            }
            set
            {
                if (_messageIsVisible == value)
                {
                    return;
                }

                _messageIsVisible = value;
                NotifyPropertyChanged();
            }
        }

        public int MessageZIndex
        {
            get
            {
                return _messageZIndex;
            }
            set
            {
                if (_messageZIndex == value)
                {
                    return;
                }

                _messageZIndex = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand OpenAboutWindowCommand => new RelayCommand(
            OpenAboutWindowCommandExecute);

        public ICommand OpenHelpWindowCommand => new RelayCommand(
            OpenHelpWindowCommandExecute);

        public Visibility ProgressBarVisibility
        {
            get
            {
                return _progressBarVisibility;
            }
            set
            {
                if (_progressBarVisibility == value)
                {
                    return;
                }

                _progressBarVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand RemoveExcludedGroup => new RelayCommand<DynamicText.DynamicText>(
            ExecuteRemoveExcludedGroup, CanRemoveExcludedGroup);

        public ICommand RemoveExcludedPath =>
            new RelayCommand<DynamicDirectoryPath>(
                ExecuteRemoveExcludedPath, CanRemoveExcludedPath);

        public ICommand RemoveSearchPath =>
            new RelayCommand<DynamicDirectoryPath>(
                ExecuteRemoveSearchPath, CanRemoveSearchPath);

        public bool SearchDepthAllIsChecked
        {
            get
            {
                return _searchDepthAllIsChecked;
            }
            set
            {
                if (_searchDepthAllIsChecked == value)
                {
                    return;
                }

                _searchDepthAllIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool SearchDepthChildrenIsChecked
        {
            get
            {
                return _searchDepthChildrenIsChecked;
            }
            set
            {
                if (_searchDepthChildrenIsChecked == value)
                {
                    return;
                }

                _searchDepthChildrenIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool SearchDepthCurrentIsChecked
        {
            get
            {
                return _searchDepthCurrentIsChecked;
            }
            set
            {
                if (_searchDepthCurrentIsChecked == value)
                {
                    return;
                }

                _searchDepthCurrentIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public DynamicDirectoryPathCollection SearchPaths
        {
            get;
        } = new DynamicDirectoryPathCollection();

        public bool SplitPathLevelsIsChecked
        {
            get
            {
                return _splitPathLevelsIsChecked;
            }
            set
            {
                if (_splitPathLevelsIsChecked == value)
                {
                    return;
                }

                _splitPathLevelsIsChecked = value;
                NotifyPropertyChanged();
            }
        }

        public string Version
        {
            get;
            private set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool CanAddExcludedGroup()
        {
            return ExcludedGroups.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        private bool CanAddExcludedPath()
        {
            return ExcludedPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        private bool CanAddSearchPath()
        {
            return SearchPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        private bool CanCancel()
        {
            return _directoryPermissionsChecker != null;
        }

        private bool CanGetDirectoryPermissions()
        {
            return SearchPaths.Any(x => !x.Text.IsNullOrWhiteSpace()) &&
                   !_isBusy &&
                   GetSearchDepth() != SearchDepth.None
                   && PathDisplayOptionIsChecked();
        }

        private bool PathDisplayOptionIsChecked()
        {
            return CombinedPathLevelsIsChecked || SplitPathLevelsIsChecked;
        }

        private bool CanRemoveExcludedGroup(DynamicText.DynamicText excludedGroup = null)
        {
            return ExcludedGroups.Count > 1 ||
                   ExcludedGroups.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        private bool CanRemoveExcludedPath(
            DynamicDirectoryPath excludedPath = null)
        {
            return ExcludedPaths.Count > 1 ||
                   ExcludedPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        private bool CanRemoveSearchPath(
            DynamicDirectoryPath searchPath = null)
        {
            return SearchPaths.Count > 1 ||
                   SearchPaths.All(x => !x.Text.IsNullOrWhiteSpace());
        }

        private void ExecuteAddExcludedGroup()
        {
            ExcludedGroups.Add(new DynamicText.DynamicText());
        }

        private void ExecuteAddExcludedPath()
        {
            ExcludedPaths.Add(new DynamicDirectoryPath());
        }

        private void ExecuteAddSearchPath()
        {
            SearchPaths.Add(new DynamicDirectoryPath());
        }

        private void ExecuteCancel()
        {
            _directoryPermissionsChecker.Cancel();
        }

        private async void ExecuteGetDirectoryPermissions()
        {
            IsBusy = true;
            CancelButtonVisibility = Visibility.Visible;
            ProgressBarVisibility = Visibility.Visible;
            HideMessage();

            try
            {
                _directoryPermissionsChecker = new PermissionChecker(
                    from x in SearchPaths
                    where !x.Text.IsNullOrWhiteSpace()
                    select x.Text,
                    from x in ExcludedPaths
                    where !x.Text.IsNullOrWhiteSpace()
                    select x.Text,
                    GetSearchDepth(),
                    IncludeFilesIsChecked,
                    SplitPathLevelsIsChecked,
                    from x in ExcludedGroups
                    where !x.Text.IsNullOrWhiteSpace()
                    select x.Text);

                await _directoryPermissionsChecker.Execute();
                _directoryPermissionsChecker = null;
            }
            catch (DirectoryNotFoundException e)
            {
                ShowMessage(e.Message);
            }
            catch (OperationCanceledException e)
            {
                ShowMessage(e.Message);
            }

            IsBusy = false;
            CancelButtonVisibility = Visibility.Hidden;
            ProgressBarVisibility = Visibility.Hidden;
        }

        private void ExecuteRemoveExcludedGroup(DynamicText.DynamicText excludedGroup)
        {
            if (ExcludedGroups.Count == 1)
            {
                excludedGroup.Text = string.Empty;
            }
            else
            {
                ExcludedGroups.Remove(excludedGroup);
            }
        }

        private void ExecuteRemoveExcludedPath(
            DynamicDirectoryPath excludedPath)
        {
            if (ExcludedPaths.Count == 1)
            {
                excludedPath.Text = string.Empty;
            }
            else
            {
                ExcludedPaths.Remove(excludedPath);
            }
        }

        private void ExecuteRemoveSearchPath(DynamicDirectoryPath searchPath)
        {
            if (SearchPaths.Count == 1)
            {
                searchPath.Text = string.Empty;
            }
            else
            {
                SearchPaths.Remove(searchPath);
            }
        }

        private SearchDepth GetSearchDepth()
        {
            if (SearchDepthAllIsChecked)
            {
                return SearchDepth.All;
            }

            if (SearchDepthChildrenIsChecked)
            {
                return SearchDepth.Children;
            }

            return SearchDepthCurrentIsChecked
                ? SearchDepth.Current
                : SearchDepth.None;
        }

        private void HideMessage()
        {
            Message = string.Empty;
            MessageIsVisible = false;
            MessageZIndex = -1;
        }

        private void NotifyPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        private void OpenAboutWindowCommandExecute()
        {
            if (_aboutWindow == null || !_aboutWindow.IsVisible)
            {
                _aboutWindow = new AboutWindow();
                _aboutWindow.Show();
            }
            else
            {
                _aboutWindow.Activate();
            }
        }

        private void OpenHelpWindowCommandExecute()
        {
            try
            {
                Process.Start("help.chm");
            }
            catch (Exception e)
            {
                ShowMessage($"Could not load help.");
            }
        }

        private void SetUpProperties()
        {
            IsBusy = false;
            MessageZIndex = -1;

            try
            {
                Version =
                    ApplicationDeployment.CurrentDeployment.CurrentVersion
                        .ToString();
            }
            catch (InvalidDeploymentException)
            {
                Version = "(Development Build)";
            }
        }

        private void ShowMessage(string message)
        {
            Message = message + "\n\nDouble-click to dismiss.";
            MessageIsVisible = true;
            MessageZIndex = 1;
        }
    }
}