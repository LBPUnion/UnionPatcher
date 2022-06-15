using Eto.Drawing;
using Eto.Forms;
using LBPUnion.UnionPatcher.Gui.Forms;

namespace LBPUnion.UnionPatcher.Gui; 

public static class Gui {
    public static void Show() {
        new Application().Run(new ModeSelectionForm());
    }

    public static Dialog CreateOkDialog(string title, string errorMessage) {
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
}