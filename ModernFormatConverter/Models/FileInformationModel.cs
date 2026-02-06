namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 文件信息数据模型
    /// </summary>
    public class FileInformationModel
    {
        /// <summary>
        /// 文件类型
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public string FileSize { get; set; }

        /// <summary>
        /// 占用空间
        /// </summary>
        public string SpaceUsage { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public string ModifyTime { get; set; }

        /// <summary>
        /// 访问时间
        /// </summary>
        public string AccessTime { get; set; }
    }
}
