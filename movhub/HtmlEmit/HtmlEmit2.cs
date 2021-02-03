using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;


namespace HtmlEmiters
{
    public abstract class AbstractGetter : PropertyInfoGetter
    {
        public abstract string GetHtmlString(object target);

        public static string FormatNotIgnoreToHtml(string name, object value)
        {
            return name + "#" + value + "$";
        }

        public static string FormatHtmlAsToHtml(string name, object value, string htmlRef)
        {
            return name + "#" + value + "#" + htmlRef + "$";
        }
        public static string FormatArrayToHtml(object[] obj)
        {
            HtmlEmit2 html = new HtmlEmit2();
            string arrayValues = "";
            for (int i = 0; i < obj.Length; i++)
            {
                arrayValues += html.ConvertArrayToHtml(obj[i]) + "@";
            }
            return arrayValues;
        }

    }
    public class HtmlEmit2
    {
        static readonly MethodInfo FormatNotIgnoreToHtml = typeof(AbstractGetter).GetMethod("FormatNotIgnoreToHtml", new Type[] { typeof(String), typeof(object) });
        static readonly MethodInfo FormatHtmlAsToHtml = typeof(AbstractGetter).GetMethod("FormatHtmlAsToHtml", new Type[] { typeof(String), typeof(object), typeof(String) });
        static readonly MethodInfo FormatArrayToHtml = typeof(AbstractGetter).GetMethod("FormatArrayToHtml", new Type[] { typeof(object[]) });
        static readonly MethodInfo concat = typeof(String).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });

        //maps a type to its emited methods
        static Dictionary<Type, PropertyInfoGetter> markedProps = new Dictionary<Type, PropertyInfoGetter>();
        public string ToHtml(object obj)
        {
            Type objType = obj.GetType();
            PropertyInfoGetter getter;
            if (!markedProps.TryGetValue(objType, out getter))
            {
                getter = EmitObjectGetter(objType);
                markedProps.Add(objType, getter);
            }
            string[] propsNameValue  = GetEachPropToStringArray(getter.GetHtmlString(obj));
          
            StringBuilder sb = new StringBuilder();
            sb.Append("<ul class='list-group'>");

            for (int i=0; i < propsNameValue.Length; i++)
            {

                string[] separatedValues = GetThisPropertyToStringArray(propsNameValue[i]);
                if (separatedValues.Length == 1) continue;
                string propName  = separatedValues[0];
                string propValue = separatedValues[1]; 
                // if this property has 3 strings it means it has HTMLAs Atttribute
                if (separatedValues.Length == 3)
                {
                    string htmlRef = separatedValues[2];

                    sb.Append(htmlRef.Replace("{name}", propName).Replace("{value}", propValue));
                    continue;
                }
              
                sb.Append(String.Format("<li class='list-group-item'><strong>{0}</strong>:{1}</li>", propName, propValue));
            }
            sb.Append("</ul>");
            return sb.ToString();
        }



        public string ToHtml(object[] arr)
        {
            if (arr == null || arr.Length == 0) return null;

            StringBuilder tableHeader = new StringBuilder();
            StringBuilder tableContent = new StringBuilder();

            Type objType = arr.GetType();
            PropertyInfoGetter getter;
            if (!markedProps.TryGetValue(objType, out getter))
            {
                getter = EmitArray(objType);
                markedProps.Add(objType, getter);
            }
      
            //table header 
            tableHeader.Append("<table class ='table table-hover'> <thead> <tr>");

            // split by object
            string[] propsArray = GetEachObjectToStringArray(getter.GetHtmlString(arr));
            // split by property 
            string[] thisProp = GetEachPropToStringArray(propsArray[0]);

            int numberOfProps = thisProp.Length;

            // Build the html Table Header 
            for (int i = 0; i < numberOfProps; i++) {
                // split property
                string[] propNameValue = thisProp[i].Split('#');
                if (propNameValue.Length == 1) continue;
                string propName = propNameValue[0];
                tableHeader.Append("<th>" + propName + "</th>");
            }
            tableHeader.Append("</tr> </thead>");

            //table content
            tableContent.Append("<tbody>");

            // Build each table row with the property values
            for (int i = 0; i < propsArray.Length; i++)
            {
                // split by property 
                string[] prop = GetEachPropToStringArray(propsArray[i]);
                tableContent.Append("<tr>");
                if (prop.Length == 1) continue;
                for (int j = 0; j < prop.Length; j++)
                {
                    // split property in name, value, htmlRef if it exists
                    string[] nameValue = GetThisPropertyToStringArray(prop[j]);
                    if (nameValue.Length == 1) continue;
                    // Get the property value 
                    string name = nameValue[0];
                    string value = nameValue[1];
                    if (nameValue.Length == 3)
                    {
                        string htmlRef = nameValue[2];
                        tableContent.Append(htmlRef.Replace("{name}", name).Replace("{value}", value));
                        continue;
                    }
                    tableContent.Append("<td>" + value + "</td>");
                }
                tableContent.Append("</tr>");
            }
            tableContent.Append("</tbody> </table>");



            return tableHeader.ToString() + tableContent.ToString(); ;
        }

        private PropertyInfoGetter EmitObjectGetter(Type objType)
        {
            string name = objType.Name + "PropertyObjectInfoGetter";
            AssemblyName asmName = new AssemblyName(name);

            AssemblyBuilder asmB = CreateAsm(asmName);

            ModuleBuilder moduleB = CreateModule(name, asmB);

            TypeBuilder typeB = CreateType(name, moduleB, typeof(AbstractGetter));
            // Build the method GetHtmlString()
            MethodBuilder methodB = CreateGetHtmlStringMethod(typeB);

            ILGenerator il = methodB.GetILGenerator();

            PropertyInfo[] props = objType.GetProperties();

            LocalBuilder target = il.DeclareLocal(objType);
            il.Emit(OpCodes.Ldarg_1);          // push target
            il.Emit(OpCodes.Castclass, objType); // castclass
            il.Emit(OpCodes.Stloc, target);    // store on local variable 

            il.Emit(OpCodes.Ldstr, "");
            foreach (PropertyInfo p in props)
            {
                object[] attrs = p.GetCustomAttributes(typeof(HtmlIgnoreAttribute), true);
                if (attrs.Length != 0) continue;
                il.Emit(OpCodes.Ldstr, p.Name);    // push on stack the property name

                // Get this property get Method
                MethodInfo pGetMethod = objType.GetProperty(p.Name,
                   BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                   ).GetGetMethod(true);

                il.Emit(OpCodes.Ldloc_0);                  // push target
                // Using this property _GetMethod() gets the property value
                il.Emit(OpCodes.Callvirt, pGetMethod);     // push property value 
                if (p.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, p.PropertyType);  // box

                // If this property has HtmlAs custom attribute executes FormatHtmlAsToHtml
                HtmlAsAttribute[] htmlAsAttrib = (HtmlAsAttribute[])p.GetCustomAttributes(typeof(HtmlAsAttribute), true);
                if (htmlAsAttrib.Length == 0)
                    il.Emit(OpCodes.Call, FormatNotIgnoreToHtml);
                else
                {
                    string htmlAs = htmlAsAttrib[0].htmlRef;
                    il.Emit(OpCodes.Ldstr, htmlAs);
                    il.Emit(OpCodes.Call, FormatHtmlAsToHtml);
                }
                il.Emit(OpCodes.Call, concat);
            }
            il.Emit(OpCodes.Ret);              // ret
            Type getterType = typeB.CreateType();

            //asmB.Save(asmName.Name + ".dll");

            return (PropertyInfoGetter)Activator.CreateInstance(getterType);
        }
        private PropertyInfoGetter EmitArray(Type objType)
        {
            string name = objType.Name + "PropertyArrayInfoGetter";
            AssemblyName asmName = new AssemblyName(name);

            AssemblyBuilder asmB = CreateAsm(asmName);

            ModuleBuilder moduleB = CreateModule(name, asmB);

            TypeBuilder typeB = CreateType(name, moduleB, typeof(AbstractGetter));

            // Build the method GetHtmlString()
            MethodBuilder methodB = CreateGetHtmlStringMethod(typeB);

            ILGenerator il = methodB.GetILGenerator();

     

            LocalBuilder target = il.DeclareLocal(objType);
            il.Emit(OpCodes.Ldarg_1);          // push target
            il.Emit(OpCodes.Castclass, objType); // castclass
            il.Emit(OpCodes.Stloc, target);    // store on local variable 

            il.Emit(OpCodes.Ldloc_0);          // push target
            il.Emit(OpCodes.Call, FormatArrayToHtml);

            il.Emit(OpCodes.Ret);              // ret


            Type getterType = typeB.CreateType();

            asmB.Save(asmName.Name + ".dll");

            return (PropertyInfoGetter)Activator.CreateInstance(getterType);
        }
        internal string ConvertArrayToHtml(object obj)
        {
            Type objType = obj.GetType();
            PropertyInfoGetter getter;
            if (!markedProps.TryGetValue(objType, out getter))
            {
                getter = EmitObjectGetter(objType);
                markedProps.Add(objType, getter);
            }
            return getter.GetHtmlString(obj);
        }
        private static TypeBuilder CreateType(string name, ModuleBuilder moduleB, Type objType)
        {
            return moduleB.DefineType(
                name,
                TypeAttributes.Public,
                objType);
        }
        private static ModuleBuilder CreateModule(string name, AssemblyBuilder asmB)
        {
            return asmB.DefineDynamicModule(name, name + ".dll");
        }
        private static AssemblyBuilder CreateAsm(AssemblyName asmName)
        {
            return AppDomain.CurrentDomain.DefineDynamicAssembly(
                    asmName,
                    AssemblyBuilderAccess.RunAndSave);
        }
        private static MethodBuilder CreateGetHtmlStringMethod(TypeBuilder typeB)
        {
            MethodBuilder methodB = typeB.DefineMethod(
                "GetHtmlString",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.ReuseSlot,
                typeof(string), // Return type
                new Type[] { typeof(object) }); // Type of arguments
            return methodB;
        }
        private string[] GetEachObjectToStringArray(string objects)
        {
            return objects.Split('@'); ;
        }
        private string[] GetThisPropertyToStringArray(string property)
        {
            return property.Split('#'); ;
        }
        private string[] GetEachPropToStringArray(string properties)
        {
            return properties.Split('$');
        }
    }
}

