using System.Windows.Forms;

namespace Zipher
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel panelHome;
        private Panel panelDialer;
        private Panel panelLogin;
        private Panel panelAdmin;
        private TextBox txtPassword;
        private Button btnAdminLogin;
        private Button btnAdminAccess;
        private Button btnCancelAdmin;
        private Button btnCloseApp;
        private Button btnShutDown;
        private TextBox txtHomeUrl;
        private TextBox txtDialerUrl;
        private Button btnSaveUrls;

        private void InitializeComponent()
        {
            this.panelHome = new Panel();
            this.panelDialer = new Panel();
            this.panelLogin = new Panel();
            this.panelAdmin = new Panel();
            this.txtPassword = new TextBox();
            this.btnAdminLogin = new Button();
            this.btnAdminAccess = new Button();
            this.btnCancelAdmin = new Button();
            this.btnCloseApp = new Button();
            this.btnShutDown = new Button();
            this.txtHomeUrl = new TextBox();
            this.txtDialerUrl = new TextBox();
            this.btnSaveUrls = new Button();

            // Panel Setup
            this.panelHome.Dock = DockStyle.Left;
            this.panelDialer.Dock = DockStyle.Fill;

            // Buttons Setup
            this.btnAdminLogin.Text = "Login as Admin";
            this.btnAdminAccess.Text = "Go to Admin Panel";
            this.btnCancelAdmin.Text = "Cancel Admin";
            this.btnCloseApp.Text = "Close App";
            this.btnShutDown.Text = "Shutdown";

            // Add components to panels
            this.panelLogin.Controls.Add(this.txtPassword);
            this.panelLogin.Controls.Add(this.btnAdminLogin);
            this.panelAdmin.Controls.Add(this.txtHomeUrl);
            this.panelAdmin.Controls.Add(this.txtDialerUrl);
            this.panelAdmin.Controls.Add(this.btnSaveUrls);

            // Add panels to form
            this.Controls.Add(this.panelHome);
            this.Controls.Add(this.panelDialer);
            this.Controls.Add(this.panelLogin);
            this.Controls.Add(this.panelAdmin);

            this.Text = "Zipher";
            this.Size = new System.Drawing.Size(800, 600);
        }
    }
}
