using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;

namespace Mvvm
{
    public class StringInterpolationException : Exception
    {
        public StringInterpolationException() : base() { }
        public StringInterpolationException(string msg) : base(msg) { }
        public StringInterpolationException(string msg, Exception ex) : base(msg, ex) { }
     }

    public static class StringInterpolation
    {
        public static string Do(string format, object o)
        {
            Contract.Requires(format != null);
            Contract.Requires(o != null);

            // find the indizes of the braces
            var openings = new List<int>();
            var closings = new List<int>();
            for (int i = 0; i < format.Length; i++)
            {
                if (format[i] == '{')
                    openings.Add(i);
                else if (format[i] == '}')
                    closings.Add(i);
            }
            // remove escaped
            int iOpening = 0;
            while (iOpening < openings.Count)
                if (openings.Count > iOpening + 1 && openings[iOpening + 1] == openings[iOpening] + 1)
                    openings.RemoveRange(iOpening, 2);
                else
                    iOpening += 1;
            int iClosing = 0;
            while (iClosing < closings.Count)
                if (closings.Count > iClosing + 1 && closings[iClosing + 1] == closings[iClosing] + 1)
                    closings.RemoveRange(iClosing, 2);
                else
                    iClosing += 1;
            if (openings.Count != closings.Count)
                throw new StringInterpolationException("The number of opening braces is not equal the number of closing braces.");

            if (openings.Count == 0)
                return format.Replace("{{", "{").Replace("}}", "}");

            var pairs = openings.Zip(closings, (iOpen, iClose) => new { iOpen, iClose }).ToArray();
            var sb = new StringBuilder();
            for (int iPair = 0; iPair < pairs.Length; iPair++)
            {
                var pair = pairs[iPair];
                if (pair.iClose < pair.iOpen)
                    throw new StringInterpolationException("Unexpected '}}' at {0}.".F(pair.iClose));
                var between = format.Substring(pair.iOpen + 1, pair.iClose - pair.iOpen - 1).Trim();
                if (between.Length == 0)
                    throw new StringInterpolationException("Error: '{}'.");
                string propName;
                string formatter = null;
                var nColons = between.Count(c => c == ':');
                if (nColons == 0)
                    propName = between;
                else if (nColons == 1)
                {
                    var asd = between.Split(':');
                    propName = asd[0];
                    formatter = asd[1];
                }
                else
                    throw new StringInterpolationException("Error: More than one ':'");
                int lastClosing = -1;
                if (iPair > 0)
                    lastClosing = pairs[iPair - 1].iClose;
                if (pair.iOpen - lastClosing > 1)
                    sb.Append(format.Substring(lastClosing + 1, pair.iOpen - lastClosing - 1).Replace("{{", "{").Replace("}}", "}"));

                try
                {
                    if (formatter != null)
                        sb.Append(DataBinder.Eval(o, propName, formatter));
                    else
                        sb.Append(DataBinder.Eval(o, propName));
                }
                catch (Exception ex)
                {
                    throw new StringInterpolationException("Error evaluating '{0}', see inner exception".F(propName), ex);
                }
            }
            var lastClose = pairs.Last().iClose;
            if (lastClose < format.Length - 1)
                sb.Append(format.Substring(lastClose + 1).Replace("{{", "{").Replace("}}", "}"));
            return sb.ToString();
        }
    }
}


#if UNIVERSAL || WINDOWS_PHONE

namespace System.Web.UI
{
    /// <summary>Stellt Unterstützung für RAD (Rapid Application Development)-Designer bereit, um Syntax für Datenbindungsausdrücke zu generieren und zu analysieren.Die Klasse kann nicht geerbt werden.</summary>
    public sealed class DataBinder
    {
        private static readonly char[] expressionPartSeparator = new char[] { '.' };
        private static readonly char[] indexExprStartChars = new char[] { '[', '(' };
        private static readonly char[] indexExprEndChars = new char[] { ']', ')' };

