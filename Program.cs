using CoinbasePro;
using CoinbasePro.Services.Accounts.Models;
using CoinbasePro.Shared.Types;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotTradingCoinbase
{
    internal class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly static string passSandbox = "name";
        private readonly static string apiKeySandbox = "apikey";
        private readonly static string secretSandbox = "secret";

        internal static string idUSDSandbox { get; } = "a1f7ea0a-948c-4c57-85da-33e197507750";
        internal static string idLTCSandbox { get; } = "659d8b21-34af-44c8-b05f-596cb877ac3b";
        internal static string idETHSandbox { get; } = "4a8b70e3-cb1c-4b66-9844-772abe1e6530";
        internal static string idBTCSandbox { get; } = "a1d4e869-266e-4682-bda5-5c41dda6572b";
        internal static string idEURSandbox { get; } = "e59bb5e1-8d58-4f01-98be-43e76f874be0";

        private static string nomDuFichier = @"TestGDAX.txt";
        private static int nombreAlgos = 0;

        private static void PrepareNLog()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = nomDuFichier };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config
            LogManager.Configuration = config;
        }

        private static async Task Main(string[] args)
        {
            PrepareNLog();

            // File.Create(nomDuFichier).Close();

            var authenticator = new CoinbasePro.Network.Authentication.Authenticator(apiKeySandbox,secretSandbox, passSandbox);
            var coinbaseProClient = new CoinbaseProClient(authenticator, sandBox: true);

            Pair pairBTCEUR = new Pair(coinbaseProClient, idBTCSandbox, idEURSandbox, ProductType.BtcEur);
            Pair pairETHEUR = new Pair(coinbaseProClient, idETHSandbox, idEURSandbox, ProductType.EthEur);
            Pair pairLTCEUR = new Pair(coinbaseProClient, idLTCSandbox, idEURSandbox, ProductType.LtcEur);

            IEnumerable<Account> accounts;

            accounts = await pairBTCEUR.ListerLesComptes();

            //Task.Run(async () =>
            //{
            //    await pair.ListerLesProduits();
            //}).GetAwaiter().GetResult();

            //Task.Run(async () =>
            //{
            //    await pair.ListerLesMonnaies();
            //}).GetAwaiter().GetResult();

            //Task.Run(async () =>
            //{
            //    await pair.DerniereCotation();
            //}).GetAwaiter().GetResult();

            Task.Run(async () =>
            {
                await pairBTCEUR.AvoirDuCompteCrypto();
            }).GetAwaiter().GetResult();

            Task.Run(async () =>
            {
                await pairBTCEUR.AvoirDuCompteFiat();
            }).GetAwaiter().GetResult();

            //Task.Run(async () =>
            //{
            //    await pair.MinimumMonnaies();
            //}).GetAwaiter().GetResult();

            //Task.Run(async () =>
            //{
            //    await pair.AnnulerTousOrdresOuverts();
            //}).GetAwaiter().GetResult();

            //AlgorithmeHaussierOrdreUnique alHETHEUR = new AlgorithmeHaussierOrdreUnique(pairETHEUR, 1000, 0.05m, 1.5m, ++nombreAlgos);
            //await alHETHEUR.Initialiser();
            //alHETHEUR.Lancer();

            //AlgorithmeHaussierOrdreUnique alHLTCEUR = new AlgorithmeHaussierOrdreUnique(pairLTCEUR, 1000, 0.05m, 1.5m, ++nombreAlgos);
            //await alHLTCEUR.Initialiser();
            //alHLTCEUR.Lancer();

            AlgorithmeHaussierOrdreUnique alHBTCEUR = new AlgorithmeHaussierOrdreUnique(pairBTCEUR, 3500, 0.05m, 1.5m, ++nombreAlgos);
            await alHBTCEUR.Initialiser();
            alHBTCEUR.Lancer();

            Console.ReadKey();
        }
    }
}
