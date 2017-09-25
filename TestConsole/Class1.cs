using ConsoleApp2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ConsoleApp2
{
    public class MyAttribute : Attribute // создаём собственный атрибут наследуясь от стандартного класса
    {
        public int Count { get; set; } // создаём своё свойство которое будет содержать атрибут
                                       // можно описать несколько свойств но для примера создаётся только одно
    }

    [MyAttribute(Count = 3)]
    public class IntArrayInitializer
    {
        public int[] GetArray()
        {
            var type = this.GetType(); // получение описания типа
            if (Attribute.IsDefined(type, typeof(MyAttribute))) // проверка на существование атрибута
            {
                var attributeValue = Attribute.GetCustomAttribute(type, typeof(MyAttribute)) as MyAttribute; // получаем значение атрибута
                return new int[attributeValue.Count]; // используем значение атрибута для формирования результата
            }
            return new int[0];
        }
    }
}

