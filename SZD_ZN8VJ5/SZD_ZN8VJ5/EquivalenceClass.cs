
namespace SZD_ZN8VJ5
{
    public class SIEquivalenceClass
    {
        public SuperimposedARCPRogram program;
        public List<ARCObject> elements = new List<ARCObject>();
        public int N;

        public SIEquivalenceClass(ARCObject obj, SuperimposedARCPRogram program, int N)
        {
            this.program = program;
            elements.Add(obj);
            this.N = N;
        }

        public bool TryIntersect(SuperimposedARCPRogram other)
        {
            var intersection = program.Intersect(other);
            if (program.noiseMaps.Count == 0 && program.noises.Count == 0)
            {
                if (!intersection.Empty())
                {
                    this.program = intersection;
                    return true;
                }
            }
            else
            {
                if (!intersection.Empty())
                {
                    if (program.noiseMaps.Count > 0 && intersection.noiseMaps.Count > 0)
                    {
                        this.program = intersection;
                        return true;
                    }
                    else if (program.noises.Count > 0 && intersection.noises.Count > 0)
                    {
                        this.program = intersection;
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
