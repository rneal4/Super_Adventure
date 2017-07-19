namespace Engine
{
    public class Item
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string NamePlural { get; set; }
        public int Price { get; set; }

        public bool IsSellable => Price != World.UNSELLABLE_ITEM_PRICE;

        public Item(int id, string name, string namePlural, int price)
        {
            ID = id;
            Name = name;
            NamePlural = namePlural;
            Price = price;
        }
    }
}
