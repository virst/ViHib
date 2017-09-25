using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ViHib
{
    class HidTypeInfo
    {
        public enum HibTypeClass {Def , External , Xml };
        public class HidFieldInfo
        {
            public MemberInfo Mem;
            public string colName;
            public Type type;
            public HibTypeClass SQlType = HibTypeClass.Def;

            public object GetValue(object o)
            {
                FieldInfo fieldInfo = Mem as FieldInfo;
                if (fieldInfo != null)
                {
                    return fieldInfo.GetValue(o);
                }
                else
                {
                    PropertyInfo propertyInfo = (PropertyInfo)Mem;
                    return propertyInfo.GetValue(o);
                }
            }

            public void SetValue(object o,object v)
            {
                FieldInfo fieldInfo = Mem as FieldInfo;
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(o,v);
                }
                else
                {
                    PropertyInfo propertyInfo = (PropertyInfo)Mem;
                    propertyInfo.SetValue(o,v);
                }
            }

            public static Type MemType(MemberInfo Mem)
            {
                FieldInfo fieldInfo = Mem as FieldInfo;
                if (fieldInfo != null)
                {
                    return fieldInfo.FieldType;
                }
                else
                {
                    PropertyInfo propertyInfo = (PropertyInfo)Mem;
                    return propertyInfo.PropertyType;
                }
            }
        }

        public List<HidFieldInfo> fields = new List<HidFieldInfo>();

        public string TableName;
        public HidFieldInfo idInfo = new HidFieldInfo();
    }
}
