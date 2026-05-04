namespace CompanyFileManager.Models
{
    public class FileNode
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public string? ParentId { get; set; }
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }
        public string Icon { get; set; } = "images/blank.svg";
        public bool HasChildren { get; set; }
        public bool IsExpanded { get; set; }
        public List<FileNode> Children { get; set; } = new();
    }
}
