namespace Engine
{
    public class Location
    {
        public int ID { get; set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Item ItemRequiredToEnter { get; set; }
        public Quest QuestAvailableHere { get; set; }
        public Monster MonsterLivingHere { get; set; }
        public Location LocationToNorth { get; set; }
        public Location LocationToEast { get; set; }
        public Location LocationToSouth { get; set; }
        public Location LocationToWest { get; set; }
        public Vendor VendorWorkingHere { get; set; }

        public bool RequiresItem => ItemRequiredToEnter != null;
        public bool HasQuest => QuestAvailableHere != null;
        public bool MonsterIsHere => MonsterLivingHere != null;
        public bool VendorIsHere => VendorWorkingHere != null;
        
        public enum Direction
        {
            North,
            East,
            South,
            West
        }

        public Location(int id, string name, string description,
            Item itemRequiredToEnter = null,
            Quest questAvailableHere = null,
            Monster monsterLivingHere = null)
        {
            ID = id;
            Name = name;
            Description = description;
            ItemRequiredToEnter = itemRequiredToEnter;
            QuestAvailableHere = questAvailableHere;
            MonsterLivingHere = monsterLivingHere;
        }
    }
}
