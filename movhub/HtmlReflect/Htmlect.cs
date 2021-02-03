using HtmlEmiters;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HtmlReflect
{
    public class Htmlect
    {
        /// maps between a Type and its Properties that aren't marked with the custom attribute HtmlIgnore
        static Dictionary<Type, List<PropertyInfo>> notIgnoredProperties = new Dictionary<Type, List<PropertyInfo>>();
        /// maps between a Type and its Properties that are marked with the custom attribute HtmlAs
        static Dictionary<PropertyInfo, string> htmlAsProperties = new Dictionary<PropertyInfo, string>();

        public string ToHtml(object obj)
        {
            List<PropertyInfo> props = new List<PropertyInfo>();
            props = GetPropsWithCustomAttrib(obj.GetType());

            StringBuilder sb = new StringBuilder();
            sb.Append("<ul class='list-group'>");

            foreach (PropertyInfo p in props)
            {
                string htmlRef = GetHtmlAsAttribStringRef(p);
                if (htmlRef != null)
                {
                    sb.Append(htmlRef.Replace("{name}", p.Name).Replace("{value}", p.GetValue(obj).ToString()));
                    continue;
                }

                sb.Append(String.Format("<li class='list-group-item'><strong>{0}</strong>:{1}</li>", p.Name, p.GetValue(obj)));
            }
            sb.Append("</ul>");
            return sb.ToString();
        }

        public string ToHtml(object[] arr)
        {
            if (arr == null || arr.Length == 0) return null;

            StringBuilder tableHeader = new StringBuilder();
            StringBuilder tableContent = new StringBuilder();

            List<PropertyInfo> props = GetPropsWithCustomAttrib(arr[0].GetType());

            //table header 
            tableHeader.Append("<table class ='table table-hover'> <thead> <tr>");

            foreach (PropertyInfo currProperty in props)
            {
                tableHeader.Append("<th>" + currProperty.Name + "</th>");
            }
            tableHeader.Append("</tr> </thead>");

            //table content
            tableContent.Append("<tbody>");
            //each object is a table row starts at <tr> ends in </tr>
            foreach (object currObject in arr)
            {
                tableContent.Append("<tr>");

                //each property is table data in the current row starts at <td> ends in </td>
                foreach (PropertyInfo currObjectProperty in props)
                {
                    string htmlRef = GetHtmlAsAttribStringRef(currObjectProperty);
                    if (htmlRef != null)
                    {
                        tableContent.Append(htmlRef.Replace("{name}", currObjectProperty.Name).Replace("{value}", currObjectProperty.GetValue(currObject).ToString()));
                        continue;
                    }
                    tableContent.Append("<td>" + currObjectProperty.GetValue(currObject) + "</td>");
                }
                tableContent.Append("</tr>");
            }
            tableContent.Append("</tbody> </table>");
            return tableHeader.ToString() + tableContent.ToString();
        }

        /// <summary>
        ///  Receives a type and checks if it exists in cache(dictionary notIgnoredProperties) 
        ///  If it exists returns the value stored for that property. 
        ///  If the type isn't cached it will check each every property of the type for the IgnoreAttribute.
        ///  If a given property isn't marked with this attribute it is added to cache.
        ///  It is also added to the list that will be returned.
        /// </summary>
        /// <param name="klass">
        ///  Type in which to check list of properties.
        /// </param>
        /// <returns>
        ///  Returns a list of properties not marked with HtmlIgnoreAttribute.
        /// </returns>

        private List<PropertyInfo> GetPropsWithCustomAttrib(Type klass)
        {
            List<PropertyInfo> res;
            if (notIgnoredProperties.TryGetValue(klass, out res)) return res;
            PropertyInfo[] props = klass.GetProperties();
            res = new List<PropertyInfo>();
            foreach (PropertyInfo p in props)
            {
                object[] attrs = p.GetCustomAttributes(typeof(HtmlIgnoreAttribute), true);
                if (attrs.Length != 0) continue;
                res.Add(p);
            }
            notIgnoredProperties.Add(klass, res);
            return res;
        }

        /// <summary>
        ///  Checks the cache (htmlsAsProperties dictionary ) if the given property exists.
        ///  If it exists returns the value stored in cache.
        ///  If the property doesn't exist in cache it checks via reflection for the  HtmlAsAttribute.
        ///  If the property is marked with this attribute the string value stored in the htmlRef field
        ///  of the HtmlAsAttribute is stored in cache, and is returned.
        /// </summary>
        /// <param name="p">The property in which there could be the HtmlAsAttribute</param>
        /// <returns>
        ///  String provided by custom attribute HtmlAsAttribute  
        ///  If theres HtmlAsAtttribute in p returns null.
        /// </returns>

        private string GetHtmlAsAttribStringRef(PropertyInfo p)
        {
            String htmlRef = "";
            if (htmlAsProperties.TryGetValue(p, out htmlRef)) return htmlRef;

            HtmlAsAttribute attribute = (HtmlAsAttribute)p.GetCustomAttribute(typeof(HtmlAsAttribute));
            if (attribute != null) {
                htmlAsProperties.Add(p, attribute.htmlRef);
                return attribute.htmlRef;
            }
            return null;
        }
    }

  
}
