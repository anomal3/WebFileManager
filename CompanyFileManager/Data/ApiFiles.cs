namespace CompanyFileManager.Data
{
    public class ApiFiles
    {
        public static List<ApiFiles> ServerFiles { get; set; } = new List<ApiFiles>();
        private static string folderIcon = "images/folders-icon-col.svg";
        private static string fileIcon = "images/document.svg";

        public static void Initialize(string path)
        {

            if (ServerFiles.Count > 0) ServerFiles.Clear();

            foreach (var dir in Directory.GetDirectories(path))
            {

                FolderSwitcher(dir);

                ServerFiles.Add(new ApiFiles()
                {
                    Name = Path.GetFileName(dir),
                    Id = dir,
                    IsDirectory = true,
                    Type = "container",
                    Icon = folderIcon,
                });

            }

            foreach (var file in Directory.GetFiles(path))
            {
                IconsSwitcher(file);

                ServerFiles.Add(new ApiFiles()
                {
                    Name = Path.GetFileName(file),
                    Id = file,
                    Type = "container",
                    Icon = fileIcon,
                    NavigateUrl = $"/File/download?link={file}"
                });
            }

        }

        public static void SubDir(string path)
        {

            foreach (var dir in Directory.GetDirectories(path))
            {
                if (!ApiFiles.ServerFiles.Any(n => n.Id == dir))
                    ServerFiles.Add(new ApiFiles()
                    {
                        Name = Path.GetFileName(dir),
                        Id = dir,
                        ParentId = path,
                        Type = "container",
                        Icon = folderIcon
                    });
            }

            foreach (var file in Directory.GetFiles(path))
            {
                IconsSwitcher(Path.GetFileName(file));

                if (!ServerFiles.Any(n => n.Id == file))
                    ServerFiles.Add(new ApiFiles()
                    {
                        Name = Path.GetFileName(file),
                        Id = file,
                        ParentId = path,
                        Type = "container",
                        Icon = fileIcon,
                        NavigateUrl = $"/File/download?link={file}"
                    });
            }
        }

        static void ProcessDirectory(string targetDirectory)
        {
            // Рекурсивно вызываем этот метод для всех подпапок в текущей директории
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                ServerFiles.Add(new ApiFiles() { Name = Path.GetFileName(subdirectory), Id = subdirectory, ParentId = Path.GetDirectoryName(subdirectory), Type = "container" });
                ProcessDirectory(subdirectory);
            }

        }

        /// <summary>
        /// Меняет ссылку на иконку в зависимости от расширения
        /// </summary>
        /// <param name="filename"></param>
        static void IconsSwitcher(string filename)
        {
            switch (Path.GetFileName(filename).Substring(Path.GetFileName(filename).LastIndexOf(".") + 1))
            {
                case "doc":
                case "docx":
                    fileIcon = "images/square/doc.svg";
                    break;
                case "log":
                case "txt":
                case "ini":
                case "conf":
                    fileIcon = "images/txt.svg"; break;

                case "csv":
                case "xls":
                case "xlsx":
                case "xlsm":
                case "xlsb":
                    fileIcon = "images/square/xlsx.svg"; break;

                case "odt": fileIcon = "images/odt.svg"; break;

                case "lnk": fileIcon = "images/lnk.svg"; break;


                case "rvt":
                case "rte":
                case "rfa": fileIcon = "images/revit.svg"; break;


                
                case "bat": fileIcon = "images/bat.svg"; break;
                
                
                case "msi": fileIcon = "images/msi.svg"; break;


                case "db":
                case "database":
                case "db3":
                case "sbyte":
                case "sql":
                    fileIcon = "images/square/db.svg"; break;


                case "exe":
                    fileIcon = "images/exe.svg"; break;

                case "dwg":
                    fileIcon = "images/dwg.svg"; break;

                case "pdf":
                    fileIcon = "images/pdf.svg"; break;

                case "png":
                case "jpg":
                case "jpeg":
                case "bmp":
                    fileIcon = "images/jpg.svg"; break;

                case "7z":
                case "zip":
                case "rar":
                case "tar.gz":
                    fileIcon = "images/zip.svg"; break;

                default:
                    fileIcon = "images/blank.svg"; break;
            }
        }

        static void FolderSwitcher(string folername)
        {
            switch (Path.GetFileName(folername))
            {
                case "App_Data":
                    folderIcon = "images/folders-noaccess.svg";
                    break;

                default:
                    folderIcon = "images/folders-icon-col.svg";
                    break;
            }
        }

        public string Id { get; set; }
        public string Style => "icons-style";
        public string ParentId { get; set; }
        public string Icon { get; set; }

        public string Name { get; set; }
        public string Type { get; set; }

        public bool IsDirectory { get; set; }

        public string IconCssCalss { get; set; }

        public string enables { get; set; }

        public string NavigateUrl { get; set; }

    }
}

