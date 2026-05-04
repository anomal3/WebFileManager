using CompanyFileManager.Models;

namespace CompanyFileManager.Services
{
    public class FileService
    {
        public List<FileNode> GetChildren(string path)
        {
            var nodes = new List<FileNode>();
            try
            {
                foreach (var dir in Directory.GetDirectories(path).OrderBy(d => Path.GetFileName(d)))
                {
                    try
                    {
                        nodes.Add(new FileNode
                        {
                            Id = dir,
                            Name = Path.GetFileName(dir),
                            Path = dir,
                            IsDirectory = true,
                            Icon = "images/folders-icon-col.svg",
                            Modified = Directory.GetLastWriteTime(dir),
                            HasChildren = DirectoryHasChildren(dir)
                        });
                    }
                    catch { }
                }

                foreach (var file in Directory.GetFiles(path).OrderBy(f => Path.GetFileName(f)))
                {
                    try
                    {
                        var info = new FileInfo(file);
                        nodes.Add(new FileNode
                        {
                            Id = file,
                            Name = Path.GetFileName(file),
                            Path = file,
                            IsDirectory = false,
                            Icon = GetIconForFile(file),
                            Size = info.Length,
                            Modified = info.LastWriteTime
                        });
                    }
                    catch { }
                }
            }
            catch { }

            return nodes;
        }

        private bool DirectoryHasChildren(string path)
        {
            try { return Directory.EnumerateFileSystemEntries(path).Any(); }
            catch { return false; }
        }

        public async IAsyncEnumerable<FileNode> SearchAsync(
            string query,
            string rootPath,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            await foreach (var node in SearchRecursiveAsync(query.ToLowerInvariant().Trim(), rootPath, ct))
                yield return node;
        }

        private async IAsyncEnumerable<FileNode> SearchRecursiveAsync(
            string query,
            string path,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            if (ct.IsCancellationRequested) yield break;

            string[] dirs;
            string[] files;
            try { dirs = Directory.GetDirectories(path); } catch { dirs = Array.Empty<string>(); }
            try { files = Directory.GetFiles(path); } catch { files = Array.Empty<string>(); }

            foreach (var file in files)
            {
                if (ct.IsCancellationRequested) yield break;
                if (Path.GetFileName(file).ToLowerInvariant().Contains(query))
                {
                    FileInfo? info = null;
                    try { info = new FileInfo(file); } catch { continue; }
                    yield return new FileNode
                    {
                        Id = file,
                        Name = Path.GetFileName(file),
                        Path = file,
                        IsDirectory = false,
                        Icon = GetIconForFile(file),
                        Size = info.Length,
                        Modified = info.LastWriteTime
                    };
                }
                await Task.Yield();
            }

            foreach (var dir in dirs)
            {
                if (ct.IsCancellationRequested) yield break;
                if (Path.GetFileName(dir).ToLowerInvariant().Contains(query))
                {
                    yield return new FileNode
                    {
                        Id = dir,
                        Name = Path.GetFileName(dir),
                        Path = dir,
                        IsDirectory = true,
                        Icon = "images/folders-icon-col.svg",
                        Modified = Directory.GetLastWriteTime(dir),
                        HasChildren = DirectoryHasChildren(dir)
                    };
                }
                await foreach (var node in SearchRecursiveAsync(query, dir, ct))
                    yield return node;
            }
        }

        public async Task<string?> GetPreviewContentAsync(string path, int maxChars = 65536)
        {
            try
            {
                if (!IsTextFile(path)) return null;
                using var sr = new StreamReader(path, detectEncodingFromByteOrderMarks: true);
                var buffer = new char[maxChars];
                var read = await sr.ReadAsync(buffer, 0, maxChars);
                return new string(buffer, 0, read);
            }
            catch { return null; }
        }

        public bool IsTextFile(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant().TrimStart('.');
            return ext is "txt" or "log" or "ini" or "conf" or "cfg" or "json" or "xml"
                or "yaml" or "yml" or "md" or "csv" or "cs" or "js" or "ts" or "html"
                or "htm" or "css" or "py" or "java" or "cpp" or "h" or "c" or "bat"
                or "sh" or "ps1" or "sql" or "toml" or "rs" or "env";
        }

        public string GetIconForFile(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant().TrimStart('.');
            return ext switch
            {
                "doc" or "docx" => "images/square/doc.svg",
                "txt" or "log" or "ini" or "conf" => "images/txt.svg",
                "csv" or "xls" or "xlsx" or "xlsm" or "xlsb" => "images/square/xlsx.svg",
                "odt" => "images/odt.svg",
                "lnk" => "images/lnk.svg",
                "rvt" or "rte" or "rfa" => "images/revit.svg",
                "bat" => "images/bat.svg",
                "msi" => "images/msi.svg",
                "db" or "database" or "db3" or "sql" => "images/square/db.svg",
                "exe" => "images/exe.svg",
                "dwg" => "images/dwg.svg",
                "pdf" => "images/pdf.svg",
                "png" or "jpg" or "jpeg" or "bmp" or "gif" or "webp" => "images/jpg.svg",
                "7z" or "zip" or "rar" or "gz" or "tar" => "images/zip.svg",
                _ => "images/blank.svg"
            };
        }

        public string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1_048_576) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1_073_741_824) return $"{bytes / 1_048_576.0:F1} MB";
            return $"{bytes / 1_073_741_824.0:F1} GB";
        }
    }
}
