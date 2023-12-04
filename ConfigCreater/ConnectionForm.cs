using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ConfigCreater
{
    public partial class ConnectionForm : Form
    {
        private Sender _sender;
        public ConnectionForm(Sender sender)
        {
            InitializeComponent();

            _sender = sender;
        }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "340 files (*.340)|*.340";

            textBox1.Text = _sender.Host;

            label1.Select();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _sender.Host = textBox1.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var filePath = openFileDialog1.FileName;

                using (FileStream steam = new FileStream(filePath, FileMode.Open))
                {
                    var packages = filePath.Split('\\');
                    if (_sender.Send(steam, packages[packages.Length - 1]))
                    {
                        MessageBox.Show($"Файл '{packages[packages.Length - 1]}' отправлен на контроллер!{_sender.RemoteDirectory + "/" + packages[packages.Length - 1]}", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Файл не отправлен, проверьте соединение с контроллером", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _sender.Host = textBox1.Text;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            _sender.Host = textBox1.Text;
        }
    }
}
