using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace TrialRound.Extentions
{

    public class MatrixCell<T>
{
        public  T Value { get; set; }
       public Point Position { get; set; }
        public MatrixCell(T value,Point position )
        {
            Value = value;
            Position = position;
        }
}
    public static class Helpers
    {
        #region Path helpers



        #endregion


        #region Stream Helpers

        public static Stream AsStream(this string input)
        {
            // convert string to stream
            byte[] byteArray = Encoding.UTF8.GetBytes(input);
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            MemoryStream stream = new MemoryStream(byteArray);
            return stream;
        }

        public static string GetString(this Stream stream)
        {
            // convert stream to string
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();
            return text;
        }
        public static IEnumerable<T> ExtractValues<T>(this StreamReader reader, char separator = ' ')
        {
            var line = reader.ReadLine();
            return line.ExtractValues<T>(separator);
        }


        #endregion

        #region String Helpers
        public static T Convert<T>(this string input)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                //Cast ConvertFromString(string text) : object to (T)
                return (T)converter.ConvertFromString(input);
            }
            return default(T);
        }

        public static IEnumerable<T> ExtractValues<T>(this string input, char separator = ' ')
        {
            var splited = input.Split(separator);
            var result = splited.Select(Convert<T>);
            return result;
        }

        #endregion

        #region Object Helper
        public static T ChangeType<T>(this object obj)
        {
            return (T)System.Convert.ChangeType(obj, typeof(T));
        }

        #endregion

        #region Matrix Helper



        public static void ToBitmap<T>(this T[,] matrix, string outputPath, Func<T, Color> colorExporter, bool overwriteOutputFile = true)
        {
            if (!overwriteOutputFile && File.Exists(outputPath))
                return;

            Bitmap bitmap = new Bitmap(matrix.GetLength(1), matrix.GetLength(0));

            matrix.ForEach((cell, point) =>
            {
                var color = colorExporter(cell);

                bitmap.SetPixel(point.X, point.Y, color);
            });

            bitmap.Save(outputPath);
        }


        //public static T[,] CreateMatrix<T>(this StreamReader reader,Size matrixSize , char columnSeparator = ' ')
        //{
        //    int i = 0, j = 0;
        //    var matrix = new T[matrixSize.Width, matrixSize.Height];

        //    var line = reader.ReadLine();

        //    while (!string.IsNullOrEmpty(line))
        //    {
        //        var cols =line.Split(columnSeparator);
        //        foreach (var col in cols)
        //        {
        //            matrix[i, j] = col.Convert<T>();
        //            j++;
        //        }
        //        i++;
        //    }

        //    return matrix;
        //}

        public static TMatrix[,] CreateMatrix<TMatrix, TVal>(this StreamReader reader, Size matrixSize, Func<int, int, TVal, TMatrix[,], TMatrix> matcher)
        {
            int y = 0, x = 0;
            var matrix = new TMatrix[matrixSize.Height, matrixSize.Width];

            var line = reader.ReadLine();

            while (!string.IsNullOrEmpty(line))
            {
                x = 0;
                foreach (var col in line)
                {
                    var val = col.ToString().Convert<TVal>();
                    matrix[y, x] = matcher(y, x, val, matrix);
                    x++;
                }
                y++;
                line = reader.ReadLine();
            }

            return matrix;
        }

        public static Size Size<TMatrix>(this TMatrix[,] matrix)
        {
            return new Size(matrix.GetLength(1), matrix.GetLength(0));
        }

        public static IEnumerable<TMatrix> ToEnumerable<TMatrix>(this TMatrix[,] matrix)
        {
            for (int y = 0; y < matrix.GetLength(0); ++y)
                for (int x = 0; x < matrix.GetLength(1); ++x)
                {
                    yield return matrix[y, x];
                }
        }

        public static List<MatrixCell<TMatrix>> ToCellList<TMatrix>(this TMatrix[,] matrix)
        {
            var list = new List<MatrixCell<TMatrix>>();
            matrix.ForEach((cell, point) => list.Add(new MatrixCell<TMatrix>(cell,point)));
            return list;
        }

        public static void ForEach<TMatrix>(this TMatrix[,] matrix, Action<TMatrix, Point> action)
        {
            //if (matrix == null)
            //    throw Guard.ArgumentNull("source");
            //if (selector == null)
            //    throw Error.ArgumentNull("selector");
            for (int y = 0; y < matrix.GetLength(0); ++y)
                for (int x = 0; x < matrix.GetLength(1); ++x)
                {
                    action(matrix[y, x], new Point(x, y));
                }
        }

        public static void Save<TMatrix>(this TMatrix[,] matrix, string filename, bool overwriteOutputFile = true)
        {
            var list = matrix.ToCellList();
            list.SaveToFile(filename,overwriteOutputFile);
        }

        public static void Load<TMatrix>(this TMatrix[,] matrix, string filename)
        {
            var list = LoadFromFile<List<MatrixCell<TMatrix>>>(filename);
            foreach (var matrixCell in list)
            {
                matrix[matrixCell.Position.Y, matrixCell.Position.X] = matrixCell.Value;
            }
        }

        #endregion


        # region Serializer (protobuf)

        public static void SaveToFile<T>(this T objectToSerialize, string filename, bool overwriteOutputFile = true)
        {
            if (!overwriteOutputFile && File.Exists(filename))
                return;

            using (var file = File.Create(filename))
            {
                JsonSerializer serializer = new JsonSerializer();
                BsonWriter writer = new BsonWriter(file);
                serializer.Serialize(writer, objectToSerialize);
                
            }

        }

        public static T LoadFromFile<T>(string filename)
        {
            T objectToSerialize;
            using (var file = File.OpenRead(filename))
            {
                JsonSerializer serializer = new JsonSerializer();
                BsonReader reader = new BsonReader(file);
               objectToSerialize = serializer.Deserialize<T>(reader);
            }

            return objectToSerialize;
        }

        #endregion

        #region ConsoleHelper

        

        #endregion

    }
}
