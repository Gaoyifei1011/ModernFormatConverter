namespace ModernFormatConverter.Views.Controls
{
    /// <summary>
    /// This denotes the format used when saving a bitmap to a file
    /// </summary>
    public enum BitmapFileFormat
    {
        /// <summary>
        /// Indicates Windows Imaging Component's bitmap encoder
        /// </summary>
        Bmp,

        /// <summary>
        /// Indicates Windows Imaging Component's PNG encoder
        /// </summary>
        Png,

        /// <summary>
        /// Indicates Windows Imaging Component's bitmap JPEG encoder
        /// </summary>
        Jpeg,

        /// <summary>
        /// Indicates Windows Imaging Component's TIFF encoder
        /// </summary>
        Tiff,

        /// <summary>
        /// Indicates Windows Imaging Component's GIF encoder
        /// </summary>
        Gif,

        /// <summary>
        /// Indicates Windows Imaging Component's JPEGXR encoder
        /// </summary>
        JpegXR
    }
}
