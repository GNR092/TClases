using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace TClases
{
    public class Config
    {
        public Dictionary<int, bool> BlockNPCs = new Dictionary<int, bool>() 
        {
        { 1, true },
        { 21, true },
        { 46, true },
        { 49, true }, 
        { 55, true }, 
        { 58, true }, 
        { 63, true }, 
        { 64, true }, 
        { 65, true }, 
        { 67, true }, 
        { 74, true }, 
        { 85, true }, 
        { 101, true }, 
        { 230, true }, 
        { 242, true }, 
        { 256, true }, 
        { 448, true }
        };

        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public static Config Read(string path)
        {
            return File.Exists(path) ? JsonConvert.DeserializeObject<Config>(File.ReadAllText(path)) : new Config();
        }
    }
}
