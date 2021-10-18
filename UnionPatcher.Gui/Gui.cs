using System;
using Eto.Forms;

namespace UnionPatcher.Gui {
    public static class Gui {
        public static void Show() {
            new Application().Run(new MainForm());
        }
    }
}