using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi;
using TerrariaApi.Server;
using TShockAPI;

namespace TClases
{
    [ApiVersion(1,23)]
    public class TClasesPlugin : TerrariaPlugin
    {
        public override string Name { get { return "TClases"; } }
        public override string Author { get { return "GNR092"; } }
        public override string Description { get { return ""; } }
        public override Version Version { get { return new Version("1.0.0"); } }
        public TClasesPlugin(Main game) : base(game) { Order += 1; }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        public void Zukulencia()
        {
        	
        }
        
    }
}
