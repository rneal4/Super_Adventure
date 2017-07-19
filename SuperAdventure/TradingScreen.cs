using System;
using System.Windows.Forms;
using Engine;

namespace SuperAdventure
{
    public partial class TradingScreen : Form
    {
        private Player _currentPlayer { get; set; }

        public TradingScreen(Player player)
        {
            _currentPlayer = player;

            InitializeComponent();

            DataGridViewCellStyle rightAlignedCellStyle = new DataGridViewCellStyle();
            rightAlignedCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvMyItems.RowHeadersVisible = false;
            dgvMyItems.AutoGenerateColumns = false;

            dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(InventoryItem.ItemID),
                Visible = false
            });

            dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 100,
                DataPropertyName = nameof(InventoryItem.Description)
            });

            dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Qty",
                Width = 30,
                DefaultCellStyle = rightAlignedCellStyle,
                DataPropertyName = nameof(InventoryItem.Quantity)
            });

            dgvMyItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Price",
                Width = 35,
                DefaultCellStyle = rightAlignedCellStyle,
                DataPropertyName = nameof(InventoryItem.Price)
            });

            dgvMyItems.Columns.Add(new DataGridViewButtonColumn
            {
                Text = "Sell 1",
                UseColumnTextForButtonValue = true,
                Width = 50,
                DataPropertyName = nameof(InventoryItem.ItemID)
            });

            dgvMyItems.DataSource = _currentPlayer.Inventory;

            dgvMyItems.CellClick += dgvMyItems_Click;

            dgvVendorItems.RowHeadersVisible = false;
            dgvVendorItems.AutoGenerateColumns = false;

            dgvVendorItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(InventoryItem.ItemID),
                Visible = false
            });

            dgvVendorItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 100,
                DataPropertyName = nameof(InventoryItem.Description)
            });

            dgvVendorItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Price",
                Width = 35,
                DefaultCellStyle = rightAlignedCellStyle,
                DataPropertyName = nameof(InventoryItem.Price)
            });

            dgvVendorItems.Columns.Add(new DataGridViewButtonColumn
            {
                Text = "Buy 1",
                UseColumnTextForButtonValue = true,
                Width = 50,
                DataPropertyName = nameof(InventoryItem.ItemID)
            });

            dgvVendorItems.DataSource = _currentPlayer.CurrentLocation.VendorWorkingHere.Inventory;

            dgvVendorItems.CellClick += dgvVendorItems_CellClick;
        }

        private void dgvMyItems_Click(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                var itemID = dgvMyItems.Rows[e.RowIndex].Cells[0].Value;

                Item itemBeingSold = World.ItemByID(Convert.ToInt32(itemID));

                if (!itemBeingSold.IsSellable)
                {
                    MessageBox.Show($"You cannot sell the {itemBeingSold.Name}");
                }
                else
                {
                    _currentPlayer.RemoveItemFromInventory(itemBeingSold);

                    _currentPlayer.Gold += itemBeingSold.Price;
                }
            }
        }

        private void dgvVendorItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                var itemID = dgvVendorItems.Rows[e.RowIndex].Cells[0].Value;

                Item itemBeingBought = World.ItemByID(Convert.ToInt32(itemID));

                if (_currentPlayer.Gold >= itemBeingBought.Price)
                {
                    _currentPlayer.AddItemToInventory(itemBeingBought);

                    _currentPlayer.Gold -= itemBeingBought.Price;
                }
                else
                {
                    MessageBox.Show($"You do not have enough gold to buy the {itemBeingBought.Name}");
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
