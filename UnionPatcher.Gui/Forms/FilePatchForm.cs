using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Eto;
using Eto.Drawing;
using Eto.Forms;

namespace LBPUnion.UnionPatcher.Gui.Forms; 

public class FilePatchForm : Form {
    #region UI
    private readonly FilePicker filePicker;
    private readonly TextBox serverUrl;
    private readonly FilePicker outputFileName;

    public Control CreatePatchButton(int tabIndex = 0) {
        Button control = new() {
            Text = "Patch!",
            TabIndex = tabIndex,
        };

        control.Click += this.Patch;

        return control;
    }

    public Control CreateHelpButton(int tabIndex = 0) {
        Button control = new() {
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

    public FilePatchForm() {
        this.Title = "UnionPatcher - File Patch";
        this.ClientSize = new Size(500, -1);
        this.Content = new TableLayout {
            Spacing = new Size(5,5),
            Padding = new Padding(10, 10, 10, 10),
            Rows = {
                new TableRow(
                    new TableCell(new Label { Text = "EBOOT.elf: ", VerticalAlignment = VerticalAlignment.Center }),
                    new TableCell(this.filePicker = new FilePicker { TabIndex = 0 , FileAction = FileAction.OpenFile, Filters = { new FileFilter("ELF files", "*.elf", "*.ELF"), new FileFilter("All Files", "*.*") }})
                ),
                new TableRow(
                    new TableCell(new Label { Text = "Server URL: ", VerticalAlignment = VerticalAlignment.Center }),
                    new TableCell(this.serverUrl = new TextBox { TabIndex = 1 })
                ),
                new TableRow(
                    new TableCell(new Label { Text = "Output filename: ", VerticalAlignment = VerticalAlignment.Center }),
                    new TableCell(this.outputFileName = new FilePicker { TabIndex = 2, FileAction = FileAction.SaveFile,  Filters = { new FileFilter("ELF files", "*.elf", "*.ELF"), new FileFilter("All Files", "*.*") }})
                ),
                new TableRow(
                    new TableCell(this.CreateHelpButton(4)),
                    new TableCell(this.CreatePatchButton(3))
                ),
            },
        };
    }
    #endregion
    private void Patch(object sender, EventArgs e) {
        this.Patch();
    }

    private void Patch() {
        if(string.IsNullOrWhiteSpace(this.filePicker.FilePath)) {
            Gui.CreateOkDialog("Form Error", "No file specified!");
            return;
        }

        if(string.IsNullOrWhiteSpace(this.serverUrl.Text)) {
            Gui.CreateOkDialog("Form Error", "No server URL specified!");
            return;
        }

        if(string.IsNullOrWhiteSpace(this.outputFileName.FilePath)) {
            Gui.CreateOkDialog("Form Error", "No output file specified!");
            return;
        }

        if(this.filePicker.FilePath == this.outputFileName.FilePath) {
            Gui.CreateOkDialog("Form Error", "Input and output filename are the same! Please save the patched file with a different name so you have a backup of your the original EBOOT.ELF.");
            return;
        }

        if(!Uri.TryCreate(this.serverUrl.Text, UriKind.Absolute, out _)) {
            Gui.CreateOkDialog("Form Error", "Server URL is invalid! Please enter a valid URL.");
            return;
        }

        if(!Regex.IsMatch(this.serverUrl.Text, "LITTLEBIGPLANETPS3_XML")) {
            bool userCertain = Gui.CreateConfirmationDialog("URL Mistype", $"Server URL {this.serverUrl.Text} does not match LITTLEBIGPLANETPS3_XML, are you sure you want to use this?");
            if (!userCertain) {
                return;
            }
            // else, godspeed, captain 
        }
            
        // Validate EBOOT after validating form; more expensive

        ElfFile eboot = new(this.filePicker.FilePath);

        if(eboot.IsValid == false) {
            Gui.CreateOkDialog("EBOOT Error", $"{eboot.Name} is not a valid ELF file (magic number mismatch)\n" + "The EBOOT must be decrypted before using this tool");
            return;
        }

        if(eboot.Is64Bit == null) {
            Gui.CreateOkDialog("EBOOT Error", $"{eboot.Name} does not target a valid system");
            return;
        }

        if(string.IsNullOrWhiteSpace(eboot.Architecture)) {
            Gui.CreateOkDialog("EBOOT Error", $"{eboot.Name} does not target a valid architecture (PowerPC or ARM)");
            return;
        }

        try {
            Patcher.PatchFile(this.filePicker.FilePath, this.serverUrl.Text, this.outputFileName.FilePath);
        }
        catch(Exception e) {
            Gui.CreateOkDialog("Error occurred while patching", "An error occured while patching:\n" + e);
            return;
        }

        Gui.CreateOkDialog("Success!", "The Server URL has been patched to " + this.serverUrl.Text);
    }
}
