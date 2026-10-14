namespace AdvancedEFCore.Models
{




    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        // Foreign Key
        public int CategoryId { get; set; }

        // Navigation Property
        public Category Category { get; set; } = null!;

        // Soft Delete
        public bool IsDeleted { get; set; } = false;

        // SQL Server Optimistic Concurrency Token
        public byte[] RowVersion { get; set; } = [];
    }


}
