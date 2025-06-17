using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmplApp
{
    public abstract class Employee
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Salary { get; set; }
        public long Experience { get; set; }
        public long DepartmentId { get; set; }
        public long WorkerId { get; set; }

        public virtual void SaveToDatabase(NpgsqlConnection conn)
        {
            string sql = @"
                INSERT INTO worker (name, salary, experience, department_id)
                VALUES (@name, @salary, @experience, @deptid)
                RETURNING id;";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("name", Name);
                cmd.Parameters.AddWithValue("salary", Salary);
                cmd.Parameters.AddWithValue("experience", Experience);
                cmd.Parameters.AddWithValue("deptid", DepartmentId);
                WorkerId = (long)cmd.ExecuteScalar();
            }
        }
    }
}
