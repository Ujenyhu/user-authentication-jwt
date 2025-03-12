namespace userauthjwt.DataAccess.Interfaces
{
    public interface ISoftDelete
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
