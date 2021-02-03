using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace HtmlEmit2
{
    namespace HtmlEmit2
    {
        public abstract class PropertyInfoGetter
        {
            public abstract string[] GetNames();

            public abstract object GetValue(string name);

            public abstract PropertyInfo GetPropertyInfo(string name);

        }

        class HtmlEmit
        {

            /// maps between a Type and its Propertie names that aren't marked with the custom attribute HtmlIgnore
            static Dictionary<Type, string[]> notIgnoredPropertieNames = new Dictionary<Type, string[]>();
            //maps a type to its emited methods
            static Dictionary<Type, PropertyInfoGetter> getters = new Dictionary<Type, PropertyInfoGetter>();
            /// maps between a Type and its Properties that are marked with the custom attribute HtmlAs
            static Dictionary<string, string> htmlAsProperties = new Dictionary<string, string>();


            public string ToHtml(object obj)
            {
                string[] propNames;
                PropertyInfoGetter getter;
                Type objType = obj.GetType();

                if (!getters.TryGetValue(objType, out getter))
                {
                    getter = CreateGetter(objType);
                    getters.Add(objType, getter);
                }

                propNames = GetPropNamesWithCustomAttrib(objType, getter);

                StringBuilder sb = new StringBuilder();
                sb.Append("<ul class='list-group'>");

                foreach (String p in propNames)
                {
                    string htmlRef = GetHtmlAsAttribStringRef(p, getter);
                    if (htmlRef != null)
                    {
                        sb.Append(htmlRef.Replace("{name}", (string)getter.GetValue(p)).Replace("{value}", (string)getter.GetValue(p)));
                        continue;
                    }

                    sb.Append(String.Format("<li class='list-group-item'><strong>{0}</strong>:{1}</li>", getter.GetValue(p), getter.GetValue(p)));
                }
                sb.Append("</ul>");
                return sb.ToString();
            }

            public string ToHtml(object[] arr)
            {
                return null;
            }

            private string[] GetPropNamesWithCustomAttrib(Type klass, PropertyInfoGetter getter)
            {
                string[] res;
                if (notIgnoredPropertieNames.TryGetValue(klass.GetType(), out res)) return res;

                res = getter.GetNames();

                return res;
            }

            private string GetHtmlAsAttribStringRef(string name, PropertyInfoGetter getter)
            {
                String htmlRef = "";
                if (htmlAsProperties.TryGetValue(name, out htmlRef)) return htmlRef;
                PropertyInfo p = getter.GetPropertyInfo(name);

                HtmlAsAttribute attribute = (HtmlAsAttribute)p.GetCustomAttribute(typeof(HtmlAsAttribute));
                if (attribute != null)
                {
                    htmlAsProperties.Add(name, attribute.htmlRef);
                    return attribute.htmlRef;
                }
                return null;
            }

            private PropertyInfoGetter CreateGetter(Type objType)
            {

                string name = objType.Name + "PropertyInfoGetter";
                AssemblyName asmName = new AssemblyName(name);

                AssemblyBuilder asmB = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);

                ModuleBuilder moduleB = asmB.DefineDynamicModule(name, name + ".dll");

                TypeBuilder typeB = moduleB.DefineType(name, TypeAttributes.Public, objType);

                MethodBuilder methodB = typeB.DefineMethod("GetNames", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.ReuseSlot, typeof(string[]), Type.EmptyTypes);

                ILGenerator ilG = methodB.GetILGenerator();

                //TODO GetNames

                methodB = typeB.DefineMethod("GetValue", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.ReuseSlot, typeof(Object), new Type[] { typeof(string) });

                //TODO GetValue

                Type getterType = typeB.CreateType();

                asmB.Save(asmName.Name + ".dll");

                return (PropertyInfoGetter)Activator.CreateInstance(getterType);
            }

            public class HtmlIgnoreAttribute : Attribute
            {
            }

            public class HtmlAsAttribute : Attribute
            {
                public string htmlRef { get; set; }
                public HtmlAsAttribute(string htmlRef)
                {
                    this.htmlRef = htmlRef;
                }

            }
        }
    }
}
