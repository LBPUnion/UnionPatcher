using Eto.Drawing;
using Eto.Forms;

namespace UnionPatcher.Gui {
    public class TestForm : Form {
        public TestForm() {
            this.Title = "test";
            this.ClientSize = new Size(200, 200);
            this.Content = new Label { Text = "i'm stuff" };
        }
    }
}