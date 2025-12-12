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
    public partial class administrator : Form
    {
        public administrator()
        {
            InitializeComponent();
        }
        // Открытая форма
        private Form ActiveForm = null;

        //Метод открытия дочерних форм
        private void OpenChildForm(Form ChildForm)
        {
            if (ActiveForm != null)
                ActiveForm.Close();

            ActiveForm = ChildForm;

            //Настройка
            ChildForm.TopLevel = false;
            ChildForm.FormBorderStyle = FormBorderStyle.None;
            ChildForm.Dock = DockStyle.Fill;

            panel2.Controls.Add(ChildForm);
            panel2.Tag = ChildForm;
            ChildForm.BringToFront();

            ChildForm.Show();
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

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Users());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Spravochnik());
        }

        

        private void button3_Click(object sender, EventArgs e)
        {
            OpenChildForm(new Manufacture());
        }

        private void administrator_Load(object sender, EventArgs e)
        {

        }
    }
}
