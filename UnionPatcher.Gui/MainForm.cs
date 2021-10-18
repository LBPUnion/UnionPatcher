using System;
using System.Diagnostics;
using Eto;
using Eto.Drawing;
using Eto.Forms;

namespace UnionPatcher.Gui {
    public class MainForm : Form {
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
                Text = "Patch!",
                TabIndex = tabIndex,
            };

            control.Click += delegate {
                if(string.IsNullOrEmpty(this.filePicker.FilePath)) {
                    this.CreateOkDialog("Form Error", "No file specified!").ShowModal();
                    return;
                }

                if(string.IsNullOrEmpty(this.serverUrl.Text)) {
                    this.CreateOkDialog("Form Error", "No server URL specified!").ShowModal();
                    return;
                }

                if(string.IsNullOrEmpty(this.outputFileName.FilePath)) {
                    this.CreateOkDialog("Form Error", "No output file specified!").ShowModal();
                    return;
                }

                try {
                    Patcher.PatchFile(this.filePicker.FilePath, this.serverUrl.Text, this.outputFileName.FilePath);
                }
                catch(Exception e) {
                    this.CreateOkDialog("Error occurred while patching", "An error occured while patching:\n" + e).ShowModal();
                    return;
                }

                CreateOkDialog("Success!", "The Server URL has been patched to " + this.serverUrl.Text).ShowModal();
            };

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
            this.Title = "Union Patcher";
            this.ClientSize = new Size(500, 160);
            this.Content = new TableLayout {
                Spacing = new Size(5,5),
                Padding = new Padding(10, 10, 10, 10),
                Rows = {
                    new TableRow(
                        new TableCell(new Label { Text = "EBOOT.elf: ", VerticalAlignment = VerticalAlignment.Center }),
                        new TableCell(filePicker = new FilePicker { TabIndex = 0 })
                    ),
                    new TableRow(
                        new TableCell(new Label { Text = "Server URL: ", VerticalAlignment = VerticalAlignment.Center }),
                        new TableCell(serverUrl = new TextBox { TabIndex = 1 })
                    ),
                    new TableRow(
                        new TableCell(new Label { Text = "Output filename: ", VerticalAlignment = VerticalAlignment.Center }),
                        new TableCell(this.outputFileName = new FilePicker { TabIndex = 2, FileAction = FileAction.SaveFile })
                    ),
                    new TableRow(
                        new TableCell(CreateHelpButton(4)),
                        new TableCell(CreatePatchButton(3))
                    ),
                },
            };
        }
    }
}