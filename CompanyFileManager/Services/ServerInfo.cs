namespace CompanyFileManager.Services
{
    /// <summary>
    /// Информация о текущем запущенном сервере (отличается от настроек после изменения).
    /// </summary>
    public class ServerInfo
    {
        public int LivePort { get; init; }
    }
}
