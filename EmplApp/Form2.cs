using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmplApp
{
    public partial class Form2 : Form
    {
        private readonly bool isDeveloper;
        private readonly long? employeeId; // null — добавление, иначе — редактирование
        private string connectionString = "Host=localhost;Username=postgres;Password=lab_user;Database=employee";

        public Form2(bool isDeveloper, long? employeeId = null)
        {
            InitializeComponent();

            this.isDeveloper = isDeveloper;
            this.employeeId = employeeId;

            label1.Text = isDeveloper ? "Добавление / Редактирование Разработчика" : "Добавление / Редактирование Менеджера";

            if (isDeveloper)
            {
                label6.Text = "Кол-во проектов";
                label7.Text = "Квалификация";
            }
            else
            {
                label6.Text = "Основные задачи";
                label7.Text = "Кол-во подчинённых";
            }

            if (employeeId.HasValue)
            {
                LoadEmployeeData(employeeId.Value);
            }
        }

        private void LoadEmployeeData(long id)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                if (isDeveloper)
                {
                    var cmd = new NpgsqlCommand("SELECT w.name, w.salary, w.experience, d.count_projects, d.qualification FROM developer d JOIN worker w ON d.id_worker = w.id WHERE d.id = @id", conn);
                    cmd.Parameters.AddWithValue("id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            textBox1.Text = reader.GetString(0);
                            textBox2.Text = reader.GetInt64(1).ToString();
                            textBox3.Text = reader.GetInt64(2).ToString();
                            
                            textBox5.Text = reader.GetInt64(4).ToString();
                            textBox6.Text = reader.GetInt64(5).ToString();
                        }
                        else
                        {
                            MessageBox.Show("Разработчик не найден.");
                            this.Close();
                        }
                    }
                }
                else
                {
                    var cmd = new NpgsqlCommand("SELECT w.name, w.salary, w.experience, m.main_tasks, m.subordinates FROM manager m JOIN worker w ON m.id_worker = w.id WHERE m.id = @id", conn);
                    cmd.Parameters.AddWithValue("id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            textBox1.Text = reader.GetString(0);
                            textBox2.Text = reader.GetInt64(1).ToString();
                            textBox3.Text = reader.GetInt64(2).ToString();
                            textBox5.Text = reader.GetString(3);
                            textBox6.Text = reader.GetInt64(4).ToString();
                        }
                        else
                        {
                            MessageBox.Show("Менеджер не найден.");
                            this.Close();
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Сбор данных из полей
            string name = textBox1.Text.Trim();
            string salaryText = textBox2.Text.Trim();
            string experienceText = textBox3.Text.Trim();
           
            string spec1 = textBox5.Text.Trim();
            string spec2 = textBox6.Text.Trim();
            // Валидация имени
            if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[А-Яа-яA-Za-z\s]+$"))
            {
                MessageBox.Show("Имя должно содержать только буквы и пробелы.");
                return;
            }

            if (!long.TryParse(salaryText, out long salary) ||
                !long.TryParse(experienceText, out long experience) )
            {
                MessageBox.Show("Проверьте числовые поля (зарплата, опыт, отдел).");
                return;
            }

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                if (isDeveloper)
                {
                    if (!long.TryParse(spec1, out long countProjects) ||
                        !long.TryParse(spec2, out long qualification))
                    {
                        MessageBox.Show("Поля для разработчика должны быть числами (Кол-во проектов, Квалификация).");
                        return;
                    }

                    if (employeeId.HasValue)
                    {
                        // Обновление

                        // Сначала обновляем worker
                        var cmdWorker = new NpgsqlCommand(
                            @"UPDATE worker SET name = @name, salary = @salary, experience = @experience, departmentid = @departmentid 
                              WHERE id = (SELECT id_worker FROM developer WHERE id = @id)", conn);
                        cmdWorker.Parameters.AddWithValue("name", name);
                        cmdWorker.Parameters.AddWithValue("salary", salary);
                        cmdWorker.Parameters.AddWithValue("experience", experience);
                        
                        cmdWorker.Parameters.AddWithValue("id", employeeId.Value);
                        cmdWorker.ExecuteNonQuery();

                        // Затем обновляем developer
                        var cmdDev = new NpgsqlCommand(
                            @"UPDATE developer SET count_projects = @countProjects, qualification = @qualification 
                              WHERE id = @id", conn);
                        cmdDev.Parameters.AddWithValue("countProjects", countProjects);
                        cmdDev.Parameters.AddWithValue("qualification", qualification);
                        cmdDev.Parameters.AddWithValue("id", employeeId.Value);
                        cmdDev.ExecuteNonQuery();
                    }
                    else
                    {
                        // Вставка нового

                        // Вставляем в worker
                        var cmdWorker = new NpgsqlCommand(
                            @"INSERT INTO worker (name, salary, experience, departmentid) VALUES (@name, @salary, @experience, @departmentid) RETURNING id", conn);
                        cmdWorker.Parameters.AddWithValue("name", name);
                        cmdWorker.Parameters.AddWithValue("salary", salary);
                        cmdWorker.Parameters.AddWithValue("experience", experience);
                        

                        long newWorkerId = (long)cmdWorker.ExecuteScalar();

                        // Вставляем в developer
                        var cmdDev = new NpgsqlCommand(
                            @"INSERT INTO developer (id_worker, count_projects, qualification) VALUES (@id_worker, @countProjects, @qualification)", conn);
                        cmdDev.Parameters.AddWithValue("id_worker", newWorkerId);
                        cmdDev.Parameters.AddWithValue("countProjects", countProjects);
                        cmdDev.Parameters.AddWithValue("qualification", qualification);
                        cmdDev.ExecuteNonQuery();
                    }
                }
                else
                {
                    if (!long.TryParse(spec2, out long subordinates))
                    {
                        MessageBox.Show("Поле 'Кол-во подчинённых' должно быть числом.");
                        return;
                    }

                    if (employeeId.HasValue)
                    {
                        // Обновление

                        var cmdWorker = new NpgsqlCommand(
                            @"UPDATE worker SET name = @name, salary = @salary, experience = @experience 
                              WHERE id = (SELECT id_worker FROM manager WHERE id = @id)", conn);
                        cmdWorker.Parameters.AddWithValue("name", name);
                        cmdWorker.Parameters.AddWithValue("salary", salary);
                        cmdWorker.Parameters.AddWithValue("experience", experience);
                        
                        cmdWorker.Parameters.AddWithValue("id", employeeId.Value);
                        cmdWorker.ExecuteNonQuery();

                        var cmdMan = new NpgsqlCommand(
                            @"UPDATE manager SET main_tasks = @mainTasks, subordinates = @subordinates WHERE id = @id", conn);
                        cmdMan.Parameters.AddWithValue("mainTasks", spec1);
                        cmdMan.Parameters.AddWithValue("subordinates", subordinates);
                        cmdMan.Parameters.AddWithValue("id", employeeId.Value);
                        cmdMan.ExecuteNonQuery();
                    }
                    else
                    {
                        // Вставка нового

                        var cmdWorker = new NpgsqlCommand(
                            @"INSERT INTO worker (name, salary, experience, departmentid) VALUES (@name, @salary, @experience, @departmentid) RETURNING id", conn);
                        cmdWorker.Parameters.AddWithValue("name", name);
                        cmdWorker.Parameters.AddWithValue("salary", salary);
                        cmdWorker.Parameters.AddWithValue("experience", experience);
                        

                        long newWorkerId = (long)cmdWorker.ExecuteScalar();

                        var cmdMan = new NpgsqlCommand(
                            @"INSERT INTO manager (id_worker, main_tasks, subordinates) VALUES (@id_worker, @mainTasks, @subordinates)", conn);
                        cmdMan.Parameters.AddWithValue("id_worker", newWorkerId);
                        cmdMan.Parameters.AddWithValue("mainTasks", spec1);
                        cmdMan.Parameters.AddWithValue("subordinates", subordinates);
                        cmdMan.ExecuteNonQuery();
                    }
                }
            }

            MessageBox.Show("Сотрудник успешно сохранён.");
            this.Close();
        }
    }
}