using MahApps.Metro.Controls.Dialogs;
using System.Net;
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
    }
}