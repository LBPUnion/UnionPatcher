using System;
using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;

namespace LBPUnion.UnionPatcher.Gui.Forms; 

public class RemotePatchForm : Form
{
    public RemotePatchForm()
    {
        this.InitializeComponent();
        Console.WriteLine("Welcome to UnionRemotePatcher");
    }

    public readonly RemotePatch RemotePatcher = new();

    private TextBox ps3LocalIP;
    private TextBox lbpGameID;
    private TextBox serverUrl;
    private TextBox ftpUser;
    private TextBox ftpPass;
        
    #region UI
    public Control CreatePatchButton(int tabIndex = 0)
    {
        Button control = new()
        {
            Text = "Patch!",
            TabIndex = tabIndex,
            Width = 200,
        };

        control.Click += delegate {
            if (string.IsNullOrEmpty(this.ps3LocalIP.Text))
            {
                Gui.CreateOkDialog("Error", "No PS3 IP address specified!").ShowModal();
                return;
            }

            if (string.IsNullOrEmpty(this.lbpGameID.Text))
            {
                Gui.CreateOkDialog("Error", "No title ID specified!").ShowModal();
                return;
            }

            if (string.IsNullOrEmpty(this.serverUrl.Text))
            {
                Gui.CreateOkDialog("Error", "No server URL specified!").ShowModal();
                return;
            }
                
            if (!Uri.TryCreate(this.serverUrl.Text, UriKind.Absolute, out _))
            {
                Gui.CreateOkDialog("Error", "Server URL is invalid! Please enter a valid URL.").ShowModal();
                return;
            }

            try
            {
                if (this.lbpGameID.Text.ToUpper().StartsWith('B'))
                {
                    this.RemotePatcher.DiscEBOOTRemotePatch(this.ps3LocalIP.Text, this.lbpGameID.Text, this.serverUrl.Text, this.ftpUser.Text, this.ftpPass.Text);
                }
                else
                {
                    this.RemotePatcher.PSNEBOOTRemotePatch(this.ps3LocalIP.Text, this.lbpGameID.Text, this.serverUrl.Text, this.ftpUser.Text, this.ftpPass.Text);
                }
            }
            catch (Exception e)
            {
                Gui.CreateOkDialog("Error occurred while patching", "An error occured while patching:\n" + e).ShowModal();
                return;
            }

            Gui.CreateOkDialog("Success!", $"The Server URL for {this.lbpGameID.Text} on the PS3 at {this.ps3LocalIP.Text} has been patched to {this.serverUrl.Text}").ShowModal();
        };

        return control;
    }
        
    public Control CreateRevertEBOOTButton(int tabIndex = 0)
    {
        Button control = new()
        {
            Text = "Revert EBOOT",
            TabIndex = tabIndex,
            Width = 200,
        };

        control.Click += delegate {
            if (string.IsNullOrEmpty(this.ps3LocalIP.Text))
            {
                Gui.CreateOkDialog("Form Error", "No PS3 IP address specified!").ShowModal();
                return;
            }

            if (string.IsNullOrEmpty(this.lbpGameID.Text))
            {
                Gui.CreateOkDialog("Form Error", "No game ID specified!").ShowModal();
                return;
            }
                
            try
            {
                this.RemotePatcher.RevertEBOOT(this.ps3LocalIP.Text, this.lbpGameID.Text, this.serverUrl.Text, this.ftpUser.Text, this.ftpPass.Text);
            }
            catch (Exception e)
            {
                Gui.CreateOkDialog("Error occurred while reverting EBOOT", "An error occured while patching:\n" + e).ShowModal();
                return;
            }

            Gui.CreateOkDialog("Success!", $"UnionRemotePatcher reverted your the EBOOT for {this.lbpGameID.Text} to stock. You're ready to patch your EBOOT again.").ShowModal();
        };

        return control;
    }

    public Control CreateHelpButton(int tabIndex = 0)
    {
        Button control = new()
        {
            Text = "Help",
            TabIndex = tabIndex,
        };

        control.Click += delegate {
            Process process = new();

            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = "https://www.lbpunion.com";
            process.Start();
        };

        return control;
    }
        
    void InitializeComponent()
    {
        this.Title = "UnionPatcher - Remote Patch";
        this.MinimumSize = new Size(450, 200);
        this.Resizable = false;
        this.Padding = 10;

        this.Content = new TableLayout
        {
            Spacing = new Size(5, 5),
            Padding = new Padding(10, 10, 10, 10),
            Rows = {
                new TableRow(
                    new TableCell(new Label { Text = "PS3 Local IP: ", VerticalAlignment = VerticalAlignment.Center }),
                    new TableCell(this.ps3LocalIP = new TextBox { TabIndex = 0 })
                ),
                new TableRow(
                    new TableCell(new Label { Text = "Server URL: ", VerticalAlignment = VerticalAlignment.Center }),
                    new TableCell(this.serverUrl = new TextBox { TabIndex = 1 })
                ),
                new TableRow(
                    new TableCell(new Label { Text = "Title ID (e.g. BCUS98245): ", VerticalAlignment = VerticalAlignment.Center }),
                    new TableCell(this.lbpGameID = new TextBox { TabIndex = 2 })
                ),
                new TableRow(
                    new TableCell(new Label { Text = "FTP Username: ", VerticalAlignment = VerticalAlignment.Center }),
                    new TableCell(this.ftpUser = new TextBox { TabIndex = 3, Text = "anonymous" })
                ),
                new TableRow(
                    new TableCell(new Label { Text = "FTP Password: ", VerticalAlignment = VerticalAlignment.Center }),
                    new TableCell(this.ftpPass = new TextBox { TabIndex = 4 })
                ),
                new TableRow(
                    new TableCell(this.CreateHelpButton(7)),
                    new TableRow(
                        new TableCell(this.CreatePatchButton(5)),
                        new TableCell(this.CreateRevertEBOOTButton(6))
                    )
                ),
            },
        };
    }
    #endregion
}