namespace SapLichThiLib.DataObjects
{
    public class School
    {
        string id;
        public string ID => id;
        string name;
        public string Name => name;
        int maxProctor;
        public int MaxProctor => maxProctor;

        public School(string id)
        {
            this.id = id;
        }
        public School(string id, string name)
        {
            this.id = id;
            this.name = name;
        }
        public School(string id, string name, int maxProctor) : this(id, name)
        {
            this.maxProctor = maxProctor;
        }
    }
}
