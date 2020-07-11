using CoinbasePro.Services.Orders.Models.Responses;
using CoinbasePro.Services.Orders.Types;
using CoinbasePro.Services.Products.Types;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BotTradingCoinbase
{
    // TODO gerer les erreurs (surtout le post-only)
    // TODO trouver un moyen d'evaluer la volatilité et fractionner en fonction.
    // TODO ajouter stop order
    internal class AlgorithmeHaussierOrdreUnique
    {
        private const int MillisecondsDelay = 10000;
        public int PourcentageAvantAnnulationAchat { get; set; } = 3;
        private static readonly NLog.Logger LoggerAH = NLog.LogManager.GetCurrentClassLogger();

        public OrderResponse DernierOrdre { get; set; }
        public bool ContinuerDeBoucler { get; set; } = true;

        private int iDAlgorithmeHaussier;
        private decimal montantFiatOrdre;
        private IPair pair;
        private decimal pourcentageBaisseAvantAchat;
        private decimal pourcentageHausseAvantVente;
        private string sauvegardeDernierOrdre;
        private int tailleMinimum;

        private ProductTicker derniereCotation;

        public AlgorithmeHaussierOrdreUnique(IPair pair, decimal montantFiat, decimal pourcentageAchat, decimal pourcentageVente, int id)
        {
            this.pair = pair;
            montantFiatOrdre = montantFiat;
            pourcentageBaisseAvantAchat = pourcentageAchat;
            pourcentageHausseAvantVente = pourcentageVente;
            iDAlgorithmeHaussier = id;
            sauvegardeDernierOrdre = "logAlgH_" + iDAlgorithmeHaussier + ".json";
        }

        public OrderResponse ChopperDernierOrdre()
        {
            return (File.Exists(sauvegardeDernierOrdre) && new FileInfo(sauvegardeDernierOrdre).Length > 0) ? JsonConvert.DeserializeObject<OrderResponse>(File.ReadAllText(sauvegardeDernierOrdre)) : null;
        }

        public async Task Initialiser()
        {
            if (pair.MinimumOrdre == 0)
            {
                await pair.MinimumMonnaies();
            }
            tailleMinimum = ChiffresApresVirgule(pair.MinimumOrdre);
            LoggerAH.Debug(nameof(tailleMinimum) + " " + tailleMinimum);
        }

        public async Task<OrderResponse> Lancer()
        {
            while (ContinuerDeBoucler)
            {
                var cotation = await pair.DerniereCotation();
                DernierOrdre = ChopperDernierOrdre();
                if (DernierOrdre != null)
                {
                    DernierOrdre = await pair.EtatOrdre(DernierOrdre.Id.ToString());
                }

                if (DernierOrdre != null && DernierOrdre.Status != OrderStatus.Done  && DernierOrdre.Side == OrderSide.Buy && ((DernierOrdre.Price - cotation.Price) > 0 || ((cotation.Price - DernierOrdre.Price)) > (cotation.Price * PourcentageAvantAnnulationAchat / 100)))
                {
                    await pair.AnnulerOrdreOuvert(DernierOrdre.Id.ToString());
                    DernierOrdre = null;
                }

                if (DernierOrdre == null || (DernierOrdre.Side == OrderSide.Sell && DernierOrdre.Status == OrderStatus.Done))
                {
                    var compte = await pair.AvoirDuCompteFiat();
                    var derniereCotation = await pair.DerniereCotation();
                    var equivalentCryptoDuMontant = (montantFiatOrdre / derniereCotation.Price);

                    if (compte.Available < equivalentCryptoDuMontant)
                    {
                        montantFiatOrdre = compte.Available;
                    }

                    LoggerAH.Debug(nameof(montantFiatOrdre) + ": " + montantFiatOrdre + " " + pair.NomFiat);
                    var limiteAchat = derniereCotation.Price - (derniereCotation.Price * (pourcentageBaisseAvantAchat / 100));
                    var ordre = await pair.Acheter(Math.Round(equivalentCryptoDuMontant, tailleMinimum), Math.Round(limiteAchat, tailleMinimum));
                    SauvegarderOrdre(ordre);
                    // return ordre;
                }

                if (DernierOrdre != null && DernierOrdre.Side == OrderSide.Buy && DernierOrdre.Status == OrderStatus.Done)
                {
                    var compte = await pair.AvoirDuCompteCrypto();
                    var derniereCotation = await pair.DerniereCotation();
                    var equivalentCryptoDuMontant = (montantFiatOrdre / derniereCotation.Price);

                    if (compte.Available < equivalentCryptoDuMontant)
                    {
                        montantFiatOrdre = compte.Available;
                    }

                    var limiteVente = derniereCotation.Price + (derniereCotation.Price * pourcentageHausseAvantVente / 100);
                    var ordre = await pair.Vendre(Math.Round(equivalentCryptoDuMontant, tailleMinimum), Math.Round(limiteVente, tailleMinimum));
                    SauvegarderOrdre(ordre);
                    // return ordre;
                }

                //var dureeAttente = DateTime.Now.Subtract(dernierOrdre.CreatedAt.ToLocalTime());
                //if (dureeAttente > TimeSpan.FromMinutes(20) && statusOrdre.Side == OrderSide.Buy && (statusOrdre.Status == OrderStatus.Open || statusOrdre.Status == OrderStatus.Pending)) // si ordre d'achat prend trop de temps, renouveler
                //{
                //    await pair.AnnulerOrdreOuvert(dernierOrdre.Id.ToString());
                //    dernierOrdre = null;
                //}
                await Task.Delay(MillisecondsDelay);
            }
            return DernierOrdre;
        }

        private int ChiffresApresVirgule(decimal nombre)
        {
            int chiffres = 0;
            while (nombre % 1 != 0)
            {
                nombre *= 10;
                chiffres++;
            }
            return chiffres;
        }

        private void SauvegarderOrdre(OrderResponse ordre)
        {
            DernierOrdre = ordre;
            var cotation = pair.DerniereCotation();
            LoggerAH.Debug(ordre.Side + " " + ordre.OrderType + " pour " + ordre.Size + " à " + ordre.Price + " cours actuel: " + cotation.Result.Price + " à " + ordre.CreatedAt.ToLocalTime());
            string json = JsonConvert.SerializeObject(ordre, Formatting.Indented); // a mettre dans la db / ram dans le futur
            File.WriteAllText(sauvegardeDernierOrdre, json);
        }
    }
}