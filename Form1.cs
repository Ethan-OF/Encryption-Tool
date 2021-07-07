using AES;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace ScreenBlock
{
    public partial class Form1 : Form
    {

        static IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        public Form1()
        {
            InitializeComponent();
        }

        public string[][] FileBreaker(string Input)
        {
            var paragraphs = Input.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string[][] results = new string[paragraphs.Length][];

            for (int i = 0; i < results.Length; i++)
            {
                results[i] = paragraphs[i].Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return results;
        }


        public void Delay(int Delay)
        {
            Stopwatch SW2 = new Stopwatch();
            SW2.Start();
            do
            {
            }
            while (SW2.ElapsedTicks <= Delay);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!button1.Visible)
            {
                Debug.WriteLine("Tried to Execute during RunTime");
                return;
            }
            else
            {
                button1.Visible = false;
                DataBox.ReadOnly = true;
            }
            var timer = new DispatcherTimer();

            //Aes myAes = Aes.Create();
            var Key = new byte[]
            {
                132, 194, 79, 174, 92, 68, 165, 251, 159, 129, 92, 221, 224, 240, 43, 167, 33, 209, 241, 71, 102, 69, 122, 14, 171, 74, 162, 160, 206, 223, 222, 251
            };
            var IV = new byte[]
            {
                128, 231, 178, 191, 64, 241, 37, 247, 240, 231, 146, 78, 10, 101, 11, 231
            };
            //myAes.IV.ToList().ForEach(i => Console.WriteLine(i.ToString()));
            //label1.Text = myAes.Key[].ToString();

            if (EncryptTool.Properties.Settings.Default.Key != "")
            {
                string[] s = EncryptTool.Properties.Settings.Default.Key.Split(' ');
                byte[] data = new byte[s.Length];
                for (int i = 0; i < data.Length; i++)
                    data[i] = byte.Parse(s[i]);
                Key = data;
            }
            if (Mode.Checked)
            {
                if (DataBox.Text.Length == 0)
                {
                    button1.Visible = true;
                    DataBox.ReadOnly = false;
                    return;
                }

                Thread thread = new Thread(() => EncryptThread(Key, IV));
                thread.Start();
            }
            else
            {
                if (!DataBox.Text.Any(Char.IsDigit))
                {
                    button1.Visible = true;
                    DataBox.ReadOnly = false;
                    return;
                }

                Thread thread = new Thread(() => DecryptThread(Key, IV));
                thread.Start();

            }
        }

        public void EncryptThread(byte[] Key, byte[] IV)
        {

            //byte[] encrypted = ImprovedAES.EncryptStringToBytes_Aes(DataBox.Text, Key, IV);
            //IEnumerable<string> test = ChunksUpto(String.Join(" ", encrypted), 50);
            //List<string> tes = test.ToList();
            //DataBox.Text = "";

            //foreach (string item in tes)
            //{
            //    DataBox.AppendText(item);
            //    Delay(250000);
            //}
            //button1.Visible = true;




            //OR
            string DBT = "";
            Invoke(new Action(() =>
            {
                DBT = DataBox.Text;
            }));
            IEnumerable<string> test = ChunksUpto(String.Join(" ", DBT), 50);
            List<string> tes = test.ToList();
            Invoke(new Action(() =>
            {
                DataBox.Text = "";
            }));
            foreach (string item in tes)
            {
                byte[] EnChunk = ImprovedAES.EncryptStringToBytes_Aes(item, Key, IV);
                Invoke(new Action(() =>
                {
                    DataBox.AppendText(String.Join(" ", EnChunk) + ":");
                }));
            }
            Invoke(new Action(() =>
            {
                button1.Visible = true;
                DataBox.ReadOnly = false;
            }));
        }

        public void DecryptThread(byte[] Key, byte[] IV)
        {
            string DBT = "";
            Invoke(new Action(() =>
            {
                DBT = DataBox.Text;
            }));

            string[][] Main = FileBreaker(DBT);
            Invoke(new Action(() =>
            {
                DataBox.Text = "";
            }));

            //For each paragraph.
            foreach (var Str in Main)
            {
                var T = string.Join(" ", Str);
                T = T.Replace("  ", ". ");
                System.Console.WriteLine(T);
                //T = the whole paragraph!
                var ChunkSplit = T.Split(':');
                foreach (string item in ChunkSplit)
                {
                    if (item == "")
                    {
                        break;
                    }
                    Console.WriteLine(item + " Ass1");
                    string[] s = item.Split(' ');
                    byte[] data = new byte[s.Length];
                    for (int i = 0; i < data.Length; i++)
                        try
                        {
                            data[i] = byte.Parse(s[i]);
                        }
                        catch
                        {
                            System.Console.WriteLine("REEE");
                        }
                    string decrypted = "";
                    //TAKE OUT OF TRY AND REMOVE CATCH IF CRASH ON ATTEMPT IS WANTED!!!!!
                    try
                    {
                        decrypted = ImprovedAES.DecryptStringFromBytes_Aes(data, Key, IV);
                    }
                    catch
                    {
                        Invoke(new Action(() =>
                        {
                            DataBox.Text = "";
                        }));
                        break;
                    }
                    Invoke(new Action(() =>
                    {
                        DataBox.AppendText(decrypted);
                        DataBox.Text = Regex.Replace(DataBox.Text, @"(^\p{Zs}*\r\n){2,}", "\r\n", RegexOptions.Multiline);
                    }));
                }
                Invoke(new Action(() =>
                {
                    DataBox.AppendText("\n");
                }));
                Invoke(new Action(() =>
                {
                    button1.Visible = true;
                    DataBox.ReadOnly = false;
                }));
            }


        }


        /// <summary>
        /// This is a test?.?
        /// </summary>
        public void Test()
        {
            Console.WriteLine("Test");
        }

        private void NewKey_Click(object sender, EventArgs e)
        {
            KeyBox.Visible = true;
            Aes myAes = Aes.Create();
            var Key = String.Join(" ", myAes.Key);
            EncryptTool.Properties.Settings.Default.Key = Key;
            EncryptTool.Properties.Settings.Default.Save();
            KeyBox.Text = Key;
        }

        private void DefaultKEY_Click(object sender, EventArgs e)
        {
            KeyBox.Visible = false;
            EncryptTool.Properties.Settings.Default.Key = "";
            EncryptTool.Properties.Settings.Default.Save();
            KeyBox.Text = "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (EncryptTool.Properties.Settings.Default.Key == "")
            {
                return;
            }
            else
            {
                KeyBox.Text = EncryptTool.Properties.Settings.Default.Key;
                KeyBox.Visible = true;
            }
        }

        private void KeyBox_TextChanged(object sender, EventArgs e)
        {
            EncryptTool.Properties.Settings.Default.Key = KeyBox.Text;
            EncryptTool.Properties.Settings.Default.Save();
        }
    }
}


