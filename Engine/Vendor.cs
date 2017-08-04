using System.ComponentModel;
using System.Linq;

namespace Engine
{
    public class Vendor : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public BindingList<InventoryItem> Inventory { get; set; }

        public Vendor(string name)
        {
            Name = name;
            Inventory = new BindingList<InventoryItem>();
        }

        public void AddItemToInventory(Item itemToAdd, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.ItemID == itemToAdd.ID);

            if (item == null)
                Inventory.Add(new InventoryItem(itemToAdd, quantity));
            else
                item.Quantity += quantity;

            OnPropertyChanged(nameof(Inventory));
        }

        public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.ItemID == itemToRemove.ID);

            if (item != null)
            {
                item.Quantity -= quantity;

                if (item.Quantity < 0)
                    item.Quantity = 0;

                if (item.Quantity == 0)
                    Inventory.Remove(item);

                OnPropertyChanged(nameof(Inventory));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
