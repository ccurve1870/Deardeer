using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace awcsc
{
    public class JsonSerializer
    {
        /// <summary>
        /// 通常用
        /// </summary>
        /// <typeparam name="TYpe">任意の型</typeparam>
        /// <returns></returns>
        public static DataContractJsonSerializer Serializer<TYpe>() => new DataContractJsonSerializer(typeof(TYpe));

        /// <summary>
        /// Listオブジェクト用
        /// </summary>
        /// <typeparam name="TYpe">任意の型</typeparam>
        /// <returns></returns>
        public static DataContractJsonSerializer SerializerList<TYpe>() => new DataContractJsonSerializer(typeof(List<TYpe>));

        /// <summary>
        /// Dictionaryオブジェクト用
        /// </summary>
        /// <typeparam name="TYpe1">任意の型</typeparam>
        /// <typeparam name="TYpe2">任意の型</typeparam>
        /// <returns></returns>
        public static DataContractJsonSerializer SerializerDictionary<TYpe1, TYpe2>() => new DataContractJsonSerializer(typeof(Dictionary<TYpe1, TYpe2>));
        /// <summary>
        /// Dictionaryオブジェクト用
        /// </summary>
        /// <typeparam name="TYpe1">任意の型</typeparam>
        /// <typeparam name="TYpe2">任意の型</typeparam>
        /// <returns></returns>
        public static DataContractJsonSerializer SerializerListDictionary<TYpe1, TYpe2>() => new DataContractJsonSerializer(typeof(List<Dictionary<TYpe1, TYpe2>>));

        public static void WriteResult(string path, List<Dictionary<string, List<double>>> dic)
        {
            using  (var fs1 = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                (new DataContractJsonSerializer(typeof(List<Dictionary<string, List<double>>>))).WriteObject(fs1, dic);

            }
        }
    }
}
