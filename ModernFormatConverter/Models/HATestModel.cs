using ModernFormatConverter.Extensions.DataType.Enums;
using System.ComponentModel;

namespace ModernFormatConverter.Models
{
    /// <summary>
    /// 硬件加速测试数据模型
    /// </summary>
    public class HATestModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 硬件加速测试名称
        /// </summary>
        public string HATestName { get; set; }

        /// <summary>
        /// 硬件加速测试类型
        /// </summary>
        private HATestKind _haTestKind;

        public HATestKind HATestKind
        {
            get { return _haTestKind; }

            set
            {
                _haTestKind = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HATestKind)));
            }
        }

        /// <summary>
        /// 硬件加速测试结果类型
        /// </summary>
        private HATestResultKind _haTestResultKind;

        public HATestResultKind HATestResultKind
        {
            get { return _haTestResultKind; }

            set
            {
                _haTestResultKind = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HATestResultKind)));
            }
        }

        /// <summary>
        /// 硬件加速测试失败原因
        /// </summary>
        private string _haTestFailedReason;

        public string HATestFailedReason
        {
            get { return _haTestFailedReason; }

            set
            {
                _haTestFailedReason = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HATestFailedReason)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
