namespace LibraryManagementSystem.Application.DTOs.Common
{
    public class BaseQuery
    {
        public string? SearchTerm { get; set; }

        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value <= 0 ? 10 : Math.Min(value, 50);
        }

        public int Skip => (Math.Max(PageNumber, 1) - 1) * PageSize;

        public int Take => PageSize;
    }
}
