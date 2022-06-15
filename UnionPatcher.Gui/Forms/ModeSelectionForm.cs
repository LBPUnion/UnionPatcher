using System;
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
    
    private void openRemotePatcher(object sender, EventArgs e) {
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