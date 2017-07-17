using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class PlayerDataMapper
    {
        private static readonly string _connectionString =
            "Data Source=(local);Intial Catalog=SuperAdventure;Integrated Security=True";

        public static Player CreateFromDatabase()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    Player player;

                    using (SqlCommand savedGameCommaned = connection.CreateCommand())
                    {
                        savedGameCommaned.CommandType = CommandType.Text;

                        savedGameCommaned.CommandText = "SELECT TOP 1 * FROM SavedGame";

                        SqlDataReader reader = savedGameCommaned.ExecuteReader();

                        if (!reader.HasRows)
                            return null;

                        reader.Read();

                        int currentHitPoints = (int)reader["CurrentHitPoints"];
                        int maximumHitPoints = (int)reader["MaximumHitPoints"];
                        int gold = (int)reader["Gold"];
                        int experiencePoints = (int)reader["ExperiencePoints"];
                        int currentLocationID = (int)reader["CurrentLocationID"];

                        player = Player.CreatePlayerFromDatabase(currentHitPoints, maximumHitPoints, gold, experiencePoints, currentLocationID);
                    }

                    using (SqlCommand questCommand = connection.CreateCommand())
                    {
                        questCommand.CommandType = CommandType.Text;
                        questCommand.CommandText = "SELECT * FROM Quest";

                        SqlDataReader reader = questCommand.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                int questID = (int)reader["QuestID"];
                                bool isCompleted = (bool)reader["IsCompleted"];

                                PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(questID));
                                playerQuest.IsCompleted = isCompleted;

                                player.Quests.Add(playerQuest);
                            }
                        }
                    }

                    using (SqlCommand inventoryCommand = connection.CreateCommand())
                    {
                        inventoryCommand.CommandType = CommandType.Text;
                        inventoryCommand.CommandText = "SELECT * FROM Inventory";

                        SqlDataReader reader = inventoryCommand.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                int inventoryItemID = (int)reader["InventoryItemID"];
                                int quantity = (int)reader["Quantity"];

                                player.AddItemToInventory(World.ItemByID(inventoryItemID), quantity);
                            }
                        }
                    }

                    return player;
                }
            }
            catch
            {
                //TODO: Implement specific exception handling in the future
            }

            return null;
        }

        public static void SaveToDatabase(Player player)
        {
            try
            {
                //TODO: Implement SaveToDatabase
            }
            catch
            {
                //TODO: Implement specific exception handling in the future
            }
        }
    }
}
