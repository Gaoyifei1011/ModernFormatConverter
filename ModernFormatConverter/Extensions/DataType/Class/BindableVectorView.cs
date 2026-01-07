using Microsoft.UI.Xaml.Interop;
using System.Collections;

namespace ModernFormatConverter.Extensions.DataType.Class
{
    public class BindableVectorView(IList list) : BindableVector(list), IBindableVectorView
    {
        public override bool IsReadOnly
        {
            get { return true; }
        }
    }
}
