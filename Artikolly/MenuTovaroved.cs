using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Artikolly
{
    public partial class MenuTovaroved : Form
    {
        public MenuTovaroved()
        {
            InitializeComponent();
        }
        // Открытая форма
        private Form ActiveForm = null;
        private void OpenChildForm(Form ChildForm)
        {
            if (ActiveForm != null)
                ActiveForm.Close();

            ActiveForm = ChildForm;

            //Настройка
            ChildForm.TopLevel = false;
            ChildForm.FormBorderStyle = FormBorderStyle.None;
            ChildForm.Dock = DockStyle.Fill;

            panel3.Controls.Add(ChildForm);
            panel3.Tag = ChildForm;
            ChildForm.BringToFront();

            ChildForm.Show();
        }

        private void MenuTovaroved_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Product());
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            Authorization authForm = new Authorization();

            authForm.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Otchet otchet = new Otchet();

            otchet.Show();
            this.Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Nakladnaya nak = new Nakladnaya();

            nak.Show();
            this.Hide();
        }

       

        private void button3_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Suppliers());
        }
    }
}
