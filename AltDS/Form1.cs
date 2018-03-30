using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinet.Core.IO.Ntfs;


namespace AltDS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string path = null;
        System.IO.DirectoryInfo info;
        System.IO.DirectoryInfo[] dirs;
        System.IO.FileInfo[] files;
        string[] all;
        int c = 0;
        private void button1_Click(object sender, EventArgs e)
        {

            using (var dialog = new FolderBrowserDialog())
                if (dialog.ShowDialog() == DialogResult.OK)
                    path = dialog.SelectedPath+"\\";
            label2.Text = path;
            info = new System.IO.DirectoryInfo(path);
            dirs = info.GetDirectories();
            files = info.GetFiles();
            if (files.Count()+dirs.Count() == 0)
            {
             
                MessageBox.Show("В выбранной папке нет файлов!");
                button2.Enabled = false;
                button3.Enabled = false;
                checkBox2.Enabled = false;
            }
            else
            {
              
                button2.Enabled = true;
                button3.Enabled = true;
                checkBox2.Enabled = true;
                c = 0;
                all = new string[files.Count() + dirs.Count()];
                ///////////////////////Добавление файлов единый массив////////////////
                for (; c < files.Count(); c++)
                {
                    all[c] = Convert.ToString(files[c]);

                }
                ///////////////////////Добавление файлов единый массив////////////////
                ///////////////////////Добавление папок единый массив////////////////          
                    for (int b = 0; b < dirs.Count(); b++)
                    {
                        all[c] = Convert.ToString(dirs[b]);
                        c++;
                    }
                ///////////////////////Добавление папок единый массив////////////////
                if (checkBox2.Checked == true)
                {
                    numericUpDown1.Maximum = all.Count();
                }
                else
                {
                    numericUpDown1.Maximum = all.Count() - dirs.Count();
                }
                if (files.Count() == 0)
                {
                    checkBox2.Checked = true;
                    numericUpDown1.Value = dirs.Count();

                }
            }
        }
        List<string> Divider(string str, int blockLength)
        {
            ///////////////Очистка AltDS////////////
            for (int i = 0; i < files.Count(); i++)
            {
                FileInfo file = new FileInfo(path + Convert.ToString(files[i]));
                foreach (AlternateDataStreamInfo s in file.ListAlternateDataStreams())
                {
                    s.Delete();
                }             
            }
            for (int i = 0; i < dirs.Count(); i++)
            {
                DirectoryInfo dir = new DirectoryInfo(path + Convert.ToString(dirs[i]));
                foreach (AlternateDataStreamInfo s in dir.ListAlternateDataStreams())
                {
                    s.Delete();
                }
            }
    
            //////////////Очистка AltDS/////////////
            int count = 0;
            List<string> Blocks = new List<string>(str.Length / blockLength + 1);
 
                    for (int i = 0; i < str.Length; i += blockLength)
            {
                WriteAlternateStream(path + all[count] + ":" + count + ".txt", str.Substring(i, str.Length - i > blockLength ? blockLength : str.Length - i));
                count++;
            }
            return Blocks;
        }
        public void button2_Click(object sender, EventArgs e)
        {
            if((checkBox1.Checked==true)&&(textBox3.Text!=""))
            {
              //  string encrypted;
                string encryptedstring = RSA.StringCipher.Encrypt(textBox1.Text, textBox3.Text);
                // textBox3.Text = encryptedstring;
                textBox4.Text = encryptedstring;
                Divider(encryptedstring, (Convert.ToInt32(encryptedstring.Length) / Convert.ToInt32(numericUpDown1.Value)) + Convert.ToInt32(numericUpDown1.Value)-1);
            }
            else { 
            Divider(textBox1.Text, (Convert.ToInt32(textBox1.Text.Length) / Convert.ToInt32(numericUpDown1.Value)) + Convert.ToInt32(numericUpDown1.Value)-1);
            }
            MessageBox.Show("Записано успешно!");
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public static void WriteAlternateStream(string path, string text)
        {
            const int GENERIC_WRITE = 1073741824;
            const int FILE_SHARE_DELETE = 4;
            const int FILE_SHARE_WRITE = 2;
            const int FILE_SHARE_READ = 1;
            const int OPEN_ALWAYS = 4;

            var stream = CreateFileW(path, GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_ALWAYS, 0, IntPtr.Zero);
            using (var fs = new FileStream(stream, FileAccess.Write))
            using (var sw = new StreamWriter(fs))
                sw.Write(text);
        }

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW")]
        public static extern System.IntPtr CreateFileW(
            [InAttribute()] [MarshalAsAttribute(UnmanagedType.LPWStr)] string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            [InAttribute()] IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            [InAttribute()] IntPtr hTemplateFile
        );

        private void button3_Click(object sender, EventArgs e)
        {
            string encrypttext="";
            listBox1.Items.Clear();
            textBox2.Clear();
            
            for (int i = 0; i < files.Count(); i++)
            {
                FileInfo file = new FileInfo(path + Convert.ToString(files[i]));
                foreach (AlternateDataStreamInfo s in file.ListAlternateDataStreams())
                {
                    AlternateDataStreamInfo r = file.GetAlternateDataStream(s.Name,FileMode.Open);
                    using (TextReader reader = r.OpenText())
                    {                      
                           encrypttext += reader.ReadToEnd();                   
                    }        
                        listBox1.Items.Add(s.Name + " " + s.Size+" B "+ s.StreamType);
                }
            }
            for (int i = 0; i < dirs.Count(); i++)
            {
                DirectoryInfo dir = new DirectoryInfo(path + Convert.ToString(dirs[i]));
                foreach (AlternateDataStreamInfo s in dir.ListAlternateDataStreams())
                {
                    AlternateDataStreamInfo r = dir.GetAlternateDataStream(s.Name, FileMode.Open);
                    using (TextReader reader = r.OpenText())
                    {
                        encrypttext += reader.ReadToEnd();
                    }
                    listBox1.Items.Add(s.Name + " " + s.Size + " B " + s.StreamType);
                
            }
            }
            if ((checkBox1.Checked == true) && (textBox3.Text != ""))
            {
                try
                {
                    textBox2.Text = RSA.StringCipher.Decrypt(encrypttext, textBox3.Text);
                    MessageBox.Show("Считано успешно!");
                }
                catch {
                    MessageBox.Show("Неверный пароль!");
                }
            }
            else
            {
                textBox2.Text = encrypttext;
                MessageBox.Show("Считано успешно!");
            }
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                textBox3.Enabled = true;
            }
            else
            {
                textBox3.Enabled = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {    
                    numericUpDown1.Maximum = all.Count();           
            }
            else
            {
                    numericUpDown1.Maximum = all.Count() - dirs.Count();  
            }
        }
    }
}
