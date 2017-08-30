using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Engine;
using static Engine.Location;
using static Engine.Player;

namespace SuperAdventure
{

    //TODO Different from AddUnitTest branch
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private const string PLAYER_DATA_FILE_NAME_XML = "PlayerData.xml";
        private const string PLAYER_DATA_FILE_NAME_JSON = "PlayerData.json";
        public SuperAdventure()
        {
            InitializeComponent();

            _player = PlayerDataMapper.CreateFromDatabase();

            if (_player == null)
            {
                if (File.Exists(PLAYER_DATA_FILE_NAME_XML))
                    _player = Player.CreatePlayerFromXMLString(File.ReadAllText(PLAYER_DATA_FILE_NAME_XML));
                else if (File.Exists(PLAYER_DATA_FILE_NAME_XML))
                    _player = Player.CreatePlryerFromJSONString(File.ReadAllText(PLAYER_DATA_FILE_NAME_JSON));
                else
                    _player = Player.CreateDefaultPlayer();
            }

            lblHitPoints.DataBindings.Add(nameof(Label.Text), _player, nameof(Player.CurrentHitPoints));
            lblGold.DataBindings.Add(nameof(Label.Text), _player, nameof(Player.Gold));
            lblExperience.DataBindings.Add(nameof(Label.Text), _player, nameof(Player.ExperiencePoints));
            lblLevel.DataBindings.Add(nameof(Label.Text), _player, nameof(Player.Level));

            dgvInventory.RowHeadersVisible = false;
            dgvInventory.AutoGenerateColumns = false;
            dgvInventory.DataSource = _player.Inventory;
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = nameof(InventoryItem.Description)
            });
            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Quantity",
                DataPropertyName = nameof(InventoryItem.Quantity)
            });

            dgvQuests.RowHeadersVisible = false;
            dgvQuests.AutoGenerateColumns = false;
            dgvQuests.DataSource = _player.Quests;
            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = nameof(PlayerQuest.Name)
            });
            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Done?",
                DataPropertyName = nameof(PlayerQuest.IsCompleted)
            });

            cboWeapons.DataSource = _player.Weapons;
            cboWeapons.DisplayMember = nameof(Weapon.Name);
            cboWeapons.ValueMember = nameof(Weapon.ID);
            if (_player.HasWeaponEquiped)
                cboWeapons.SelectedItem = _player.EquipedWeapon;
            cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;

            cboPotions.DataSource = _player.Potions;
            cboPotions.DisplayMember = nameof(HealingPotion.Name);
            cboPotions.ValueMember = nameof(HealingPotion.ID);

            _player.PropertyChanged += PlayerOnPropertyChanged;
            _player.OnMessage += DisplayMessage;

            _player.MoveTo(_player.CurrentLocation);
        }

        private void Move_Click(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                Button button = sender as Button;
                if (button.Tag is Direction)
                    _player.MoveTo((Direction)button.Tag);
            }
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            _player.UseWeapon(currentWeapon);
        }           

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            _player.UsePotion(potion);
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            PlayerDataMapper.SaveToDatabase(_player);

            File.WriteAllText(PLAYER_DATA_FILE_NAME_XML, _player.ToXMLString());

            File.WriteAllText(PLAYER_DATA_FILE_NAME_JSON, _player.ToJSONString());
        }

        private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            _player.EquipedWeapon = (Weapon)cboWeapons.SelectedItem;
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Player.Weapons))
            {
                cboWeapons.DataSource = _player.Weapons;

                if (!_player.Weapons.Any())
                {
                    cboWeapons.Visible = false;
                    btnUseWeapon.Visible = false;
                }
            }
            else if (propertyChangedEventArgs.PropertyName == nameof(Player.Potions))
            {
                cboPotions.DataSource = _player.Potions;

                if (!_player.Potions.Any())
                {
                    cboPotions.Visible = false;
                    btnUsePotion.Visible = false;
                }
            }
            else if (propertyChangedEventArgs.PropertyName == nameof(Player.CurrentLocation))
            {
                btnNorth.Visible = (_player.CurrentLocation.LocationToNorth != null);
                btnEast.Visible = (_player.CurrentLocation.LocationToEast != null);
                btnSouth.Visible = (_player.CurrentLocation.LocationToSouth != null);
                btnWest.Visible = (_player.CurrentLocation.LocationToWest != null);

                rtbLocation.Text = $"{_player.CurrentLocation.Name}{Environment.NewLine}";
                rtbLocation.Text = $"{_player.CurrentLocation.Description}{Environment.NewLine}";

                if (!_player.CurrentLocation.MonsterIsHere)
                {
                    cboWeapons.Visible = false;
                    cboPotions.Visible = false;
                    btnUseWeapon.Visible = false;
                    btnUsePotion.Visible = false;
                }
                else
                {
                    cboWeapons.Visible = _player.Weapons.Any();
                    cboPotions.Visible = _player.Potions.Any();
                    btnUseWeapon.Visible = _player.Weapons.Any();
                    btnUsePotion.Visible = _player.Potions.Any();
                }

                btnTrade.Visible = _player.CurrentLocation.VendorIsHere;
            }
        }

        private void DisplayMessage(object sender, MessageEventArgs messageEventArgs)
        {
            rtbMessages.Text += $"{messageEventArgs.Message}{Environment.NewLine}";

            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void btnTrade_Click(object sender, EventArgs e)
        {
            TradingScreen tradingScreen = new TradingScreen(_player);
            tradingScreen.StartPosition = FormStartPosition.CenterParent;
            tradingScreen.ShowDialog(this);
        }
    }
}
