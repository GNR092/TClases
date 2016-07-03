using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TClases
{
    public class Config
    {
        public decimal DmamagePoint = 1.0M;
        public Dictionary<int, bool> BlockNPCs = new Dictionary<int, bool>() 
        {
        { 1, true },    //Slime
        { 21, true },   //Skeleton
        { 22, true },   //Guide
        { 46, true },   //Bunny
        { 49, true },   //Cave Bat
        { 54, true },   //Clothier
        { 55, true },   //Cave Bat
        { 58, true },   //Piranha
        { 63, true },   //Blue Jellyfish
        { 64, true },   //Pink Jellyfish
        { 65, true },   //Shark
        { 67, true },   //Crab
        { 74, true },   //Bird
        { 85, true },   //Mimic
        { 86, true },   //Unicorn
        { 101, true },  //Clinger
        { 103, true },  //Green Jellyfish
        { 148, true },  //Penguin
        { 149, true },  //Penguin
        { 165, true },  //Wall Creeper 
        { 167, true },  //Undead Viking
        { 171, true },  //Pigron  
        { 230, true },  //Walking Goldfish
        { 242, true },  //Blood Jelly
        { 256, true },  //Fungo Fish
        { 299, true },  //Squirrel
        { 300, true },  //Mouse
        { 355, true },  //Firefly
        { 356, true },  //Butterfly
        { 357, true },  //Worm
        { 358, true },  //Lightning Bug
        { 359, true },  //Snail
        { 360, true },  //Glowing Snail
        { 361, true },  //Frog
        { 362, true },  //Mallard Duck
        { 363, true },  //Mallard Duck
        { 364, true },  //Duck
        { 365, true },  //Duck
        { 267, true },  //Scorpion
        { 377, true },  //Grasshopper
        { 448, true },  //Gold Worm
        { 449, true },  //Skeleton
        { 485, true },  //Grubby
        { 486, true },  //Sluggy
        { 487, true },  //Buggy
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
