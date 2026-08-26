namespace BE
{
    public class AccessGrant
    {
        public int id { get; set; }
        public Section Section { get; set; }
        public Operation Operation { get; set; }
        public UserGroup UserGroup { get; set; }
    }
}
