using System.Collections.Generic;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 主页面项目分组模型
    /// </summary>
    internal sealed partial class HomeItemGroupModel
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 主页面项目数据列表
        /// </summary>
        public List<HomeItemModel> HomeItemList { get; set; }
    }
}
