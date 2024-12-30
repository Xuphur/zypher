using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Zipher
{
    public partial class MainForm : Form
    {
        private const string CorrectPassword = "zafar";

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private TabControl tabControlHome;
        private TabControl tabControlDialer;
        private ContextMenuStrip dropdownMenu;

        public MainForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            _hookID = SetHook(_proc); // Set up global hook
            //this.WindowState = FormWindowState.Maximized; // Make the form start maximized
            this.Load += MainForm_Load; // Attach form load event
            this.KeyDown += new KeyEventHandler(MainForm_KeyDown); // Add KeyDown event for intercepting copy/paste
            this.KeyPreview = true; // Ensure the form can intercept key presses
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnFormClosing(e);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            SetInitialLayout();

            // Initialize WebView2 instances without the previous default URLs
            //await InitializeWebView2(webViewHome, "https://www.example.com"); // Default for now, update via Admin Panel
            //await InitializeWebView2(webViewDialer, "https://www.dialer.crossnotch.com"); // Default for now, update via Admin Panel
        }

        private void SetInitialLayout()
        {
            // Set up TabControls
            tabControlHome = new TabControl { Dock = DockStyle.Fill };
            tabControlDialer = new TabControl { Dock = DockStyle.Fill };

            // Add tabs for Home (Example URLs for now)
            AddTabsToTabControl(tabControlHome, new string[]
            {
                "https://www.google.com",
                "https://www.bing.com",
                "https://www.example.com"
            });

            // Add tabs for Dialer (Example URLs for now)
            AddTabsToTabControl(tabControlDialer, new string[]
            {
                "https://www.yahoo.com",
                "https://www.duckduckgo.com",
                "https://www.wikipedia.org"
            });

            // Add Notes tab to the right (Dialer) panel
            AddNotesTab(tabControlDialer);

            // Add TabControls to panels
            panelHome.Controls.Add(tabControlHome);
            panelDialer.Controls.Add(tabControlDialer);

            // Dock panels
            panelHome.Dock = DockStyle.Left;
            panelHome.Width = this.ClientSize.Width / 2;
            panelDialer.Dock = DockStyle.Fill;

            panelHome.Visible = true;
            panelDialer.Visible = true;
            panelLogin.Visible = false;
            panelAdmin.Visible = false;

            // Create dropdown menu for Admin/Shutdown buttons
            CreateDropdownMenu();
        }

        private void AddTabsToTabControl(TabControl tabControl, string[] urls)
        {
            foreach (var url in urls)
            {
                var tabPage = new TabPage("Tab " + (tabControl.TabPages.Count + 1));
                var webView = new WebView2 { Dock = DockStyle.Fill };

                tabPage.Controls.Add(webView);
                tabControl.TabPages.Add(tabPage);

                InitializeWebView2(webView, url); // Initialize WebView2 with the URL
            }
        }

        private async Task InitializeWebView2(WebView2 webView, string url)
        {
            try
            {
                if (webView.CoreWebView2 == null)
                {
                    await webView.EnsureCoreWebView2Async();
                }

                if (Uri.TryCreate(url, UriKind.Absolute, out Uri result))
                {
                    webView.CoreWebView2.Navigate(url);
                }
                else
                {
                    MessageBox.Show("Invalid URL: " + url, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (COMException ex) when (ex.Message.Contains("Class not registered"))
            {
                MessageBox.Show("WebView2 runtime is not installed. Please install it and try again.", "Runtime Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize WebView2 or navigate: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AttemptLogin()
        {
            string enteredPassword = txtPassword.Text.Trim();

            if (enteredPassword == CorrectPassword)
            {
                txtPassword.Clear();
                panelLogin.Visible = false;
                panelAdmin.Visible = true;
                LoadSavedUrls();
            }
            else
            {
                MessageBox.Show("Incorrect password. Please try again.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private void LoadSavedUrls()
        {
            if (File.Exists("urls.json"))
            {
                var json = File.ReadAllText("urls.json");
                dynamic urls = JsonConvert.DeserializeObject(json);
                txtHomeUrl.Text = urls?.HomeUrl ?? "https://www.example.com";
                txtDialerUrl.Text = urls?.DialerUrl ?? "https://139.9.58.158:7890/example.com";
            }
        }

        private async void BtnSaveUrls_Click(object sender, EventArgs e)
        {
            SaveUrls(txtHomeUrl.Text, txtDialerUrl.Text);

            //// Initialize WebView2 controls with error handling
            //await InitializeWebView2(webViewHome, txtHomeUrl.Text);
            //await InitializeWebView2(webViewDialer, txtDialerUrl.Text);

            MessageBox.Show("URLs saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            panelAdmin.Visible = false;
            ShowHomeAndDialer();
        }

        private void SaveUrls(string homeUrl, string dialerUrl)
        {
            if (!Uri.TryCreate(homeUrl, UriKind.Absolute, out _) || !Uri.TryCreate(dialerUrl, UriKind.Absolute, out _))
            {
                MessageBox.Show("One or both URLs are invalid. Please provide valid URLs.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            File.WriteAllText("urls.json", JsonConvert.SerializeObject(new
            {
                HomeUrl = homeUrl,
                DialerUrl = dialerUrl
            }));
        }

        private void ShowHomeAndDialer()
        {
            panelHome.Visible = true;
            panelDialer.Visible = true;
            panelLogin.Visible = false;
            panelAdmin.Visible = false;
            SetInitialLayout();
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                AttemptLogin();
            }
        }

        private void btnAdminLogin_Click(object sender, EventArgs e)
        {
            AttemptLogin();
        }

        private void btnAdminAccess_Click(object sender, EventArgs e)
        {
            panelHome.Visible = false;
            panelDialer.Visible = false;
            panelLogin.Visible = true;
        }

        private void btnCancelAdmin_Click(object sender, EventArgs e)
        {
            panelAdmin.Visible = false;
            ShowHomeAndDialer();
        }

        private void backToWork_Click(object sender, EventArgs e)
        {
            panelLogin.Visible = false;
            panelDialer.Visible = true;
            panelHome.Visible = true;
            SetInitialLayout();
        }

        private void btnCloseApp_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnShutDown_Click(object sender, EventArgs e)
        {
            Process.Start("shutdown", "/s /f /t 0");
        }

        // Prevent copying/pasting outside the app
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.C || e.KeyCode == Keys.V || e.KeyCode == Keys.X))
            {
                e.Handled = true; // Prevent the key press
            }
        }

        // Add a Notes Tab
        private void AddNotesTab(TabControl tabControl)
        {
            var tabPage = new TabPage("Notes");
            var textBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,  // Add a scrollbar
                Font = new System.Drawing.Font("Arial", 10),
                AcceptsReturn = true
            };

            tabPage.Controls.Add(textBox);
            tabControl.TabPages.Add(tabPage);
        }

        // Create a dropdown menu with Admin and Shutdown options
        private void CreateDropdownMenu()
        {
            dropdownMenu = new ContextMenuStrip();
            var adminMenuItem = new ToolStripMenuItem("Admin Panel");
            var shutdownMenuItem = new ToolStripMenuItem("Shutdown");

            adminMenuItem.Click += (sender, e) => ShowAdminPanel();
            shutdownMenuItem.Click += (sender, e) => Process.Start("shutdown", "/s /f /t 0");

            dropdownMenu.Items.Add(adminMenuItem);
            dropdownMenu.Items.Add(shutdownMenuItem);

            var dropdownButton = new Button
            {
                Text = "☰",  // Three-line icon
                Dock = DockStyle.Top,
                Width = 30,
                Height = 30,
                BackColor = System.Drawing.Color.LightGray
            };

            dropdownButton.Click += (sender, e) => dropdownMenu.Show(dropdownButton, new System.Drawing.Point(0, dropdownButton.Height));

            panelDialer.Controls.Add(dropdownButton); // Add to the right panel (Dialer)
        }

        private void ShowAdminPanel()
        {
            panelHome.Visible = false;
            panelDialer.Visible = false;
            panelLogin.Visible = true;
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if ((Keys)vkCode == Keys.Tab && (Control.ModifierKeys & Keys.Alt) == Keys.Alt ||
                    (Keys)vkCode == Keys.F4 && (Control.ModifierKeys & Keys.Alt) == Keys.Alt ||
                    (Keys)vkCode == Keys.LWin || (Keys)vkCode == Keys.RWin ||
                    (Keys)vkCode == Keys.Escape && (Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(13, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
    }
}
