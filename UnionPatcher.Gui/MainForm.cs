using System;
using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;

namespace UnionPatcher.Gui {
    public class MainForm : Form {
        public static Control CreatePatchButton(int tabIndex = 0) {
            var control = new Button {
                Text = "Patch!",
                TabIndex = tabIndex,
            };

            control.Click += delegate {
                Console.WriteLine("patch button clicked");
            };

            return control;
        }

        public static Control CreateHelpButton(int tabIndex = 0) {
            var control = new Button {
                Text = "Help",
                TabIndex = tabIndex,
            };
            
            control.Click += delegate {
                var process = new Process();

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
                        new TableCell(new FilePicker { TabIndex = 0 })
                    ),
                    new TableRow(
                        new TableCell(new Label { Text = "Server URL: ", VerticalAlignment = VerticalAlignment.Center }),
                        new TableCell(new TextBox { TabIndex = 1 })
                    ),
                    new TableRow(
                        new TableCell(new Label { Text = "Output filename: ", VerticalAlignment = VerticalAlignment.Center }),
                        new TableCell(new TextBox { TabIndex = 2 })
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