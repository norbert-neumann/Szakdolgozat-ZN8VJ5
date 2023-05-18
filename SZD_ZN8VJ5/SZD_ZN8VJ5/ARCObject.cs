using SZD_ZN8VJ5.Groups;

namespace SZD_ZN8VJ5
{
    public class ARCObject
    {
        public Group Group;
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public int[] RegionToColor;

        public List<ARCObject> Noises = new List<ARCObject>();
        public List<ARCObject> ContainedObjects = new List<ARCObject>();
        public ARCObject Parent;
        public ARCObject ContainedBy;

        public Visual _Visual;

        public static int ObjectCount = 0;
        public int objectIndex;

        public ARCObject()
        {

        }

        public ARCObject(Group group, int[] regionToColor)
        {
            this.objectIndex = ObjectCount++;
            this.Group = group;
            this.RegionToColor = regionToColor;
        }

        public ARCObject(Group group, int x, int y, int width, int height, int[] regionToColor)
        {
            this.objectIndex = ObjectCount++;
            this.Group = group;
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.RegionToColor = regionToColor;

            this._Visual = new Visual(this);
        }

        public int[,] Appearance(bool isNoise)
        {
            int[,] visual = this.Visual();

            if (!isNoise)
            {
                for (int i = 0; i < visual.GetLength(0); i++)
                {
                    for (int j = 0; j < visual.GetLength(1); j++)
                    {
                        if (visual[i, j] == -1)
                        {
                            visual[i, j] = 0;
                        }
                    }
                }
            }

            foreach (var noise in this.Noises)
            {
                int[,] noiseVisual = noise.Appearance(true);
                for (int i = 0; i < noiseVisual.GetLength(0); i++)
                {
                    for (int j = 0; j < noiseVisual.GetLength(1); j++)
                    {
                        if (noiseVisual[i, j] != -1)
                        {
                            visual[i + noise.X, j + noise.Y] = noiseVisual[i, j];
                        }
                    }
                }
            }

            return visual;
        }

        public int[,] Visual()
        {
            int[,] slice = this.Group.Slice(Width, Height);

            int regionCount = 0;
            Dictionary<int, int> globalToLocal = new Dictionary<int, int>();

            for (int i = 0; i < slice.GetLength(0); i++)
            {
                for (int j = 0; j < slice.GetLength(1); j++)
                {
                    if (slice[i, j] >= 0)
                    {
                        if (!globalToLocal.ContainsKey(slice[i, j]))
                        {
                            globalToLocal.Add(slice[i, j], regionCount++);
                        }
                        slice[i, j] = globalToLocal[slice[i, j]];
                    }
                }
            }

            for (int i = 0; i < slice.GetLength(0); i++)
            {
                for (int j = 0; j < slice.GetLength(1); j++)
                {
                    int region = slice[i, j];
                    if (region >= 0)
                    {
                        slice[i, j] = RegionToColor[slice[i, j]];
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

            int[,] slice = this.Group.Slice(Width, Height);
            for (int i = 0; i < slice.GetLength(0); i++)
            {
                for (int j = 0; j < slice.GetLength(1); j++)
                {
                    int region = slice[i, j];
                    if (region >= 0)
                    {
                        result[i + X, j + Y] = RegionToColor[slice[i, j]];
                    }
                }
            }
            return result;
        }

        public int[,] Visual(int wHeight, int wWidth, int startX, int startY)
        {
            int[,] slice = this.Group.Slice(Width, Height);

            int regionCount = 0;
            Dictionary<int, int> globalToLocal = new Dictionary<int, int>();

            for (int i = 0; i < slice.GetLength(0); i++)
            {
                for (int j = 0; j < slice.GetLength(1); j++)
                {
                    if (slice[i, j] >= 0)
                    {
                        if (!globalToLocal.ContainsKey(slice[i, j]))
                        {
                            globalToLocal.Add(slice[i, j], regionCount++);
                        }
                        slice[i, j] = globalToLocal[slice[i, j]];
                    }
                }
            }

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
                        result[startX + i, startY + j] = RegionToColor[slice[i, j]];
                    }
                }
            }
            return result;
        }

        public Visual GetVisual()
        {
            return new Visual(this);
        }

        public int Sum()
        {
            int sum = 0;
            int[,] slice = this.Group.Slice(Width, Height);
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
            return this.X == obj.X &&
                this.Y == obj.Y &&
                this.Height == obj.Height &&
                this.Width == obj.Width &&
                this.Group.AbstractEquals(obj.Group) &&
                Enumerable.SequenceEqual(this.RegionToColor, obj.RegionToColor) &&
                this.Noises.All(n => obj.Noises.Contains(n)) && 
                this.Noises.Count == obj.Noises.Count;
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
                this.Height == obj.Height &&
                this.Width == obj.Width &&
                this.Group.AbstractEquals(obj.Group) &&
                this.RegionToColor.SequenceEqual(obj.RegionToColor) &&
                this.Noises.All(n => obj.Noises.Contains(n)) &&
                this.Noises.Count == obj.Noises.Count;
        }

        public bool VisualEquals2(ARCObject other)
        {
            int[,] v1 = this.Appearance(false);
            int[,] v2 = other.Appearance(false);

            if (v1.GetLength(0) == v2.GetLength(0) && v1.GetLength(1) == v2.GetLength(1))
            {
                for (int i = 0; i < v1.GetLength(0); i++)
                {
                    for (int j = 0; j < v1.GetLength(1); j++)
                    {
                        if (v1[i, j] != v2[i, j])
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            return false;
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
