using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;

namespace Engine
{
    public class Player : LivingCreature
    {
        private Monster _currentMonster;
        
        private int _gold;
        public int Gold
        {
            get { return _gold; }
            set
            {
                _gold = value;
                OnPropertyChanged(nameof(Gold));
            }
        }

        private int _experiencePoints;
        public int ExperiencePoints
        {
            get { return _experiencePoints; }
            private set
            {
                _experiencePoints = value;
                OnPropertyChanged(nameof(ExperiencePoints));
                OnPropertyChanged(nameof(Level));
            }
        }

        private Location _currentLocation;
        public Location CurrentLocation
        {
            get { return _currentLocation; }
            set
            {
                _currentLocation = value;
                OnPropertyChanged(nameof(CurrentLocation));
            }
        }
        
        public int Level => ((ExperiencePoints / 100) + 1);

        public Weapon EquipedWeapon { get; set; }
    
        public List<Weapon> Weapons => Inventory
            .Where(x => x.Details is Weapon)
            .Select(x => x.Details as Weapon)
            .ToList();

        public List<HealingPotion> Potions => Inventory
            .Where(x => x.Details is HealingPotion)
            .Select(x => x.Details as HealingPotion)
            .ToList();
        

        public BindingList<InventoryItem> Inventory { get; set; }
        public BindingList<PlayerQuest> Quests { get; set; }
        
        
        private Player(int currentHitPoints, int maximumHitPoints, int gold,
            int experiencePoints) : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new BindingList<InventoryItem>();
            Quests = new BindingList<PlayerQuest>();
        }

        public static Player CreateDefaultPlayer()
        {
            Player player = new Player(10, 10, 20, 0);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);