        /// <summary>Wertet Datenbindungsausdrücke zur Laufzeit aus.</summary>
        /// <returns>Eine <see cref="T:System.Object" />-Instanz, die bei der Auswertung des Datenbindungsausdrucks entsteht.</returns>
        /// <param name="container">Der Objektverweis, für den der Ausdruck ausgewertet wird.Dabei muss es sich um einen gültigen Objektbezeichner in der angegebenen Sprache der Seite handeln.</param>
        /// <param name="expression">Der Pfad vom <paramref name="container" />-Objekt zum Wert der öffentlichen Eigenschaften, der in der Eigenschaft des gebundenen Steuerelements platziert werden soll.Dieser muss eine Zeichenfolge von Eigenschaften- oder Feldnamen sein, die durch Punkte voneinander getrennt sind, z. B. Tables[0].DefaultView.[0].Price in C# oder Tables(0).DefaultView.(0).Price in Visual Basic.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="expression" /> ist null oder entspricht nach dem Verkürzen einer leeren Zeichenfolge.</exception>
        public static object Eval(object container, string expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            expression = expression.Trim();
            if (expression.Length == 0)
                throw new ArgumentNullException("expression");

            if (container == null)
                return null;

            string[] expressionParts = expression.Split(DataBinder.expressionPartSeparator);
            return DataBinder.Eval(container, expressionParts);
        }

        /// <summary>Wertet zur Laufzeit Datenbindungsausdrücke aus und formatiert das Ergebnis als Zeichenfolge.</summary>
        /// <returns>Ein <see cref="T:System.String" />-Objekt, das sich beim Auswerten des Datenbindungsausdrucks und dem Konvertieren in einen Zeichenfolgentyp ergibt.</returns>
        /// <param name="container">Der Objektverweis, für den der Ausdruck ausgewertet wird.Dabei muss es sich um einen gültigen Objektbezeichner in der angegebenen Sprache der Seite handeln.</param>
        /// <param name="expression">Der Pfad vom <paramref name="container" />-Objekt zum Wert der öffentlichen Eigenschaften, der in der Eigenschaft des gebundenen Steuerelements platziert werden soll.Dieser muss eine Zeichenfolge von Eigenschaften- oder Feldnamen sein, die durch Punkte voneinander getrennt sind, z. B. Tables[0].DefaultView.[0].Price in C# oder Tables(0).DefaultView.(0).Price in Visual Basic.</param>
        /// <param name="format">Eine .NET Framework-Formatierungszeichenfolge (wie von <see cref="M:System.String.Format(System.String,System.Object)" /> verwendet), die die vom Datenbindungsausdruck zurückgegebene <see cref="T:System.Object" />-Instanz in ein <see cref="T:System.String" />-Objekt konvertiert. </param>
        public static string Eval(object container, string expression, string format)
        {
            object obj = DataBinder.Eval(container, expression);
            if (obj == null)
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(format))
            {
                return obj.ToString();
            }
            return string.Format(format, obj);
        }

        /// <summary>Ruft den Wert der angegebenen Eigenschaft des angegebenen Objekts ab.</summary>
        /// <returns>Der Wert der angegebenen Eigenschaft.</returns>
        /// <param name="container">Das Objekt, das die Eigenschaft enthält. </param>
        /// <param name="propName">Der Name der Eigenschaft, die den abzurufenden Wert enthält. </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="container" /> ist null.– oder – <paramref name="propName" /> ist null oder eine leere Zeichenfolge (""). </exception>
        /// <exception cref="T:System.Web.HttpException">Das Objekt im <paramref name="container" /> weist nicht die von <paramref name="propName" /> angegebene Eigenschaft auf. </exception>
        public static object GetPropertyValue(object container, string propName)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            if (string.IsNullOrEmpty(propName))
                throw new ArgumentNullException("propName");

