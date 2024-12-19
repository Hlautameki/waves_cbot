using System;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.FullAccess, AddIndicators = true)]
    public class waves_cbot : Robot
    {
        [Parameter("Position Size Type", DefaultValue = PositionSizeType.Fixed, Group = "Volume")]
        public PositionSizeType PositionSizeType { get; set; }

        [Parameter("Quantity (Lots)", Group = "Volume", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        [Parameter("Deposit Risk Percentage", DefaultValue = 1, Group = "Volume")]
        public double DepositRiskPercentage { get; set; }

        // private HullMovingAverage _hullMa;

        [Parameter(DefaultValue = "Hello world!")]
        public string Message { get; set; }

        [Parameter("Label", Group = "Positions", DefaultValue = "HullMovingAverageSample")]
        public string Label { get; set; }

        // [Parameter("Source", Group = "Hull MA")]
        // public DataSeries HullMaSource { get; set; }
        //
        // [Parameter("Period", DefaultValue = 30, Group = "Hull MA")]
        // public int HullMaPeriod { get; set; }

        [Parameter("Fast MA Period", Group = "Waves")]
        public int FastMaPeriod { get; set; }

        [Parameter("Slow MA Period", Group = "Waves")]
        public int SlowMaPeriod { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.WilderSmoothing, Group = "Waves")]
        public MovingAverageType MaType { get; set; }

        [Parameter("Stop Loss In Pips", DefaultValue = 10, Group = "Stop Loss")]
        public double StopLossInPips { get; set; }

        [Parameter("Relative to slow band", DefaultValue = 0, Group = "Stop Loss")]
        public double StopLossRelativeToSlowBand { get; set; }

        // For now only for relative to slower band
        // [Parameter("Trailing Stop Loss", DefaultValue = false, Group = "Stop Loss")]
        // public bool UseTrailingStopLoss { get; set; }

        [Parameter("Take Profit", DefaultValue = false, Group = "Take Profit")]
        public double TakeProfit { get; set; }

        [Parameter("Required bands distance to enter", DefaultValue = 0, Group = "Entry")]
        public double RequiredBandsDistanceToEnter { get; set; }

        [Parameter("Exit if price crosses slower band", DefaultValue = false, Group = "Exit")]
        public bool ExitIfPriceCrossesSlowerBand { get; set; }

        [Parameter("Bands crossover exit", DefaultValue = false, Group = "Exit")]
        public bool BandsCrossoverExit { get; set; }

        private TradeManager _tradeManager;

        private PositionManager _positionManager;

        private FourMovingAveragesWithCloud _wavesIndicator;

        protected override void OnStart()
        {
            var result = System.Diagnostics.Debugger.Launch();

            if (result is false)
            {
                Print("Debugger launch failed");
            }
            // To learn more about cTrader Automate visit our Help Center:
            // https://help.ctrader.com/ctrader-automate

            Logger.Print = Print;

            _wavesIndicator = Indicators.GetIndicator<FourMovingAveragesWithCloud>(FastMaPeriod, SlowMaPeriod, MaType);

            // _hullMa = Indicators.HullMovingAverage(HullMaSource, HullMaPeriod);

            var positionSizeCalculator =
                new PositionSizeCalculator(Account, DepositRiskPercentage, Symbol, Quantity, PositionSizeType);

            var stopLossCalculator = new WavesStopLossCalculator(StopLossInPips, Bars, _wavesIndicator, StopLossRelativeToSlowBand, Symbol);

            var takeProfitCalculator = new WavesTakeProfitCalculator(TakeProfit);

            _positionManager = new PositionManager(ClosePosition, Positions, Label, SymbolName, Print, ExecuteMarketOrder, stopLossCalculator, positionSizeCalculator, takeProfitCalculator);

            var entrySignalGenerator = new WavesEntrySignalGenerator(Bars, Symbol, _wavesIndicator, RequiredBandsDistanceToEnter);

            var exitSignalGenerator = new WavesExitSignalGenerator(Bars, _wavesIndicator, ExitIfPriceCrossesSlowerBand, BandsCrossoverExit);

            _tradeManager = new TradeManager(entrySignalGenerator,
                Print, _positionManager, exitSignalGenerator);
        }

        protected override void OnBarClosed()
        {
            _tradeManager.ManageTrade();

            // if (UseTrailingStopLoss)
            // {
            _positionManager.UpdateStopLoss();
            // }
        }

        protected override void OnTick()
        {
            // Handle price updates here
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}