            return player;
        }

        public static Player CreatePlayerFromXMLString(string xmlPlayerData)
        {
            try
            {
                XmlDocument playerData = new XmlDocument();

                playerData.LoadXml(xmlPlayerData);

                int currentHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                int maximumHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                int experiencePoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

                Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);

                int currentLocationID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = World.LocationByID(currentLocationID);

                if (playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
                {
                    int currentWeaponID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText);
                    player.EquipedWeapon = (Weapon)World.ItemByID(currentWeaponID);
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/InventoryItems/InventoryItem"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                    for (int i = 0; i < quantity; i++)
                        player.AddItemToInventory(World.ItemByID(id));
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);

                    PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
                    playerQuest.IsCompleted = isCompleted;

                    player.Quests.Add(playerQuest);
                }

                return player;
            }
            catch
            {
                return Player.CreateDefaultPlayer();
            }
        }

        public static Player CreatePlayerFromDatabase(int currentHitPoints, int maximumHutPoints, int gold,
            int experiencePoints, int currentLocationID)
        {
            Player player = new Player(currentHitPoints, maximumHutPoints, gold, experiencePoints);

            player.MoveTo(World.LocationByID(currentLocationID));

            return player;
        }

        //TODO Add CreatePlayerFromJSON


        public bool HasWeaponEquiped => EquipedWeapon != null;


        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.RequiresItem)
                return true;

            return Inventory.Any(ii => ii.Details.ID == location.ItemRequiredToEnter.ID);
        }


        public bool HasThisQuest(Quest quest)
        {
            return Quests.Any(pq => pq.Details.ID == quest.ID);
        }

        public bool HasCompletedThisQuest(Quest quest)
        {
            foreach (PlayerQuest pq in Quests)
            {
                if (pq.Details.ID == quest.ID)
                    return pq.IsCompleted;
            }

            return false;
        }

        public void GiveQuest(Quest quest)
        {
            if (HasThisQuest(quest))
                return;

            RaiseMessage($"You receieve the {quest.Name} quest.");
            RaiseMessage($"{quest.Description}");
            RaiseMessage($"To complete it, return with:");
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                if (qci.Quantity == 1)
                    RaiseMessage($"{qci.Quantity} {qci.Details.Name}");
                else
                    RaiseMessage($"{qci.Quantity} {qci.Details.NamePlural}");
            }
            RaiseMessage("");

            Quests.Add(new PlayerQuest(quest));
        }

        public void CompleteQuest(Quest quest)
        {
            if (!HasThisQuest(quest) || !HasAllQuestCompletionItems(quest))
                return;

            PlayerQuest playerQuest = Quests.SingleOrDefault(pq => pq.Details.ID == quest.ID);

            RaiseMessage("");
            RaiseMessage($"You complete the {quest.Name} quest.");

            RemoveQuestCompletionItems(quest);

            RaiseMessage($"You receive: ");
            RaiseMessage($"{quest.RewardExperiencePoints} experience points");
            RaiseMessage($"{quest.RewardGold} gold");
            RaiseMessage($"{quest.RewardItem.Name}");

            AddExperiencePoints(quest.RewardExperiencePoints);

            Gold += quest.RewardGold;

            AddItemToInventory(quest.RewardItem);

            playerQuest.IsCompleted = true;
        }
        
        public bool HasAllQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                if (!Inventory.Any(ii => ii.Details.ID == qci.Details.ID && ii.Quantity >= qci.Quantity))
                    return false;
            }

            return true;
        }    

        private void RemoveQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == qci.Details.ID);

                if (item != null)
                    RemoveItemFromInventory(item.Details, qci.Quantity);
            }
        }

    


        public void AddItemToInventory(Item itemToAdd, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

            if (item == null)
                Inventory.Add(new InventoryItem(itemToAdd, quantity));
            else
                item.Quantity += quantity;

            RaiseInventoryChangedEvent(itemToAdd);
        }

        public void RemoveItemFromInventory(Item itemToRemove, int quantuty = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToRemove.ID);

            if (item == null)
            {
               //TODO Add NullReference Exception
            }
            else
            {
                item.Quantity -= quantuty;

                if (item.Quantity < 0)
                    item.Quantity = 0;

                if (item.Quantity == 0)
                    Inventory.Remove(item);

                RaiseInventoryChangedEvent(itemToRemove);
            }
        }


        public void AddExperiencePoints(int experiencePointsToAdd)
        {
            ExperiencePoints += experiencePointsToAdd;
            MaximumHitPoints = (Level * 10);
        }


        public void SetMonsterFromLocation(Location location)
        {
            if (!location.MonsterIsHere)
                return;

            RaiseMessage($"You see a {location.MonsterLivingHere.Name}");

            Monster standardMonster = World.MonsterByID(location.MonsterLivingHere.ID);

            _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage,
                standardMonster.RewardExperiencePoints, standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

            foreach (LootItem lootItem in standardMonster.LootTable)
                _currentMonster.LootTable.Add(lootItem);
        }



        public string ToXMLString()
        {
            XmlDocument playerData = new XmlDocument();

            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);

            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);

            XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
            currentHitPoints.AppendChild(playerData.CreateTextNode(CurrentHitPoints.ToString()));
            stats.AppendChild(currentHitPoints);

            XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
            maximumHitPoints.AppendChild(playerData.CreateTextNode(MaximumHitPoints.ToString()));
            stats.AppendChild(maximumHitPoints);

            XmlNode gold = playerData.CreateElement("Gold");
            gold.AppendChild(playerData.CreateTextNode(Gold.ToString()));
            stats.AppendChild(gold);

            XmlNode experiencePoints = playerData.CreateElement("ExperiencePoints");
            experiencePoints.AppendChild(playerData.CreateTextNode(ExperiencePoints.ToString()));
            stats.AppendChild(experiencePoints);


            XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
            currentLocation.AppendChild(playerData.CreateTextNode(CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);

            if (HasWeaponEquiped)
            {
                XmlNode currentWeapon = playerData.CreateElement("CurrentWeapon");
                currentWeapon.AppendChild(playerData.CreateTextNode(EquipedWeapon.ID.ToString()));
                stats.AppendChild(currentWeapon);
            }

            XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);

            foreach (InventoryItem item in Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = item.Details.ID.ToString();
                inventoryItem.Attributes.Append(idAttribute);

                XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
                quantityAttribute.Value = item.Quantity.ToString();
                inventoryItem.Attributes.Append(quantityAttribute);

                inventoryItems.AppendChild(inventoryItem);
            }

            XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
            player.AppendChild(playerQuests);

            foreach (PlayerQuest quest in Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = quest.Details.ID.ToString();
                playerQuest.Attributes.Append(idAttribute);
                
                XmlAttribute isCompletedAttribute = playerData.CreateAttribute("IsCompleted");
                isCompletedAttribute.Value = quest.IsCompleted.ToString();
                playerQuest.Attributes.Append(isCompletedAttribute);

                playerQuests.AppendChild(playerQuest);
            }

            return playerData.InnerXml;
        }
        
        //TODO Add ToJSON


        

        public void MoveNorth()
        {
            if (CurrentLocation.LocationToNorth != null)
                MoveTo(CurrentLocation.LocationToNorth);
        }

        public void MoveEast()
        {
            if (CurrentLocation.LocationToEast != null)
                MoveTo(CurrentLocation.LocationToEast);
        }

        public void MoveSouth()
        {
            if (CurrentLocation.LocationToSouth != null)
                MoveTo(CurrentLocation.LocationToSouth);
        }

        public void MoveWest()
        {
            if (CurrentLocation.LocationToWest != null)
                MoveTo(CurrentLocation.LocationToWest);
        }

        private void MoveHome()
        {
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
        }

        public void MoveTo(Location location)
        {
            if (!HasRequiredItemToEnterThisLocation(location))
            {
                RaiseMessage($"You must have a {location.ItemRequiredToEnter.Name} " +
                     $"to enter this location.");

                return;
            }

            CurrentLocation = location;

            this.FullHeal();

            if (location.HasQuest)
            {
                if (!HasThisQuest(location.QuestAvailableHere))
                    GiveQuest(location.QuestAvailableHere);
                else
                    CompleteQuest(location.QuestAvailableHere);
            }

            SetMonsterFromLocation(location);
        }

        public void UseWeapon(Weapon weapon)
        {
            int damageToMonster = RandomNumberGenerator.NumberBetween(weapon.MinimumDamage, weapon.MaximumDamage);

            _currentMonster.CurrentHitPoints -= damageToMonster;

            RaiseMessage($"You hit the {_currentMonster.Name} for {damageToMonster} points.");

            if (_currentMonster.CurrentHitPoints <= 0)
            {
                RaiseMessage("");
                RaiseMessage($"You defeated the {_currentMonster.Name}");

                AddExperiencePoints(_currentMonster.RewardExperiencePoints);
                RaiseMessage($"You receive {_currentMonster.RewardExperiencePoints} experience points");

                Gold += _currentMonster.RewardGold;
                RaiseMessage($"You receieve {_currentMonster.RewardGold} gold");

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

                foreach (InventoryItem inventoryItem in lootedItems)
                {
                    AddItemToInventory(inventoryItem.Details);

                    if (inventoryItem.Quantity == 1)
                        RaiseMessage($"You loot {inventoryItem.Quantity} {inventoryItem.Details.Name}");
                    else
                        RaiseMessage($"You loot {inventoryItem.Quantity} {inventoryItem.Details.NamePlural}");
                }

                RaiseMessage("");

                MoveTo(CurrentLocation);
            }
            else
            {
                int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

                RaiseMessage($"The {_currentMonster.Name} did {damageToPlayer} points of damage.");

                CurrentHitPoints -= damageToPlayer;

                if (CurrentHitPoints <= 0)
                {
                    RaiseMessage($"The {_currentMonster.Name} killed you.");

                    MoveHome();
                }
            }
        }

        public void UsePotion(HealingPotion potion)
        {
            CurrentHitPoints = (CurrentHitPoints + potion.AmountToHeal);

            if (CurrentHitPoints > MaximumHitPoints)
                this.FullHeal();

            RemoveItemFromInventory(potion, 1);

            RaiseMessage($"You drink a {potion.Name}");

            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

            RaiseMessage($"The {_currentMonster.Name} did {damageToPlayer} points of damage.");

            CurrentHitPoints -= damageToPlayer;

            if (CurrentHitPoints <= 0)
            {
                RaiseMessage($"The {_currentMonster.Name} killed you.");

                MoveHome();
            }
        }




        public event EventHandler<MessageEventArgs> OnMessage;

        private void RaiseMessage(string message, bool addExtraLine = false)
        {
            OnMessage?.Invoke(this, new MessageEventArgs(message, addExtraLine));
        }

        private void RaiseInventoryChangedEvent(Item item)
        {
            if (item is Weapon)
                OnPropertyChanged(nameof(Weapons));

            if (item is HealingPotion)
                OnPropertyChanged(nameof(Potions));
        }

    }
}
