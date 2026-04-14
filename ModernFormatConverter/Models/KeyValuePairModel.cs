using System.ComponentModel;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 键值对数据模型
    /// </summary>
    public class KeyValuePairModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 是否被选中
        /// </summary>
        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }

            set
            {
                _isChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
            }
        }

        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
