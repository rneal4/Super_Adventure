using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Engine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace EngineTest
{
    [TestClass]
    public class PlayerTest
    {
        static JsonSerializerSettings JsonSetting = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, TypeNameHandling = TypeNameHandling.All };

        [TestMethod]
        public void Gold_EventsRaised_GoldChanged()
        {
            List<string> receivedEvents = new List<string>();
            Player player = new Player(10, 20, 50, 0);

            player.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            player.Gold += 32;
            Assert.AreEqual(1, receivedEvents.Count);
            Assert.AreEqual(nameof(Player.Gold), receivedEvents[0]);
            Assert.AreEqual(82, player.Gold);
        }

        [TestMethod]
        public void ExperiencePoints_EventsRaised_ExperiencePointsChanged()
        {
            List<string> receivedEvents = new List<string>();
            Player player = new Player(10, 20, 50, 0);

            player.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            player.AddExperiencePoints(12);
            Assert.AreEqual(2, receivedEvents.Count);
            Assert.AreEqual(nameof(Player.ExperiencePoints), receivedEvents[0]);
            Assert.AreEqual(nameof(Player.Level), receivedEvents[1]);
            Assert.AreEqual(12, player.ExperiencePoints);
        }

        [TestMethod]
        public void ExperiencePoints_NegativeExp_ShouldThrowException()
        {
            List<string> receivedEvents = new List<string>();
            Player player = new Player(10, 20, 50, 0);

            Assert.ThrowsException<ArgumentException>(() => AddExperience());

            void AddExperience()
            {
                player.AddExperiencePoints(-3);
            }
        }

        [TestMethod]
        public void ExperiencePoints_LevelChanged_MaximumHitPointsChanged()
        {
            List<string> receivedEvents = new List<string>();
            Player player = new Player(10, 20, 50, 0);


            player.AddExperiencePoints(280);


            Assert.AreEqual(3, player.Level);
            Assert.AreEqual(30, player.MaximumHitPoints);
        }

        [TestMethod]
        public void Location_EventRaised_LocationChanged()
        {
            List<string> receivedEvents = new List<string>();
            Player player = new Player(10, 20, 50, 0);

            player.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            player.MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            Assert.IsTrue(receivedEvents.Contains(nameof(player.CurrentLocation)));
            Assert.AreEqual(World.LocationByID(World.LOCATION_ID_HOME), player.CurrentLocation);
        }

        [TestMethod]
        public void CreateDefaultPlayer_Test()
        {
            Player player = Player.CreateDefaultPlayer();

            Assert.AreEqual(10, player.CurrentHitPoints);
            Assert.AreEqual(10, player.MaximumHitPoints);
            Assert.AreEqual(20, player.Gold);
            Assert.AreEqual(0, player.ExperiencePoints);
            Assert.IsTrue(player.Inventory.Any(ii => ii.Details.ID == World.ITEM_ID_RUSTY_SWORD));
            Assert.AreEqual(World.LocationByID(World.LOCATION_ID_HOME), player.CurrentLocation);
        }

        [TestMethod]
        public void CreatePlayerJSON_SaveFile_ReadPlayerFromJSONFile()
        {
            Player playerBefore = Player.CreateDefaultPlayer();
            playerBefore.AddExperiencePoints(670);
            playerBefore.Gold = 500;
            playerBefore.AddItemToInventory(World.ItemByID(World.ITEM_ID_ADVENTURER_PASS));
            playerBefore.GiveQuest(World.QuestByID(World.QUEST_ID_CLEAR_ALCHEMIST_GARDEN));
            playerBefore.EquipedWeapon = (Weapon)World.ItemByID(World.ITEM_ID_RUSTY_SWORD);
            playerBefore.MoveTo(World.LocationByID(World.LOCATION_ID_BRIDGE));

            File.WriteAllText("TestJSON.json", playerBefore.ToJSONString());

            Player playerAfter = Player.CreatePlryerFromJSONString(File.ReadAllText("TestJSON.json"));

            Assert.AreEqual(playerBefore.ExperiencePoints, playerAfter.ExperiencePoints);
            Assert.AreEqual(playerBefore.MaximumHitPoints, playerAfter.MaximumHitPoints);
            Assert.AreEqual(playerBefore.Gold, playerAfter.Gold);
            Assert.AreEqual(playerBefore.CurrentLocation, playerAfter.CurrentLocation);
            Assert.AreEqual(JsonConvert.SerializeObject(playerBefore.Inventory, JsonSetting), JsonConvert.SerializeObject(playerAfter.Inventory, JsonSetting));
            Assert.AreEqual(JsonConvert.SerializeObject(playerBefore.Quests, JsonSetting), JsonConvert.SerializeObject(playerAfter.Quests, JsonSetting));
            Assert.AreEqual(JsonConvert.SerializeObject(playerBefore.EquipedWeapon, JsonSetting), JsonConvert.SerializeObject(playerAfter.EquipedWeapon, JsonSetting));

            File.Delete("TestJSON.json");
        }

        [TestMethod]
        public void CreatePlayerXML_SaveFile_ReadPlayerFromXMLFile()
        {
            Player playerBefore = Player.CreateDefaultPlayer();
            playerBefore.AddExperiencePoints(670);
            playerBefore.Gold = 500;
            playerBefore.AddItemToInventory(World.ItemByID(World.ITEM_ID_ADVENTURER_PASS));
            playerBefore.GiveQuest(World.QuestByID(World.QUEST_ID_CLEAR_ALCHEMIST_GARDEN));
            playerBefore.EquipedWeapon = (Weapon)World.ItemByID(World.ITEM_ID_RUSTY_SWORD);
            playerBefore.MoveTo(World.LocationByID(World.LOCATION_ID_BRIDGE));

            File.WriteAllText("TestXML.xml", playerBefore.ToXMLString());

            Player playerAfter = Player.CreatePlayerFromXMLString(File.ReadAllText("TestXML.xml"));

            Assert.AreEqual(playerBefore.ExperiencePoints, playerAfter.ExperiencePoints);
            Assert.AreEqual(playerBefore.MaximumHitPoints, playerAfter.MaximumHitPoints);
            Assert.AreEqual(playerBefore.Gold, playerAfter.Gold);
            Assert.AreEqual(playerBefore.CurrentLocation, playerAfter.CurrentLocation);
            Assert.AreEqual(JsonConvert.SerializeObject(playerBefore.Inventory, JsonSetting), JsonConvert.SerializeObject(playerAfter.Inventory, JsonSetting));
            Assert.AreEqual(JsonConvert.SerializeObject(playerBefore.Quests, JsonSetting), JsonConvert.SerializeObject(playerAfter.Quests, JsonSetting));
            Assert.AreEqual(JsonConvert.SerializeObject(playerBefore.EquipedWeapon, JsonSetting), JsonConvert.SerializeObject(playerAfter.EquipedWeapon, JsonSetting));

            File.Delete("TestXML.json");
        }

        [TestMethod]
        public void CompleteQuest_MarkedCompleted_ItemsRemoved_RewardGiven()
        {
            Player player = Player.CreateDefaultPlayer();
            player.GiveQuest(World.QuestByID(World.QUEST_ID_CLEAR_ALCHEMIST_GARDEN));
            player.AddItemToInventory(World.ItemByID(World.ITEM_ID_RAT_TAIL), 5);

            player.CompleteQuest(World.QuestByID(World.QUEST_ID_CLEAR_ALCHEMIST_GARDEN));

            Assert.IsTrue(player.Quests.SingleOrDefault(x => x.Details.ID == World.QUEST_ID_CLEAR_ALCHEMIST_GARDEN).IsCompleted);
            Assert.AreEqual(2, player.Inventory.SingleOrDefault(x => x.Details.ID == World.ITEM_ID_RAT_TAIL).Quantity);
            Assert.IsTrue(player.Inventory.Any(x => x.Details.ID == World.QuestByID(World.QUEST_ID_CLEAR_ALCHEMIST_GARDEN).RewardItem.ID));
        }

        [TestMethod]
        public void MoveToByDirect_MoveSuccesful()
        {
            Player player = Player.CreateDefaultPlayer();
            player.AddItemToInventory(World.ItemByID(World.ITEM_ID_ADVENTURER_PASS));
            player.MoveTo(World.LocationByID(World.LOCATION_ID_GUARD_POST));

            player.MoveTo(Location.Direction.West);

            Assert.AreEqual(World.LocationByID(World.LOCATION_ID_TOWN_SQUARE), player.CurrentLocation);

            player.MoveTo(Location.Direction.South);

            Assert.AreEqual(World.LocationByID(World.LOCATION_ID_HOME), player.CurrentLocation);

            player.MoveTo(Location.Direction.North);

            Assert.AreEqual(World.LocationByID(World.LOCATION_ID_TOWN_SQUARE), player.CurrentLocation);

            player.MoveTo(Location.Direction.East);

            Assert.AreEqual(World.LocationByID(World.LOCATION_ID_GUARD_POST), player.CurrentLocation);
        }

        [TestMethod]
        public void UseWeapon_EveryoneLives()
        {
            Player player = Player.CreateDefaultPlayer();
            player.AddExperiencePoints(670);
            player.Gold = 500;
            player.AddItemToInventory(World.ItemByID(World.ITEM_ID_ADVENTURER_PASS));
            player.GiveQuest(World.QuestByID(World.QUEST_ID_CLEAR_ALCHEMIST_GARDEN));
            player.EquipedWeapon = (Weapon)World.ItemByID(World.ITEM_ID_RUSTY_SWORD);
            player.MoveTo(World.LocationByID(World.LOCATION_ID_SPIDER_FIELD));

            player.UseWeapon(player.EquipedWeapon);

            Assert.AreEqual(60, player.CurrentHitPoints, 10);
            Monster monster = (Monster)player.GetPrivateField("_currentMonster");
            Assert.AreEqual(8, monster.CurrentHitPoints, 3);
        }

        [TestMethod]
        public void UseWeapon_MonsterDies_RewardsGiven()
        {
            Player player = Player.CreateDefaultPlayer();
            player.AddExperiencePoints(670);
            player.Gold = 500;
            player.AddItemToInventory(World.ItemByID(World.ITEM_ID_ADVENTURER_PASS));
            player.AddItemToInventory(World.ItemByID(World.ITEM_ID_CLUB));
            player.GiveQuest(World.QuestByID(World.QUEST_ID_CLEAR_ALCHEMIST_GARDEN));
            player.EquipedWeapon = (Weapon)World.ItemByID(World.ITEM_ID_CLUB);
            player.MoveTo(World.LocationByID(World.LOCATION_ID_ALCHEMISTS_GARDEN));

            Monster monster = (Monster)player.GetPrivateField("_currentMonster");
            player.UseWeapon(player.EquipedWeapon);

            Assert.AreEqual(670 + monster.RewardExperiencePoints, player.ExperiencePoints);
            Assert.AreEqual(500 + monster.RewardGold, player.Gold);
            var result = player.Inventory.Where(pi => monster.LootTable.Any(mlt => pi.Details.ID == mlt.Details.ID)).Count();
            Assert.AreNotSame(0, result);
        }
    }
}
