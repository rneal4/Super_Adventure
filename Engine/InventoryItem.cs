using System.ComponentModel;

namespace Engine
{
    public class InventoryItem : INotifyPropertyChanged
    {
        private Item _details;
        public Item Details
        {
            get { return _details; }
            set
            {
                _details = value;
                OnPropertyChanged(nameof(Details));
            }
        }
        
        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Description));
            }
        }

        public int ItemID => Details.ID;

        public int Price => Details.Price;

        public string Description => Quantity > 1 ? Details.NamePlural : Details.Name;

        public event PropertyChangedEventHandler PropertyChanged;

        public InventoryItem(Item details, int quantity)
        {
            Details = details;
            Quantity = quantity;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
