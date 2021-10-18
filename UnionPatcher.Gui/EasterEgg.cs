using System;

namespace UnionPatcher.Gui {
    public static class EasterEgg {
        private static bool? restitch = null;

        public static bool Restitch {
            get {
                if(restitch == null) {
                    return (bool)(restitch = new Random().Next(1, 10_000) == 1);
                }
                return (bool)restitch;
            }
        }
    }
}