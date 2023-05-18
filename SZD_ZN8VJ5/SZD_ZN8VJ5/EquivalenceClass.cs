
namespace SZD_ZN8VJ5
{
    public class SIEquivalenceClass
    {
        public SuperimposedARCPRogram Program;
        public List<ARCObject> Elements = new List<ARCObject>();

        public SIEquivalenceClass(ARCObject obj, SuperimposedARCPRogram program)
        {
            this.Program = program;
            Elements.Add(obj);
        }

        public bool TryIntersect(SuperimposedARCPRogram other)
        {
            var intersection = Program.Intersect(other);
            if (Program.NoiseMaps.Count == 0 && Program.Noises.Count == 0)
            {
                if (!intersection.Empty())
                {
                    this.Program = intersection;
                    return true;
                }
            }
            else
            {
                if (!intersection.Empty())
                {
                    if (Program.NoiseMaps.Count > 0 && intersection.NoiseMaps.Count > 0)
                    {
                        this.Program = intersection;
                        return true;
                    }
                    else if (Program.Noises.Count > 0 && intersection.Noises.Count > 0)
                    {
                        this.Program = intersection;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }

            return false;
        }
    }
}
