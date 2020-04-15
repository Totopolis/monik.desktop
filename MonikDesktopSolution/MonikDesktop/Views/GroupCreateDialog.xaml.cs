using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using MonikDesktop.ViewModels;

namespace MonikDesktop.Views
{
    public partial class GroupCreateDialog : CustomDialog
    {
        public GroupCreateDialog()
        {
            InitializeComponent();
        }

        internal Task<GroupCreateDialogResult> WaitForButtonPressAsync()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Focus();
                this.PART_GroupNameTextBox.Focus();
            }));

            var tcs = new TaskCompletionSource<GroupCreateDialogResult>();

            RoutedEventHandler negativeHandler = null;
            KeyEventHandler negativeKeyHandler = null;

            RoutedEventHandler affirmativeHandler = null;
            KeyEventHandler affirmativeKeyHandler = null;

            KeyEventHandler escapeKeyHandler = null;

            Action cleanUpHandlers = null;

            var cancellationTokenRegistration = this.DialogSettings.CancellationToken.Register(() =>
            {
                cleanUpHandlers();
                tcs.TrySetResult(null);
            });

            cleanUpHandlers = () =>
            {
                this.KeyDown -= escapeKeyHandler;

                this.PART_NegativeButton.Click -= negativeHandler;
                this.PART_AffirmativeButton.Click -= affirmativeHandler;

                this.PART_NegativeButton.KeyDown -= negativeKeyHandler;
                this.PART_AffirmativeButton.KeyDown -= affirmativeKeyHandler;

                cancellationTokenRegistration.Dispose();
            };

            escapeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(null);
                }
            };

            negativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(null);
                }
            };

            affirmativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(this.Result);
                }
            };

            negativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(null);

                e.Handled = true;
            };

            affirmativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(this.Result);

                e.Handled = true;
            };

            this.PART_NegativeButton.KeyDown += negativeKeyHandler;
            this.PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;

            this.KeyDown += escapeKeyHandler;

            this.PART_NegativeButton.Click += negativeHandler;
            this.PART_AffirmativeButton.Click += affirmativeHandler;

            return tcs.Task;
        }

        private GroupCreateDialogResult Result => new GroupCreateDialogResult
        {
            Name = InputName,
            IsDeafult = InputIsDefault,
            Description = InputDescription
        };

        public static readonly DependencyProperty InputNameProperty = DependencyProperty.Register("InputName", typeof(string), typeof(GroupCreateDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty InputDescriptionProperty = DependencyProperty.Register("InputDescription", typeof(string), typeof(GroupCreateDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty InputIsDefaultProperty = DependencyProperty.Register("InputIsDefault", typeof(bool), typeof(GroupCreateDialog), new PropertyMetadata(default(bool)));

        public string InputName
        {
            get { return (string)this.GetValue(InputNameProperty); }
            set { this.SetValue(InputNameProperty, value); }
        }

        public string InputDescription
        {
            get { return (string)this.GetValue(InputDescriptionProperty); }
            set { this.SetValue(InputDescriptionProperty, value); }
        }

        public bool InputIsDefault
        {
            get { return (bool)this.GetValue(InputIsDefaultProperty); }
            set { this.SetValue(InputIsDefaultProperty, value); }
        }
    }
}