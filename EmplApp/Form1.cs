using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Npgsql;



namespace EmplApp
{
    public partial class Form1 : Form
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=lab_user;Database=employee";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                dataGridView1.DataSource = LoadDevelopers();
                dataGridView1.Columns["Id"].DisplayIndex = 0;
                dataGridView1.Columns["Name"].DisplayIndex = 1;
                dataGridView1.Columns["Salary"].DisplayIndex = 2;
                dataGridView1.Columns["Experience"].DisplayIndex = 3;
                dataGridView1.Columns["CountProjects"].DisplayIndex = 4;
                dataGridView1.Columns["Qualification"].DisplayIndex = 5;
            }
            else if (radioButton1.Checked)
            {
                dataGridView1.DataSource = LoadManagers();
                dataGridView1.Columns["Id"].DisplayIndex = 0;
                dataGridView1.Columns["Name"].DisplayIndex = 1;
                dataGridView1.Columns["Salary"].DisplayIndex = 2;
                dataGridView1.Columns["Experience"].DisplayIndex = 3;
                dataGridView1.Columns["MainTasks"].DisplayIndex = 4;
                dataGridView1.Columns["Subordinates"].DisplayIndex = 5;
            }
        }

        private List<Developer> LoadDevelopers()
        {
            var developers = new List<Developer>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
            SELECT d.id, w.name, w.salary, w.experience, d.count_projects, d.qualification
            FROM developer d
            JOIN worker w ON d.id_worker = w.id";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            developers.Add(new Developer
                            {
                                Id = reader.GetInt64(0),
                                Name = reader.GetString(1),
                                Salary = reader.GetDecimal(2),
                                Experience = reader.GetInt32(3),
                                CountProjects = reader.GetInt32(4),
                                Qualification = reader.GetInt32(5)
                            });
                        }
                    }
                    return developers;
                }
            }
        }
       

        private List<Manager> LoadManagers()
        {
            var managers = new List<Manager>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
            SELECT m.id, w.name, w.salary, w.experience, m.main_tasks, m.subordinates
            FROM manager m
            JOIN worker w ON m.id_worker = w.id";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            managers.Add(new Manager
                            {
                                Id = reader.GetInt64(0),
                                Name = reader.GetString(1),
                                Salary = reader.GetDecimal(2),
                                Experience = reader.GetInt32(3),
                                MainTasks = reader.GetString(4),
                                Subordinates = reader.GetInt32(5)
                            });
                        }
                    }
                    return managers;
                }
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите строку для удаления.");
                return;
            }

            long entityId = Convert.ToInt64(dataGridView1.SelectedRows[0].Cells[2].Value);
            long workerId = 0;

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                if (radioButton2.Checked) // Разработчик
                {
                    // Найти id_worker по developer.id
                    using (var cmdGet = new NpgsqlCommand("SELECT id_worker FROM developer WHERE id = @id", conn))
                    {
                        cmdGet.Parameters.AddWithValue("id", entityId);
                        var result = cmdGet.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("Разработчик не найден.");
                            return;
                        }
                        workerId = (long)result;
                    }

                    // Удалить из developer
                    using (var cmdDelDev = new NpgsqlCommand("DELETE FROM developer WHERE id = @id", conn))
                    {
                        cmdDelDev.Parameters.AddWithValue("id", entityId);
                        cmdDelDev.ExecuteNonQuery();
                    }
                }
                else if (radioButton1.Checked) // Менеджер
                {
                    // Найти id_worker по manager.id
                    using (var cmdGet = new NpgsqlCommand("SELECT id_worker FROM manager WHERE id = @id", conn))
                    {
                        cmdGet.Parameters.AddWithValue("id", entityId);
                        var result = cmdGet.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("Менеджер не найден.");
                            return;
                        }
                        workerId = (long)result;
                    }

                    // Удалить из manager
                    using (var cmdDelMan = new NpgsqlCommand("DELETE FROM manager WHERE id = @id", conn))
                    {
                        cmdDelMan.Parameters.AddWithValue("id", entityId);
                        cmdDelMan.ExecuteNonQuery();
                    }
                }

                // Удалить из worker
                using (var cmdDelWorker = new NpgsqlCommand("DELETE FROM worker WHERE id = @wid", conn))
                {
                    cmdDelWorker.Parameters.AddWithValue("wid", workerId);
                    cmdDelWorker.ExecuteNonQuery();
                }

                MessageBox.Show("Удаление успешно.");
                button1_Click_1(null, null); // Обновить список
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var form = new Form2(radioButton2.Checked);
            form.ShowDialog();
            button1_Click_1(null, null);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            long entityId = Convert.ToInt64(dataGridView1.SelectedRows[0].Cells[2].Value);

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                if (radioButton2.Checked)
                {
                    string sql = @"
                SELECT d.count_projects, d.qualification, w.name
                FROM developer d
                JOIN worker w ON d.id_worker = w.id
                WHERE d.id = @id";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("id", entityId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var dev = new Developer
                                {
                                    Name = reader.GetString(2),
                                    CountProjects = reader.GetInt64(0),
                                    Qualification = reader.GetInt64(1)
                                };

                                double workload = dev.Workload();
                                double time = dev.EstimatedTime();

                                MessageBox.Show(
                                    $"Разработчик: {dev.Name}\n" +
                                    $"Нагруженность: {workload:F2}\n" +
                                    $"Оценка времени выполнения: {time:F1} дней");
                            }
                        }
                    }
                }
                else if (radioButton1.Checked)
                {
                    string sql = @"
                SELECT m.main_tasks, m.subordinates, w.name
                FROM manager m
                JOIN worker w ON m.id_worker = w.id
                WHERE m.id = @id";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("id", entityId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var man = new Manager
                                {
                                    Name = reader.GetString(2),
                                    MainTasks = reader.GetString(0),
                                    Subordinates = reader.GetInt64(1)
                                };

                                string canAssign = man.CanAssignMoreSubordinates()
                                    ? "Да, можно назначить ещё подчинённого."
                                    : "Нет, достигнут лимит.";

                                MessageBox.Show(
                                    $"Менеджер: {man.Name}\n" +
                                    $"Основные задачи: {man.MainTasks}\n" +
                                    $"Подчинённые: {man.Subordinates}\n" +
                                    $"{canAssign}");
                            }
                        }
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сотрудника для редактирования.");
                return;
            }

            long entityId = Convert.ToInt64(dataGridView1.SelectedRows[0].Cells[2].Value);

            bool isDeveloper = radioButton2.Checked; // true — разработчик, false — менеджер

            var form2 = new Form2(isDeveloper, entityId);
            form2.ShowDialog();

        }
    }
}
