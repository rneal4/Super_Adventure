using System;
using System.Collections.Generic;
using System.ComponentModel;
using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngineTest
{
    [TestClass]
    public class InventoryItemTest
    {
        [TestMethod]
        public void Details_EventsRaised_DetailsChanged()
        {
            List<string> receivedEvents = new List<string>();
            InventoryItem inventoryItem = new InventoryItem(World.ItemByID(World.ITEM_ID_CLUB), 1);

            inventoryItem.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            inventoryItem.Details = World.ItemByID(World.ITEM_ID_ADVENTURER_PASS);
            Assert.AreEqual(1, receivedEvents.Count);
            Assert.AreEqual(nameof(InventoryItem.Details), receivedEvents[0]);
            Assert.AreEqual(World.ItemByID(World.ITEM_ID_ADVENTURER_PASS), inventoryItem.Details);
        }

        [TestMethod]
        public void Quantity_EventsRaised_QuantityChanged()
        {
            List<string> receivedEvents = new List<string>();
            InventoryItem inventoryItem = new InventoryItem(World.ItemByID(World.ITEM_ID_CLUB), 1);

            inventoryItem.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            inventoryItem.Quantity += 5;
            Assert.AreEqual(2, receivedEvents.Count);
            Assert.AreEqual(nameof(InventoryItem.Quantity), receivedEvents[0]);
            Assert.AreEqual(nameof(InventoryItem.Description), receivedEvents[1]);
            Assert.AreEqual(6, inventoryItem.Quantity);
            Assert.AreEqual(World.ItemByID(World.ITEM_ID_CLUB).NamePlural, inventoryItem.Details.NamePlural);
        }
    }
}
