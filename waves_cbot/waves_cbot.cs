using cAlgo.API;

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

        [Parameter("Label", Group = "Positions", DefaultValue = "WavesBot")]
        public string Label { get; set; }

        // [Parameter("Source", Group = "Hull MA")]
        // public DataSeries HullMaSource { get; set; }
        //
        // [Parameter("Period", DefaultValue = 30, Group = "Hull MA")]
        // public int HullMaPeriod { get; set; }

        [Parameter("Fast MA Period", Group = "Waves", DefaultValue = 33)]
        public int FastMaPeriod { get; set; }

        [Parameter("Slow MA Period", Group = "Waves", DefaultValue = 144)]
        public int SlowMaPeriod { get; set; }

        [Parameter("Ultimate MA Period", Group = "Waves", DefaultValue = 576)]
        public int UltimateMaPeriod { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.WilderSmoothing, Group = "Waves")]
        public MovingAverageType MaType { get; set; }

        [Parameter("Stop Loss In Pips", DefaultValue = 10, Group = "Stop Loss")]
        public double StopLossInPips { get; set; }

        [Parameter("Relative to slow band", DefaultValue = 0, Group = "Stop Loss")]
        public double StopLossRelativeToSlowBand { get; set; }

        [Parameter("Relative to fast band", DefaultValue = 0, Group = "Stop Loss")]
        public double StopLossRelativeToFastBand { get; set; }

        [Parameter("Relative to fast band Trigger (Pips)", DefaultValue = 0, Group = "Stop Loss")]
        public double StopLossRelativeToFastBandTrigger { get; set; }

        [Parameter("Break Even Trigger (Pips)", DefaultValue = 0, Group = "Stop Loss")]
        public double BreakEvenTrigger { get; set; }

        [Parameter("Break Even Offset (Pips)", DefaultValue = 1, Group = "Stop Loss")]
        public double BreakEvenOffset { get; set; }

        // For now only for relative to slower band
        // [Parameter("Trailing Stop Loss", DefaultValue = false, Group = "Stop Loss")]
        // public bool UseTrailingStopLoss { get; set; }

        [Parameter("Take Profit", DefaultValue = false, Group = "Take Profit")]
        public double TakeProfit { get; set; }

        [Parameter("Required bands distance to enter", DefaultValue = 0, Group = "Entry")]
        public double RequiredBandsDistanceToEnter { get; set; }

        [Parameter("Price to fast band maximal distance", DefaultValue = 0, Group = "Entry")]
        public double PriceToFastBandMaximalDistance { get; set; }

        [Parameter("Entry number per crossover", DefaultValue = 0, Group = "Entry")]
        public int EntryNumberPerCrossOver { get; set; }

        [Parameter("Higher TimeFrame Condition", DefaultValue = HigherTimeFrameConditionEnum.None, Group = "Entry")]
        public HigherTimeFrameConditionEnum HigherTimeFrameCondition { get; set; }

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

            _wavesIndicator = Indicators.GetIndicator<FourMovingAveragesWithCloud>(FastMaPeriod, SlowMaPeriod, UltimateMaPeriod,this.HigherTimeFrameCondition != HigherTimeFrameConditionEnum.None, MaType);

            var positionSizeCalculator =
                new PositionSizeCalculator(Account, DepositRiskPercentage, Symbol, Quantity, PositionSizeType);

            var stopLossCalculator = new StopLossCalculator(StopLossInPips, Bars, _wavesIndicator, StopLossRelativeToSlowBand, Symbol, StopLossRelativeToFastBand, StopLossRelativeToFastBandTrigger);

            var takeProfitCalculator = new TakeProfitCalculator(TakeProfit);

            _positionManager = new PositionManager(ClosePosition, Positions, Label, SymbolName, Print, ExecuteMarketOrder, stopLossCalculator, positionSizeCalculator, takeProfitCalculator);

            var higherTimeFrameEntryCondition =
                new HigherTimeFrameEntryCondition(Bars, _wavesIndicator, HigherTimeFrameCondition);

            var entrySignalGenerator = new EntrySignalGenerator(Bars, Symbol, History, _wavesIndicator, RequiredBandsDistanceToEnter, PriceToFastBandMaximalDistance, EntryNumberPerCrossOver, higherTimeFrameEntryCondition);

            var exitSignalGenerator = new ExitSignalGenerator(Bars, _wavesIndicator, ExitIfPriceCrossesSlowerBand, BandsCrossoverExit);

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
            // Disable Break Even strategy if BreakEvenTrigger is 0
            if (BreakEvenTrigger == 0)
                return;

            foreach (var position in Positions)
            {
                // Ensure we're dealing with the positions for this robot
                if (position.Label != this.Label)
                    continue;

                // Calculate profit in pips
                double profitPips = position.TradeType == TradeType.Buy
                    ? (Symbol.Bid - position.EntryPrice) / Symbol.PipSize
                    : (position.EntryPrice - Symbol.Ask) / Symbol.PipSize;

                // Check if profit has reached the Break Even Trigger
                if (profitPips >= BreakEvenTrigger)
                {
                    MoveStopLossToBreakEven(position);
                }
            }
        }

        private void MoveStopLossToBreakEven(Position position)
        {
            double newStopLossPrice = position.TradeType == TradeType.Buy
                ? position.EntryPrice + Symbol.PipSize * BreakEvenOffset
                : position.EntryPrice - Symbol.PipSize * BreakEvenOffset;

            // Check if Stop Loss is already at or beyond the Break Even price
            if ((position.TradeType == TradeType.Buy && position.StopLoss >= newStopLossPrice) ||
                (position.TradeType == TradeType.Sell && position.StopLoss <= newStopLossPrice))
                return;

            // Modify the position to move the stop loss
            ModifyPosition(position, newStopLossPrice, position.TakeProfit);

            Print($"Moved Stop Loss to Break Even for {position.TradeType} position: {position.Id}");
        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
    }
}
