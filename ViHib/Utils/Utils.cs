using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;


namespace ViHib
{
     static class UtilsClass
    {
        public const string ValAll = "0";
        public const string TextAll = "Все";

        /// <summary>
        /// Преобразование типов
        /// </summary>
        /// <typeparam name="T">Итоговый тип</typeparam>
        /// <param name="o">Объект для преобразование</param>
        /// <param name="def">Значение по умолчанию (на случай неудачи преобразования)</param>
        /// <returns>Результат преобразования</returns>
        public static T ConvertTo<T>(object o, T def = default(T))
        {
            try
            {
                var type = typeof(T);
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition())
                {
                    type = Nullable.GetUnderlyingType(type);
                }

                return (T)Convert.ChangeType(o, type);
            }
            catch (Exception)
            {
                return def;
            }
        }

        /// <summary>
        /// Определяет вхождение элимента в коллекцию
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">Объект для прорки</param>
        /// <param name="items">Коллекция</param>
        /// <returns>Входит</returns>
        public static bool In<T>(this T item, params T[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));


            return items.Contains(item);
        }

        /// <summary>
        /// Привести к заданному типу
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="def">Значение по умолчанию (на случай неудачи преобразования)</param>
        /// <returns>Результат преобразования</returns>
        public static T ConvertItTo<T>(this object o, T def = default(T))
        {
            return ConvertTo<T>(o, def);
        }

        /// <summary>
        /// Собирает ФИО из составляющих
        /// </summary>
        /// <param name="LastName">Фамилия</param>
        /// <param name="FirstName">Имя</param>
        /// <param name="MiddleName">Отчество</param>
        /// <returns>И.О.Фамилия</returns>
        public static string GetFio(string LastName, string FirstName, string MiddleName)
        {
            return FirstName.Substring(0, 1) + "." + MiddleName.Substring(0, 1) + "." + LastName;
        }

        /// <summary>
        /// Приводит нуловые объекты к DBNull
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object ToDBnull(this object o)
        {
            return o ?? DBNull.Value;
        }

        /// <summary>
        /// преобразует пустые строки в текстовое значение 'null'
        /// </summary>
        /// <param name="o">Строковое представление</param>
        /// <returns>результат преобразования</returns>
        public static string ToDBnullStr(this object o)
        {
            return string.IsNullOrEmpty(o?.ToString()) ? "null" : o.ToString();
        }

        /// <summary>
        /// Преобразует строки в number для запросов БД
        /// </summary>
        /// <param name="o">строковое представление</param>
        /// <returns>результат приведения</returns>
        public static string ToDBnullDouble(this string o)
        {
            if (String.IsNullOrEmpty(o))
                return "null";
            return o.Replace(',', '.');
        }

        public static Dictionary<string , object> ReaderToDic(DbDataReader reader)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();

            for(int i=0;i<reader.FieldCount;i++)
            {
                ret.Add(reader.GetName(i), reader[i]);
            }

            return ret;

        }
    }

}
