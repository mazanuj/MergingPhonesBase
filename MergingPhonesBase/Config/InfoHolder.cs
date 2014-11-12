namespace MergingPhonesBase.Config
{
    public class InfoHolder
    {
        public SiteEnum Site { get; set; }

        public DirectionEnum Direction { get; set; }

        public string Name { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
    }

    public enum DirectionEnum
    {
        @base
    }

    public enum SiteEnum
    {
        torgshop
    }
}