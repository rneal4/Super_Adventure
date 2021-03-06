﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Engine;
using static Engine.Location;
using static Engine.Player;

namespace SuperAdventureConsole
{
    public class Program
    {
        private static Player _player;
        private const string PLAYER_DATA_FILE_NAME_XML = "PlayerData.xml";
        private const string PLAYER_DATA_FILE_NAME_JSON = "PlayerData.json";

        static void Main(string[] args)
        {
            LoadGameData();

            Console.WriteLine("Type 'Help' to see a list of commands");
            Console.WriteLine("");

            DisplayCurrentLocation();

            _player.PropertyChanged += Player_OnPropertyChanged;
            _player.OnMessage += Player_OnMessage;

            while (true)
            {
                Console.WriteLine(">");

                string userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput))
                    continue;

                string cleanedInput = userInput.ToLower().Trim();

                if (cleanedInput == "exit")
                {
                    SaveGameData();

                    break;
                }

                ParseInput(cleanedInput);
            }
        }

        private static void Player_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Player.CurrentLocation))
            {
                DisplayCurrentLocation();

                if (_player.CurrentLocation.VendorWorkingHere != null)
                    Console.WriteLine($"You see a vendor here : {_player.CurrentLocation.VendorWorkingHere.Name}");
            }
        }

        private static void Player_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private static void ParseInput(string input)
        {
            if (input.Contains("help") || input == "?")
            {
                Console.WriteLine("Available commands");
                Console.WriteLine("====================================");
                Console.WriteLine("Stats - Display player information");
                Console.WriteLine("Look - Get the description of your location");
                Console.WriteLine("Inventory - Display your inventory");
                Console.WriteLine("Quests - Display your quests");
                Console.WriteLine("Attack - Fight the monster");
                Console.WriteLine("Equip <weapon name> - Set your current weapon");
                Console.WriteLine("Drink <potion name> - Drink a potion");
                Console.WriteLine("Trade - display your inventory and vendor's inventory");
                Console.WriteLine("Buy <item name> - Buy an item from a vendor");
                Console.WriteLine("Sell <item name> - Sell an item to a vendor");
                Console.WriteLine("North - Move North");
                Console.WriteLine("South - Move South");
                Console.WriteLine("East - Move East");
                Console.WriteLine("West - Move West");
                Console.WriteLine("Exit - Save the game and exit");
            }
            else if (input == "stats")
            {
                Console.WriteLine("Current hit points: {0}", _player.CurrentHitPoints);
                Console.WriteLine("Maximum hit points: {0}", _player.MaximumHitPoints);
                Console.WriteLine("Experience Points: {0}", _player.ExperiencePoints);
                Console.WriteLine("Level: {0}", _player.Level);
                Console.WriteLine("Gold: {0}", _player.Gold);
            }
            else if (input == "look")
            {
                DisplayCurrentLocation();
            }
            else if (input == "north" || input == "east" || input == "south" || input == "west")
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                Direction direction = (Direction)Enum.Parse(typeof(Direction), textInfo.ToTitleCase(input));
                _player.MoveTo(direction);
            }
            else if (input == "inventory")
            {
                foreach (InventoryItem inventoryItem in _player.Inventory)
                    Console.WriteLine($"{inventoryItem.Description}: {inventoryItem.Quantity}");
            }
            else if (input == "quets")
            {
                if (_player.Quests.Count == 0)
                {
                    Console.WriteLine("You do not have any quests");
                }
                else
                {
                    foreach (PlayerQuest playerQuest in _player.Quests)
                        Console.WriteLine($"{playerQuest.Name}: {(playerQuest.IsCompleted ? "Completed" : "Incomplete")}");
                }
            }
            else if (input.Contains("attack"))
            {
                if (_player.CurrentLocation.MonsterLivingHere == null)
                {
                    Console.WriteLine("There is nothing here to attack");
                }
                else
                {
                    if (_player.EquipedWeapon == null)
                        _player.EquipedWeapon = _player.Weapons.FirstOrDefault();

                    if (_player.HasWeaponEquiped)
                        _player.UseWeapon(_player.EquipedWeapon);
                    else
                        Console.WriteLine("Yuo do not have any weapons equiped");
                }
            }
            else if (input.StartsWith("equip "))
            {
                string inputWeaponName = input.Substring(6).Trim();

                if (string.IsNullOrWhiteSpace(inputWeaponName))
                {
                    Console.WriteLine("You must enter the name of the weapon to equip");
                }
                else
                {
                    Weapon weaponToEquip = _player.Weapons
                        .SingleOrDefault(x => x.Name.ToLower() == inputWeaponName || x.NamePlural.ToLower() == inputWeaponName);

                    if (weaponToEquip == null)
                    {
                        Console.WriteLine($"You not have the weapon: {inputWeaponName}");
                    }
                    else
                    {
                        _player.EquipedWeapon = weaponToEquip;

                        Console.WriteLine($"You equip your {_player.EquipedWeapon.Name}");
                    }
                }
            }
            else if (input.StartsWith("drink "))
            {
                string inputPotionName = input.Substring(6).Trim();

                if (string.IsNullOrWhiteSpace(inputPotionName))
                {
                    Console.WriteLine("You must enter the name of the potion to drink");
                }
                else
                {
                    HealingPotion potionToDrink = _player.Potions
                        .SingleOrDefault(x => x.Name.ToLower() == inputPotionName || x.NamePlural.ToLower() == inputPotionName);

                    if (potionToDrink == null)
                        Console.WriteLine($"You do not have the potion : {inputPotionName}");
                    else
                        _player.UsePotion(potionToDrink);
                }
            }
            else if (input == "trade")
            {
                if (_player.CurrentLocation.VendorWorkingHere == null)
                {
                    Console.WriteLine("There is no vendor here");
                }
                else
                {
                    Console.WriteLine("PLAYER INVENTORY");
                    Console.WriteLine("================");

                    if (_player.Inventory.Count(x => x.Price != World.UNSELLABLE_ITEM_PRICE) == 0)
                    {
                        Console.WriteLine("You do  not have any inventory");
                    }
                    else
                    {
                        foreach (InventoryItem inventoryItem in _player.Inventory.Where(x => x.Price != World.UNSELLABLE_ITEM_PRICE))
                            Console.WriteLine($"{inventoryItem.Quantity} {inventoryItem.Description} Price: {inventoryItem.Price}");
                    }


                    Console.WriteLine("");
                    Console.WriteLine("VENDOR INVENTORY");
                    Console.WriteLine("================");

                    if (_player.CurrentLocation.VendorWorkingHere.Inventory.Count == 0)
                    {
                        Console.WriteLine("The vendor does not have any inventory");
                    }
                    else
                    {
                        foreach (InventoryItem inventoryItem in _player.CurrentLocation.VendorWorkingHere.Inventory)
                            Console.WriteLine($"{inventoryItem.Quantity} {inventoryItem.Description} Price: {inventoryItem.Price}");
                    }
                }
            }
            else if (input.StartsWith("buy "))
            {
                if (!_player.CurrentLocation.VendorIsHere)
                {
                    Console.WriteLine("There is no vendor at this location");
                }
                else
                {
                    string itemName = input.Substring(4).Trim();

                    if (string.IsNullOrWhiteSpace(itemName))
                    {
                        Console.WriteLine("You must enter the name of the item to buy");
                    }
                    else
                    {
                        InventoryItem itemToBuy = _player.CurrentLocation.VendorWorkingHere.Inventory
                            .SingleOrDefault(x => x.Details.Name.ToLower() == itemName);

                        if (itemToBuy == null)
                        {
                            Console.WriteLine($"The vendor does not have any {itemName}");
                        }
                        else
                        {
                            if (_player.Gold < itemToBuy.Price)
                            {
                                Console.WriteLine($"You do not have enough gold to buy a {itemToBuy.Description}");
                            }
                            else
                            {
                                _player.AddItemToInventory(itemToBuy.Details);
                                _player.Gold -= itemToBuy.Price;

                                Console.WriteLine($"You bought one {itemToBuy.Details.Name} for {itemToBuy.Price} gold");
                            }
                        }
                    }
                }
            }
            else if (input.StartsWith("sell "))
            {
                if (!_player.CurrentLocation.VendorIsHere)
                {
                    Console.WriteLine("There is no vendor at this location");
                }
                else
                {
                    string itemName = input.Substring(5).Trim();

                    if (string.IsNullOrWhiteSpace(itemName))
                    {
                        Console.WriteLine("You must enter the name of the item to sell");
                    }
                    else
                    {
                        InventoryItem itemToSell = _player.Inventory
                            .SingleOrDefault(x => x.Details.Name.ToLower() == itemName && x.Quantity > 0 && x.Price != World.UNSELLABLE_ITEM_PRICE);

                        if (itemToSell == null)
                        {
                            Console.WriteLine($"The player cannot sell any {itemName}");
                        }
                        else
                        {
                            _player.RemoveItemFromInventory(itemToSell.Details);
                            _player.Gold += itemToSell.Price;

                            Console.WriteLine($"You receive {itemToSell.Price} gold for your {itemToSell.Details.Name}");
                        }

                    }
                }
            }
            else
            {
                Console.WriteLine("I do not understand");
                Console.WriteLine("Type 'Help' to see a list of available commands");
            }

            Console.WriteLine("");
        }

        private static void DisplayCurrentLocation()
        {
            Console.WriteLine($"You are at: {_player.CurrentLocation.Name}");

            if (!string.IsNullOrWhiteSpace(_player.CurrentLocation.Description))
                Console.WriteLine(_player.CurrentLocation.Description);
        }

        private static void LoadGameData()
        {
            if (File.Exists(PLAYER_DATA_FILE_NAME_XML))
                _player = Player.CreatePlayerFromXMLString(File.ReadAllText(PLAYER_DATA_FILE_NAME_XML));
            else if (File.Exists(PLAYER_DATA_FILE_NAME_XML))
                _player = Player.CreatePlryerFromJSONString(File.ReadAllText(PLAYER_DATA_FILE_NAME_JSON));
            else
                _player = Player.CreateDefaultPlayer();
        }

        private static void SaveGameData()
        {
            PlayerDataMapper.SaveToDatabase(_player);

            File.WriteAllText(PLAYER_DATA_FILE_NAME_XML, _player.ToXMLString());

            File.WriteAllText(PLAYER_DATA_FILE_NAME_JSON, _player.ToJSONString());
        }
    }
}
