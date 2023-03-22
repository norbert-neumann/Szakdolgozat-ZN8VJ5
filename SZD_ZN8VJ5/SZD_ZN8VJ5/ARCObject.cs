using SZD_ZN8VJ5.Groups;

namespace SZD_ZN8VJ5
{
    public class ARCObject
    {
        public Group Group { get; set; }
        public int x;
        public int y;
        public int width;
        public int height;

        public int[] regionToTile;
        public int[] regionToColor;

        public List<ARCObject> noises = new List<ARCObject>();
        public List<ARCObject> contains = new List<ARCObject>();
        public ARCObject noiseTo;
        public ARCObject containedBy;

        public Visual _Visual;

        public static int ObjectCount = 0;
        public int objectIndex;

        public static List<Func<ARCObject, object>> Properties = new List<Func<ARCObject, object>>();
        public static List<string> PropertyNames = new List<string>();

        public static List<Func<ARCObject, ARCObject>> Parents = new List<Func<ARCObject, ARCObject>>();
        public static List<string> ParentNames = new List<string>();

        public ARCObject(Group group, int[] tileToColor, bool dummy)
        {
            this.Group = group;
            this.objectIndex = ObjectCount++;

            /*this.regionTocolor = tileToColor;
            List<int>[] colors = new List<int>[10];
            for (int i = 0; i < 10; i++)
            {
                colors[i] = new List<int>();
            }

            int regionCount = 0;
            for (int tileIdx = 0; tileIdx < tileToColor.Length; tileIdx++)
            {
                if (tileToColor[tileIdx] >= 0)
                {
                    if (colors[tileToColor[tileIdx]].Count() == 0)
                    {
                        ++regionCount;
                    }
                    colors[tileToColor[tileIdx]].Add(tileIdx);
                }
            }

            this.regionToTile = new int[tileToColor.Length];
            for (int i = 0; i < regionToTile.Length; i++)
            {
                this.regionToTile[i] = -1;
            }

            this.regionToColor = new int[regionCount];

            int regionIndex = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].Count() > 0)
                {
                    foreach (int tileIdx in colors[i])
                    {
                        this.regionToTile[tileIdx] = regionIndex;
                    }
                    this.regionToColor[regionIndex] = i;
                    ++regionIndex;
                }
            }*/
        }

        public ARCObject()
        {

        }

        public ARCObject(Group group, int[] regionToColor)
        {
            this.objectIndex = ObjectCount++;
            this.Group = group;
            this.regionToColor = regionToColor;
        }

        public ARCObject(Group group, int x, int y, int width, int height, int[] regionToColor)
        {
            this.objectIndex = ObjectCount++;
            this.Group = group;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.regionToColor = regionToColor;

            this._Visual = new Visual(this);
        }

        public bool LooksLike(ARCObject other)
        {
            if (other == null)
            {
                return false;
            }
            foreach (var selector in Properties)
            {
                if (!selector.Invoke(this).Equals(selector.Invoke(other)))
                {
                    return false;
                }
            }
            return Array.Equals(this.regionToColor, other.regionToColor); // TODO: this might not be the right comparison
        }

        public int[,] Visual()
        {
            int[,] slice = this.Group.Slice(width, height);
            for (int i = 0; i < slice.GetLength(0); i++)
            {
                for (int j = 0; j < slice.GetLength(1); j++)
                {
                    int region = slice[i, j];
                    if (region >= 0)
                    {
                        slice[i, j] = regionToColor[slice[i, j]];
                    }
                }
            }
            return slice;
        }

        public int[,] FullVisual()
        {
            int[,] result = new int[30, 30];
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    result[i, j] = -1;
                }
            }

