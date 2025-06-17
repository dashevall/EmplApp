using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmplApp
{
    public class Manager: Employee
    {
        public string MainTasks { get; set; }
        public long Subordinates { get; set; }

        public void UpdateMainTasks(string newTasks)
        {
            MainTasks = newTasks;
        }

        public bool CanAssignMoreSubordinates()
        {
            // ограничение: до 10 подчинённых
            return Subordinates < 10;
        }

        public override void SaveToDatabase(NpgsqlConnection conn)
        {
            base.SaveToDatabase(conn);

            string sql = @"
                INSERT INTO manager (main_tasks, subordinates, id_worker)
                VALUES (@tasks, @subs, @wid)";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("tasks", MainTasks);
                cmd.Parameters.AddWithValue("subs", Subordinates);
                cmd.Parameters.AddWithValue("wid", WorkerId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}


