using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
namespace LOSStudio
{
    public partial class Form1 : Form
    {
        public string ProjectPath = "";
        public string ProjectLanguage = "ASM";
        public static Form1 instance = null;
        private static bool Generate = false;
        public Form1()
        {
            InitializeComponent();
            instance = this;
            if (!Generate)
            {
                FrontPage();
                Generate = true;
            }
        }
        public void open()
        {
            openProjectToolStripMenuItem_Click(null,null);
        }
        public void create()
        {
            newProjectToolStripMenuItem_Click(null,null);
        }
        public void FrontPage()
        {
            Front front = new Front();
            front.TopMost = true;
            front.Show();
            front.BringToFront();
        }
        public void press(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 9)
            {
                e.Handled = false;
            }
        }
        private void key_down(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                save();
            }
        }
        public void new_tab(string tabname, string filename)
        {
            RichTextBox box = new RichTextBox();
            box.AcceptsTab = true;
            box.KeyDown += new KeyEventHandler(key_down);
            box.KeyPress += new KeyPressEventHandler(press);
            box.Name = "text";
            box.Size = new Size(1920,1080);
            TabPage tab = new TabPage(tabname);
            tab.Tag = filename;
            tab.Controls.Add(box);
            tabControl1.TabPages.Add(tab);
            box.DoubleClick += new EventHandler(edit_item);
            box.Text = File.ReadAllText(filename);
        }
        public void edit_item(object sender, EventArgs e)
        {

        }
        private void RichTextBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                ProjectPath = dialog.SelectedPath;
                foreach (string f in Directory.GetFiles(ProjectPath))
                {
                    new_tab(f.Split('\\')[f.Split('\\').Length - 1], f);
                }
            }
        }

        private void buildProjectWithBatchFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                System.Diagnostics.Process.Start(dialog.FileName);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                new_tab(dialog.FileName.Split('\\')[dialog.FileName.Split('\\').Length - 1], dialog.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
        }
        private void save()
        {
            RichTextBox box = (RichTextBox)tabControl1.SelectedTab.Controls["text"];
            string path = (string)tabControl1.SelectedTab.Tag;
            List<string> lines = new List<string>();
            File.WriteAllLines(path, box.Lines);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            DialogResult result = save.ShowDialog();
            if (result != DialogResult.OK)
                return;
            string path = save.FileName;
            RichTextBox box = (RichTextBox)tabControl1.SelectedTab.Controls["text"];
            File.WriteAllLines(path, box.Lines);
        }
        private void AddSyntaxHighlighting(RichTextBox box)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.Remove(tabControl1.SelectedTab);
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string value = ShowDialog("","Project Path:");
            
            Directory.CreateDirectory(value);
            AddConfigurationFiles(value);
            FileStream stream = File.Create(value + "\\kernel.asm");
            string data = @"
            BITS 16

start:
	mov ax, 07C0h		; Set up 4K stack space after this bootloader
	add ax, 288		; (4096 + 512) / 16 bytes per paragraph
	mov ss, ax
	mov sp, 4096

	mov ax, 07C0h		; Set data segment to where we're loaded
	mov ds, ax


	mov si, text_string	; Put string position into SI
	call print_string	; Call our string-printing routine

	jmp $			; Jump here - infinite loop


	text_string db 'This is my cool new OS!', 0


print_string:			; Routine: output string in SI to screen
	mov ah, 0Eh		; int 10h 'print char' function

.repeat:
	lodsb			; Get character from string
	cmp al, 0
	je .done		; If char is zero, end of string
	int 10h			; Otherwise, print it
	jmp .repeat

.done:
	ret


	times 510-($-$$) db 0	; Pad remainder of boot sector with 0s
	dw 0xAA55		; The standard PC boot signature";
            stream.Close();
            File.WriteAllText(value + "\\kernel.asm", data);

            ProjectPath = value;
            foreach (string f in Directory.GetFiles(ProjectPath))
            {
                new_tab(f.Split('\\')[f.Split('\\').Length - 1], f);
            }
        }
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void buildProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("nasm.exe", $"-f bin -o {ProjectPath + '\\'}image.bin {ProjectPath + '\\'}kernel.asm");
            System.Diagnostics.Process.Start("cmd.exe", $"/c copy /b {ProjectPath + '\\'}image.bin {ProjectPath + '\\'}image.flp");
            System.Diagnostics.Process.Start("cmd.exe", $"/c md {ProjectPath + '\\'}images");
            System.Diagnostics.Process.Start("cmd.exe", $"/c copy {ProjectPath + '\\'}image.flp {ProjectPath + '\\'}images");
            MessageBox.Show("Build successed.");
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = ShowDialog("","File Name(Contains path):");
            var stream = File.Create(path);
            stream.Close();
            new_tab(path.Split('\\')[path.Split('\\').Length - 1], path);
        }
        private void AddConfigurationFiles(string path)
        {
            path = path + '\\';
            var stream = File.Create(path + ".losproj");
            stream.Close();
        }
    }
}
