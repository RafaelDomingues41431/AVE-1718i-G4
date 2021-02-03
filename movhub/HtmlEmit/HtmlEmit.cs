using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace HtmlEmiters
{
    interface PropertyInfoGetter
    {
        string GetHtmlString(object target);
    }

    public abstract class AbstractPropObjGetter : PropertyInfoGetter
    {
        public abstract string GetHtmlString(object target);

        public static string FormatNotIgnoreToHtml(string name, object value)
        {
            return String.Format("<li class='list-group-item'><strong>{0}</strong>:{1}</li>", name, value);
        }

        public static string FormatHtmlAsToHtml(string name, object value, string htmlRef)
        {
            return htmlRef.Replace("{name}", name.Replace("{value}", value.ToString()));
        }
    }

    public abstract class AbstractPropArrayGetter : PropertyInfoGetter
    {
        public abstract string GetHtmlString(object target);
        public static string FormatArrayToHtmlHeader(object[] obj)
        { 
            string header = "<table class ='table table-hover'> <thead> <tr>";
            header += HtmlEmit.GetArrayHeader(obj[0]);
            header += "</tr> </thead>";
            return header;
        }

        public static string FormatHeader(string name) => String.Format("<th>" + name + " </th>");
        public static string FormatBody(object value) => String.Format("<tr>" + value.ToString()  + "</tr>");
        public static string FormatBodyHtmlAs(string name, object value, string htmlRef) {
            return htmlRef.Replace("{name}", name.Replace("{value}", value.ToString()));
        }
        public static string FormatArrayToHtmlBody(object []obj)
        {
            string body = "<tbody>";
            for (int i = 0; i < obj.Length; i++) {

                body += HtmlEmit.GetArrayBody(obj[i]);
                    
            } 
            body+= "</tbody>";
            return body;
        }
        
    }


    public class HtmlEmit
    {
        static readonly MethodInfo FormatNotIgnoreToHtml = typeof(AbstractPropObjGetter).GetMethod("FormatNotIgnoreToHtml", new Type[] { typeof(String), typeof(object) });
        static readonly MethodInfo FormatHtmlAsToHtml = typeof(AbstractPropObjGetter).GetMethod("FormatHtmlAsToHtml", new Type[] {  typeof(String), typeof(object), typeof(String) });
        static readonly MethodInfo FormatArrayToHtmlHeader = typeof(AbstractPropArrayGetter).GetMethod("FormatArrayToHtmlHeader", new Type[] { typeof(object[]) });
        static readonly MethodInfo FormatArrayToHtmlBody = typeof(AbstractPropArrayGetter).GetMethod("FormatArrayToHtmlBody", new Type[] { typeof(object[]) });
        static readonly MethodInfo FormatHeader = typeof(AbstractPropArrayGetter).GetMethod("FormatHeader", new Type[] { typeof(string) });
        static readonly MethodInfo FormatBody = typeof(AbstractPropArrayGetter).GetMethod("FormatBody", new Type[] { typeof(object) });
        static readonly MethodInfo FormatBodyHtmlAs = typeof(AbstractPropArrayGetter).GetMethod("FormatBodyHtmlAs", new Type[] { typeof(string), typeof(object), typeof(string)});
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

            StringBuilder sb = new StringBuilder();
            sb.Append("<ul class='list-group'>");
            sb.Append(getter.GetHtmlString(obj));
            sb.Append("</ul>");
            return sb.ToString();
        }


        public string ToHtml(object[] arr)
        {
            if (arr == null || arr.Length == 0) return null;

            Type objType = arr.GetType();
            PropertyInfoGetter getter;
            if (!markedProps.TryGetValue(objType, out getter))
            {
                getter = EmitArray(objType);
                markedProps.Add(objType, getter);
            }
            return getter.GetHtmlString(arr);
        }


        private PropertyInfoGetter EmitObjectGetter(Type objType)
        {
            string name = objType.Name + "PropertyObjectInfoGetter";
            AssemblyName asmName = new AssemblyName(name);
            
            AssemblyBuilder asmB = CreateAsm(asmName);
            
            ModuleBuilder moduleB = CreateModule(name, asmB);

            TypeBuilder typeB = CreateType(name, moduleB, typeof(AbstractPropObjGetter));
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

            asmB.Save(asmName.Name + ".dll");

            return (PropertyInfoGetter)Activator.CreateInstance(getterType);
        }
        private PropertyInfoGetter EmitArray(Type objType)
        {
            string name = objType.Name + "PropertyArrayInfoGetter";
            AssemblyName asmName = new AssemblyName(name);

            AssemblyBuilder asmB = CreateAsm(asmName);

            ModuleBuilder moduleB = CreateModule(name, asmB);

            TypeBuilder typeB = CreateType(name, moduleB, typeof(AbstractPropArrayGetter));

            // Build the method GetHtmlString()
            MethodBuilder methodB = CreateGetHtmlStringMethod(typeB);

            ILGenerator il = methodB.GetILGenerator();


            LocalBuilder target = il.DeclareLocal(objType);
            il.Emit(OpCodes.Ldarg_1);          // push target
            il.Emit(OpCodes.Castclass, objType); // castclass
            il.Emit(OpCodes.Stloc, target);    // store on local variable 

            il.Emit(OpCodes.Ldstr, "");
            //Get the HTML Header 
            il.Emit(OpCodes.Ldloc_0);          // push target
            il.Emit(OpCodes.Call, FormatArrayToHtmlHeader);  // get the header HTML
            il.Emit(OpCodes.Call, concat);

            //GET the HTML BODY
            il.Emit(OpCodes.Ldloc_0);          // push target
            il.Emit(OpCodes.Call, FormatArrayToHtmlBody);  // get the header HTML
            il.Emit(OpCodes.Call, concat);
    
            il.Emit(OpCodes.Ret);              // ret
            Type getterType = typeB.CreateType();

            asmB.Save(asmName.Name + ".dll");

            return (PropertyInfoGetter)Activator.CreateInstance(getterType);
        }

        private static PropertyInfoGetter EmitArrayBody(Type objType)
        {
            string name = objType.Name + "PropertyValueInArrayBody";
            AssemblyName asmName = new AssemblyName(name);

            AssemblyBuilder asmB = CreateAsm(asmName);

            ModuleBuilder moduleB = CreateModule(name, asmB);

            TypeBuilder typeB = CreateType(name, moduleB, typeof(AbstractPropArrayGetter));
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
                    il.Emit(OpCodes.Call, FormatBody);
                else
                {
                    string htmlAs = htmlAsAttrib[0].htmlRef;
                    il.Emit(OpCodes.Ldstr, htmlAs);
                    il.Emit(OpCodes.Call, FormatBodyHtmlAs);
                }
                il.Emit(OpCodes.Call, concat);
            }
            il.Emit(OpCodes.Ret);              // ret
            Type getterType = typeB.CreateType();

            asmB.Save(asmName.Name + ".dll");

            return (PropertyInfoGetter)Activator.CreateInstance(getterType);
        }

        private static PropertyInfoGetter EmitArrayHeader(Type objType)
        {
            string name = objType.Name + "PropertyNameInArrayHeader";
            AssemblyName asmName = new AssemblyName(name);

            AssemblyBuilder asmB = CreateAsm(asmName);

            ModuleBuilder moduleB = CreateModule(name, asmB);

            TypeBuilder typeB = CreateType(name, moduleB, typeof(AbstractPropArrayGetter));
            // Build the method GetHtmlString()
            MethodBuilder methodB = CreateGetHtmlStringMethod(typeB);

            ILGenerator il = methodB.GetILGenerator();

            LocalBuilder target = il.DeclareLocal(objType);
            il.Emit(OpCodes.Ldarg_1);          // push target
            il.Emit(OpCodes.Castclass, objType); // castclass
            il.Emit(OpCodes.Stloc, target);    // store on local variable 

            PropertyInfo[] props = objType.GetProperties();

            il.Emit(OpCodes.Ldstr, "");
            foreach (PropertyInfo p in props)
            {
                object[] attrs = p.GetCustomAttributes(typeof(HtmlIgnoreAttribute), true);
                if (attrs.Length != 0) continue;
                il.Emit(OpCodes.Ldstr, p.Name);    // push on stack the property name
                il.Emit(OpCodes.Call, FormatHeader);
                il.Emit(OpCodes.Call, concat);
            }
            il.Emit(OpCodes.Ret);              // ret
            Type getterType = typeB.CreateType();

            asmB.Save(asmName.Name + ".dll");

            return (PropertyInfoGetter)Activator.CreateInstance(getterType);
        }

        //maps a type to its emited methods
        static Dictionary<Type, PropertyInfoGetter> bodyValuesInArray = new Dictionary<Type, PropertyInfoGetter>();

        public static string GetArrayBody(object obj)
        {
            Type objType = obj.GetType();
            PropertyInfoGetter getter;
            if (!bodyValuesInArray.TryGetValue(objType, out getter))
            {
                getter = EmitArrayBody(objType);
                bodyValuesInArray.Add(objType, getter);
            }
            return getter.GetHtmlString(obj);
        }

        //maps a type to its emited methods
        static Dictionary<Type, PropertyInfoGetter> headerValueInArray = new Dictionary<Type, PropertyInfoGetter>();

        public static  string GetArrayHeader(object property)
        {
            Type objType = property.GetType();
            PropertyInfoGetter getter;
            if (!bodyValuesInArray.TryGetValue(objType, out getter))
            {
                getter = EmitArrayHeader(objType);
                bodyValuesInArray.Add(objType, getter);
            }
            return getter.GetHtmlString(property);
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

   
    }
}

