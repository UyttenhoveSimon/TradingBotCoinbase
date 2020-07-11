using CoinbasePro.Services.Orders.Models.Responses;
using CoinbasePro.Services.Orders.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BotTradingCoinbase
{
    public class GestionFractionneeAlgorithmeHaussier
    {
        private List<AlgorithmeHaussierOrdreUnique> algoHaussiers;
        public int NombreMaxAlgos { get; set; }
        private Pair pair;
        private decimal montantTotal;
        private static double tempsAttenteAchat = 2;

        public GestionFractionneeAlgorithmeHaussier(Pair pair, int nombreMaxAlgos, decimal montantTotal)
        {
            algoHaussiers = new List<AlgorithmeHaussierOrdreUnique>();
            this.pair = pair;
            NombreMaxAlgos = nombreMaxAlgos;
            this.montantTotal = montantTotal;
        }

        // apres 1h voir parmi la liste si (createdAt > 2h ou si delta > 1%) && si ordre achat
        public async Task Lancer()
        {
            Stopwatch sw = new Stopwatch();
            while (sw.Elapsed < TimeSpan.FromMinutes(240))
            {
                var ordresAchatEcartTemps = algoHaussiers.Where(s => ValiderEcartTemps(s.DernierOrdre));

                // utiliser messages pour signifier quand s'arreter
                if (algoHaussiers.Count < NombreMaxAlgos)
                {
                    AlgorithmeHaussierOrdreUnique alH = new AlgorithmeHaussierOrdreUnique(pair, 1000, 0.05m, 0.5m, algoHaussiers.Count + 1);
                    algoHaussiers.Add(alH);
                    await alH.Initialiser();
                    var order = await alH.Lancer();
                }
            }
        }

        private static bool ValiderEcartTemps(OrderResponse ordre)
        {
            return ordre.Side == OrderSide.Buy && (DateTime.Now.Subtract(ordre.CreatedAt.ToLocalTime()) > TimeSpan.FromHours(tempsAttenteAchat));
        }
    }
}