using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    public interface IErrorSender
    {
        void SendError(string message);
    }

    public interface IFileDialog
    {
        public string OpenFileDialog();
        public string SaveFileDialog();
    }
}
