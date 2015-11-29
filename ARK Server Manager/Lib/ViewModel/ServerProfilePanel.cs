using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ARK_Server_Manager.Lib.ViewModel
{
    public class ServerProfilePanel
    {
        public string Title { get; private set; }
        public ContentControl Control { get; private set; } 

        public ServerProfilePanel(string title, ContentControl panel)
        {
            this.Title = title;
            this.Control = panel;
        }
    }
}