            int[,] slice = this.Group.Slice(width, height);
            for (int i = 0; i < slice.GetLength(0); i++)
            {
                for (int j = 0; j < slice.GetLength(1); j++)
                {
                    int region = slice[i, j];
                    if (region >= 0)
                    {
                        result[i + x, j + y] = regionToColor[slice[i, j]];
                    }
                }
            }
            return result;
        }

        public int[,] Visual(int wHeight, int wWidth, int startX, int startY)
        {
            int[,] slice = this.Group.Slice(width, height);
            int[,] result = new int[wHeight, wWidth];
            for (int i = 0; i < wHeight; i++)
            {
                for (int j = 0; j < wWidth; j++)
                {
                    result[i, j] = -1;
                }
            }

            for (int i = 0; i < slice.GetLength(0); i++)
            {
                for (int j = 0; j < slice.GetLength(1); j++)
                {
                    int region = slice[i, j];
                    if (region >= 0)
                    {
                        result[startX + i, startY + j] = regionToColor[slice[i, j]];
                    }
                }
            }
            return result;
        }

        public Visual GetVisual()
        {
            return new Visual(this);
        }

        public string ToTerm(int exampleId, string type)
        {
            //object(id(0), group(1), 0, 1, 2, 3, [8, 9], id(10), id(11)).
            string noiseToIdx = this.noiseTo == null ? "null": string.Format("id({0})", this.noiseTo.objectIndex);
            string containedByIdx = this.containedBy == null ? "null" : string.Format("id({0})", this.containedBy.objectIndex);
            string colors = string.Join(", ", regionToColor);

            return string.Format($"object(id({objectIndex}), ex_id({exampleId}), {type}, group({Group.groupIndex}), {x}, {y}, {width}, {height}, " +
                $"[{colors}], {noiseToIdx}, {containedByIdx}).");
        }

        public int Sum()
        {
            int sum = 0;
            int[,] slice = this.Group.Slice(width, height);
            for (int i = 0; i < slice.GetLength(0); i++)
            {
                for (int j = 0; j < slice.GetLength(1); j++)
                {
                    int region = slice[i, j];
                    if (region >= 0)
                    {
                        ++sum;
                    }
                }
            }
            return sum;
        }

        public override bool Equals(object other)
        {
            ARCObject obj = other as ARCObject;
            if (obj == null)
            {
                return false;
            }
            if (this == obj)
            {
                return true;
            }
            return this.x == obj.x &&
                this.y == obj.y &&
                this.height == obj.height &&
                this.width == obj.width &&
                this.Group.AbstractEquals(obj.Group) &&
                Enumerable.SequenceEqual(this.regionToColor, obj.regionToColor) &&
                this.noises.All(n => obj.noises.Contains(n)) && 
                this.noises.Count == obj.noises.Count;
        }

        public bool VisualEquals(object other)
        {
            ARCObject obj = other as ARCObject;
            if (obj == null)
            {
                return false;
            }
            if (this == obj)
            {
                return true;
            }
            return
                this.height == obj.height &&
                this.width == obj.width &&
                this.Group.AbstractEquals(obj.Group) &&
                this.regionToColor.SequenceEqual(obj.regionToColor) &&
                this.noises.All(n => obj.noises.Contains(n)) &&
                this.noises.Count == obj.noises.Count;
        }

        public bool Explains(ARCObject other)
        {
            if (this == other)
            {
                return false;
            }

            int[,] thisVisual = this.FullVisual();
            int[,] otherVisual = other.FullVisual();

            for (int i = 0; i < otherVisual.GetLength(0); i++)
            {
                for (int j = 0; j < otherVisual.GetLength(1); j++)
                {
                    if (thisVisual[i, j] == otherVisual[i, j])
                    {
                        otherVisual[i, j] = -1;
                    }
                }
            }

            for (int i = 0; i < otherVisual.GetLength(0); i++)
            {
                for (int j = 0; j < otherVisual.GetLength(1); j++)
                {
                    if (otherVisual[i, j] != -1 && otherVisual[i, j] != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override string ToString()
        {
            return this.objectIndex.ToString();
        }
    }
}
