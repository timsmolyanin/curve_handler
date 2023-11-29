using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace ConfigCreater
{
    public partial class MainForm : Form
    {

        private Config _config = new Config();
        private Sender _sender = new Sender();
        private string _filePath = null;
        private bool _isChange;

        public MainForm()
        {
            InitializeComponent();

            comboBox1.Items.Add(new CurveFormat("mini-Volts", "Kelvin"));
            comboBox1.Items.Add(new CurveFormat("Volts", "Kelvin"));
            comboBox1.Items.Add(new CurveFormat("Ohms", "Kelvin"));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "340 files (*.340)|*.340";
            openFileDialog1.Filter = "340 files (*.340)|*.340";

            textBox1.Text = "Curve Name";
            textBox2.Text = "";
            textBox4.Text = "325.5";
            comboBox1.SelectedItem = comboBox1.Items[0];

            _isChange = false;
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _filePath = openFileDialog1.FileName;
                using (StreamReader reader = new StreamReader(new FileStream(_filePath, FileMode.Open)))
                {
                    dataGridView1.Rows.Clear();
                    _config.Load(reader.ReadToEnd(), ref dataGridView1);
                }
                UpdateChart();
                textBox1.Text = _config.SensorModel;
                textBox2.Text = _config.SerialNumber;
                textBox4.Text = _config.SetPoint;
                comboBox1.SelectedIndex = _config.NumberDataFormat - 1;
                _isChange = false;
            }
        }

        private void UpdateChart()
        {
            _isChange = true;

            try
            {
                chart1.Series[0].Points.Clear();
            }
            catch (Exception exeption)
            {
                Console.WriteLine(exeption);
            }

            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Ascending);

            for (int i = 0; i < dataGridView1.RowCount; i++)
            {

                if (dataGridView1.Rows[i].Cells[0].Value == null)
                {
                    continue;
                }

                if (dataGridView1.Rows[i].Cells[1].Value == null)
                {
                    continue;
                }

                double x = double.Parse(dataGridView1.Rows[i].Cells[1].Value.ToString().Replace(',', '.'));
                double y = double.Parse(dataGridView1.Rows[i].Cells[0].Value.ToString().Replace(',', '.'));

                chart1.Series[0].Points.AddXY(x, y);
            }
        }

        private bool Save()
        {
            if (_filePath == null)
            {
                if (saveFileDialog1.ShowDialog() != DialogResult.OK) return false;

                _filePath = saveFileDialog1.FileName;
            }
            using (StreamWriter writer = new StreamWriter(new FileStream(_filePath, FileMode.Create)))
            {
                writer.Write(_config.Save(dataGridView1));
            }

            _isChange = false;
            return true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isChange)
            {
                var state = MessageBox.Show("Сохранить?", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (state == DialogResult.Yes)
                {
                    e.Cancel = !Save();
                }
                else if (state == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void dataGridView1_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column.Index == 0)
            {
                if (double.TryParse(e.CellValue1?.ToString().Replace(',', '.'), out double cell1) && double.TryParse(e.CellValue2?.ToString().Replace(',', '.'), out double cell2))
                {
                    e.SortResult = cell1.CompareTo(cell2);
                    e.Handled = true;
                }
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            UpdateChart();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void saveUsButton_Click(object sender, EventArgs e)
        {
            _filePath = null;
            Save();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _isChange = true;

            CurveFormat curveFormat = comboBox1.SelectedItem as CurveFormat;

            chart1.ChartAreas[0].AxisY.Title = curveFormat.XName;
            chart1.ChartAreas[0].AxisX.Title = curveFormat.YName;

            dataGridView1.Columns[0].HeaderCell.Value = curveFormat.XName;
            dataGridView1.Columns[1].HeaderCell.Value = curveFormat.YName;

            _config.DataFormat = curveFormat;
            _config.NumberDataFormat = comboBox1.SelectedIndex + 1;
        }

        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            dataGridView1.Rows[e.RowIndex].ErrorText = "";

            if (dataGridView1.Rows[e.RowIndex].IsNewRow) return;

            if (string.IsNullOrEmpty(e.FormattedValue.ToString()))
            {
                e.Cancel = true;
                dataGridView1.Rows[e.RowIndex].ErrorText = "the value mustn`t be a empty";
            }
            if (!double.TryParse(e.FormattedValue.ToString().Replace(',', '.'), out double value))
            {
                e.Cancel = true;
                dataGridView1.Rows[e.RowIndex].ErrorText = "the value must be a double whit dot";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            _isChange = true;

            chart1.Titles[0].Text = textBox1.Text;
            _config.SensorModel = textBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            _isChange = true;

            _config.SerialNumber = textBox2.Text;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            _isChange = true;

            _config.SetPoint = textBox4.Text;
        }

        private void connectionButton_Click(object sender, EventArgs e)
        {
            _sender.Host = textBox3.Text;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var filePath = openFileDialog1.FileName;

                using (FileStream steam = new FileStream(filePath, FileMode.Open))
                {
                    var packages = filePath.Split('\\');
                    if (_sender.Send(steam, packages[packages.Length - 1]))
                    {
                        MessageBox.Show($"Успешно\nОтправлен по пути{_sender.RemoteDirectory + "/" + packages[packages.Length - 1]}", "Файл успешно отправлен", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка", "Файл не отправлен", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
        }
    }
}
