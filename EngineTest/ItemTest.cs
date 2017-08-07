using System;
using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngineTest
{
    [TestClass]
    public class ItemTest
    {
        [TestMethod]
        public void IsSellable_False()
        {
            Item item = World.ItemByID(World.ITEM_ID_ADVENTURER_PASS);

            Assert.IsFalse(item.IsSellable);
        }
    }
}
