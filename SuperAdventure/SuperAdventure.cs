﻿using Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;
        public SuperAdventure()
        {
            InitializeComponent();

            _player = new Player(10, 10, 20, 0, 1);
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));


            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            Weapon currentWeapon = (Weapon)cboPotions.SelectedItem;

            int damageToMonster = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            _currentMonster.CurrentHitPoints -= damageToMonster;

            rtbMessages.Text += $"You hit the {_currentMonster.Name} for {damageToMonster} points.{Environment.NewLine}";

            if (_currentMonster.CurrentHitPoints <= 0)
            {
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += $"You defeated the {_currentMonster.Name}{Environment.NewLine}";

                _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
                rtbMessages.Text += $"You receive {_currentMonster.RewardExperiencePoints} experience points{Environment.NewLine}";

                _player.Gold += _currentMonster.RewardGold;
                rtbMessages.Text += $"You receieve {_currentMonster.RewardGold} gold{Environment.NewLine}";

                List<InventoryItem> lootedItems = new List<InventoryItem>();

                foreach (LootItem lootItem in _currentMonster.LootTable)
                {
                    if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                }

                if (lootedItems.Count == 0)
                {
                    foreach (LootItem lootItem in _currentMonster.LootTable)
                    {
                        if (lootItem.IsDefaultItem)
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }


            }
        }           

        private void btnUsePotion_Click(object sender, EventArgs e)
        {

        }

        private void MoveTo(Location newLocation)
        {
            if (newLocation.ItemRequiredToEnter != null)
            {
                if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
                {
                    rtbMessages.Text += $"You must have a {newLocation.ItemRequiredToEnter.Name} " +
                        $"to enter this location.{Environment.NewLine}";
                    return;
                }

                _player.CurrentLocation = newLocation;

                btnNorth.Visible = (newLocation.LocationToNorth != null);
                btnEast.Visible = (newLocation.LocationToEast != null);
                btnSouth.Visible = (newLocation.LocationToSouth != null);
                btnWest.Visible = (newLocation.LocationToWest != null);

                rtbLocation.Text = $"{newLocation.Name}{Environment.NewLine}";
                rtbLocation.Text += $"{newLocation.Description}{Environment.NewLine}";

                _player.CurrentHitPoints = _player.MaximumHitPoints;

                lblHitPoints.Text = _player.CurrentHitPoints.ToString();

                if (newLocation.QuestAvailableHere != null)
                {
                    bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
                    bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);

                    if (playerAlreadyHasQuest)
                    {
                        if (!playerAlreadyCompletedQuest)
                        {
                            bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                            if (playerHasAllItemsToCompleteQuest)
                            {
                                rtbMessages.Text += Environment.NewLine;
                                rtbMessages.Text += $"You complete the {newLocation.QuestAvailableHere.Name} quest.{Environment.NewLine}";

                                _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                                //Give quest rewards
                                rtbMessages.Text += $"You receive: {Environment.NewLine}";
                                rtbMessages.Text += $"{newLocation.QuestAvailableHere.RewardExperiencePoints} experience points{Environment.NewLine}";
                                rtbMessages.Text += $"{newLocation.QuestAvailableHere.RewardGold} gold{Environment.NewLine}";
                                rtbMessages.Text += $"{newLocation.QuestAvailableHere.RewardItem}{Environment.NewLine}";
                                rtbMessages.Text += Environment.NewLine;

                                _player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
                                _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                                _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                                _player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                            }
                        }
                    }
                }
                else
                {
                    rtbMessages.Text += $"You receieve the {newLocation.QuestAvailableHere.Name} quest.{Environment.NewLine}";
                    rtbMessages.Text += $"{newLocation.QuestAvailableHere.Description}{Environment.NewLine}";
                    rtbMessages.Text += $"To complete it, return with:{Environment.NewLine}";
                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                            rtbMessages.Text += $"{qci.Quantity} {qci.Details.Name}{Environment.NewLine}";
                        else
                            rtbMessages.Text += $"{qci.Quantity} {qci.Details.NamePlural}{Environment.NewLine}";
                    }
                    rtbMessages.Text += Environment.NewLine;

                    _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += $"You see a {newLocation.MonsterLivingHere.Name}{Environment.NewLine}";

                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage,
                    standardMonster.RewardExperiencePoints, standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                    _currentMonster.LootTable.Add(lootItem);

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else
            {
                _currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }

            UpdateInventoryListInUI();

            UpdateQuestListInUI();

            UpdateWeaponListInUI();

            UpdatePotionListInUI();
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                    dgvInventory.Rows.Add(inventoryItem.Details.Name, inventoryItem.Quantity.ToString());
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(playerQuest.Details.Name, playerQuest.IsCompleted.ToString());
            }
        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                        weapons.Add((Weapon)inventoryItem.Details);
                }
            }

            if (weapons.Count == 0)
            {
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                }
            }

            if (healingPotions.Count == 0)
            {
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }
    }
}