            return container.GetType().GetTypeInfo().GetDeclaredProperty(propName).GetMethod.Invoke(container, null);
            throw new Exception("DataBinder_Prop_Not_Found");
        }

        /// <summary>Ruft den Wert einer Eigenschaft des angegebenen Containers und Navigationspfades ab.</summary>
        /// <returns>Ein Objekt, das bei der Auswertung des Datenbindungsausdrucks entsteht.</returns>
        /// <param name="container">Der Objektverweis, anhand dessen <paramref name="expr" /> ausgewertet wird.Dabei muss es sich um einen gültigen Objektbezeichner in der angegebenen Sprache der Seite handeln.</param>
        /// <param name="expr">Der Navigationspfad vom <paramref name="container" />-Objekt zum Wert der öffentlichen Eigenschaften, der in der Eigenschaft des gebundenen Steuerelements platziert werden soll.Dieser muss eine Zeichenfolge von Eigenschaften- oder Feldnamen sein, die durch Punkte voneinander getrennt sind, z. B. Tables[0].DefaultView.[0].Price in C# oder Tables(0).DefaultView.(0).Price in Visual Basic.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="container" /> ist null.- oder -<paramref name="expr" /> ist null oder eine leere Zeichenfolge ("").</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="expr" /> ist kein gültiger indizierter Ausdruck.- oder -<paramref name="expr" /> lässt keinen indizierten Zugriff zu.</exception>
        public static object GetIndexedPropertyValue(object container, string expr)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            if (string.IsNullOrEmpty(expr))
                throw new ArgumentNullException("expr");

            object result = null;
            bool flag = false;
            int num = expr.IndexOfAny(DataBinder.indexExprStartChars);
            int num2 = expr.IndexOfAny(DataBinder.indexExprEndChars, num + 1);
            if (num < 0 || num2 < 0 || num2 == num + 1)
                throw new ArgumentException("DataBinder_Invalid_Indexed_Expr");

            string text = null;
            object obj = null;
            string text2 = expr.Substring(num + 1, num2 - num - 1).Trim();
            if (num != 0)
                text = expr.Substring(0, num);

            if (text2.Length != 0)
            {
                if ((text2[0] == '"' && text2[text2.Length - 1] == '"') || (text2[0] == '\'' && text2[text2.Length - 1] == '\''))
                {
                    obj = text2.Substring(1, text2.Length - 2);
                }
                else
                {
                    if (char.IsDigit(text2[0]))
                    {
                        int num3;
                        flag = int.TryParse(text2, NumberStyles.Integer, CultureInfo.InvariantCulture, out num3);
                        if (flag)
                        {
                            obj = num3;
                        }
                        else
                        {
                            obj = text2;
                        }
                    }
                    else
                    {
                        obj = text2;
                    }
                }
            }
            if (obj == null)
                throw new ArgumentException("DataBinder_Invalid_Indexed_Expr");

            object obj2;
            if (text != null && text.Length != 0)
            {
                obj2 = DataBinder.GetPropertyValue(container, text);
            }
            else
            {
                obj2 = container;
            }
            if (obj2 != null)
            {
                Array array = obj2 as Array;
                if (array != null && flag)
                {
                    result = array.GetValue((int)obj);
                }
                else
                {
                    if (obj2 is IList && flag)
                    {
                        result = ((IList)obj2)[(int)obj];
                    }
                    else
                    {
                        PropertyInfo property = obj2.GetType().GetRuntimeProperty("Item");
                        if (!(property != null))
                        {
                            throw new ArgumentException("DataBinder_No_Indexed_Accessor");
                        }
                        result = property.GetValue(obj2, new object[]
						{
							obj
						});
                    }
                }
            }
            return result;
        }

        private static object Eval(object container, string[] expressionParts)
        {
            object obj = container;
            int num = 0;
            while (num < expressionParts.Length && obj != null)
            {
                string text = expressionParts[num];
                if (text.IndexOfAny(DataBinder.indexExprStartChars) < 0)
                {
                    obj = DataBinder.GetPropertyValue(obj, text);
                }
                else
                {
                    obj = DataBinder.GetIndexedPropertyValue(obj, text);
                }
                num++;
            }
            return obj;
        }
    }
}


#endif