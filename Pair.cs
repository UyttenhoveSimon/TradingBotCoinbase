using CoinbasePro;
using CoinbasePro.Services.Accounts.Models;
using CoinbasePro.Services.Orders.Models.Responses;
using CoinbasePro.Services.Orders.Types;
using CoinbasePro.Services.Products.Models;
using CoinbasePro.Services.Products.Types;
using CoinbasePro.Shared.Types;
using CoinbasePro.Shared.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotTradingCoinbase
{
    // TODO Voir pour la gestion des erreurs
    public class Pair : IPair
    {
        private static readonly NLog.Logger LoggerPair = NLog.LogManager.GetCurrentClassLogger();

        internal Pair()
        {
        }

        internal Pair(CoinbaseProClient coinbaseProClient, string idCrypto, string idFiat, ProductType productType)
        {
            this.coinbaseProClient = coinbaseProClient;
            this.IdCrypto = idCrypto;
            this.IdFiat = idFiat;
            this.TypeProduit = productType;
            NomCrypto = productType.BaseCurrency();
            NomFiat = productType.QuoteCurrency();
        }

        public string IdCrypto { get; set; }
        public string IdFiat { get; set; }
        public decimal MinimumOrdre { get; set; }
        public Currency NomCrypto { get; set; }
        public Currency NomFiat { get; set; }
        public ProductType TypeProduit { get; set; }
        private CoinbaseProClient coinbaseProClient { get; set; }

        public virtual async Task<OrderResponse> Acheter(decimal combien, decimal prix)
        {
            return await OrdreLimite(OrderSide.Buy, combien, prix);
        }

        public virtual async Task<Account> AvoirDuCompte(string id)
        {
            try
            {
                var compte = await coinbaseProClient.AccountsService.GetAccountByIdAsync(id);
                LoggerPair.Debug(nameof(compte.Available) + " " + compte.Available);
                return compte;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<Account> AvoirDuCompteCrypto()
        {
            return await AvoirDuCompte(IdCrypto);
        }

        public virtual async Task<Account> AvoirDuCompteFiat()
        {
            return await AvoirDuCompte(IdFiat);
        }

        public virtual async Task<ProductTicker> DerniereCotation()
        {
            try
            {
                var lastTick = await coinbaseProClient.ProductsService.GetProductTickerAsync(TypeProduit);
                Extensions.LoopProperties(lastTick);
                return lastTick;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<IEnumerable<Account>> ListerLesComptes()
        {
            try
            {
                var allAccounts = await coinbaseProClient.AccountsService.GetAllAccountsAsync();
                allAccounts.ToList().ForEach(account => Extensions.LoopProperties(account));
                return allAccounts;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<IEnumerable<CoinbasePro.Services.Currencies.Models.Currency>> ListerLesMonnaies()
        {
            try
            {
                var allCurrencies = await coinbaseProClient.CurrenciesService.GetAllCurrenciesAsync();
                allCurrencies.ToList().ForEach(account => Extensions.LoopProperties(account));
                return allCurrencies;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<IList<IList<OrderResponse>>> ListerLesOrdres()
        {
            try
            {
                var ordres = await coinbaseProClient.OrdersService.GetAllOrdersAsync();
                return ordres;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<IEnumerable<Product>> ListerLesProduits()
        {
            try
            {
                var products = await coinbaseProClient.ProductsService.GetAllProductsAsync();
                return products;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<IList<Candle>> ListerLesTaux(DateTime debut, DateTime fin, CandleGranularity granularite)
        {
            try
            {
                var taux = await coinbaseProClient.ProductsService.GetHistoricRatesAsync(TypeProduit, debut, fin, granularite);
                return taux;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<Decimal> MinimumMonnaies()
        {
            var produits = await ListerLesProduits();
            MinimumOrdre = produits.Single(produit => produit.Id == TypeProduit).QuoteIncrement;
            LoggerPair.Debug(MinimumOrdre);
            return MinimumOrdre;
        }

        public virtual async Task<OrderResponse> OrdreLimite(OrderSide order, decimal size, decimal price)
        {
            try
            {
                var orderBuy = await coinbaseProClient.OrdersService.PlaceLimitOrderAsync(order, TypeProduit, size, price);
                LoggerPair.Trace(Extensions.LoopProperties(orderBuy));
                return orderBuy;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<IList<OrderResponse>> OrdresEnCours()
        {
            try
            {
                var ordres = await ListerLesOrdres();
                return ordres.SelectMany(ordre => ordre).Where(ord => ord.Status == OrderStatus.Open).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<IList<OrderResponse>> OrdresAchatEnCours()
        {
            try
            {
                var ordres = await ListerLesOrdres();
                return ordres.SelectMany(s => s).Where(ord => ord.Side == OrderSide.Buy).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<IList<OrderResponse>> OrdresVenteEnCours()
        {
            try
            {
                var ordres = await ListerLesOrdres();
                return ordres.SelectMany(s => s).Where(ord => ord.Side == OrderSide.Sell).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<OrderResponse> EtatOrdre(string id)
        {
            try
            {
                return await coinbaseProClient.OrdersService.GetOrderByIdAsync(id);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<CancelOrderResponse> AnnulerTousOrdresOuverts()
        {
            try
            {
                var annulationOrdres = coinbaseProClient.OrdersService.CancelAllOrdersAsync();
                return await annulationOrdres;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<CancelOrderResponse> AnnulerOrdreOuvert(string id)
        {
            try
            {
                var annulationOrdre = coinbaseProClient.OrdersService.CancelOrderByIdAsync(id);
                return await annulationOrdre;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual async Task<OrderResponse> Vendre(decimal combien, decimal prix)
        {
            return await OrdreLimite(OrderSide.Sell, combien, prix);
        }
    }
}