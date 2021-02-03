using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HtmlEmiters;

namespace HtmlReflect
{
    interface IGetter
    {
        string GetPropertyName();
        string GetValueAsString(object target);
        string GetHtmlString();
    }

    class GetterObject : IGetter
    {
        PropertyInfo p;
        public GetterObject(PropertyInfo p)
        {
            this.p = p;
        }
        public string GetPropertyName()
        {
            return p.Name;
        }
        public string GetValueAsString(object target)
        {
            return p.GetValue(target) + "";
        }
        public string GetHtmlString() {
            HtmlAsAttribute attribute = (HtmlAsAttribute)p.GetCustomAttribute(typeof(HtmlAsAttribute));
            if (attribute != null)
                return attribute.htmlRef;

            return null;
        }
    }

    public class HtlmReflect
    {
        static Dictionary<Type, List<IGetter>> markedProps = new Dictionary<Type, List<IGetter>>();

        public string ToHtml(object obj)
        {
            List<IGetter> props = new List<IGetter>();
            props = GetPropsWithCustomAttrib(obj.GetType());

            StringBuilder sb = new StringBuilder();
            sb.Append("<ul class='list-group'>");

            foreach (IGetter getter in props)
            {
                string htmlRef = getter.GetHtmlString();
                if (htmlRef != null)
                {
                    sb.Append(htmlRef.Replace("{name}", getter.GetPropertyName()).Replace("{value}", getter.GetValueAsString(obj)));
                    continue;
                }

                sb.Append(String.Format("<li class='list-group-item'><strong>{0}</strong>:{1}</li>", getter.GetPropertyName(), getter.GetValueAsString(obj)));
            }
            sb.Append("</ul>");
            return sb.ToString();
        }

        public string ToHtml(object[] arr)
        {
            if (arr == null || arr.Length == 0) return null;

            StringBuilder tableHeader = new StringBuilder();
            StringBuilder tableContent = new StringBuilder();

            List<IGetter> props = GetPropsWithCustomAttrib(arr[0].GetType());

            //table header 
            tableHeader.Append("<table class ='table table-hover'> <thead> <tr>");

            foreach (IGetter getter in props)
            {
                tableHeader.Append("<th>" + getter.GetPropertyName() + "</th>");
            }
            tableHeader.Append("</tr> </thead>");

            //table content
            tableContent.Append("<tbody>");
            //each object is a table row starts at <tr> ends in </tr>
            foreach (object currObject in arr)
            {
                tableContent.Append("<tr>");

                //each property is table data in the current row starts at <td> ends in </td>
                foreach (IGetter getter in props)
                {
                    string htmlRef = getter.GetHtmlString();
                    if (htmlRef != null)
                    {
                        tableContent.Append(
                            htmlRef.Replace("{name}", 
                            getter.GetPropertyName()).Replace("{value}",
                            getter.GetValueAsString(currObject))
                        );
                        continue;
                    }
                    tableContent.Append("<td>" + getter.GetValueAsString(currObject) + "</td>");
                }
                tableContent.Append("</tr>");
            }
            tableContent.Append("</tbody> </table>");
            return tableHeader.ToString() + tableContent.ToString();
        }

        private List<IGetter> GetPropsWithCustomAttrib(Type klass)
        {
            List<IGetter> res;
            if (markedProps.TryGetValue(klass, out res)) return res;

            PropertyInfo[] props = klass.GetProperties();

            res = new List<IGetter>();
            foreach (PropertyInfo p in props)
            {
                object[] attrs = p.GetCustomAttributes(typeof(HtmlIgnoreAttribute), true);
                if (attrs.Length != 0) continue;
                res.Add(new GetterObject(p));
            }
            markedProps.Add(klass, res);
            return res;
        }
    }

 
}
