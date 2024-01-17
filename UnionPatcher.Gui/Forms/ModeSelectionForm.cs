using System;
using System.IO;
using System.Reflection;
using System.Text;
using Eto;
using Eto.Drawing;
using Eto.Forms;

namespace LBPUnion.UnionPatcher.Gui.Forms;

public class ModeSelectionForm : Form {
    #region UI
    public ModeSelectionForm() {
        this.Title = "Welcome to UnionPatcher";
        this.ClientSize = new Size(500, -1);
        this.Content = new TableLayout {
            Spacing = new Size(5, 5),
            Padding = new Padding(10, 10, 10, 10),
            Rows = {
                new TableRow(
                    new TableCell(new Button(openRemotePatcher) { Text = "Remote Patcher (PS3)" })
                ),
                new TableRow(
                    new TableCell(new Button(openLocalPatcher) { Text = "Local Patch (RPCS3)", Enabled = false })
                ),
                new TableRow(
                    new TableCell(new Button(openFilePatcher) { Text = "File Patch (PS3/RPCS3)" })
                ),
            },
        };
    }

    private void openRemotePatcher(object sender, EventArgs e)
    {
        // If we're on macOS then set the CWD to the app bundle MacOS folder, so that SCETool can be found.
        if (OSUtil.GetPlatform() == OSPlatform.OSX) Directory.SetCurrentDirectory(OSUtil.GetExecutablePath());
        
        if (!Directory.Exists("scetool"))
        {
            // This will always occur on macOS, so don't show this message for macOS users.
            if (OSUtil.GetPlatform() != OSPlatform.OSX) Gui.CreateOkDialog("Workaround Triggered", ".NET could not locate the required files, triggering workaround.");
            
            
            
            Gui.CreateOkDialog("Workaround",
                $"UnionPatcher RemotePatcher requires a staging folder on macOS or in special circumstances on Windows, please set this to the directory of the UnionPatcher app or executable! {EtoEnvironment.GetFolderPath(EtoSpecialFolder.ApplicationResources)}");
            SelectFolderDialog dialog = new SelectFolderDialog();
            if (dialog.ShowDialog(this) != DialogResult.Ok)
            {
                Gui.CreateOkDialog("Workaround", "User did not specify a staging folder, aborting!");
                return;
            }
            Directory.SetCurrentDirectory(dialog.Directory);
            if (!Directory.Exists("scetool"))
            {
                Gui.CreateOkDialog("Workaround", "Invalid folder, remember to set the folder to the directory of the UnionPatcher app or executable!");
                return;
            } 
        }
        RemotePatchForm rpForm = new RemotePatchForm();
        rpForm.Show();
        rpForm.Closed += OnSubFormClose;

        this.Visible = false;
    }
    private void openLocalPatcher(object sender, EventArgs e) {
        throw new NotImplementedException();
    }
    private void openFilePatcher(object sender, EventArgs e) {
        FilePatchForm fpForm = new FilePatchForm();
        fpForm.Show();
        fpForm.Closed += OnSubFormClose;

        this.Visible = false;
    }
    private void OnSubFormClose(object sender, EventArgs e)
    {
        this.Close();
    }

    #endregion
}