using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Artikolly
{
    public partial class UsersADD : Form
    {
        public event EventHandler UserChanged;
        public UsersADD()
        {
            InitializeComponent();
            LoadRoles();
        }
     
        private void LoadRoles()
        {
            // Загрузка ролей из базы данных в comboBoxRole
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT RoleId, RoleName FROM role";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                try
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        comboBox1.DataSource = dt;
                        comboBox1.DisplayMember = "RoleName";
                        comboBox1.ValueMember = "RoleId";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки ролей: " + ex.Message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            administrator authForm = new administrator();

            authForm.Show();
            this.Hide();
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Проверяем длину текста
            if (textBox1.Text.Length >= 20)
            {
                e.Handled = true;
                return;
            }

            // Разрешаем пробел и тире
            if (e.KeyChar == ' ' || e.KeyChar == '-')
            {
                return;
            }

            // Разрешаем только русские буквы
            if (!IsRussianLetter(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Если текстовое поле пустое или курсор находится в начале, делаем букву заглавной
            // Но только если это буква, а не пробел или тире
            if ((textBox1.Text.Length == 0 || textBox1.SelectionStart == 0) && IsRussianLetter(e.KeyChar))
            {
                e.KeyChar = char.ToUpper(e.KeyChar);
            }
        }
        private bool IsRussianLetter(char c)
        {
            // Русские буквы в диапазоне от 'А' до 'я' (включая Ё и ё)
            return (c >= 'А' && c <= 'я') || c == 'Ё' || c == 'ё';
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Проверяем длину текста
            if (textBox2.Text.Length >= 20)
            {
                e.Handled = true;
                return;
            }

            // Разрешаем пробел и тире
            if (e.KeyChar == ' ' || e.KeyChar == '-')
            {
                return;
            }

            // Разрешаем только русские буквы
            if (!IsRussianLetter(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Если текстовое поле пустое или курсор находится в начале, делаем букву заглавной
            // Но только если это буква, а не пробел или тире
            if ((textBox2.Text.Length == 0 || textBox2.SelectionStart == 0) && IsRussianLetter(e.KeyChar))
            {
                e.KeyChar = char.ToUpper(e.KeyChar);
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Проверяем длину текста
            if (textBox3.Text.Length >= 20)
            {
                e.Handled = true;
                return;
            }

            // Разрешаем пробел и тире
            if (e.KeyChar == ' ' || e.KeyChar == '-')
            {
                return;
            }

            // Разрешаем только русские буквы
            if (!IsRussianLetter(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Если текстовое поле пустое или курсор находится в начале, делаем букву заглавной
            // Но только если это буква, а не пробел или тире
            if ((textBox3.Text.Length == 0 || textBox3.SelectionStart == 0) && IsRussianLetter(e.KeyChar))
            {
                e.KeyChar = char.ToUpper(e.KeyChar);
            }
        }

       

        

        private void maskedTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Разрешаем только цифры
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Проверяем максимальную длину (11 цифр для российского номера)
            string digitsOnly = new string(maskedTextBox2.Text.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length >= 11 && maskedTextBox2.SelectedText.Length == 0)
            {
                e.Handled = true;
            }
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Разрешаем английские буквы
            if (char.IsLetter(e.KeyChar) && IsEnglishLetter(e.KeyChar))
            {
                return;
            }

            // Разрешаем цифры
            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            // Разрешаем специальные символы для логина
            // Обычно разрешают: подчеркивание _, точку ., дефис -, @
            if (e.KeyChar == '_' || e.KeyChar == '.' || e.KeyChar == '-' || e.KeyChar == '@')
            {
                return;
            }

            // Если символ не прошел ни одну проверку - запрещаем ввод
            e.Handled = true;
        }

        // Метод для проверки английских букв
        private bool IsEnglishLetter(char c)
        {
            // Английские буквы в диапазонах a-z и A-Z
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Разрешаем английские буквы
            if (char.IsLetter(e.KeyChar) && IsEnglishLetter(e.KeyChar))
            {
                return;
            }

            // Разрешаем цифры
            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            // Разрешаем специальные символы для логина
            // Обычно разрешают: подчеркивание _, точку ., дефис -, @
            if (e.KeyChar == '_' || e.KeyChar == '.' || e.KeyChar == '-' || e.KeyChar == '@')
            {
                return;
            }

            // Если символ не прошел ни одну проверку - запрещаем ввод
            e.Handled = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text) ||
                string.IsNullOrEmpty(maskedTextBox2.Text) || string.IsNullOrEmpty(textBox5.Text) ||
                string.IsNullOrEmpty(textBox6.Text) || comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Необходимо заполнить все обязательные поля!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка даты рождения
            DateTime dateOfBirth = dateTimePicker1.Value;
            if (dateOfBirth > DateTime.Today)
            {
                MessageBox.Show("Дата рождения не может быть больше текущей даты!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //if (dateOfBirth < new DateTime(2011, 1, 1))
            //{
            //    MessageBox.Show("Дата рождения не может быть раньше 01.01.2011!", "Ошибка",
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            string surname = textBox1.Text.Trim();
            string name = textBox2.Text.Trim();
            string patronymic = textBox3.Text.Trim();
            string phone = maskedTextBox2.Text.Trim();
            string login = textBox5.Text.Trim();
            string password = textBox6.Text.Trim();
            int roleId = Convert.ToInt32(comboBox1.SelectedValue);

            // Проверка на дублирование по логину
            if (IsDuplicateUser(login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Хэшируем пароль
            string hashedPassword = HashPassword(password);

            // Добавляем пользователя с хэшированным паролем
            if (AddUser(surname, name, patronymic, dateOfBirth, phone, login, hashedPassword, roleId))
            {
                MessageBox.Show("Пользователь успешно добавлен!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields();
                UserChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении пользователя.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }


        private bool IsDuplicateUser(string login)
        {
            bool isDuplicate = false;
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM user WHERE Login = @login";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@login", login);
                try
                {
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count > 0)
                        isDuplicate = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при проверке дублирования: " + ex.Message);
                }
            }
            return isDuplicate;
        }

        private bool AddUser(string surname, string name, string patronymic, DateTime dob, string phone, string login, string password, int roleId)
        {
            bool success = false;
            using (var conn = DatabaseHelper.GetConnection())
            {
                string insertQuery = "INSERT INTO user (UserSurname, UserName, UserPatronomic, DateOfBirth, Phone, Login, Password, Role) " +
                                     "VALUES (@surname, @name, @patronymic, @dob, @phone, @login, @password, @role)";
                MySqlCommand cmd = new MySqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@surname", surname);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@patronymic", patronymic);
                cmd.Parameters.AddWithValue("@dob", dob);
                cmd.Parameters.AddWithValue("@phone", phone);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@role", roleId);
                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    success = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении: " + ex.Message);
                }
            }
            return success;
        }

        private void ClearFields()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            maskedTextBox2.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}

