using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MetaSkillStudio.Views
{
    /// <summary>
    /// A dialog window for collecting text input from the user.
    /// Supports data binding via INotifyPropertyChanged.
    /// </summary>
    public partial class InputDialog : Window, INotifyPropertyChanged
    {
        private string _responseText = "";
        private string _title = "";
        private string _message = "";

        /// <summary>
        /// Gets or sets the text response entered by the user.
        /// </summary>
        public string ResponseText
        {
            get => _responseText;
            set => SetProperty(ref _responseText, value);
        }

        /// <summary>
        /// Gets or sets the dialog window title.
        /// </summary>
        public new string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// Gets or sets the message displayed to the user in the dialog.
        /// </summary>
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        /// <summary>
        /// Initializes a new instance of the InputDialog class.
        /// </summary>
        /// <param name="title">The title of the dialog window.</param>
        /// <param name="message">The message to display to the user.</param>
        /// <param name="defaultResponse">The default response text, if any.</param>
        /// <exception cref="ArgumentNullException">Thrown when title or message is null.</exception>
        public InputDialog(string title, string message, string defaultResponse = "")
        {
            InitializeComponent();
            DataContext = this;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            ResponseText = defaultResponse ?? string.Empty;
        }

        /// <summary>
        /// Handles the OK button click event. Sets the dialog result to true and closes the window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The routed event data.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Handles the Cancel button click event. Sets the dialog result to false and closes the window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The routed event data.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #region INotifyPropertyChanged
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sets the property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="field">Reference to the backing field.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">Name of the property (automatically determined).</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        #endregion
    }
}
