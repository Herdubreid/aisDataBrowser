using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dragablz;

namespace Celin
{
    public class MainInterTabClient : IInterLayoutClient
    {
        public INewTabHost<UIElement> GetNewHost(object partition, TabablzControl source)
        {
            throw new NotImplementedException();
        }
    }
}
