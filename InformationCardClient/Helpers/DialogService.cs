using InformationCardClient.ViewModels;
using InformationCardClient.Views;
using Microsoft.Win32;
using System.Windows;

namespace InformationCardClient.Helpers
{
    internal enum DialogResult
    {
        None,
        Yes,
        No,
    }

    internal enum MessageType
    {
        Error,
        Warning,
        Question,
    }

    internal static class DialogService
    {
        public static DialogResult ShowEditCardDialog(EditCardDialogViewModel dialogViewModel)
        {
            var dialog = new EditCardDialog { DataContext = dialogViewModel };
            dialog.ShowDialog();
            DialogResult result = (dialog.DataContext as EditCardDialogViewModel).DialogResult;
            return result;
        }

        public static DialogResult ShowSelectImageDialog(out string filePath)
        {
            filePath = null;
            var dialog = new OpenFileDialog { Filter = "Image files | *.png; *.jpg" };
            if (dialog.ShowDialog() == true)
            {
                filePath = dialog.FileName;
                return DialogResult.Yes;
            }
            else
            {
                return DialogResult.No;
            }
        }

        public static DialogResult ShowMessage(string errorText, MessageType type)
        {
            var buttons = MessageBoxButton.OK;
            var image = MessageBoxImage.Error;
            switch (type)
            {
                case MessageType.Error:
                    image = MessageBoxImage.Error;
                    break;
                case MessageType.Warning:
                    image = MessageBoxImage.Warning;
                    break;
                case MessageType.Question:
                    image = MessageBoxImage.Question;
                    buttons = MessageBoxButton.OKCancel;
                    break;
                default:
                    image = MessageBoxImage.None;
                    break;
            }

            if (MessageBox.Show(errorText, type.ToString(), buttons, image) == MessageBoxResult.OK)
            {
                return DialogResult.Yes;
            }
            else
            {
                return DialogResult.No;
            }
        }
    }
}
