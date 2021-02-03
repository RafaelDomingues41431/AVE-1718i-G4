using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlEmiters
{

    public class CustomAttribute : Attribute { }

    public class HtmlIgnoreAttribute : CustomAttribute 
    {
    }

    public class HtmlAsAttribute : CustomAttribute
    {
        public string htmlRef { get; set; }
        public HtmlAsAttribute(string htmlRef)
        {
            this.htmlRef = htmlRef;
        }

    }
}
