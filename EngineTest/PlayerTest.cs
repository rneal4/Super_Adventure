using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Engine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EngineTest
{
    [TestClass]
    public class PlayerTest
    {
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
    }
}
