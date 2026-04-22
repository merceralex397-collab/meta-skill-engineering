using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MetaSkillStudio.ViewModels
{
    public abstract class MainViewModelSectionBase : INotifyPropertyChanged
    {
        protected MainViewModelSectionBase(MainViewModel coordinator)
        {
            Coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            Coordinator.PropertyChanged += HandleCoordinatorPropertyChanged;
        }

        protected MainViewModel Coordinator { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected abstract void OnCoordinatorPropertyChanged(string? propertyName);

        protected void Forward(string? sourcePropertyName, params string[] propertyNames)
        {
            if (string.IsNullOrEmpty(sourcePropertyName))
            {
                foreach (var propertyName in propertyNames)
                {
                    RaisePropertyChanged(propertyName);
                }

                return;
            }

            foreach (var propertyName in propertyNames)
            {
                if (string.Equals(sourcePropertyName, propertyName, StringComparison.Ordinal))
                {
                    RaisePropertyChanged(propertyName);
                }
            }
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void HandleCoordinatorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnCoordinatorPropertyChanged(e.PropertyName);
        }
    }
}
