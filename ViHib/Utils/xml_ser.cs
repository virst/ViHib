using System;
using System.IO;
using System.Xml.Serialization;

namespace ViHib
{

    /// <summary>
    /// Класс для серриализация значений в строки
    /// </summary>
    /// <typeparam name="T">Серриализуемый тип</typeparam>
    static class XmlSer<T>
    {
        /// <summary>
        /// Преобразует объект в строковое значение
        /// </summary>
        /// <param name="obj">Объект для сериализации</param>
        /// <returns>строковое XML представление объекты </returns>
        public static string ToXmlString(T obj)
        {
            var serializer = new XmlSerializer(typeof(T));
            var writer = new StringWriter();
            serializer.Serialize(writer, obj);
            return writer.ToString();
        }

        /// <summary>
        /// Преобразует строковое XML представление в сам объект
        /// </summary>
        /// <param name="xml">строковое XML представление</param>
        /// <returns>Десерриализованный объект</returns>
        public static T FromString(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            var reader = new StringReader(xml);
            return (T)serializer.Deserialize(reader);
        }
    }

    static class XmlSer
    {
        /// <summary>
        /// Преобразует объект в строковое значение
        /// </summary>
        /// <param name="obj">Объект для сериализации</param>
        /// <returns>строковое XML представление объекты </returns>
        public static string ToXmlString(object obj,Type t)
        {
            var serializer = new XmlSerializer(t);
            var writer = new StringWriter();
            serializer.Serialize(writer, obj);
            return writer.ToString();
        }

        /// <summary>
        /// Преобразует строковое XML представление в сам объект
        /// </summary>
        /// <param name="xml">строковое XML представление</param>
        /// <returns>Десерриализованный объект</returns>
        public static object FromString(string xml, Type t)
        {
            var serializer = new XmlSerializer(t);
            var reader = new StringReader(xml);
            return serializer.Deserialize(reader);
        }
    }
}