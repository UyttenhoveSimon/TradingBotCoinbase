using CoinbasePro.Services.Accounts.Models;
using CoinbasePro.Services.Orders.Models.Responses;
using CoinbasePro.Services.Orders.Types;
using CoinbasePro.Services.Products.Models;
using CoinbasePro.Services.Products.Types;
using CoinbasePro.Shared.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotTradingCoinbase
{
    public interface IPair
    {
        string IdCrypto { get; set; }
        string IdFiat { get; set; }
        decimal MinimumOrdre { get; set; }
        CoinbasePro.Shared.Types.Currency NomCrypto { get; set; }
        CoinbasePro.Shared.Types.Currency NomFiat { get; set; }
        ProductType TypeProduit { get; set; }

        Task<OrderResponse> Acheter(decimal combien, decimal prix);
        Task<CancelOrderResponse> AnnulerOrdreOuvert(string id);
        Task<CancelOrderResponse> AnnulerTousOrdresOuverts();
        Task<Account> AvoirDuCompte(string id);
        Task<Account> AvoirDuCompteCrypto();
        Task<Account> AvoirDuCompteFiat();
        Task<ProductTicker> DerniereCotation();
        Task<OrderResponse> EtatOrdre(string id);
        Task<IEnumerable<Account>> ListerLesComptes();
        Task<IEnumerable<CoinbasePro.Services.Currencies.Models.Currency>> ListerLesMonnaies();
        Task<IList<IList<OrderResponse>>> ListerLesOrdres();
        Task<IEnumerable<Product>> ListerLesProduits();
        Task<IList<Candle>> ListerLesTaux(DateTime debut, DateTime fin, CandleGranularity granularite);
        Task<decimal> MinimumMonnaies();
        Task<OrderResponse> OrdreLimite(OrderSide order, decimal size, decimal price);
        Task<IList<OrderResponse>> OrdresAchatEnCours();
        Task<IList<OrderResponse>> OrdresEnCours();
        Task<IList<OrderResponse>> OrdresVenteEnCours();
        Task<OrderResponse> Vendre(decimal combien, decimal prix);
    }
}