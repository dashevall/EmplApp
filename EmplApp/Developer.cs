using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmplApp
{
    public class Developer : Employee
    {
        public long CountProjects { get; set; }
        public long Qualification { get; set; }

        public double Workload()
        {
            return (double)CountProjects / Qualification;
        }

        public double EstimatedTime()
        {
            // среднее время на проект = 30 дней / квалификацию
            return CountProjects * (30.0 / Qualification);
        }

        public override void SaveToDatabase(NpgsqlConnection conn)
        {
            base.SaveToDatabase(conn);

            string sql = @"
                INSERT INTO developer (count_projects, qualification, id_worker)
                VALUES (@cp, @qual, @wid)";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("cp", CountProjects);
                cmd.Parameters.AddWithValue("qual", Qualification);
                cmd.Parameters.AddWithValue("wid", WorkerId);
                cmd.ExecuteNonQuery();
            }
        }
    }

 }

