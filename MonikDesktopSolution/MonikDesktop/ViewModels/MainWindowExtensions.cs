using MahApps.Metro.Controls.Dialogs;
using MonikDesktop.Views;
using System.Net;
using System.Threading.Tasks;
using Ui.Wpf.Common;

namespace MonikDesktop.ViewModels
{
    public static class MainWindowExtensions
    {
        public static void ShowWebExceptionMessage(this IDockWindow window, WebException e)
        {
            ((MainWindow) window).ShowMessageAsync("Unsuccessful request", WebExceptionToString(e));
        }

        private static string WebExceptionToString(WebException e)
        {
            if (e.Response == null)
                return e.Message;

            switch (((HttpWebResponse) e.Response).StatusCode)
            {
                case HttpStatusCode.NotFound: return "Cannot find requested item";
                case HttpStatusCode.Unauthorized: return "You are not authorized";
                case HttpStatusCode.InternalServerError: return "Internal Server Error";
                default:
                    return e.ToString();
            }
        }


        public static async Task<GroupCreateDialogResult> ShowGroupCreateDialog(this IDockWindow window)
        {
            var view = (MainWindow) window;

            var dialog = new GroupCreateDialog();

            await view.ShowMetroDialogAsync(dialog);

            var result = await dialog.WaitForButtonPressAsync();
                
            await view.HideMetroDialogAsync(dialog);

            return result;
        }

    }
}