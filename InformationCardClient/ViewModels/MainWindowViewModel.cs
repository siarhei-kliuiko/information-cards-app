using InformationCardClient.Helpers;
using InformationCardClient.Models;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InformationCardClient.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<InformationCard> cards = new ObservableCollection<InformationCard>();

        public MainWindowViewModel()
        {
            this.DelCardCommand = new RelayCommand(this.RemoveCardsAsync, p => this.Cards.Count != 0);
            this.EditCardCommand = new RelayCommand(this.OpenEditCardDialog, p => this.Cards.Count != 0);
            this.AddCardCommand = new RelayCommand(this.OpenAddCardDialog);
            this.SortCommand = new RelayCommand(this.Sort, p => this.Cards.Count > 1);
            Task.Factory.StartNew(GetCardsAsync);
        }

        public ICommand DelCardCommand { get; }
        public ICommand EditCardCommand { get; }
        public ICommand AddCardCommand { get; }
        public ICommand SortCommand { get; }

        public ObservableCollection<InformationCard> Cards
        {
            get
            {
                return this.cards;
            }
            set
            {
                this.cards = value;
                OnPropertyChanged(nameof(this.Cards));
            }
        }

        private void OpenEditCardDialog(object parameter)
        {
            if (parameter != null && parameter is IList selectedCards)
            {
                var cards = selectedCards.Cast<InformationCard>().ToArray();
                if (cards.Length == 1)
                {
                    var dialogViewModel = new EditCardDialogViewModel(cards[0]);
                    if (DialogService.ShowEditCardDialog(dialogViewModel) == DialogResult.Yes)
                    {
                        GetCardsAsync();
                    }
                }
                else
                {
                    DialogService.ShowMessage("Selected more than one card", MessageType.Warning);
                }
            }
        }

        private void Sort(object parameter)
        {
            if (parameter != null)
            {
                switch (parameter.ToString())
                {
                    case "asc":
                        this.Cards = new ObservableCollection<InformationCard>(this.Cards.ToArray().OrderBy(card => card.Name, StringComparer.Ordinal));
                        break;
                    case "desc":
                        this.Cards = new ObservableCollection<InformationCard>(this.Cards.ToArray().OrderByDescending(card => card.Name, StringComparer.Ordinal));
                        break;
                }
            }
        }

        private void OpenAddCardDialog(object parameter)
        {
            var dialogViewModel = new EditCardDialogViewModel();
            if (DialogService.ShowEditCardDialog(dialogViewModel) == DialogResult.Yes)
            {
                GetCardsAsync();
            }
        }

        private async void GetCardsAsync()
        {
            var responseData = await HttpService.GetCardsAsync();
            if (responseData.Data != null)
            {
                this.Cards = new ObservableCollection<InformationCard>(responseData.Data);
            }
            else
            {
                DialogService.ShowMessage("Failed to get cards" + Environment.NewLine + responseData.ErrorMessage, MessageType.Error);
            }
        }


        private async void RemoveCardsAsync(object parameter)
        {
            if (parameter != null && parameter is IList cards)
            {
                var cardsToRemove = cards.Cast<InformationCard>().ToArray();
                if (DialogService.ShowMessage($"{cardsToRemove.Length} {(cardsToRemove.Length > 1 ? "cards" : "card")} will be removed. Proceed?", MessageType.Question) == DialogResult.Yes)
                {
                    foreach (var card in cards.Cast<InformationCard>())
                    {
                        var errorText = await HttpService.RemoveCardAsync(card);
                        if (errorText != null)
                        {
                            DialogService.ShowMessage("Failed to remove card" + Environment.NewLine + errorText, MessageType.Error);
                            return;
                        }
                    }

                    GetCardsAsync();
                }
            }
        }
    }
}
