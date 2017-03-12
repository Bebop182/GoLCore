namespace GOLCore.Structures {
    public struct Size {
        public static readonly Size Null;
        public int Width {get; private set;}
        public int Height {get; private set;}

        static Size() {
            Null = new Size(0, 0);
        }
        public Size(int width, int height) {
            Width = width > 0 ? width : 0;
            Height = height > 0 ? height : 0;
        }

        public int Area() {
            return Width*Height;
        }
    }
}
