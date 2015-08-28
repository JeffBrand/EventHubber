using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventHubber.Services
{
    public interface IDialogService
    {
        void ShowDialog(string dialogName, object data = null);
    }
}
