using System;
using System.Collections.Generic;
using System.ComponentModel;
using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngineTest
{
    [TestClass]
    public class LivingCreateTest
    {
        [TestMethod]
        public void CurrentHitPoints_EventRaised_Changed()
        {
            List<string> receivedEvents = new List<string>();
            LivingCreature lc = new LivingCreature(20, 50);
            
            lc.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents.Add(e.PropertyName);
            };

            lc.CurrentHitPoints += 12;
            Assert.AreEqual(1, receivedEvents.Count);
            Assert.AreEqual(nameof(LivingCreature.CurrentHitPoints), receivedEvents[0]);
        }

        [TestMethod]
        public void IsDead_True()
        {
            LivingCreature lc = new LivingCreature(-5, 10);

            Assert.IsTrue(lc.IsDead);
        }
    }
}
