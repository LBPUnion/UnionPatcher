using System;
using System.IO;
using System.Diagnostics;
using Eto;
using Eto.Drawing;
using Eto.Forms;

namespace LBPUnion.UnionPatcher.Gui {
    public class MainForm : Form {
        #region UI
        private readonly FilePicker filePicker;
        private readonly TextBox serverUrl;
        private readonly FilePicker outputFileName;
        public Dialog CreateOkDialog(string title, string errorMessage) {
            DynamicLayout layout = new();
            Button button;

            layout.Spacing = new Size(5, 5);
            layout.MinimumSize = new Size(350, 100);

            layout.BeginHorizontal();
            layout.Add(new Label {
                Text = errorMessage,
            });

            layout.BeginHorizontal();
            layout.BeginVertical();
            layout.Add(null);
            layout.Add(button = new Button {
                Text = "OK",
            });

            layout.EndVertical();
            layout.EndHorizontal();
            layout.EndHorizontal();

            Dialog dialog = new() {
                Content = layout,
                Padding = new Padding(10, 10, 10, 10),
                Title = title,
            };

            button.Click += delegate {
                dialog.Close();
            };

            return dialog;
        }

        public Control CreatePatchButton(int tabIndex = 0) {
            Button control = new() {
                Text = EasterEgg.Restitch ? "Restitch!" : "Patch!",
                TabIndex = tabIndex,
            };

            control.Click += Patch;

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

        public MainForm() {
            this.Title = EasterEgg.Restitch ? "Union Restitcher" : "Union Patcher";
            this.ClientSize = new Size(500, -1);
            this.Content = new TableLayout {
                Spacing = new Size(5,5),
                Padding = new Padding(10, 10, 10, 10),
                Rows = {
                    new TableRow(
                        new TableCell(new Label { Text = "EBOOT.elf: ", VerticalAlignment = VerticalAlignment.Center }),
                        new TableCell(this.filePicker = new FilePicker { TabIndex = 0 })
                    ),
                    new TableRow(
                        new TableCell(new Label { Text = "Server URL: ", VerticalAlignment = VerticalAlignment.Center }),
                        new TableCell(this.serverUrl = new TextBox { TabIndex = 1 })
                    ),
                    new TableRow(
                        new TableCell(new Label { Text = "Output filename: ", VerticalAlignment = VerticalAlignment.Center }),
                        new TableCell(this.outputFileName = new FilePicker { TabIndex = 2, FileAction = FileAction.SaveFile })
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
                this.CreateOkDialog("Form Error", "No file specified!").ShowModal();
                return;
            }

            if(string.IsNullOrWhiteSpace(this.serverUrl.Text)) {
                this.CreateOkDialog("Form Error", "No server URL specified!").ShowModal();
                return;
            }

            if(string.IsNullOrWhiteSpace(this.outputFileName.FilePath)) {
                this.CreateOkDialog("Form Error", "No output file specified!").ShowModal();
                return;
            }

            if(this.filePicker.FilePath == this.outputFileName.FilePath) {
                this.CreateOkDialog("Form Error", "Input and output filename are the same! Please save the patched file with a different name so you have a backup of your the original EBOOT.ELF.").ShowModal();
                return;
            }

            if(!Uri.TryCreate(this.serverUrl.Text, UriKind.Absolute, out _)) {
                this.CreateOkDialog("Form Error", "Server URL is invalid! Please enter a valid URL.").ShowModal();
                return;
            }
            
            // Validate EBOOT after validating form; more expensive

            ElfFile eboot = new(this.filePicker.FilePath);

            if(eboot.IsValid == false) {
                this.CreateOkDialog("EBOOT Error", $"{eboot.Name} is not a valid ELF file (magic number mismatch)\n" + "The EBOOT must be decrypted before using this tool").ShowModal();
                return;
            }

            if(eboot.Is64Bit == null) {
                this.CreateOkDialog("EBOOT Error", $"{eboot.Name} does not target a valid system").ShowModal();
                return;
            }

            if(string.IsNullOrWhiteSpace(eboot.Architecture)) {
                this.CreateOkDialog("EBOOT Error", $"{eboot.Name} does not target a valid architecture (PowerPC or ARM)").ShowModal();
                return;
            }

            try {
                Patcher.PatchFile(this.filePicker.FilePath, this.serverUrl.Text, this.outputFileName.FilePath);
            }
            catch(Exception e) {
                this.CreateOkDialog("Error occurred while patching", "An error occured while patching:\n" + e).ShowModal();
                return;
            }

            this.CreateOkDialog("Success!", "The Server URL has been patched to " + this.serverUrl.Text).ShowModal();
        }
    }
}
