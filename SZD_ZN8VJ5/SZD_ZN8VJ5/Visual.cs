using SZD_ZN8VJ5.Groups;

namespace SZD_ZN8VJ5
{
    public class Visual
    {
        public Group Group;
        public int Width;
        public int Height;
        //public int[] regionToColor;

        public Visual(Group group, int width, int height)
        {
            Group = group;
            Width = width;
            Height = height;
            //this.regionToColor = regionToColor;
        }

        public Visual(ARCObject obj)
        {
            Group = obj.Group;
            Width = obj.Width;
            Height = obj.Height;
            //this.regionToColor = obj.regionToColor;
        }

        public override bool Equals(object? obj)
        {
            Visual other = obj as Visual;
            if (other != null)
            {
                return this.Group.Equals(other.Group) &&
                    this.Width == other.Width &&
                    this.Height == other.Height;
                    //Enumerable.SequenceEqual(this.regionToColor, other.regionToColor);
            }
            return false;
        }
    }
}
