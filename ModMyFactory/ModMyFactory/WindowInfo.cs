using System;
using System.Windows;
using Newtonsoft.Json;

namespace ModMyFactory
{
    [JsonObject(MemberSerialization.OptIn)]
    struct WindowInfo : IEquatable<WindowInfo>
    {
        public static WindowInfo Empty => default(WindowInfo);

        [JsonProperty]
        bool isNotEmpty;

        [JsonProperty]
        WindowState state;

        [JsonProperty]
        int posX, posY, width, height;

        public bool IsEmpty
        {
            get { return !isNotEmpty; }
            private set { isNotEmpty = !value; }
        }

        public WindowState State
        {
            get { return state; }
            set
            {
                state = value;
                IsEmpty = false;
            }
        }

        public int PosX
        {
            get { return posX; }
            set
            {
                posX = value;
                IsEmpty = false;
            }
        }

        public int PosY
        {
            get { return posY; }
            set
            {
                posY = value;
                IsEmpty = false;
            }
        }

        public int Width
        {
            get { return width; }
            set
            {
                width = value;
                IsEmpty = false;
            }
        }

        public int Height
        {
            get { return height; }
            set
            {
                height = value;
                IsEmpty = false;
            }
        }

        public bool Equals(WindowInfo other)
        {
            if (this.IsEmpty && other.IsEmpty) return true;
            if (this.IsEmpty || other.IsEmpty) return false;

            return (this.State == other.State)
                   && (this.PosX == other.PosX) && (this.PosY == other.PosY)
                   && (this.Width == other.Width) && (this.Height == other.Height);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            return Equals((WindowInfo)obj);
        }

        public override int GetHashCode()
        {
            if (IsEmpty) return 0;

            return State.GetHashCode()
                   ^ PosX.GetHashCode() ^ PosY.GetHashCode()
                   ^ Width.GetHashCode() ^ Height.GetHashCode();
        }

        public static bool operator ==(WindowInfo first, WindowInfo second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(WindowInfo first, WindowInfo second)
        {
            return !first.Equals(second);
        }
    }
}
