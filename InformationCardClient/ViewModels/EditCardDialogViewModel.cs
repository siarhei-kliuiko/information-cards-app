using InformationCardClient.Helpers;
using InformationCardClient.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace InformationCardClient.ViewModels
{
    internal class EditCardDialogViewModel : ViewModelBase, IDataErrorInfo
    {
        private string name;
        private byte[] imageData;
        private bool isDataValidationDisabled = true;
        private readonly int id;
        public EditCardDialogViewModel(InformationCard card = null)
        {
            if (card != null)
            {
                this.id = card.Id;
                this.name = card.Name;
                this.imageData = card.ImageData;
                this.IsInEditMode = true;
            }

            this.DialogOKCommand = new RelayCommand(this.DialogOK, p => !string.IsNullOrWhiteSpace(this.Name) && this.ImageData != null);
            this.SelectImageCommand = new RelayCommand(this.SelectImage);
            this.DialogCancelCommand = new RelayCommand(this.DialogCancel);
        }

        public ICommand SelectImageCommand { get; }

        public ICommand DialogOKCommand { get; }

        public ICommand DialogCancelCommand { get; }

        public bool IsInEditMode { get; }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.isDataValidationDisabled = false;
                this.name = value;
                this.OnPropertyChanged(nameof(this.Name));
            }
        }

        public byte[] ImageData
        {
            get
            {
                return this.imageData;
            }
            set
            {
                this.imageData = value;
                this.OnPropertyChanged(nameof(this.ImageData));
            }
        }

        public DialogResult DialogResult { get; private set; }

        public string Error => null;

        public string this[string propertyName]
        {
            get
            {
                if (this.isDataValidationDisabled)
                {
                    return string.Empty;
                }

                return this.GetValidationError(propertyName);
            }
        }

        private static bool TryParseImageData(string filePath, out byte[] fileBytes)
        {
            fileBytes = null;
            byte[] imageTypeHeader = null;
            switch (Path.GetExtension(filePath))
            {
                case ".jpg":
                    imageTypeHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
                    break;
                case ".png":
                    imageTypeHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
                    break;
            }

            if (imageTypeHeader != null)
            {
                fileBytes = File.ReadAllBytes(filePath);
                if (fileBytes != null && fileBytes.Length > imageTypeHeader.Length)
                {
                    for (int i = 0; i < imageTypeHeader.Length; i++)
                    {
                        if (fileBytes[i] != imageTypeHeader[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private static BitmapImage ShrinkImage(BitmapImage image, MemoryStream stream)
        {
            var shrinkedImage = new BitmapImage();
            shrinkedImage.BeginInit();
            shrinkedImage.CacheOption = BitmapCacheOption.OnLoad;
            shrinkedImage.StreamSource = stream;
            if (image.PixelHeight > 150)
            {
                shrinkedImage.DecodePixelHeight = 150;
            }

            if (image.PixelWidth > 150)
            {
                shrinkedImage.DecodePixelWidth = 150;
            }

            shrinkedImage.EndInit();
            return shrinkedImage;
        }

        private string GetValidationError(string propertyName)
        {
            string validationResult = string.Empty;
            if (propertyName == nameof(this.Name) && string.IsNullOrWhiteSpace(Name))
            {
                validationResult = $"{nameof(this.Name)} must be not empty";
            }

            return validationResult;
        }

        private void SelectImage(object parameter)
        {
            if (DialogService.ShowSelectImageDialog(out string filePath) == DialogResult.Yes)
            {
                if (TryParseImageData(filePath, out byte[] fileBytes))
                {
                    var image = new BitmapImage();
                    using (var stream = new MemoryStream(fileBytes))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream;
                        image.EndInit();
                        if (image.PixelHeight > 150 || image.PixelWidth > 150)
                        {
                            stream.Position = 0;
                            image = ShrinkImage(image, stream);
                        }
                    }

                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    using (var stream = new MemoryStream())
                    {
                        encoder.Save(stream);
                        ImageData = stream.ToArray();
                    }
                }
                else
                {
                    DialogService.ShowMessage("Selected file is not valid JPG or PNG image.", MessageType.Error);
                }
            }
        }

        private void DialogCancel(object parameter)
        {
            this.CloseDialogWithResult(parameter as Window, DialogResult.No);
        }

        private async void DialogOK(object parameter)
        {
            var cardResult = new InformationCard { Id = this.id, ImageData = this.ImageData, Name = this.Name };
            var errorMessage = this.IsInEditMode ? await HttpService.UpdateCardAsync(cardResult) : await HttpService.AddCardAsync(cardResult);
            if (errorMessage == null)
            {
                this.CloseDialogWithResult(parameter as Window, DialogResult.Yes);
            }
            else
            {
                DialogService.ShowMessage((this.IsInEditMode ? "Failed to update card" : "Failed to add new card") + Environment.NewLine + errorMessage, MessageType.Error);
                this.CloseDialogWithResult(parameter as Window, DialogResult.No);
            }
        }

        public void CloseDialogWithResult(Window dialog, DialogResult result)
        {
            this.DialogResult = result;
            if (dialog != null)
            {
                dialog.DialogResult = true;
            }
        }
    }
}
