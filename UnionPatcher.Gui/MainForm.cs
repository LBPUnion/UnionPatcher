using Eto.Drawing;
using Eto.Forms;

namespace UnionPatcher.Gui {
    public class MainForm : Form {
        public MainForm() {
            this.Title = "Union Patcher";
            this.ClientSize = new Size(500, 160);
            this.Content = new TableLayout {
                Spacing = new Size(5,5),
                Padding = new Padding(10, 10, 10, 10),
                Rows = {
                    new TableRow(
                        new TableCell(new Label { Text = "EBOOT.elf: ", VerticalAlignment = VerticalAlignment.Center }),
                        new TableCell(new FilePicker())
                    ),
                    new TableRow(
                        new TableCell(new Label { Text = "Server URL: ", VerticalAlignment = VerticalAlignment.Center }),
                        new TableCell(new TextBox())
                    ),
                    new TableRow(
                        new TableCell(new Label { Text = "Output filename: ", VerticalAlignment = VerticalAlignment.Center }),
                        new TableCell(new TextBox())
                    ),
                    new TableRow(
                        new TableCell(new Button { Text = "Help" }),
                        new TableCell(new Button { Text = "Patch!" })
                    ),
                },
            };
        }
    }
}