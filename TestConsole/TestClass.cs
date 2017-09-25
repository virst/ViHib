using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ConsoleApp2
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    class MyAttr : Attribute
    {
        public object Value { get; private set; }

        public MyAttr(object value)
        {
            Value = value;
        }
    }

    class TestClass
    {
        [MyAttr("str1")]
        public string Val1 { get; set; }

        [MyAttr(152)]
        public int Val2 { get; set; }

        public int Val3 { get; set; }

        [MyAttr("str4")]
        public string Val4;

        public TestClass()
        {
            // Получение списка свойств и полей класса с атрибутом MyAttr
            List<MemberInfo> options = GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(n => Attribute.IsDefined(n, typeof(MyAttr))).ToList();
            // Для каждого члена с атрибутом MyAttr
            foreach (MemberInfo info in options)
            {
                MyAttr myAttr = (MyAttr)info.GetCustomAttributes(typeof(MyAttr), true)[0];

                // Запись значение поля или свойства
                FieldInfo fieldInfo = info as FieldInfo;
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(this, myAttr.Value);
                }
                else
                {
                    PropertyInfo propertyInfo = (PropertyInfo)info;
                    propertyInfo.SetValue(this, myAttr.Value, null);
                }
            }
        }
    }
}
